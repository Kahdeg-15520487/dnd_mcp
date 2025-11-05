namespace NetHackChatGame.LlmProxy.Models;

public class ChatRequest
{
    public Guid ConversationId { get; set; }
    public string UserMessage { get; set; } = string.Empty;
}
