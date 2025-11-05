namespace NetHackChatGame.Core.Models;

/// <summary>
/// Represents a room in the dungeon
/// </summary>
public class Room
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public RoomType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Visited { get; set; }
    public bool IsCleared { get; set; }
    public List<Monster> Monsters { get; set; } = new();
    public List<Item> Items { get; set; } = new();
    public List<RoomExit> Exits { get; set; } = new();
}
