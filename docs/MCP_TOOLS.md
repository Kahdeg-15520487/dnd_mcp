# MCP Tools Reference

This document describes all Model Context Protocol (MCP) tools exposed by the MCP Server for game interaction.

## Overview

The MCP Server exposes 5 core tools that allow the LLM to:
- Query game state
- Execute player actions
- Manage combat encounters
- Handle inventory and looting

All tools accept a `conversationId` parameter to identify which game session to operate on.

---

## Tool: `get_current_room`

Get detailed information about the player's current location.

### Input Schema

```json
{
  "conversationId": "string (UUID)"
}
```

### Output Schema

```json
{
  "roomId": "string (UUID)",
  "roomType": "string (Normal | Combat | Treasure | Boss | Secret)",
  "description": "string",
  "visited": "boolean",
  "monsters": [
    {
      "id": "string (UUID)",
      "name": "string",
      "hp": "number",
      "maxHp": "number",
      "attack": "number",
      "defense": "number",
      "isAlive": "boolean"
    }
  ],
  "items": [
    {
      "id": "string (UUID)",
      "name": "string",
      "type": "string (Weapon | Armor | Potion | Treasure)",
      "description": "string",
      "value": "number"
    }
  ],
  "exits": [
    {
      "direction": "string (North | South | East | West)",
      "roomId": "string (UUID)",
      "isLocked": "boolean"
    }
  ]
}
```

### Example Usage

**LLM Request:**
```json
{
  "name": "get_current_room",
  "arguments": {
    "conversationId": "123e4567-e89b-12d3-a456-426614174000"
  }
}
```

**Response:**
```json
{
  "roomId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "roomType": "Combat",
  "description": "A dank stone chamber with moss-covered walls. The air is thick with the smell of decay.",
  "visited": false,
  "monsters": [
    {
      "id": "monster-001",
      "name": "Goblin Scout",
      "hp": 15,
      "maxHp": 15,
      "attack": 3,
      "defense": 1,
      "isAlive": true
    }
  ],
  "items": [],
  "exits": [
    {
      "direction": "North",
      "roomId": "room-002",
      "isLocked": false
    },
    {
      "direction": "East",
      "roomId": "room-003",
      "isLocked": false
    }
  ]
}
```

### Use Cases

- Player asks "What do I see?" or "Look around"
- LLM needs to describe the current situation
- Before narrating any action, check room state

### Error Conditions

| Error | Reason | Response |
|-------|--------|----------|
| `ConversationNotFound` | Invalid conversationId | Error message |
| `GameStateCorrupted` | Cannot load game state | Error message |

---

## Tool: `get_player_stats`

Retrieve the player's current statistics and inventory.

### Input Schema

```json
{
  "conversationId": "string (UUID)"
}
```

### Output Schema

```json
{
  "name": "string",
  "hp": "number",
  "maxHp": "number",
  "level": "number",
  "experience": "number",
  "experienceToNextLevel": "number",
  "gold": "number",
  "inventory": [
    {
      "id": "string (UUID)",
      "name": "string",
      "type": "string (Weapon | Armor | Potion | Treasure)",
      "description": "string",
      "equipped": "boolean",
      "quantity": "number"
    }
  ],
  "equippedWeapon": {
    "name": "string",
    "damage": "number"
  },
  "equippedArmor": {
    "name": "string",
    "defense": "number"
  }
}
```

### Example Usage

**LLM Request:**
```json
{
  "name": "get_player_stats",
  "arguments": {
    "conversationId": "123e4567-e89b-12d3-a456-426614174000"
  }
}
```

**Response:**
```json
{
  "name": "Adventurer",
  "hp": 28,
  "maxHp": 30,
  "level": 2,
  "experience": 150,
  "experienceToNextLevel": 200,
  "gold": 45,
  "inventory": [
    {
      "id": "item-001",
      "name": "Health Potion",
      "type": "Potion",
      "description": "Restores 15 HP",
      "equipped": false,
      "quantity": 2
    },
    {
      "id": "item-002",
      "name": "Iron Sword",
      "type": "Weapon",
      "description": "A sturdy iron blade",
      "equipped": true,
      "quantity": 1
    }
  ],
  "equippedWeapon": {
    "name": "Iron Sword",
    "damage": 5
  },
  "equippedArmor": {
    "name": "Leather Armor",
    "defense": 2
  }
}
```

