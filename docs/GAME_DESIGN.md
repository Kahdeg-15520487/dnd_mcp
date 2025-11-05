# Game Design Document

## Overview

NetHack Chat Game is a text-based dungeon crawler where players interact with an AI Game Master through natural language. The game combines classic roguelike mechanics with modern LLM-powered storytelling.

---

## Core Concept

**Genre**: Text-based dungeon crawler / Interactive fiction  
**Style**: NetHack-inspired roguelike  
**Platform**: Web browser (with potential for other platforms)  
**Players**: Single-player (v1), expandable to multiplayer (future)

**Hook**: An AI dungeon master that understands natural language and creates dynamic, engaging narratives based on traditional roguelike mechanics.

---

## Game Mechanics

### Player Character

#### Starting Stats

| Attribute | Initial Value | Description |
|-----------|---------------|-------------|
| **HP** | 30 | Health points, player dies at 0 |
| **Max HP** | 30 | Maximum health, increases with level |
| **Level** | 1 | Character level, affects damage and defense |
| **Experience** | 0 | Experience points, levels up at thresholds |
| **Gold** | 0 | Currency for purchasing items (future) |
| **Inventory Size** | 10 slots | Maximum items carried |

#### Leveling System

| Level | Experience Required | HP Bonus | Damage Bonus | Defense Bonus |
|-------|---------------------|----------|--------------|---------------|
| 1 | 0 | 30 | +0 | +0 |
| 2 | 100 | +5 | +1 | +0 |
| 3 | 250 | +5 | +1 | +1 |
| 4 | 500 | +5 | +2 | +1 |
| 5 | 1000 | +10 | +2 | +1 |
| 6+ | +500 each | +5 | +1 per 2 levels | +1 per 3 levels |

#### Starting Equipment

- **Weapon**: Rusty Dagger (Damage: 3)
- **Armor**: Tattered Cloth (Defense: 1)
- **Items**: 1x Health Potion (restores 15 HP)

---

### Dungeon Layout

#### Static Dungeon (v1)

A pre-designed dungeon with 10 rooms:

```
┌─────────────────────────────────────────┐
│         Static Dungeon Map              │
├─────────────────────────────────────────┤
│                                         │
│  [1. Entrance]                         │
│       │                                 │
│       ↓                                 │
│  [2. Dark Corridor]                    │
│       │                                 │
│   ┌───┴───┐                            │
│   ↓       ↓                            │
│ [3. Guard    [4. Storage Room]         │
│  Room]        (Treasure)                │
│   │                                     │
│   ↓                                     │
│ [5. Armory]                            │
│  (Treasure)                             │
│   │                                     │
│   ↓                                     │
│ [6. Prison Cell]                       │
│   │                                     │
│   ↓                                     │
│ [7. Torture Chamber]                   │
│  (Combat - 2 Goblins)                  │
│   │                                     │
│   ↓                                     │
│ [8. Secret Passage]                    │
│   │                                     │
│   ↓                                     │
│ [9. Treasure Vault]                    │
│  (Treasure - Chest)                    │
│   │                                     │
│   ↓                                     │
│ [10. Boss Room]                        │
│  (Combat - Orc Chieftain)              │
│   │                                     │
│   ↓                                     │
│  [EXIT]                                │
│                                         │
└─────────────────────────────────────────┘
```

#### Room Types

**1. Normal Room**
- Empty or minor decorations
- May have exits in multiple directions
- Safe, no monsters or treasure

**2. Combat Room**
- Contains 1-3 monsters
- Cannot leave until all monsters defeated or player flees
- Rewards: Experience, gold, possible item drops

**3. Treasure Room**
- Contains items or gold
- Usually empty of monsters
- Player can loot freely

**4. Boss Room**
- Single powerful enemy
- Significant rewards
- Often gates progression

**5. Secret Room** (future)
- Hidden, requires detection
- Valuable loot
- Rare encounters

---

### Combat System

#### Turn-Based Combat

**Turn Order**: Player acts first, then all living monsters.

#### Action Types

