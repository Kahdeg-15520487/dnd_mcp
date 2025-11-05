using System.ClientModel;
using NetHackChatGame.Data.Entities;
using NetHackChatGame.LlmProxy.Models;
using OpenAI;
using ModelContextProtocol.Client;
using Microsoft.Extensions.AI;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using OpenAI.Responses;
using NetHackChatGame.Data;
using Microsoft.EntityFrameworkCore;

namespace NetHackChatGame.LlmProxy.Services;

public class LlmService : ILlmService
{
    private readonly NetHackDbContext _dbContext;
    private readonly IToolExecutor _toolExecutor;
    private readonly ILlmConfigurationService _configService;
    private readonly ILogger<LlmService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private McpClient? _mcpClient;
    private IChatClient? _chatClient;

    public LlmService(
        NetHackDbContext dbContext,
        IToolExecutor toolExecutor,
        ILlmConfigurationService configService,
        ILogger<LlmService> logger,
        ILoggerFactory loggerFactory)
    {
        _dbContext = dbContext;
        _toolExecutor = toolExecutor;
        _configService = configService;
        _logger = logger;
        _loggerFactory = loggerFactory;
    }

    private async Task<McpClient> GetMcpClientAsync(LlmConfiguration config)
    {
        if (_mcpClient != null)
        {
            return _mcpClient;
        }

        // Create HttpClient for MCP transport with increased timeout
        var httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5) // 5 minute timeout for LLM responses
        };

        // Create MCP transport to connect to MCP server
        _logger.LogInformation(config.MCPEndpoint);
        var transport = new HttpClientTransport(new()
        {
            Endpoint = new Uri(config.MCPEndpoint ?? "http://mcp-server:5002"),
            Name = "NetHack MCP Client"
            // No OAuth for internal service-to-service communication
        }, httpClient, _loggerFactory);

        // Create MCP client
        _mcpClient = await McpClient.CreateAsync(transport, loggerFactory: _loggerFactory);
        _logger.LogInformation("MCP client initialized and connected to server");

        return _mcpClient;
    }

    private IChatClient GetOpenAIChatClient(LlmConfiguration config)
    {
        if (_chatClient != null)
        {
            return _chatClient;
        }

        OpenAIClient openAIClient;
        var openAIOptions = new OpenAIClientOptions
        {
            NetworkTimeout = TimeSpan.FromMinutes(5) // 5 minute timeout for LLM responses
        };

        if (!string.IsNullOrEmpty(config.Endpoint))
        {
            // Use custom endpoint (e.g., Ollama, Azure OpenAI)
            var apiKey = config.ApiKey ?? "not-needed";
            openAIOptions.Endpoint = new Uri(config.Endpoint);
            openAIClient = new OpenAIClient(new ApiKeyCredential(apiKey), openAIOptions);
            _logger.LogInformation("Using custom endpoint: {Endpoint}", config.Endpoint);
        }
        else if (!string.IsNullOrEmpty(config.ApiKey))
        {
            // Use OpenAI with API key
            openAIClient = new OpenAIClient(new ApiKeyCredential(config.ApiKey), openAIOptions);
            _logger.LogInformation("Using OpenAI API");
        }
        else
        {
            // Default to unauthenticated (for local LLM servers like Ollama)
            _logger.LogWarning("No OpenAI API key or endpoint configured");
            openAIClient = new OpenAIClient(new ApiKeyCredential("not-needed"), openAIOptions);
        }

        // Convert OpenAI ChatClient to IChatClient
        _chatClient = Microsoft.Extensions.AI.OpenAIClientExtensions.AsIChatClient(openAIClient.GetChatClient(config.Model));

        return _chatClient;
    }

    public async Task<Models.ChatResponse> ProcessChatAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        // Load conversation history
        var conversation = await _dbContext.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

        if (conversation == null)
        {
            throw new InvalidOperationException($"Conversation {request.ConversationId} not found");
        }

        // Add user message to database
        var userMessage = new MessageEntity
        {
            ConversationId = request.ConversationId,
            Role = "user",
            Content = request.UserMessage,
            SequenceNumber = conversation.Messages.Count
        };
        _dbContext.Messages.Add(userMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Build messages for LLM using Microsoft.Extensions.AI types
        var messages = new List<ChatMessage>();

        // Add system message if this is the first message
        if (conversation.Messages.Count == 0)
        {
            messages.Add(new ChatMessage(ChatRole.System, "You are a game master for a NetHack-style text adventure game. Guide the player through dungeon exploration, combat, and treasure hunting. Use the available tools to check game state and execute player actions."));
        }

        foreach (var msg in conversation.Messages.OrderBy(m => m.SequenceNumber))
        {
            if (msg.Role == "user")
            {
                messages.Add(new ChatMessage(ChatRole.User, msg.Content));
            }
            else if (msg.Role == "assistant")
            {
                messages.Add(new ChatMessage(ChatRole.Assistant, msg.Content));
            }
        }

        // Add the new user message
        messages.Add(new ChatMessage(ChatRole.User, request.UserMessage));

        // Get LLM configuration
        var config = _configService.GetConfiguration();

        // Get MCP client and available tools
        var mcpClient = await GetMcpClientAsync(config);
        IList<McpClientTool> tools = await mcpClient.ListToolsAsync();

        _logger.LogInformation("Found {ToolCount} MCP tools available", tools.Count);

        // Get chat client
        var chatClient = GetOpenAIChatClient(config);

        var chatOptions = new ChatOptions
        {
            Temperature = (float?)config.Temperature,
            MaxOutputTokens = config.MaxTokens,
            Tools = [.. tools]  // Pass MCP tools directly to IChatClient
        };

        // Use the extension method from Microsoft.Extensions.AI
        var completion = await chatClient.GetResponseAsync(
            messages,
            chatOptions,
            cancellationToken);

        // Handle tool calls if requested
        while (completion.FinishReason == ChatFinishReason.ToolCalls)
        {
            _logger.LogInformation("LLM requested tool call(s)");
            
            // Process all tool calls from the completion
            foreach (var message in completion.Messages)
            {
                // Add assistant's message with tool calls to conversation
                messages.Add(message);

                foreach (var content in message.Contents)
                {
                    if (content is FunctionCallContent toolCallContent)
                    {
                        _logger.LogInformation("Executing tool: {ToolName}", toolCallContent.Name);
                        
                        try
                        {
                            // Execute the tool via MCP
                            var toolCallResult = await mcpClient.CallToolAsync(
                                toolCallContent.Name, 
                                toolCallContent.Arguments?.AsReadOnly() ?? new Dictionary<string, object?>().AsReadOnly());
                            
                            _logger.LogInformation("Tool {ToolName} executed successfully", toolCallContent.Name);
                            
                            // Add tool result to messages
                            var resultContent = new FunctionResultContent(
                                toolCallContent.CallId,
                                toolCallContent.Name)
                            {
                                Result = toolCallResult
                            };
                            
                            messages.Add(new ChatMessage(ChatRole.Tool, [resultContent]));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error executing tool {ToolName}", toolCallContent.Name);
                            
                            // Add error result to messages
                            var errorContent = new FunctionResultContent(
                                toolCallContent.CallId,
                                toolCallContent.Name)
                            {
                                Result = $"Error executing tool: {ex.Message}"
                            };
                            
                            messages.Add(new ChatMessage(ChatRole.Tool, [errorContent]));
                        }
                    }
                }
            }
            
            // Send tool results back to LLM for final response
            _logger.LogInformation("Sending tool results back to LLM");
            completion = await chatClient.GetResponseAsync(
                messages,
                chatOptions,
                cancellationToken);
        }

        var assistantContent = completion.Text ?? string.Empty;

        // Save the assistant message
        var finalMessage = new MessageEntity
        {
            ConversationId = request.ConversationId,
            Role = "assistant",
            Content = assistantContent,
            SequenceNumber = conversation.Messages.Count + 1
        };
        _dbContext.Messages.Add(finalMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new Models.ChatResponse
        {
            AssistantMessage = assistantContent,
            RequiresToolCall = false
        };
    }
}