### Use Cases

- Player asks "What's my health?" or "Check inventory"
- LLM needs to know if player can survive a fight
- Before using an item, verify it's in inventory

---

## Tool: `move_to_room`

Move the player to an adjacent room in the specified direction.

### Input Schema

```json
{
  "conversationId": "string (UUID)",
  "direction": "string (North | South | East | West)"
}
```

### Output Schema

```json
{
  "success": "boolean",
  "message": "string",
  "previousRoomId": "string (UUID)",
  "newRoomId": "string (UUID)",
  "newRoom": {
    // Same structure as get_current_room response
  }
}
```

### Example Usage

**LLM Request:**
```json
{
  "name": "move_to_room",
  "arguments": {
    "conversationId": "123e4567-e89b-12d3-a456-426614174000",
    "direction": "North"
  }
}
```

**Success Response:**
```json
{
  "success": true,
  "message": "You move north into a new chamber.",
  "previousRoomId": "room-001",
  "newRoomId": "room-002",
  "newRoom": {
    "roomId": "room-002",
    "roomType": "Treasure",
    "description": "A small alcove with a wooden chest in the corner.",
    "visited": false,
    "monsters": [],
    "items": [
      {
        "id": "item-101",
        "name": "Gold Coins",
        "type": "Treasure",
        "description": "A pile of shiny gold coins",
        "value": 50
      }
    ],
    "exits": [
      {
        "direction": "South",
        "roomId": "room-001",
        "isLocked": false
      }
    ]
  }
}
```

**Failure Response:**
```json
{
  "success": false,
  "message": "There is no exit to the north.",
  "previousRoomId": "room-001",
  "newRoomId": "room-001",
  "newRoom": null
}
```

### Use Cases

- Player says "Go north", "Move east", "Head west"
- LLM decides to explore a different area
- Fleeing from combat (if flee is successful)

### Error Conditions

| Error | Reason | success = false |
|-------|--------|-----------------|
| `NoExit` | No exit in that direction | Message explains |
| `LockedDoor` | Exit is locked | Message explains |
| `InCombat` | Cannot move during combat | Message explains |

---

## Tool: `combat_action`

Perform a combat action against an enemy.

### Input Schema

```json
{
  "conversationId": "string (UUID)",
  "action": "string (Attack | Defend | Flee | UseItem)",
  "targetMonsterId": "string (UUID, optional)",
  "itemId": "string (UUID, optional, required if action=UseItem)"
}
```

### Output Schema

```json
{
  "success": "boolean",
  "message": "string",
  "playerDamageDealt": "number",
  "playerDamageTaken": "number",
  "monsterKilled": "boolean",
  "monsterName": "string",
  "monsterHpRemaining": "number",
  "playerHpRemaining": "number",
  "combatOver": "boolean",
  "victory": "boolean",
  "experienceGained": "number",
  "goldDropped": "number",
  "itemsDropped": [
    {
      "id": "string (UUID)",
      "name": "string",
      "type": "string"
    }
  ]
}
```

### Example Usage: Attack

**LLM Request:**
```json
{
  "name": "combat_action",
  "arguments": {
    "conversationId": "123e4567-e89b-12d3-a456-426614174000",
    "action": "Attack",
    "targetMonsterId": "monster-001"
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "You swing your sword at the Goblin Scout!",
  "playerDamageDealt": 7,
  "playerDamageTaken": 2,
  "monsterKilled": false,
  "monsterName": "Goblin Scout",
  "monsterHpRemaining": 8,
  "playerHpRemaining": 26,
  "combatOver": false,
  "victory": false,
  "experienceGained": 0,
  "goldDropped": 0,
  "itemsDropped": []
}
```

### Example Usage: Kill Monster

