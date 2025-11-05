using System.Text;
using System.Text.Json;

namespace NetHackChatGame.LlmProxy.Services;

public class ToolExecutor : IToolExecutor
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ToolExecutor> _logger;

    public ToolExecutor(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<ToolExecutor> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> ExecuteToolAsync(
        Guid conversationId,
        string toolName,
        string arguments,
        CancellationToken cancellationToken = default)
    {
        var mcpServerUrl = _configuration["McpServer:BaseUrl"] ?? "http://localhost:5002";
        
        _logger.LogInformation("Executing tool {ToolName} for conversation {ConversationId}", toolName, conversationId);

        try
        {
            var endpoint = toolName switch
            {
                "get_current_room" => $"{mcpServerUrl}/api/game/current-room",
                "get_player_stats" => $"{mcpServerUrl}/api/game/player-stats",
                "move_to_room" => $"{mcpServerUrl}/api/game/move",
                "combat_action" => $"{mcpServerUrl}/api/game/combat",
                "loot_treasure" => $"{mcpServerUrl}/api/game/loot",
                _ => throw new InvalidOperationException($"Unknown tool: {toolName}")
            };

            // Parse arguments
            var args = JsonSerializer.Deserialize<JsonElement>(arguments);

            // Build request based on tool
            HttpResponseMessage response;

            switch (toolName)
            {
                case "get_current_room":
                case "get_player_stats":
                    // GET requests
                    response = await _httpClient.GetAsync(
                        $"{endpoint}?conversationId={conversationId}",
                        cancellationToken);
                    break;

                case "move_to_room":
                {
                    var direction = args.GetProperty("direction").GetString();
                    var requestBody = new
                    {
                        conversationId,
                        direction
                    };
                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                    break;
                }

                case "combat_action":
                {
                    var action = args.GetProperty("action").GetString();
                    var requestBody = new
                    {
                        conversationId,
                        action
                    };
                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                    break;
                }

                case "loot_treasure":
                {
                    var itemName = args.GetProperty("item_name").GetString();
                    var requestBody = new
                    {
                        conversationId,
                        itemName
                    };
                    var json = JsonSerializer.Serialize(requestBody);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                    break;
                }

                default:
                    throw new InvalidOperationException($"Unknown tool: {toolName}");
            }

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogInformation("Tool {ToolName} executed successfully", toolName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing tool {ToolName}", toolName);
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
