using Microsoft.AspNetCore.Mvc;
using NetHackChatGame.McpServer.Services;

namespace NetHackChatGame.McpServer.Controllers;

/// <summary>
/// REST API controller for game tools
/// </summary>
[ApiController]
[Route("api/game")]
public class GameController : ControllerBase
{
    private readonly IGameStateService _gameStateService;
    private readonly ILogger<GameController> _logger;

    public GameController(IGameStateService gameStateService, ILogger<GameController> logger)
    {
        _gameStateService = gameStateService;
        _logger = logger;
    }

    /// <summary>
    /// Get the current room information
    /// </summary>
    [HttpGet("conversations/{conversationId:guid}/current-room")]
    public async Task<IActionResult> GetCurrentRoom(Guid conversationId)
    {
        try
        {
            var room = await _gameStateService.GetCurrentRoomAsync(conversationId);
            return Ok(new
            {
                success = true,
                room = new
                {
                    name = room.Name,
                    description = room.Description,
                    type = room.Type.ToString(),
                    exits = room.Exits.Select(e => new { direction = e.Direction.ToString() }),
                    hasMonster = room.Monsters.Count > 0 && !room.IsCleared,
                    hasTreasure = room.Type == Core.Models.RoomType.Treasure && !room.IsCleared
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current room for conversation {ConversationId}", conversationId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Get player statistics
    /// </summary>
    [HttpGet("conversations/{conversationId:guid}/player-stats")]
    public async Task<IActionResult> GetPlayerStats(Guid conversationId)
    {
        try
        {
            var player = await _gameStateService.GetPlayerStatsAsync(conversationId);
            return Ok(new
            {
                success = true,
                player = new
                {
                    name = player.Name,
                    @class = player.Class,
                    level = player.Level,
                    health = player.Health,
                    maxHealth = player.MaxHealth,
                    experience = player.Experience,
                    totalDamage = player.GetTotalDamage(),
                    totalDefense = player.GetTotalDefense(),
                    inventoryCount = player.Inventory.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player stats for conversation {ConversationId}", conversationId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Move to a room in the specified direction
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/move")]
    public async Task<IActionResult> MoveToRoom(Guid conversationId, [FromBody] MoveRequest request)
    {
        try
        {
            if (!Enum.TryParse<Core.Models.Direction>(request.Direction, true, out var direction))
            {
                return BadRequest(new { success = false, message = "Invalid direction. Use: North, South, East, or West" });
            }

            var (success, message, newRoom) = await _gameStateService.MoveToRoomAsync(conversationId, direction);
            
            if (!success)
            {
                return Ok(new { success = false, message });
            }

            return Ok(new
            {
                success = true,
                message,
                room = newRoom == null ? null : new
                {
                    name = newRoom.Name,
                    description = newRoom.Description,
                    type = newRoom.Type.ToString(),
                    exits = newRoom.Exits.Select(e => new { direction = e.Direction.ToString() }),
                    hasMonster = newRoom.Monsters.Count > 0 && !newRoom.IsCleared,
                    hasTreasure = newRoom.Type == Core.Models.RoomType.Treasure && !newRoom.IsCleared
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving in conversation {ConversationId}", conversationId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Perform a combat action
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/combat")]
    public async Task<IActionResult> PerformCombatAction(Guid conversationId, [FromBody] CombatRequest request)
    {
        try
        {
            var (success, message, combatState) = await _gameStateService.PerformCombatActionAsync(
                conversationId, request.Action);
            
            return Ok(new
            {
                success,
                message,
                combat = combatState == null ? null : new
                {
                    monster = combatState.Monster.Name,
                    monsterHealth = combatState.MonsterHealth,
                    monsterMaxHealth = combatState.Monster.Health,
                    playerTurn = combatState.PlayerTurn
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error performing combat action in conversation {ConversationId}", conversationId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Loot treasure from the current room
    /// </summary>
    [HttpPost("conversations/{conversationId:guid}/loot")]
    public async Task<IActionResult> LootTreasure(Guid conversationId)
    {
        try
        {
            var (success, message, items) = await _gameStateService.LootTreasureAsync(conversationId);
            
            return Ok(new
            {
                success,
                message,
                items = items?.Select(i => new
                {
                    name = i.Name,
                    description = i.Description,
                    type = i.Type.ToString(),
                    damageBonus = i.DamageBonus,
                    defenseBonus = i.DefenseBonus,
                    healthBonus = i.HealthBonus
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error looting treasure in conversation {ConversationId}", conversationId);
            return BadRequest(new { success = false, message = ex.Message });
        }
    }
}

public record MoveRequest(string Direction);
public record CombatRequest(string Action);