**Response:**
```json
{
  "success": true,
  "message": "You deliver a fatal blow to the Goblin Scout!",
  "playerDamageDealt": 9,
  "playerDamageTaken": 0,
  "monsterKilled": true,
  "monsterName": "Goblin Scout",
  "monsterHpRemaining": 0,
  "playerHpRemaining": 26,
  "combatOver": true,
  "victory": true,
  "experienceGained": 25,
  "goldDropped": 10,
  "itemsDropped": [
    {
      "id": "item-201",
      "name": "Rusty Dagger",
      "type": "Weapon"
    }
  ]
}
```

### Example Usage: Use Potion

**LLM Request:**
```json
{
  "name": "combat_action",
  "arguments": {
    "conversationId": "123e4567-e89b-12d3-a456-426614174000",
    "action": "UseItem",
    "itemId": "item-001"
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "You drink a Health Potion and restore 15 HP!",
  "playerDamageDealt": 0,
  "playerDamageTaken": 3,
  "monsterKilled": false,
  "monsterName": "Goblin Scout",
  "monsterHpRemaining": 8,
  "playerHpRemaining": 38,
  "combatOver": false,
  "victory": false,
  "experienceGained": 0,
  "goldDropped": 0,
  "itemsDropped": []
}
```

### Combat Rules

1. **Turn Order**: Player acts first, then all alive monsters
2. **Damage Calculation**: 
   - Player damage = Weapon damage + roll(1-6) - Monster defense
   - Monster damage = Monster attack + roll(1-6) - Player armor
3. **Critical Hits**: Roll of 6 = double damage
4. **Defend**: Reduces damage by 50% for one turn
5. **Flee**: 50% success chance, moves to random adjacent room

### Use Cases

- Player says "Attack the goblin"
- Player says "Drink health potion"
- Player says "Defend myself"
- Player says "Run away"

### Error Conditions

| Error | Reason | Response |
|-------|--------|----------|
| `NoCombat` | No active combat | success = false |
| `InvalidTarget` | Monster ID not in room | success = false |
| `ItemNotFound` | Item not in inventory | success = false |
| `PlayerDead` | Player HP <= 0 | Game over message |

---

## Tool: `loot_treasure`

Pick up an item from the current room and add it to inventory.

### Input Schema

```json
{
  "conversationId": "string (UUID)",
  "itemId": "string (UUID, optional)"
}
```

**Note**: If `itemId` is not provided and there's only one item, it will be picked up automatically.

### Output Schema

```json
{
  "success": "boolean",
  "message": "string",
  "item": {
    "id": "string (UUID)",
    "name": "string",
    "type": "string",
    "description": "string",
    "value": "number"
  },
  "goldGained": "number",
  "inventoryCount": "number"
}
```

### Example Usage: Loot Item

**LLM Request:**
```json
{
  "name": "loot_treasure",
  "arguments": {
    "conversationId": "123e4567-e89b-12d3-a456-426614174000",
    "itemId": "item-101"
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "You pick up the Gold Coins and add them to your purse!",
  "item": {
    "id": "item-101",
    "name": "Gold Coins",
    "type": "Treasure",
    "description": "A pile of shiny gold coins",
    "value": 50
  },
  "goldGained": 50,
  "inventoryCount": 3
}
```

### Example Usage: Auto-loot Single Item

**LLM Request:**
```json
{
  "name": "loot_treasure",
  "arguments": {
    "conversationId": "123e4567-e89b-12d3-a456-426614174000"
  }
}
```

**Response:**
```json
{
  "success": true,
  "message": "You pick up the Rusty Dagger.",
  "item": {
    "id": "item-201",
    "name": "Rusty Dagger",
    "type": "Weapon",
    "description": "An old, rusty dagger",
    "value": 5
  },
  "goldGained": 0,
  "inventoryCount": 4
}
```

### Item Types and Effects

| Type | Effect |
|------|--------|
| **Treasure** | Converts to gold automatically |
| **Weapon** | Can be equipped to increase damage |
| **Armor** | Can be equipped to increase defense |
| **Potion** | Single-use consumable (heal, buff) |

### Use Cases

- Player says "Take the treasure"
- Player says "Pick up the sword"
- After combat, auto-loot monster drops
- LLM suggests looting after describing a treasure room

