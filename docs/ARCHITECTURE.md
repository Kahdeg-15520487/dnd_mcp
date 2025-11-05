# Architecture Documentation

## System Overview

The NetHack Chat Game is built as a distributed microservices architecture using the Model Context Protocol (MCP) to enable an LLM to interact with game logic through well-defined tools.

## Architectural Principles

1. **Separation of Concerns**: Each service has a single, well-defined responsibility
2. **Loose Coupling**: Services communicate through well-defined interfaces (HTTP, MCP)
3. **Stateless Services**: Services don't maintain session state (except in-memory caching)
4. **Database as Source of Truth**: All persistent state stored in PostgreSQL
5. **Container-First**: All services designed to run in Docker containers

## Service Architecture

### 1. SignalR API Service

**Responsibility**: Real-time WebSocket communication with clients

**Technology**: ASP.NET Core 8, SignalR

**Ports**: 5000 (HTTP/WebSocket)

**Key Components**:
```
NetHackChatGame.SignalRApi/
├── Hubs/
│   └── ChatHub.cs              # SignalR hub for client connections
├── Services/
│   ├── ILlmProxyClient.cs      # Interface for LLM Proxy communication
│   └── LlmProxyClient.cs       # HTTP client for LLM Proxy
├── Models/
│   ├── ChatMessage.cs          # Client message model
│   └── ConnectionInfo.cs       # Connection metadata
└── Program.cs                   # Service configuration
```

**Responsibilities**:
- Accept WebSocket connections from web clients
- Maintain active connection state
- Forward user messages to LLM Proxy Service
- Stream AI responses back to clients
- Handle connection lifecycle (connect, disconnect, reconnect)

**Communication**:
- **Incoming**: SignalR WebSocket from clients
- **Outgoing**: HTTP REST to LLM Proxy Service

**Scaling Considerations**:
- Stateless design allows horizontal scaling
- Use Redis backplane for multi-instance SignalR if needed
- Connection state stored in-memory (acceptable for single-user)

---

### 2. LLM Proxy Service

**Responsibility**: Orchestrate LLM interactions and tool calling

**Technology**: ASP.NET Core 8, OpenAI SDK, MCP Client

**Ports**: 5001 (HTTP)

**Key Components**:
```
NetHackChatGame.LlmProxy/
├── Controllers/
│   └── ChatController.cs          # REST API endpoints
├── Services/
│   ├── OpenAiService.cs           # OpenAI API client wrapper
│   ├── McpClientService.cs        # MCP client for tool calling
│   └── ConversationService.cs     # Conversation management
├── Models/
│   ├── ChatRequest.cs
│   ├── ChatResponse.cs
│   └── ToolCallResult.cs
└── Program.cs
```

**Responsibilities**:
- Receive chat requests from SignalR API
- Maintain conversation context (message history)
- Call OpenAI-compatible API with conversation history
- Handle tool call requests from LLM
- Execute tools via MCP Client
- Submit tool results back to LLM
- Persist conversations and messages to database
- Create game state snapshots after each turn

**Communication**:
- **Incoming**: HTTP REST from SignalR API
- **Outgoing**: 
  - HTTP to OpenAI API (or compatible endpoint)
  - MCP protocol (Server-Sent Events) to MCP Server
  - PostgreSQL via EF Core

**Message Flow**:
```
1. Receive POST /api/chat/message { conversationId, message }
2. Load conversation history from DB
3. Call OpenAI API with history + system prompt
4. If LLM requests tool call:
   a. Call MCP Server via MCP Client
   b. Get tool result
   c. Submit result back to OpenAI API
   d. Repeat if more tool calls needed
5. Save message + tool calls to DB
6. Create game state snapshot
7. Return final response
```

**Configuration**:
```json
{
  "OpenAI": {
    "Endpoint": "https://api.openai.com/v1",
    "ApiKey": "sk-...",
    "Model": "gpt-4",
    "MaxTokens": 2000,
    "Temperature": 0.7
  },
  "McpServer": {
    "Url": "http://mcp-server:5002",
    "Transport": "sse"
  }
}
```

