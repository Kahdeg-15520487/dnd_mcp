namespace NetHackChatGame.Core.Models;

/// <summary>
/// Represents an exit from a room
/// </summary>
public class RoomExit
{
    public Direction Direction { get; set; }
    public Guid TargetRoomId { get; set; }
    public bool IsLocked { get; set; }
}
