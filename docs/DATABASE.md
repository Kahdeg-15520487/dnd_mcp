# Database Schema Documentation

## Overview

The NetHack Chat Game uses PostgreSQL 16 with Entity Framework Core 8 for data persistence.

**Database Name**: `nethack_chat`

**Purpose**: Store conversation history, messages, and game state snapshots for replay and sharing.

---

## Entity Relationship Diagram

```
┌─────────────────┐
│  conversations  │
│─────────────────│
│ id (PK)         │◄────┐
│ player_name     │     │
│ started_at      │     │
│ last_message_at │     │
│ is_active       │     │
└─────────────────┘     │
                        │
                        │ 1:N
                        │
                 ┌──────┴──────────┐
                 │                 │
         ┌───────┴───────┐  ┌──────┴──────────┐
         │   messages    │  │  game_snapshots │
         │───────────────│  │─────────────────│
         │ id (PK)       │  │ id (PK)         │
         │ conversation  │  │ conversation    │
         │   _id (FK)    │◄─┤   _id (FK)      │
         │ role          │  │ message_id (FK) │──┐
         │ content       │◄─┘ game_state_json│  │
         │ tool_calls    │  │ created_at      │  │
         │ tool_results  │  └─────────────────┘  │
         │ created_at    │                        │
         │ sequence_num  │◄───────────────────────┘
         └───────────────┘
```

---

## Tables

### 1. conversations

Represents a complete chat session / game playthrough.

#### Schema

```sql
CREATE TABLE conversations (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_name         VARCHAR(100) NOT NULL,
    started_at          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    last_message_at     TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    is_active           BOOLEAN NOT NULL DEFAULT TRUE,
    
    CONSTRAINT chk_player_name_not_empty CHECK (LENGTH(player_name) > 0)
);

CREATE INDEX idx_conversations_started_at ON conversations(started_at DESC);
CREATE INDEX idx_conversations_is_active ON conversations(is_active) WHERE is_active = TRUE;
```

#### Columns

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| `id` | UUID | No | Primary key, auto-generated |
| `player_name` | VARCHAR(100) | No | Player's display name |
| `started_at` | TIMESTAMPTZ | No | When conversation began |
| `last_message_at` | TIMESTAMPTZ | No | Last activity timestamp |
| `is_active` | BOOLEAN | No | Whether game is in progress |

#### Indexes

- `PRIMARY KEY (id)` - Fast lookups by conversation ID
- `idx_conversations_started_at` - List recent conversations
- `idx_conversations_is_active` - Find active games

#### Entity Framework Model

```csharp
public class ConversationEntity
{
    public Guid Id { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset LastMessageAt { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
    public ICollection<GameSnapshotEntity> GameSnapshots { get; set; } = new List<GameSnapshotEntity>();
}
```

---

### 2. messages

Individual messages in the conversation (user, assistant, tool calls).

#### Schema

```sql
CREATE TABLE messages (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    conversation_id     UUID NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    role                VARCHAR(20) NOT NULL,
    content             TEXT NOT NULL,
    tool_calls          JSONB NULL,
    tool_results        JSONB NULL,
    created_at          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    sequence_number     INTEGER NOT NULL,
    
    CONSTRAINT chk_role_valid CHECK (role IN ('user', 'assistant', 'system', 'tool')),
    CONSTRAINT chk_content_not_empty CHECK (LENGTH(content) > 0),
    CONSTRAINT unique_conversation_sequence UNIQUE (conversation_id, sequence_number)
);

CREATE INDEX idx_messages_conversation ON messages(conversation_id, sequence_number);
CREATE INDEX idx_messages_created_at ON messages(created_at DESC);
CREATE INDEX idx_messages_role ON messages(role);
```

#### Columns

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| `id` | UUID | No | Primary key |
| `conversation_id` | UUID | No | Foreign key to conversations |
| `role` | VARCHAR(20) | No | Message role: user, assistant, system, tool |
| `content` | TEXT | No | Message content |
| `tool_calls` | JSONB | Yes | OpenAI tool calls (if role=assistant) |
| `tool_results` | JSONB | Yes | Tool execution results (if role=tool) |
| `created_at` | TIMESTAMPTZ | No | When message was created |
| `sequence_number` | INTEGER | No | Order within conversation (1, 2, 3...) |

