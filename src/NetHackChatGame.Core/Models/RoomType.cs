namespace NetHackChatGame.Core.Models;

/// <summary>
/// Type of room in the dungeon
/// </summary>
public enum RoomType
{
    /// <summary>
    /// Normal empty room
    /// </summary>
    Normal,
    
    /// <summary>
    /// Room with monsters that must be defeated
    /// </summary>
    Combat,
    
    /// <summary>
    /// Room containing treasure or items
    /// </summary>
    Treasure,
    
    /// <summary>
    /// Room with a boss monster
    /// </summary>
    Boss,
    
    /// <summary>
    /// Hidden room with special rewards
    /// </summary>
    Secret
}
