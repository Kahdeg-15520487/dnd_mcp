namespace NetHackChatGame.LlmProxy.Models;

public class LlmConfiguration
{
    public string? ApiKey { get; set; }
    public string? Endpoint { get; set; }
    public string Model { get; set; } = "gpt-4o-mini";
    public float Temperature { get; set; } = 0.7f;
    public int MaxTokens { get; set; } = 2000;
    public string? MCPEndpoint { get; set; }
}

public class LlmConfigurationResponse
{
    public string? Endpoint { get; set; }
    public string Model { get; set; } = "gpt-4o-mini";
    public float Temperature { get; set; }
    public int MaxTokens { get; set; }
    public bool HasApiKey { get; set; }
    public string Status { get; set; } = "Not configured";
    public string? MCPEndpoint { get; set; }
}