**1. Attack**
- Deal damage to a monster
- Formula: `(Weapon Damage + Roll(1-6) + Level Bonus) - Enemy Defense`
- Critical Hit (roll = 6): Double damage
- Miss (damage < 1): No damage dealt

**2. Defend**
- Reduce incoming damage by 50% for this turn
- Player still takes their turn to attack next round
- Useful when low on HP

**3. Use Item**
- Consume a potion or use equipment
- Takes the player's turn
- Monsters still attack after

**4. Flee**
- Attempt to escape combat
- 50% success rate
- On success: Move to random adjacent room
- On failure: Take full damage from all monsters, stay in combat

#### Damage Calculation

**Player Damage to Monster:**
```
Base = Equipped Weapon Damage
Roll = Random(1, 6)
Bonus = Player Level ÷ 2 (rounded down)
Defense = Monster Defense

Damage = (Base + Roll + Bonus) - Defense
If Roll == 6: Damage = Damage × 2  (Critical Hit)
If Damage < 1: Damage = 0
```

**Monster Damage to Player:**
```
Base = Monster Attack
Roll = Random(1, 6)
Defense = Player Armor Defense + (Player Level ÷ 3)

Damage = (Base + Roll) - Defense
If Roll == 6: Damage = Damage × 2  (Critical Hit)
If Damage < 1: Damage = 0
If Player is Defending: Damage = Damage ÷ 2
```

#### Combat Flow

1. **Initiation**: Player enters combat room
2. **Player Turn**: Player chooses action
3. **Resolution**: Player action executes
4. **Monster Turn**: All living monsters attack
5. **Check Victory**: If all monsters dead → combat ends
6. **Check Defeat**: If player HP ≤ 0 → game over
7. **Repeat**: Go to step 2

#### Rewards

**Experience**: `Monster Level × 20 + Random(5, 15)`

**Gold**: `Monster Level × 5 + Random(1, 10)`

**Item Drops** (30% chance):
- Common: Health Potion, Rusty Weapon
- Uncommon: Iron Weapon, Leather Armor
- Rare (5%): Magic Item, Rare Gem

---

### Monsters

#### Monster Stats

| Monster | Level | HP | Attack | Defense | XP | Gold | Special |
|---------|-------|-----|--------|---------|-----|------|---------|
| **Rat** | 1 | 5 | 1 | 0 | 10 | 2 | None |
| **Goblin Scout** | 1 | 15 | 3 | 1 | 20 | 5 | None |
| **Goblin Warrior** | 2 | 25 | 5 | 2 | 40 | 10 | None |
| **Orc** | 3 | 40 | 7 | 3 | 60 | 15 | None |
| **Orc Berserker** | 4 | 50 | 10 | 2 | 80 | 20 | Rage (double damage when HP < 50%) |
| **Skeleton** | 2 | 20 | 4 | 3 | 35 | 8 | Brittle (takes +50% damage) |
| **Troll** | 5 | 70 | 9 | 4 | 100 | 30 | Regeneration (heals 3 HP per turn) |
| **Dark Mage** | 4 | 30 | 8 | 2 | 90 | 25 | Magic Bolt (ignores armor) |
| **Orc Chieftain** (Boss) | 6 | 100 | 12 | 5 | 200 | 50 | Rally (heals allies if present) |

#### Monster Behaviors (Future)

- **Aggressive**: Always attacks
- **Defensive**: Prefers to defend when HP < 50%
- **Cowardly**: Attempts to flee when HP < 25%
- **Pack**: Bonus damage when multiple monsters present

---

### Items and Equipment

#### Weapons

| Name | Damage | Rarity | Value |
|------|--------|--------|-------|
| Rusty Dagger | 3 | Common | 5 |
| Iron Sword | 5 | Common | 20 |
| Steel Sword | 7 | Uncommon | 50 |
| Battle Axe | 9 | Uncommon | 80 |
| Magic Blade | 12 | Rare | 150 |
| Legendary Sword | 15 | Legendary | 500 |

#### Armor

| Name | Defense | Rarity | Value |
|------|---------|--------|-------|
| Tattered Cloth | 1 | Common | 5 |
| Leather Armor | 2 | Common | 15 |
| Chainmail | 4 | Uncommon | 40 |
| Plate Armor | 6 | Uncommon | 80 |
| Enchanted Armor | 8 | Rare | 200 |