#### Role Values

- **`user`**: Message from the player
- **`assistant`**: Response from the LLM
- **`system`**: System prompt (usually first message)
- **`tool`**: Tool execution result

#### tool_calls JSON Structure

```json
{
  "calls": [
    {
      "id": "call_abc123",
      "type": "function",
      "function": {
        "name": "get_current_room",
        "arguments": "{\"conversationId\":\"uuid\"}"
      }
    }
  ]
}
```

#### tool_results JSON Structure

```json
{
  "results": [
    {
      "tool_call_id": "call_abc123",
      "output": "{\"roomId\":\"...\", \"description\":\"...\"}"
    }
  ]
}
```

#### Entity Framework Model

```csharp
public class MessageEntity
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ToolCalls { get; set; } // JSON
    public string? ToolResults { get; set; } // JSON
    public DateTimeOffset CreatedAt { get; set; }
    public int SequenceNumber { get; set; }
    
    // Navigation properties
    public ConversationEntity Conversation { get; set; } = null!;
}
```

---

### 3. game_snapshots

Complete game state at each turn for replay functionality.

#### Schema

```sql
CREATE TABLE game_snapshots (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    conversation_id     UUID NOT NULL REFERENCES conversations(id) ON DELETE CASCADE,
    message_id          UUID NOT NULL REFERENCES messages(id) ON DELETE CASCADE,
    game_state_json     JSONB NOT NULL,
    created_at          TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    
    CONSTRAINT unique_message_snapshot UNIQUE (message_id)
);

CREATE INDEX idx_snapshots_conversation ON game_snapshots(conversation_id, created_at);
CREATE INDEX idx_snapshots_message ON game_snapshots(message_id);
CREATE INDEX idx_game_state_json_player_hp ON game_snapshots USING GIN ((game_state_json->'Player'->>'hp'));
```

#### Columns

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| `id` | UUID | No | Primary key |
| `conversation_id` | UUID | No | Foreign key to conversations |
| `message_id` | UUID | No | Foreign key to messages (which turn) |
| `game_state_json` | JSONB | No | Full game state as JSON |
| `created_at` | TIMESTAMPTZ | No | Snapshot timestamp |

#### game_state_json Structure

```json
{
  "conversationId": "uuid",
  "player": {
    "name": "Adventurer",
    "hp": 28,
    "maxHp": 30,
    "level": 2,
    "experience": 150,
    "gold": 45,
    "inventory": [
      {
        "id": "uuid",
        "name": "Health Potion",
        "type": "Potion",
        "description": "Restores 15 HP",
        "equipped": false,
        "quantity": 2
      }
    ],
    "equippedWeaponId": "uuid",
    "equippedArmorId": "uuid"
  },
  "dungeon": {
    "id": "uuid",
    "seed": 12345,
    "rooms": [
      {
        "id": "uuid",
        "type": "Combat",
        "description": "A dark chamber",
        "visited": true,
        "monsters": [],
        "items": [],
        "exits": [
          {
            "direction": "North",
            "targetRoomId": "uuid",
            "isLocked": false
          }
        ]
      }
    ]
  },
  "currentRoomId": "uuid",
  "activeCombat": {
    "inCombat": true,
    "monsters": [
      {
        "id": "uuid",
        "name": "Goblin Scout",
        "hp": 8,
        "maxHp": 15,
        "attack": 3,
        "defense": 1,
        "isAlive": true
      }
    ],
    "turnNumber": 3
  },
  "lastUpdated": "2025-11-04T10:30:00Z"
}
```

#### JSONB Queries

```sql
-- Find snapshots where player HP is low
SELECT * FROM game_snapshots 
WHERE (game_state_json->'Player'->>'hp')::int < 10;

-- Find snapshots with active combat
SELECT * FROM game_snapshots 
WHERE game_state_json->'activeCombat'->>'inCombat' = 'true';

-- Get player level progression
SELECT 
    conversation_id,
    created_at,
    game_state_json->'Player'->>'level' as level
FROM game_snapshots
ORDER BY created_at;
```

#### Entity Framework Model

