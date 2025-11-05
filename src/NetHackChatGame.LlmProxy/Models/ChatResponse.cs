namespace NetHackChatGame.LlmProxy.Models;

public class ChatResponse
{
    public string AssistantMessage { get; set; } = string.Empty;
    public bool RequiresToolCall { get; set; }
    public List<ToolCall>? ToolCalls { get; set; }
}

public class ToolCall
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}