#### Consumables

| Name | Effect | Rarity | Value |
|------|--------|--------|-------|
| Health Potion | Restore 15 HP | Common | 10 |
| Greater Health Potion | Restore 30 HP | Uncommon | 25 |
| Strength Potion | +3 damage for 3 turns | Uncommon | 30 |
| Defense Potion | +2 defense for 3 turns | Uncommon | 30 |
| Full Restore | Restore to Max HP | Rare | 50 |

#### Treasure

| Name | Gold Value | Rarity |
|------|------------|--------|
| Gold Coins | 10-20 | Common |
| Silver Chalice | 30 | Uncommon |
| Jeweled Ring | 50 | Uncommon |
| Ruby Gem | 100 | Rare |
| Ancient Artifact | 250 | Legendary |

---

### Game Loop

**1. Exploration Phase**
```
Player: "I look around"
AI: "You are in a dark corridor. Exits: North, East"
Player: "I go north"
AI: *Calls move_to_room tool*
AI: "You enter a new room..."
```

**2. Combat Phase**
```
AI: *Calls get_current_room, detects monster*
AI: "A goblin leaps out at you!"
Player: "I attack it"
AI: *Calls combat_action tool*
AI: "You deal 7 damage. The goblin has 8 HP left. It counterattacks for 3 damage!"
Player: "I attack again"
AI: *Calls combat_action tool*
AI: "You strike the final blow! The goblin is defeated. You gain 20 XP and 5 gold."
```

**3. Looting Phase**
```
AI: *Calls get_current_room, detects treasure*
AI: "You see a chest in the corner."
Player: "I open the chest"
AI: *Calls loot_treasure tool*
AI: "You find 50 gold coins and a Health Potion!"
```

**4. Progression**
```
Player: "What's my status?"
AI: *Calls get_player_stats tool*
AI: "HP: 22/30, Level 2, Gold: 75, Inventory: Iron Sword, 2x Health Potion"
```

---

### Win/Loss Conditions

#### Victory Conditions

**Primary Goal**: Defeat the Orc Chieftain in the Boss Room

**Optional Goals** (future):
- Collect all treasure (100% completion)
- Complete without using potions (achievement)
- Speed run under X turns

#### Defeat Conditions

**Game Over**: Player HP reaches 0

**Options on Death**:
- Restart from beginning
- Replay from conversation history (view only)
- Share death story (generate shareable link)

---

### Narrative Elements

#### AI Game Master Personality

The AI should:
- **Describe vividly**: Paint rich descriptions of rooms and combat
- **Build tension**: Create atmosphere in combat rooms
- **Celebrate victories**: Congratulate on defeating bosses
- **Maintain consistency**: Remember previous events
- **Guide subtly**: Hint at optimal choices without forcing them

#### Example Narratives

**Room Description:**
> "You step into a dank stone chamber, the air thick with the smell of mildew. Flickering torchlight casts dancing shadows on moss-covered walls. To the north, you see a heavy wooden door. To the east, a narrow corridor disappears into darkness."

**Combat Start:**
> "As you enter the armory, your footsteps echo on the stone floor. Suddenly, a goblin warrior bursts from behind a stack of crates, rusty scimitar raised high! Roll for initiative!"

**Critical Hit:**
> "Your blade finds a gap in the orc's armor! With a mighty swing, you cleave deep into its shoulder. Critical hit! The orc roars in pain as it staggers backward."

**Victory:**
> "With a final, desperate swing, you strike down the orc chieftain. The massive brute crashes to the floor, its weapon clattering away. The room falls silent. You've won!"

**Looting:**
> "Inside the ancient chest, you discover a trove of treasure: a pile of gold coins glinting in the torchlight, a shimmering health potion, and... is that a legendary sword? You carefully add the items to your pack."

---

### Difficulty Progression

#### Room Difficulty Scaling