```csharp
public class GameSnapshotEntity
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid MessageId { get; set; }
    public string GameStateJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
    
    // Navigation properties
    public ConversationEntity Conversation { get; set; } = null!;
    public MessageEntity Message { get; set; } = null!;
}
```

---

## Database Constraints

### Foreign Key Cascades

- `messages.conversation_id` → `conversations.id` **CASCADE DELETE**
  - Deleting a conversation deletes all its messages
  
- `game_snapshots.conversation_id` → `conversations.id` **CASCADE DELETE**
  - Deleting a conversation deletes all its snapshots
  
- `game_snapshots.message_id` → `messages.id` **CASCADE DELETE**
  - Deleting a message deletes its snapshot

### Unique Constraints

- `conversations.id` - Primary key uniqueness
- `messages(conversation_id, sequence_number)` - No duplicate sequence numbers
- `game_snapshots.message_id` - One snapshot per message

### Check Constraints

- `conversations.player_name` - Must not be empty
- `messages.role` - Must be: user, assistant, system, or tool
- `messages.content` - Must not be empty

---

## Indexes Strategy

### Primary Indexes (Automatic)

- `conversations.id` (PK)
- `messages.id` (PK)
- `game_snapshots.id` (PK)

### Foreign Key Indexes

- `messages(conversation_id, sequence_number)` - Fast message retrieval for conversation
- `game_snapshots(conversation_id, created_at)` - Timeline of snapshots
- `game_snapshots(message_id)` - Snapshot lookup by message

### Query Optimization Indexes

- `conversations(started_at DESC)` - Recent conversations
- `conversations(is_active)` - Active games (partial index)
- `messages(created_at DESC)` - Recent messages
- `messages(role)` - Filter by message type

### JSONB GIN Indexes

```sql
-- Enable searching within JSON fields
CREATE INDEX idx_game_state_json_player_hp 
ON game_snapshots USING GIN ((game_state_json->'Player'->>'hp'));

-- Future: Add more GIN indexes as needed for JSON queries
```

---

## Entity Framework Core Configuration

### DbContext

```csharp
public class NetHackDbContext : DbContext
{
    public DbSet<ConversationEntity> Conversations { get; set; }
    public DbSet<MessageEntity> Messages { get; set; }
    public DbSet<GameSnapshotEntity> GameSnapshots { get; set; }

    public NetHackDbContext(DbContextOptions<NetHackDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Conversation configuration
        modelBuilder.Entity<ConversationEntity>(entity =>
        {
            entity.ToTable("conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PlayerName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.StartedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.LastMessageAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            
            entity.HasMany(e => e.Messages)
                .WithOne(e => e.Conversation)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasMany(e => e.GameSnapshots)
                .WithOne(e => e.Conversation)
                .HasForeignKey(e => e.ConversationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Message configuration
        modelBuilder.Entity<MessageEntity>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Role).HasMaxLength(20).IsRequired();
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.ToolCalls).HasColumnType("jsonb");
            entity.Property(e => e.ToolResults).HasColumnType("jsonb");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasIndex(e => new { e.ConversationId, e.SequenceNumber })
                .IsUnique();
        });

        // GameSnapshot configuration
        modelBuilder.Entity<GameSnapshotEntity>(entity =>
        {
            entity.ToTable("game_snapshots");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GameStateJson).HasColumnType("jsonb").IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            
            entity.HasIndex(e => e.MessageId).IsUnique();
        });
    }
}
```

### Connection String

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Port=5432;Database=nethack_chat;Username=nethack_user;Password=your_password"
  }
}
```

### Migrations

```bash
# Create initial migration
dotnet ef migrations add InitialCreate --project src/NetHackChatGame.Data

# Apply migrations
dotnet ef database update --project src/NetHackChatGame.Data

# Migrations run automatically on service startup in Docker
```

---

## Common Queries

### Get Full Conversation with Messages

```csharp
var conversation = await context.Conversations
    .Include(c => c.Messages.OrderBy(m => m.SequenceNumber))
    .FirstOrDefaultAsync(c => c.Id == conversationId);
```

### Get Latest Game State

```csharp
var latestSnapshot = await context.GameSnapshots
    .Where(s => s.ConversationId == conversationId)
    .OrderByDescending(s => s.CreatedAt)
    .FirstOrDefaultAsync();