---

### 3. MCP Server

**Responsibility**: Game logic, state management, and MCP tool implementation

**Technology**: ASP.NET Core 8, MCP C# SDK, EF Core

**Ports**: 5002 (HTTP/SSE for MCP)

**Key Components**:
```
NetHackChatGame.McpServer/
├── Tools/
│   ├── GetCurrentRoomTool.cs      # View current location
│   ├── GetPlayerStatsTool.cs      # View player stats
│   ├── MoveToRoomTool.cs          # Navigate dungeon
│   ├── CombatActionTool.cs        # Fight monsters
│   └── LootTreasureTool.cs        # Collect items
├── Services/
│   ├── GameEngine.cs              # Core game logic
│   ├── IDungeonGenerator.cs       # Dungeon generation interface
│   ├── StaticDungeonGenerator.cs  # Static dungeon (v1)
│   ├── CombatSystem.cs            # Combat mechanics
│   └── GameStateManager.cs        # State persistence
├── Models/
│   ├── GameState.cs               # Complete game state
│   ├── Dungeon.cs                 # Dungeon layout
│   └── CombatState.cs             # Combat encounter state
└── Program.cs                      # MCP server setup
```

**Responsibilities**:
- Expose MCP tools for game actions
- Implement game mechanics (combat, movement, looting)
- Manage game state per conversation
- Generate/load dungeon layouts
- Calculate combat outcomes
- Handle item/inventory management
- Persist game state to database

**MCP Tools Exposed**:

See [MCP_TOOLS.md](MCP_TOOLS.md) for detailed tool specifications.

**Game State Management**:
```csharp
public class GameState
{
    public Guid ConversationId { get; set; }
    public Player Player { get; set; }
    public Dungeon Dungeon { get; set; }
    public Guid CurrentRoomId { get; set; }
    public CombatState? ActiveCombat { get; set; }
    public DateTime LastUpdated { get; set; }
}
```

**State Persistence Strategy**:
- Game states cached in-memory (ConcurrentDictionary)
- Saved to database after each tool call
- Stored as JSON in `game_snapshots` table
- Linked to conversation and message for replay

**Communication**:
- **Incoming**: MCP protocol requests (via SSE)
- **Outgoing**: PostgreSQL via EF Core

---

### 4. PostgreSQL Database

**Responsibility**: Persistent storage for conversations and game state

**Technology**: PostgreSQL 16

**Ports**: 5432

**Schema**: See [DATABASE.md](DATABASE.md)

**Key Tables**:
- `conversations` - Chat sessions
- `messages` - Individual messages with role and content
- `game_snapshots` - Game state at each turn

---

## Communication Protocols

### SignalR WebSocket Protocol

**Client → Server**:
```javascript
connection.invoke("SendMessage", {
    conversationId: "uuid",
    message: "I attack the goblin"
});
```

**Server → Client**:
```javascript
connection.on("ReceiveMessage", (response) => {
    // response: { role: "assistant", content: "..." }
});
```

### HTTP REST (SignalR API ↔ LLM Proxy)

**POST /api/chat/message**:
```json
{
  "conversationId": "uuid-or-null",
  "message": "Look around the room",
  "userId": "player-name"
}
```

**Response**:
```json
{
  "conversationId": "uuid",
  "message": "You are in a dimly lit stone chamber...",
  "toolCalls": [
    {
      "toolName": "get_current_room",
      "arguments": { "conversationId": "uuid" },
      "result": { ... }
    }
  ]
}
```

### MCP Protocol (LLM Proxy ↔ MCP Server)

Uses official MCP C# SDK with Server-Sent Events transport.

**Tool Call Request**:
```json
{
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "get_current_room",
    "arguments": {
      "conversationId": "uuid"
    }
  },
  "id": 1
}
```