| Room # | Expected Player Level | Monster Count | Monster Types |
|--------|----------------------|---------------|---------------|
| 1-2 | 1 | 0 | None (tutorial) |
| 3-4 | 1 | 1 | Weak (Rat, Goblin Scout) |
| 5-6 | 2 | 1-2 | Medium (Goblin Warrior, Skeleton) |
| 7-8 | 3 | 2 | Medium-Strong (Orc, multiple Goblins) |
| 9 | 4 | 0 | Treasure only |
| 10 | 5 | 1 | Boss (Orc Chieftain) |

#### Healing Opportunities

- **Treasure Rooms**: Often contain health potions
- **Safe Rooms**: Player can rest (future mechanic)
- **Defeating Enemies**: May drop potions

---

### Future Mechanics (Planned)

#### Phase 2: Procedural Dungeons
- Random dungeon generation using seed
- Different layouts each playthrough
- Difficulty scales with depth

#### Phase 3: Character Classes
- **Warrior**: High HP, strong melee
- **Rogue**: High dodge, critical hits
- **Mage**: Spells, area damage
- **Cleric**: Healing, support

#### Phase 4: Magic System
- Spell casting with mana
- Elemental damage types
- Buff/debuff spells

#### Phase 5: Advanced Features
- Merchant NPCs (buy/sell items)
- Crafting system
- Quests and objectives
- Dialogue choices affecting story
- Multiple endings

---

### Replay and Sharing

#### Conversation Replay

Players can replay their adventure:
- View complete message history
- See game state at each turn
- Watch combat unfold again
- Observe AI decision-making

#### Sharing System (Future)

Generate shareable links:
- **Public URL**: `https://app.com/replay/{conversationId}`
- **Viewer Mode**: Read-only playback
- **Statistics**: HP graph, damage dealt, items found
- **Highlights**: Boss defeats, critical hits

---

### Game Balance

#### Design Goals

1. **Fair but Challenging**: Player should feel threatened but not hopeless
2. **Risk vs Reward**: Treasure rooms near dangerous areas
3. **Resource Management**: Limited potions, strategic use
4. **Progression Feeling**: Each level up feels meaningful
5. **Recovery Options**: Always a way to heal or escape

#### Tuning Parameters

Adjust these values for balance:

```csharp
// Global balance settings
public static class GameBalance
{
    // Player progression
    public const int BaseHP = 30;
    public const int HPPerLevel = 5;
    public const int ExperienceMultiplier = 100;
    
    // Combat
    public const int CriticalMultiplier = 2;
    public const int DefendDamageReduction = 50; // percent
    public const int FleeSuccessRate = 50; // percent
    
    // Loot
    public const int ItemDropChance = 30; // percent
    public const int RareItemChance = 5; // percent
    
    // Monsters
    public const int MonsterDamageVariance = 6; // d6
    public const int MonsterHPMultiplier = 15;
}
```

---

### Accessibility Features

#### Natural Language Commands

Players can use any phrasing:
- "Attack the goblin" = "Hit it" = "Fight"
- "Go north" = "Move north" = "Head that way"
- "Check inventory" = "What do I have?" = "Show items"

#### Difficulty Assistance (Future)

- **Easy Mode**: 50% more HP, monsters deal less damage
- **Hints**: AI suggests optimal actions
- **Undo**: Revert last action

---

### Player Experience Flow

**Session 1 (5-10 minutes)**:
1. Start conversation, enter dungeon
2. Explore 2-3 rooms
3. First combat encounter
4. Find first treasure
5. Level up to level 2
6. Save progress

**Session 2 (10-15 minutes)**:
1. Continue from where left off
2. Fight multiple enemies
3. Face first real challenge
4. Strategic item use
5. Reach boss room

**Session 3 (5-10 minutes)**:
1. Boss fight
2. Victory or death
3. Review stats
4. Share story

**Total Playtime**: 20-35 minutes for full playthrough

---

## Conclusion

This game design balances:
- ✅ Classic roguelike mechanics
- ✅ Modern AI-driven storytelling
- ✅ Accessible natural language interface
- ✅ Strategic depth with simple rules
- ✅ Replayability (procedural gen future)
- ✅ Social sharing features

The AI Game Master transforms traditional game mechanics into engaging narratives, creating a unique experience every time.
