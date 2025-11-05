namespace NetHackChatGame.LlmProxy.Services;

public interface IToolExecutor
{
    Task<string> ExecuteToolAsync(Guid conversationId, string toolName, string arguments, CancellationToken cancellationToken = default);
}