**Tool Call Response**:
```json
{
  "jsonrpc": "2.0",
  "result": {
    "content": [
      {
        "type": "text",
        "text": "{\"roomDescription\": \"...\", \"monsters\": [...], \"exits\": [...]}"
      }
    ]
  },
  "id": 1
}
```

---

## Data Flow

### Complete Message Flow Example

**User Action**: "I want to explore north"

```
┌────────┐  1. SendMessage    ┌──────────────┐
│ Client │───────────────────►│ SignalR API  │
└────────┘                     └──────┬───────┘
                                      │
                                      │ 2. POST /api/chat/message
                                      ▼
                               ┌──────────────┐
                               │  LLM Proxy   │
                               └──────┬───────┘
                                      │
                          ┌───────────┼───────────┐
                          │           │           │
                 3. Load History   4. Call OpenAI   
                          │           │           │
                          ▼           ▼           │
                    ┌──────────┐  ┌─────────┐    │
                    │ Database │  │ OpenAI  │    │
                    └──────────┘  └────┬────┘    │
                                       │         │
                              5. Tool Call Request│
                                       ▼         │
                                  ┌──────────────┐
                                  │  MCP Server  │
                                  └──────┬───────┘
                                         │
                            ┌────────────┼────────────┐
                            │            │            │
                     6. Execute Tool  7. Save State   │
                            │            │            │
                            │            ▼            │
                            │      ┌──────────┐       │
                            │      │ Database │       │
                            │      └──────────┘       │
                            │                         │
                            └─────────────────────────┘
                                         │
                              8. Tool Result
                                         │
                                         ▼
                               ┌──────────────┐
                               │  LLM Proxy   │
                               └──────┬───────┘
                                      │
                           9. Submit to OpenAI
                                      │
                                      ▼
                                ┌─────────┐
                                │ OpenAI  │
                                └────┬────┘
                                     │
                          10. Final Response
                                     │
                                     ▼
                               ┌──────────────┐
                               │  LLM Proxy   │
                               └──────┬───────┘
                                      │
                         11. Save Message & Snapshot
                                      │
                                      ▼
                                ┌──────────┐
                                │ Database │
                                └──────────┘
                                      │
                         12. Return Response
                                      │
                                      ▼
                               ┌──────────────┐
                               │ SignalR API  │
                               └──────┬───────┘
                                      │
                         13. ReceiveMessage
                                      │
                                      ▼
                                  ┌────────┐
                                  │ Client │
                                  └────────┘
```

---

## System Prompt

The LLM Proxy sends this system prompt to guide the AI game master:

```
You are the Game Master for a text-based dungeon crawler adventure game similar to NetHack.

You have access to the following tools to interact with the game:
- get_current_room: View the current room's description, monsters, items, and exits
- get_player_stats: Check the player's HP, inventory, level, and experience
- move_to_room: Move the player in a direction (north, south, east, west)
- combat_action: Perform combat actions (attack, defend, flee, use_item)
- loot_treasure: Pick up items from treasure rooms

Your role:
1. Narrate the adventure in an engaging, descriptive style
2. Call tools to get game state before describing situations
3. Execute player actions using the appropriate tools
4. Describe combat outcomes dramatically
5. Guide players but let them make decisions
6. Maintain consistency with the game state

Always check the current room and player stats before responding. Make the adventure exciting!
```

---

## Error Handling

### Service-Level Error Handling

Each service implements:
1. **Global exception handler** - Catches unhandled exceptions
2. **Logging** - Structured logging with Serilog
3. **Health checks** - `/health` endpoint for monitoring
4. **Graceful degradation** - Return sensible defaults on non-critical failures

### Database Error Handling

- **Connection retries** - Automatic retry with exponential backoff
- **Transaction management** - Rollback on errors
- **Migration failures** - Fail fast on startup if migrations fail

### MCP Communication Errors

- **Timeout handling** - 30-second timeout for tool calls
- **Retry logic** - Retry failed tool calls once
- **Fallback responses** - Return error message to LLM if tool fails

