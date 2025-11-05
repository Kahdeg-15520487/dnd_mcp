using System.ComponentModel;
using System.Text.Json;
using NetHackChatGame.Core.Models;
using NetHackChatGame.McpServer.Services;
using ModelContextProtocol.Server;

namespace NetHackChatGame.McpServer.Tools;

/// <summary>
/// MCP tools for game state management and operations
/// </summary>
[McpServerToolType]
public class GameTools
{
    private readonly IGameStateService _gameStateService;

    public GameTools(IGameStateService gameStateService)
    {
        _gameStateService = gameStateService;
    }

    [McpServerTool, Description("Get the current game state for a conversation, including player stats, current room, and combat status.")]
    public async Task<string> GetGameState(Guid conversationId)
    {
        var gameState = await _gameStateService.GetGameStateAsync(conversationId);
        return JsonSerializer.Serialize(gameState, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get detailed information about the current room including description, exits, items, and monsters.")]
    public async Task<string> GetCurrentRoom(Guid conversationId)
    {
        var room = await _gameStateService.GetCurrentRoomAsync(conversationId);
        return JsonSerializer.Serialize(room, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get player statistics including health, level, experience, gold, and inventory.")]
    public async Task<string> GetPlayerStats(Guid conversationId)
    {
        var player = await _gameStateService.GetPlayerStatsAsync(conversationId);
        return JsonSerializer.Serialize(player, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Move the player to a new room in the specified direction (North, South, East, or West).")]
    public async Task<string> MoveToRoom(
        Guid conversationId,
        [Description("Direction to move: North, South, East, or West")] string direction)
    {
        if (!Enum.TryParse<Direction>(direction, true, out var directionEnum))
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = $"Invalid direction: {direction}. Valid directions are: North, South, East, West"
            }, new JsonSerializerOptions { WriteIndented = true });
        }

        var result = await _gameStateService.MoveToRoomAsync(conversationId, directionEnum);
        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message,
            newRoom = result.NewRoom
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Perform a combat action during battle. Valid actions include: attack, defend, flee, use_item.")]
    public async Task<string> PerformCombatAction(
        Guid conversationId,
        [Description("Combat action to perform: attack, defend, flee, or use_item")] string action)
    {
        var result = await _gameStateService.PerformCombatActionAsync(conversationId, action);
        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message,
            combatState = result.CombatState
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Loot treasure from the current room. Returns the items found and added to player inventory.")]
    public async Task<string> LootTreasure(Guid conversationId)
    {
        var result = await _gameStateService.LootTreasureAsync(conversationId);
        return JsonSerializer.Serialize(new
        {
            success = result.Success,
            message = result.Message,
            items = result.Items
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Save the current game state as a snapshot associated with a message.")]
    public async Task<string> SaveGameState(
        Guid conversationId,
        Guid messageId,
        [Description("JSON string representing the game state to save")] string gameStateJson)
    {
        try
        {
            var gameState = JsonSerializer.Deserialize<GameState>(gameStateJson);
            if (gameState == null)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Failed to deserialize game state"
                }, new JsonSerializerOptions { WriteIndented = true });
            }

            await _gameStateService.SaveGameStateAsync(conversationId, messageId, gameState);
            return JsonSerializer.Serialize(new
            {
                success = true,
                message = "Game state saved successfully"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                message = $"Error saving game state: {ex.Message}"
            }, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
