namespace NetHackChatGame.Core.Models;

/// <summary>
/// Represents a dungeon layout
/// </summary>
public class Dungeon
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public int Seed { get; set; }
    public List<Room> Rooms { get; set; } = new();
    public Guid StartingRoomId { get; set; }
}
