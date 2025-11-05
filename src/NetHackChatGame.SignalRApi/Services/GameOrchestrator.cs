using System.Text;
using System.Text.Json;
using NetHackChatGame.SignalRApi.Models;

namespace NetHackChatGame.SignalRApi.Services;

public class GameOrchestrator : IGameOrchestrator
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GameOrchestrator> _logger;

    public GameOrchestrator(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GameOrchestrator> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<GameResponse> ProcessMessageAsync(Guid conversationId, string message)
    {
        try
        {
            var llmProxyUrl = _configuration["LlmProxy:BaseUrl"] ?? "http://localhost:5001";
            var endpoint = $"{llmProxyUrl}/api/chat";

            _logger.LogInformation("Sending message to LLM Proxy for conversation {ConversationId}", conversationId);

            var requestBody = new
            {
                conversationId,
                userMessage = message
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();
            var llmResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);

            var assistantMessage = llmResponse.GetProperty("assistantMessage").GetString() ?? string.Empty;

            _logger.LogInformation("Received response from LLM Proxy");

            return new GameResponse
            {
                Message = assistantMessage,
                GameState = null // Could fetch game state if needed
            };
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling LLM Proxy");
            return new GameResponse
            {
                Message = "Sorry, I'm having trouble connecting to the game server. Please try again.",
                GameState = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            return new GameResponse
            {
                Message = "An error occurred while processing your message.",
                GameState = null
            };
        }
    }
}
