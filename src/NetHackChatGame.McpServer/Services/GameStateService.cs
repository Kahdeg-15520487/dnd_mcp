using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NetHackChatGame.Core.Models;
using NetHackChatGame.Data;
using NetHackChatGame.Data.Entities;

namespace NetHackChatGame.McpServer.Services;

/// <summary>
/// Implementation of game state service
/// </summary>
public class GameStateService : IGameStateService
{
    private readonly NetHackDbContext _dbContext;
    private readonly ILogger<GameStateService> _logger;
    private static readonly Random _random = new();

    public GameStateService(NetHackDbContext dbContext, ILogger<GameStateService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<GameState> GetGameStateAsync(Guid conversationId)
    {
        // Try to get the most recent game snapshot
        var snapshot = await _dbContext.GameSnapshots
            .Where(s => s.ConversationId == conversationId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

        if (snapshot != null)
        {
            return JsonSerializer.Deserialize<GameState>(snapshot.GameStateJson)
                ?? CreateNewGameState(conversationId);
        }

        // Create a new game state if none exists
        return CreateNewGameState(conversationId);
    }

    public async Task SaveGameStateAsync(Guid conversationId, Guid messageId, GameState gameState)
    {
        var snapshot = new GameSnapshotEntity
        {
            ConversationId = conversationId,
            MessageId = messageId,
            GameStateJson = JsonSerializer.Serialize(gameState)
        };

        _dbContext.GameSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Room> GetCurrentRoomAsync(Guid conversationId)
    {
        var gameState = await GetGameStateAsync(conversationId);
        var room = gameState.Dungeon.Rooms.FirstOrDefault(r => r.Id == gameState.CurrentRoomId);
        
        if (room == null)
        {
            throw new InvalidOperationException("Current room not found in dungeon");
        }

        return room;
    }

    public async Task<Player> GetPlayerStatsAsync(Guid conversationId)
    {
        var gameState = await GetGameStateAsync(conversationId);
        return gameState.Player;
    }

    public async Task<(bool Success, string Message, Room? NewRoom)> MoveToRoomAsync(
        Guid conversationId, Direction direction)
    {
        var gameState = await GetGameStateAsync(conversationId);
        
        // Check if player is in combat
        if (gameState.ActiveCombat != null)
        {
            return (false, "You cannot move while in combat!", null);
        }

        var currentRoom = gameState.Dungeon.Rooms.FirstOrDefault(r => r.Id == gameState.CurrentRoomId);
        if (currentRoom == null)
        {
            return (false, "Current room not found", null);
        }

        // Find the exit in the specified direction
        var exit = currentRoom.Exits.FirstOrDefault(e => e.Direction == direction);
        if (exit == null)
        {
            return (false, $"There is no exit to the {direction}", null);
        }

        // Move to the new room
        var newRoom = gameState.Dungeon.Rooms.FirstOrDefault(r => r.Id == exit.TargetRoomId);
        if (newRoom == null)
        {
            return (false, "Target room not found", null);
        }

        gameState.CurrentRoomId = newRoom.Id;

        // Check if entering combat
        if (newRoom.Type == RoomType.Combat && newRoom.Monsters.Count > 0 && !newRoom.IsCleared)
        {
            var monster = newRoom.Monsters[0]; // Fight the first monster
            gameState.ActiveCombat = new CombatState
            {
                Monster = monster,
                MonsterHealth = monster.Health,
                PlayerTurn = true
            };
        }

        return (true, $"You move {direction} to {newRoom.Name}", newRoom);
    }

    public async Task<(bool Success, string Message, CombatState? CombatState)> PerformCombatActionAsync(
        Guid conversationId, string action)
    {
        var gameState = await GetGameStateAsync(conversationId);
        
        if (gameState.ActiveCombat == null)
        {
            return (false, "You are not in combat", null);
        }

        var combat = gameState.ActiveCombat;
        var player = gameState.Player;

        if (!combat.PlayerTurn)
        {
            return (false, "It's not your turn", combat);
        }

        string message;
        
        // Handle player action
        switch (action.ToLower())
        {
            case "attack":
                var playerDamage = _random.Next(1, player.GetTotalDamage() + 1);
                combat.MonsterHealth -= playerDamage;
                message = $"You deal {playerDamage} damage to the {combat.Monster.Name}!";
                
                if (combat.MonsterHealth <= 0)
                {
                    // Monster defeated
                    player.Experience += combat.Monster.ExperienceReward;
                    var currentRoom = gameState.Dungeon.Rooms.FirstOrDefault(r => r.Id == gameState.CurrentRoomId);
                    if (currentRoom != null)
                    {
                        currentRoom.IsCleared = true;
                    }
                    gameState.ActiveCombat = null;
                    message += $"\n{combat.Monster.Name} defeated! You gain {combat.Monster.ExperienceReward} XP.";
                    
                    // Check for level up
                    if (player.Experience >= player.Level * 100)
                    {
                        player.Level++;
                        player.MaxHealth += 10;
                        player.Health = player.MaxHealth;
                        message += $"\nLevel up! You are now level {player.Level}!";
                    }
                    
                    return (true, message, null);
                }
                break;
                
            case "defend":
                message = "You brace yourself for the attack!";
                combat.PlayerDefending = true;
                break;
                
            default:
                return (false, "Invalid action. Use 'attack' or 'defend'", combat);
        }

        // Monster's turn
        combat.PlayerTurn = false;
        var monsterDamage = _random.Next(1, combat.Monster.Damage + 1);
        
        if (combat.PlayerDefending)
        {
            monsterDamage = Math.Max(1, monsterDamage - player.GetTotalDefense());
            combat.PlayerDefending = false;
        }
        
        player.Health -= monsterDamage;
        message += $"\nThe {combat.Monster.Name} attacks for {monsterDamage} damage!";
        
        if (player.Health <= 0)
        {
            message += "\nYou have been defeated!";
            gameState.ActiveCombat = null;
            return (true, message, null);
        }

        combat.PlayerTurn = true;
        return (true, message, combat);
    }

    public async Task<(bool Success, string Message, List<Item>? Items)> LootTreasureAsync(
        Guid conversationId)
    {
        var gameState = await GetGameStateAsync(conversationId);
        var currentRoom = gameState.Dungeon.Rooms.FirstOrDefault(r => r.Id == gameState.CurrentRoomId);
        
        if (currentRoom == null)
        {
            return (false, "Current room not found", null);
        }

        if (currentRoom.Type != RoomType.Treasure)
        {
            return (false, "This room contains no treasure", null);
        }

        if (currentRoom.IsCleared)
        {
            return (false, "This treasure has already been looted", null);
        }

        if (currentRoom.Items == null || currentRoom.Items.Count == 0)
        {
            return (false, "The treasure chest is empty", null);
        }

        // Add items to player inventory
        gameState.Player.Inventory.AddRange(currentRoom.Items);
        currentRoom.IsCleared = true;

        var message = $"You loot the treasure and find: {string.Join(", ", currentRoom.Items.Select(i => i.Name))}";
        
        return (true, message, currentRoom.Items);
    }

    private GameState CreateNewGameState(Guid conversationId)
    {
        // Create a simple starting dungeon
        var startRoom = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Entrance Hall",
            Description = "A dimly lit stone hall with ancient pillars. The air is musty and cold.",
            Type = RoomType.Normal,
            Exits = new List<RoomExit>()
        };

        var combatRoom = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Guard Chamber",
            Description = "A chamber once used by dungeon guards. A hostile creature lurks here!",
            Type = RoomType.Combat,
            Monsters = new List<Monster>
            {
                new Monster
                {
                    Name = "Goblin",
                    Description = "A small, nasty creature with sharp teeth",
                    Health = 20,
                    Damage = 5,
                    Defense = 2,
                    ExperienceReward = 50
                }
            },
            Exits = new List<RoomExit>()
        };

        var treasureRoom = new Room
        {
            Id = Guid.NewGuid(),
            Name = "Treasure Vault",
            Description = "A small vault containing forgotten treasures",
            Type = RoomType.Treasure,
            Items = new List<Item>
            {
                new Item
                {
                    Name = "Iron Sword",
                    Description = "A sturdy iron sword",
                    Type = ItemType.Weapon,
                    DamageBonus = 5
                },
                new Item
                {
                    Name = "Healing Potion",
                    Description = "Restores 30 health",
                    Type = ItemType.Consumable,
                    HealthBonus = 30
                }
            },
            Exits = new List<RoomExit>()
        };

        // Connect rooms
        startRoom.Exits.Add(new RoomExit { Direction = Direction.North, TargetRoomId = combatRoom.Id });
        combatRoom.Exits.Add(new RoomExit { Direction = Direction.South, TargetRoomId = startRoom.Id });
        combatRoom.Exits.Add(new RoomExit { Direction = Direction.East, TargetRoomId = treasureRoom.Id });
        treasureRoom.Exits.Add(new RoomExit { Direction = Direction.West, TargetRoomId = combatRoom.Id });

        var dungeon = new Dungeon
        {
            Id = Guid.NewGuid(),
            Name = "Tutorial Dungeon",
            Rooms = new List<Room> { startRoom, combatRoom, treasureRoom }
        };

        var conversation = _dbContext.Conversations.FirstOrDefault(c => c.Id == conversationId);
        var playerName = conversation?.PlayerName ?? "Adventurer";

        var player = new Player
        {
            Name = playerName,
            Class = "Warrior",
            Level = 1,
            Health = 100,
            MaxHealth = 100,
            Experience = 0,
            Inventory = new List<Item>()
        };

        return new GameState
        {
            Player = player,
            Dungeon = dungeon,
            CurrentRoomId = startRoom.Id,
            ActiveCombat = null
        };
    }
}