### Error Conditions

| Error | Reason | Response |
|-------|--------|----------|
| `NoItems` | No items in room | success = false |
| `ItemNotFound` | itemId not in room | success = false |
| `MultipleItems` | No itemId provided but >1 item | List available items |
| `InventoryFull` | Max inventory reached | success = false |

---

## Tool Calling Best Practices

### For the LLM

1. **Always check state first**: Call `get_current_room` and `get_player_stats` before narrating
2. **One action at a time**: Don't chain multiple movement or combat actions
3. **Confirm success**: Check the `success` field before narrating outcomes
4. **Describe results**: Use tool output to create engaging narrative
5. **Handle failures gracefully**: If a tool fails, explain why to the player

### Example LLM Workflow

**Player**: "I want to explore the dungeon"

```
1. Call get_current_room() 
   → Get room description and exits
   
2. Narrate: "You stand in a stone chamber. To the north, you see a dark corridor..."

3. Wait for player to choose direction

Player: "I go north"

4. Call move_to_room(direction="North")
   → success=true, newRoom=...
   
5. Call get_current_room() (now in new room)
   → Room has a monster
   
6. Narrate: "As you enter the new room, a Goblin Scout leaps out at you!"

7. Automatic combat initiation (if room type is Combat)

Player: "I attack!"

8. Call combat_action(action="Attack", targetMonsterId=...)
   → playerDamageDealt=7, monsterHpRemaining=8
   
9. Narrate: "You swing your sword, dealing 7 damage! The goblin has 8 HP left."
```

---

## Error Response Format

All tools return errors in this format:

```json
{
  "success": false,
  "error": {
    "code": "string",
    "message": "string",
    "details": "object (optional)"
  }
}
```

### Common Error Codes

| Code | Description |
|------|-------------|
| `CONVERSATION_NOT_FOUND` | Invalid conversationId |
| `GAME_STATE_ERROR` | Cannot load/save game state |
| `INVALID_ACTION` | Action not allowed in current state |
| `NOT_IN_COMBAT` | Combat action used outside combat |
| `ALREADY_IN_COMBAT` | Movement during active combat |
| `NO_EXIT` | Invalid direction for movement |
| `ITEM_NOT_FOUND` | Item doesn't exist |
| `INSUFFICIENT_HP` | Player is dead |
| `INVENTORY_FULL` | Cannot carry more items |

---

## Testing Tools

For development and testing, use the MCP Inspector or direct HTTP calls:

### Example: Test get_current_room

```bash
curl -X POST http://localhost:5002/mcp/tools/call \
  -H "Content-Type: application/json" \
  -d '{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
      "name": "get_current_room",
      "arguments": {
        "conversationId": "test-conversation-001"
      }
    },
    "id": 1
  }'
```

---

## Conversation State Management

Each `conversationId` maintains:
- Player stats and inventory
- Current room position
- Combat state (if in combat)
- Visited rooms
- Dungeon layout

Game state is:
- **Created**: On first tool call for a conversationId
- **Loaded**: From database if exists
- **Cached**: In memory for performance
- **Saved**: After every tool call
- **Snapshot**: Full JSON saved to database

---

## Performance Considerations

- **Tool call latency**: 50-200ms (database lookup + logic)
- **State caching**: Game states cached for 30 minutes
- **Database writes**: Async, non-blocking
- **Concurrent calls**: Thread-safe state management

---

## Future Tools (Planned)

- `use_item` - Use consumables outside combat
- `equip_item` - Change equipped weapon/armor
- `rest` - Restore HP (limited uses)
- `inspect_item` - Get detailed item info
- `save_game` - Manual save checkpoint
- `get_map` - View explored areas
- `cast_spell` - Magic system
- `talk_to_npc` - Dialogue system

---

## Conclusion

These 5 core tools provide everything needed for a complete dungeon crawler experience. The LLM acts as the game master, using these tools to:
- Query game state
- Execute player actions
- Narrate outcomes
- Maintain game consistency

The MCP protocol ensures reliable, type-safe communication between the LLM and game engine.