---

## Security Considerations

### Current Security (Personal Project)

- No authentication (single user)
- No authorization checks
- API keys in environment variables
- All services on internal Docker network

### Production Recommendations

- Add JWT authentication for SignalR
- Implement user accounts and sessions
- Use Azure Key Vault or similar for secrets
- Rate limiting on API endpoints
- Input validation and sanitization
- SQL injection protection (EF Core parameterized queries)

---

## Performance Considerations

### Current Performance Profile

- **Latency**: 2-5 seconds per message (depends on OpenAI API)
- **Throughput**: Single user only
- **Database**: Small dataset (<10k messages expected)

### Optimization Opportunities

1. **Caching**:
   - Cache dungeon layouts in-memory
   - Cache conversation history (Redis)
   - Cache OpenAI responses for common queries

2. **Streaming**:
   - Stream OpenAI responses token-by-token
   - Stream through SignalR to client

3. **Connection Pooling**:
   - Database connection pooling (EF Core default)
   - HTTP client reuse (HttpClientFactory)

---

## Monitoring and Observability

### Logging

**Structured Logging** with Serilog:
- JSON format for Docker logs
- Log levels: Debug, Info, Warning, Error
- Correlation IDs for request tracing

### Metrics (Future)

- Message processing time
- Tool call latency
- OpenAI API latency
- Database query performance

### Health Checks

Each service exposes `/health`:
- Database connectivity
- OpenAI API availability
- MCP Server connectivity

---

## Deployment Architecture

### Docker Compose (Development)

All services run on single host:
- Easy local development
- Simple debugging
- Fast iteration

### Production Deployment Options

**Option 1: Docker Swarm**
- Scale SignalR API horizontally
- Single database instance
- Shared storage for game state

**Option 2: Kubernetes**
- Full orchestration
- Auto-scaling
- Service mesh for observability

**Option 3: Azure Container Apps**
- Managed Kubernetes
- Built-in scaling
- Integrated monitoring

---

## Technology Decisions

### Why SignalR?

- ✅ Native .NET support
- ✅ Automatic reconnection
- ✅ Fallback transports (WebSocket → SSE → Long Polling)
- ✅ Built-in backplane support for scaling
- ❌ Couples us to .NET ecosystem

### Why MCP?

- ✅ Standard protocol for LLM tool calling
- ✅ Separates game logic from LLM orchestration
- ✅ Reusable tools across different LLM providers
- ✅ Official C# SDK available
- ❌ Early-stage technology, may need workarounds

### Why PostgreSQL?

- ✅ JSON/JSONB support for game state snapshots
- ✅ Excellent .NET support via EF Core
- ✅ Free and open source
- ✅ Better than SQL Server for Docker deployments
- ❌ Slightly less familiar in .NET ecosystem

### Why Separate Services?

- ✅ Clear separation of concerns
- ✅ Independent scaling
- ✅ Easier to test individual components
- ✅ Can replace services independently
- ❌ More complex deployment
- ❌ Network overhead between services

---

## Future Architecture Improvements

### Phase 2: Streaming

Add streaming support:
- OpenAI streaming API
- Stream tokens through LLM Proxy
- Stream via SignalR to client
- Show "typing" indicator

### Phase 3: Multi-User

Support multiple concurrent users:
- User authentication
- Separate game instances per user
- Shared database with user partitioning
- Scale SignalR API horizontally with Redis backplane

### Phase 4: Advanced Features

- Procedural dungeon generation
- Conversation sharing (read-only replay)
- Spectator mode
- Leaderboards
- Achievement system

---

## Conclusion

This architecture balances:
- **Simplicity**: Easy to understand and develop
- **Modularity**: Clear service boundaries
- **Scalability**: Can grow with demand
- **Flexibility**: Easy to swap components

The MCP-based approach allows the LLM to interact with game mechanics in a structured, reliable way while maintaining a clean separation between AI orchestration and game logic.
