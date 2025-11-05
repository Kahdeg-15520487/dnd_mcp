using Microsoft.AspNetCore.SignalR;
using NetHackChatGame.SignalRApi.Models;
using NetHackChatGame.SignalRApi.Services;

namespace NetHackChatGame.SignalRApi.Hubs;

public class ChatHub : Hub
{
    private readonly IGameOrchestrator _orchestrator;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IGameOrchestrator orchestrator, ILogger<ChatHub> logger)
    {
        _orchestrator = orchestrator;
        _logger = logger;
    }

    public async Task SendMessage(Guid conversationId, string message)
    {
        try
        {
            _logger.LogInformation("User {ConnectionId} sent message for conversation {ConversationId}", 
                Context.ConnectionId, conversationId);

            // Send acknowledgment immediately
            await Clients.Caller.SendAsync("MessageReceived", new { conversationId, message });

            // Process the message through LLM
            var response = await _orchestrator.ProcessMessageAsync(conversationId, message);

            // Send response back to user
            await Clients.Caller.SendAsync("ReceiveMessage", response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message");
            await Clients.Caller.SendAsync("Error", new { error = ex.Message });
        }
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}