```

### Get Conversation History for LLM

```csharp
var messages = await context.Messages
    .Where(m => m.ConversationId == conversationId)
    .OrderBy(m => m.SequenceNumber)
    .Select(m => new { m.Role, m.Content })
    .ToListAsync();
```

### Create New Message

```csharp
var message = new MessageEntity
{
    ConversationId = conversationId,
    Role = "user",
    Content = "I attack the goblin",
    SequenceNumber = nextSequenceNumber,
    CreatedAt = DateTimeOffset.UtcNow
};

context.Messages.Add(message);
await context.SaveChangesAsync();
```

### Create Game Snapshot

```csharp
var snapshot = new GameSnapshotEntity
{
    ConversationId = conversationId,
    MessageId = messageId,
    GameStateJson = JsonSerializer.Serialize(gameState),
    CreatedAt = DateTimeOffset.UtcNow
};

context.GameSnapshots.Add(snapshot);
await context.SaveChangesAsync();
```

---

## Data Retention and Cleanup

### Automatic Cleanup (Future)

```sql
-- Delete inactive conversations older than 30 days
DELETE FROM conversations 
WHERE is_active = FALSE 
  AND last_message_at < NOW() - INTERVAL '30 days';

-- Archive old snapshots (keep only latest per conversation)
-- Implementation TBD
```

### Manual Cleanup

```sql
-- Delete a specific conversation and all related data
DELETE FROM conversations WHERE id = 'uuid';
-- Cascade deletes messages and snapshots automatically
```

---

## Backup Strategy

### Docker Volume Backup

```bash
# Backup PostgreSQL data volume
docker run --rm \
  -v mcp_test_postgres_data:/data \
  -v $(pwd)/backups:/backup \
  alpine tar czf /backup/postgres-backup-$(date +%Y%m%d).tar.gz /data
```

### pg_dump Backup

```bash
# Dump database to SQL file
docker exec postgres pg_dump -U nethack_user nethack_chat > backup.sql

# Restore from backup
docker exec -i postgres psql -U nethack_user nethack_chat < backup.sql
```

---

## Performance Tuning

### PostgreSQL Configuration

```sql
-- Increase shared_buffers for better caching
ALTER SYSTEM SET shared_buffers = '256MB';

-- Optimize for JSONB queries
ALTER SYSTEM SET max_parallel_workers_per_gather = 2;

-- Apply changes
SELECT pg_reload_conf();
```

### Connection Pooling

EF Core default: 100 connections max

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;...;Pooling=true;MinPoolSize=5;MaxPoolSize=20"
  }
}
```

---

## Monitoring

### Useful Queries

**Database Size:**
```sql
SELECT pg_size_pretty(pg_database_size('nethack_chat'));
```

**Table Sizes:**
```sql
SELECT 
    schemaname,
    tablename,
    pg_size_pretty(pg_total_relation_size(schemaname||'.'||tablename)) AS size
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY pg_total_relation_size(schemaname||'.'||tablename) DESC;
```

**Active Conversations:**
```sql
SELECT COUNT(*) FROM conversations WHERE is_active = TRUE;
```

**Message Count by Role:**
```sql
SELECT role, COUNT(*) FROM messages GROUP BY role;
```

---

## Future Enhancements

### Potential Schema Changes

1. **User Management Table**
   - `users` table for authentication
   - Multiple conversations per user

2. **Sharing Features**
   - `shared_conversations` table
   - Public/private visibility flags

3. **Leaderboard**
   - `achievements` table
   - `player_scores` table

4. **Audit Trail**
   - `audit_log` table for all actions
   - Track who modified what

5. **Dungeon Templates**
   - `dungeon_templates` table
   - Reusable dungeon layouts

---

## Conclusion

This database schema provides:
- ✅ Complete conversation history
- ✅ Full game state snapshots for replay
- ✅ Efficient querying with proper indexes
- ✅ JSONB for flexible game state storage
- ✅ Cascade deletes for data integrity
- ✅ Ready for Docker deployment

The design balances simplicity (single-user focus) with extensibility (easy to add features later).
