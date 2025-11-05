namespace NetHackChatGame.SignalRApi.Models;

public class GameResponse
{
    public string Message { get; set; } = string.Empty;
    public object? GameState { get; set; }
}
