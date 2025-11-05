using NetHackChatGame.Core.Models;

namespace NetHackChatGame.McpServer.Services;

/// <summary>
/// Service for managing game state and operations
/// </summary>
public interface IGameStateService
{
    /// <summary>
    /// Get the current game state for a conversation
    /// </summary>
    Task<GameState> GetGameStateAsync(Guid conversationId);
    
    /// <summary>
    /// Save game state snapshot
    /// </summary>
    Task SaveGameStateAsync(Guid conversationId, Guid messageId, GameState gameState);
    
    /// <summary>
    /// Get the current room information
    /// </summary>
    Task<Room> GetCurrentRoomAsync(Guid conversationId);
    
    /// <summary>
    /// Get player statistics
    /// </summary>
    Task<Player> GetPlayerStatsAsync(Guid conversationId);
    
    /// <summary>
    /// Move player to a new room
    /// </summary>
    Task<(bool Success, string Message, Room? NewRoom)> MoveToRoomAsync(Guid conversationId, Direction direction);
    
    /// <summary>
    /// Perform a combat action
    /// </summary>
    Task<(bool Success, string Message, CombatState? CombatState)> PerformCombatActionAsync(
        Guid conversationId, string action);
    
    /// <summary>
    /// Loot treasure from the current room
    /// </summary>
    Task<(bool Success, string Message, List<Item>? Items)> LootTreasureAsync(Guid conversationId);
}
