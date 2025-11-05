using NetHackChatGame.LlmProxy.Models;

namespace NetHackChatGame.LlmProxy.Services;

public interface ILlmService
{
    Task<ChatResponse> ProcessChatAsync(ChatRequest request, CancellationToken cancellationToken = default);
}
