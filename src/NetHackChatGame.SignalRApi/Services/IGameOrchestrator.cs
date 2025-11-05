using NetHackChatGame.SignalRApi.Models;

namespace NetHackChatGame.SignalRApi.Services;

public interface IGameOrchestrator
{
    Task<GameResponse> ProcessMessageAsync(Guid conversationId, string message);
}
