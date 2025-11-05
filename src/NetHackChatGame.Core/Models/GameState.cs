namespace NetHackChatGame.Core.Models;

/// <summary>
/// Represents the complete game state for a conversation
/// </summary>
public class GameState
{
    public Guid ConversationId { get; set; }
    public Player Player { get; set; } = new();
    public Dungeon Dungeon { get; set; } = new();
    public Guid CurrentRoomId { get; set; }
    public CombatState? ActiveCombat { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Get the current room the player is in
    /// </summary>
    public Room? GetCurrentRoom() =>
        Dungeon.Rooms.FirstOrDefault(r => r.Id == CurrentRoomId);
}
