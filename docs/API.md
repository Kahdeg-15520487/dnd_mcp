# API Documentation

This document describes all APIs exposed by the NetHack Chat Game services.

---

## Table of Contents

1. [SignalR WebSocket API](#signalr-websocket-api)
2. [LLM Proxy REST API](#llm-proxy-rest-api)
3. [MCP Server Protocol](#mcp-server-protocol)
4. [Authentication](#authentication)
5. [Error Handling](#error-handling)

---

## SignalR WebSocket API

**Service**: SignalR API  
**Base URL**: `http://localhost:5000`  
**WebSocket Endpoint**: `/chatHub`

### Connection

**Client Library**: `@microsoft/signalr`

```javascript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/chatHub")
    .withAutomaticReconnect()
    .build();

await connection.start();
```

### Hub Methods

#### 1. SendMessage

Send a user message to the game.

**Client → Server**

```javascript
await connection.invoke("SendMessage", {
    conversationId: "uuid-or-null",  // null to start new conversation
    message: "I want to explore the dungeon",
    playerName: "Adventurer"
});
```

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `conversationId` | string (UUID) | No | Existing conversation ID, or null for new |
| `message` | string | Yes | User's message |
| `playerName` | string | Yes | Player's display name |

**Response**: Returns `conversationId` (string)

```javascript
const conversationId = await connection.invoke("SendMessage", { ... });
console.log("Conversation ID:", conversationId);
```

#### 2. GetConversationHistory

Retrieve message history for a conversation.

**Client → Server**

```javascript
const history = await connection.invoke("GetConversationHistory", conversationId);
```

**Parameters:**

| Name | Type | Required | Description |
|------|------|----------|-------------|
| `conversationId` | string (UUID) | Yes | Conversation to retrieve |

**Response:**

```javascript
[
    {
        role: "user",
        content: "I want to explore",
        timestamp: "2025-11-04T10:00:00Z"
    },
    {
        role: "assistant",
        content: "You stand at the entrance...",
        timestamp: "2025-11-04T10:00:02Z"
    }
]
```

### Events (Server → Client)

#### ReceiveMessage

Receive AI responses and game updates.

**Event Handler:**

```javascript
connection.on("ReceiveMessage", (message) => {
    console.log(`${message.role}: ${message.content}`);
});
```

**Message Object:**

```javascript
{
    role: "assistant",           // "assistant" | "system" | "tool"
    content: "You enter a dark chamber...",
    timestamp: "2025-11-04T10:00:02Z",
    toolCalls: [                 // Optional: if LLM called tools
        {
            toolName: "get_current_room",
            arguments: { ... },
            result: { ... }
        }
    ]
}
```

#### ReceiveError

Receive error notifications.

```javascript
connection.on("ReceiveError", (error) => {
    console.error("Error:", error.message);
});
```

**Error Object:**

```javascript
{
    code: "INVALID_ACTION",
    message: "Cannot move while in combat",
    timestamp: "2025-11-04T10:00:02Z"
}
```

#### ConnectionStatus

Receive connection status updates.

```javascript
connection.on("ConnectionStatus", (status) => {
    console.log("Status:", status);
});
```

**Status Values**: `"connected"` | `"disconnected"` | `"reconnecting"`

### Connection Lifecycle

**Connection Events:**

```javascript
connection.onclose((error) => {
    console.log("Connection closed", error);
});

connection.onreconnecting((error) => {
    console.log("Reconnecting...", error);
});

connection.onreconnected((connectionId) => {
    console.log("Reconnected", connectionId);
});
```

### Health Check

**Endpoint**: `GET /health`

```bash
curl http://localhost:5000/health
```

**Response:**

```json
{
    "status": "healthy",
    "service": "signalr-api",
    "timestamp": "2025-11-04T10:00:00Z"
}
```

---

## LLM Proxy REST API

**Service**: LLM Proxy  
**Base URL**: `http://localhost:5001`  
**Content-Type**: `application/json`

### Endpoints

#### POST /api/chat/message

Send a message and get AI response.

**Request:**

```http
POST /api/chat/message
Content-Type: application/json

{
    "conversationId": "uuid-or-null",
    "message": "I attack the goblin",
    "playerName": "Adventurer"
}
```

**Request Body:**

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `conversationId` | string (UUID) | No | Existing conversation, null for new |
| `message` | string | Yes | User message |
| `playerName` | string | Yes | Player display name |

**Response:**

```json
{
    "conversationId": "123e4567-e89b-12d3-a456-426614174000",
    "messageId": "987fcdeb-51a2-43f8-9abc-def123456789",
    "role": "assistant",
    "content": "You swing your sword at the goblin, dealing 7 damage!",
    "toolCalls": [
        {
            "toolName": "combat_action",
            "arguments": {
                "conversationId": "123e4567-e89b-12d3-a456-426614174000",
                "action": "Attack",
                "targetMonsterId": "monster-001"
            },
            "result": {
                "success": true,
                "playerDamageDealt": 7,
                "monsterHpRemaining": 8
            }
        }
    ],
    "timestamp": "2025-11-04T10:00:02Z"
}
```

**Status Codes:**

| Code | Meaning |
|------|---------|
| 200 | Success |
| 400 | Invalid request (missing required fields) |
| 404 | Conversation not found |
| 500 | Internal server error |

---

#### GET /api/chat/conversations/{conversationId}

Retrieve conversation metadata and message history.

**Request:**

```http
GET /api/chat/conversations/123e4567-e89b-12d3-a456-426614174000
```

**Response:**

```json
{
    "conversationId": "123e4567-e89b-12d3-a456-426614174000",
    "playerName": "Adventurer",
    "startedAt": "2025-11-04T09:00:00Z",
    "lastMessageAt": "2025-11-04T10:00:02Z",
    "isActive": true,
    "messageCount": 15,
    "messages": [
        {
            "messageId": "msg-001",
            "role": "user",
            "content": "I want to explore",
            "timestamp": "2025-11-04T09:00:00Z",
            "sequenceNumber": 1
        },
        {
            "messageId": "msg-002",
            "role": "assistant",
            "content": "You stand at the entrance...",
            "timestamp": "2025-11-04T09:00:02Z",
            "sequenceNumber": 2
        }
    ]
}
```

**Status Codes:**

| Code | Meaning |
|------|---------|
| 200 | Success |
| 404 | Conversation not found |
| 500 | Internal server error |

---

#### GET /api/chat/replay/{conversationId}

Get full replay data including game state snapshots.

**Request:**

```http
GET /api/chat/replay/123e4567-e89b-12d3-a456-426614174000
```

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `includeGameState` | boolean | No | Include game snapshots (default: true) |

**Response:**

```json
{
    "conversationId": "123e4567-e89b-12d3-a456-426614174000",
    "playerName": "Adventurer",
    "startedAt": "2025-11-04T09:00:00Z",
    "endedAt": "2025-11-04T10:30:00Z",
    "isActive": false,
    "totalMessages": 45,
    "replay": [
        {
            "sequenceNumber": 1,
            "message": {
                "role": "user",
                "content": "I want to explore",
                "timestamp": "2025-11-04T09:00:00Z"
            },
            "gameState": {
                "player": {
                    "hp": 30,
                    "maxHp": 30,
                    "level": 1,
                    "gold": 0
                },
                "currentRoomId": "room-001"
            }
        }
    ]
}
```

**Status Codes:**

| Code | Meaning |
|------|---------|
| 200 | Success |
| 404 | Conversation not found |
| 500 | Internal server error |

---

#### GET /api/chat/conversations

List all conversations (with pagination).

**Request:**

```http
GET /api/chat/conversations?page=1&pageSize=10&activeOnly=true
```

**Query Parameters:**

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `page` | int | 1 | Page number (1-based) |
| `pageSize` | int | 10 | Items per page (max 100) |
| `activeOnly` | boolean | false | Only active conversations |

**Response:**

```json
{
    "page": 1,
    "pageSize": 10,
    "totalCount": 23,
    "totalPages": 3,
    "items": [
        {
            "conversationId": "uuid",
            "playerName": "Adventurer",
            "startedAt": "2025-11-04T09:00:00Z",
            "lastMessageAt": "2025-11-04T10:00:02Z",
            "isActive": true,
            "messageCount": 15
        }
    ]
}
```

---

#### DELETE /api/chat/conversations/{conversationId}

Delete a conversation and all associated data.

**Request:**

```http
DELETE /api/chat/conversations/123e4567-e89b-12d3-a456-426614174000
```

**Response:**

```json
{
    "success": true,
    "message": "Conversation deleted successfully"
}
```

**Status Codes:**

| Code | Meaning |
|------|---------|
| 200 | Success |
| 404 | Conversation not found |
| 500 | Internal server error |

---

#### GET /health

Health check endpoint.

**Request:**

```http
GET /health
```

**Response:**

```json
{
    "status": "healthy",
    "service": "llm-proxy",
    "dependencies": {
        "database": "healthy",
        "openai": "healthy",
        "mcpServer": "healthy"
    },
    "timestamp": "2025-11-04T10:00:00Z"
}
```

---

## MCP Server Protocol

**Service**: MCP Server  
**Base URL**: `http://localhost:5002`  
**Protocol**: JSON-RPC 2.0 over Server-Sent Events (SSE)

The MCP Server implements the [Model Context Protocol specification](https://modelcontextprotocol.io/).

### Transport: Server-Sent Events (SSE)

**Connection Endpoint**: `GET /mcp/sse`

The LLM Proxy connects to this endpoint to establish an SSE connection for bidirectional communication.

### MCP Methods

#### tools/list

List all available tools.

**Request:**

```json
{
    "jsonrpc": "2.0",
    "method": "tools/list",
    "id": 1
}
```

**Response:**

```json
{
    "jsonrpc": "2.0",
    "result": {
        "tools": [
            {
                "name": "get_current_room",
                "description": "Get information about the current room",
                "inputSchema": {
                    "type": "object",
                    "properties": {
                        "conversationId": {
                            "type": "string",
                            "description": "The conversation ID"
                        }
                    },
                    "required": ["conversationId"]
                }
            }
        ]
    },
    "id": 1
}
```

---

#### tools/call

Execute a tool.

**Request:**

```json
{
    "jsonrpc": "2.0",
    "method": "tools/call",
    "params": {
        "name": "get_current_room",
        "arguments": {
            "conversationId": "123e4567-e89b-12d3-a456-426614174000"
        }
    },
    "id": 2
}
```

**Response:**

```json
{
    "jsonrpc": "2.0",
    "result": {
        "content": [
            {
                "type": "text",
                "text": "{\"roomId\":\"room-001\",\"roomType\":\"Combat\",\"description\":\"A dark chamber\",\"monsters\":[...],\"items\":[],\"exits\":[...]}"
            }
        ]
    },
    "id": 2
}
```

**Error Response:**

```json
{
    "jsonrpc": "2.0",
    "error": {
        "code": -32602,
        "message": "Invalid params",
        "data": {
            "detail": "conversationId is required"
        }
    },
    "id": 2
}
```

### MCP Tools

See [MCP_TOOLS.md](MCP_TOOLS.md) for detailed documentation of all 5 game tools.

---

## Authentication

### Current Implementation (v1)

**No Authentication** - Single-user personal project

All endpoints are open. Do not deploy publicly without adding authentication.

### Future Authentication (v2)

Planned authentication methods:

1. **JWT Tokens**
   - `POST /api/auth/login` - Get JWT token
   - `Authorization: Bearer <token>` header required

2. **API Keys**
   - `X-API-Key: <key>` header

3. **OAuth 2.0**
   - Support for GitHub, Google, Microsoft accounts

---

## Error Handling

### Standard Error Response

All REST API errors follow this format:

```json
{
    "error": {
        "code": "ERROR_CODE",
        "message": "Human-readable error message",
        "details": {
            "field": "Additional context",
            "timestamp": "2025-11-04T10:00:00Z"
        }
    }
}
```

### HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| 200 | OK | Successful request |
| 201 | Created | New conversation created |
| 400 | Bad Request | Invalid input (missing fields, validation errors) |
| 401 | Unauthorized | Authentication required (future) |
| 403 | Forbidden | Insufficient permissions (future) |
| 404 | Not Found | Conversation/message not found |
| 409 | Conflict | Duplicate conversation ID |
| 429 | Too Many Requests | Rate limit exceeded (future) |
| 500 | Internal Server Error | Server-side error |
| 503 | Service Unavailable | Service temporarily down |

### Error Codes

| Code | Description |
|------|-------------|
| `CONVERSATION_NOT_FOUND` | Invalid conversation ID |
| `INVALID_INPUT` | Missing or invalid fields |
| `GAME_STATE_ERROR` | Cannot load/save game state |
| `OPENAI_API_ERROR` | OpenAI API call failed |
| `MCP_ERROR` | MCP tool call failed |
| `DATABASE_ERROR` | Database operation failed |
| `INTERNAL_ERROR` | Unexpected server error |

### SignalR Error Events

Errors are sent via `ReceiveError` event:

```javascript
connection.on("ReceiveError", (error) => {
    // error.code: string
    // error.message: string
    // error.timestamp: string
});
```

---

## Rate Limiting (Future)

**Not implemented in v1.**

Planned rate limits for v2:
- 10 messages per minute per connection
- 100 messages per hour per user
- 1000 messages per day per user

Response headers:
```
X-RateLimit-Limit: 10
X-RateLimit-Remaining: 7
X-RateLimit-Reset: 1699099200
```

---

## CORS Configuration

### Development

Allows all origins for local testing:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
```

### Production

Restrict to specific origins:

```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});
```

---

## API Versioning (Future)

**Not implemented in v1.**

Planned versioning strategy:
- URL path versioning: `/api/v1/chat/message`, `/api/v2/chat/message`
- Support multiple versions simultaneously
- Deprecation notices in response headers

---

## WebSocket Message Size Limits

**SignalR Configuration:**

```csharp
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 32 * 1024; // 32 KB
    options.StreamBufferCapacity = 10;
    options.EnableDetailedErrors = true; // Development only
});
```

---

## Request/Response Examples

### Complete Message Flow

**1. Client connects:**

```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/chatHub")
    .build();

await connection.start();
```

**2. Client sends message:**

```javascript
connection.on("ReceiveMessage", (msg) => {
    console.log(msg.content);
});

const conversationId = await connection.invoke("SendMessage", {
    conversationId: null,
    message: "I want to explore the dungeon",
    playerName: "Adventurer"
});
```

**3. Server processes:**

- SignalR API → LLM Proxy (HTTP POST)
- LLM Proxy → OpenAI API
- LLM Proxy → MCP Server (tool calls)
- LLM Proxy → Database (save message)

**4. Client receives response:**

```javascript
// Event fires automatically
ReceiveMessage({
    role: "assistant",
    content: "You stand at the entrance to a dark dungeon...",
    timestamp: "2025-11-04T10:00:02Z"
})
```

---

## Testing APIs

### Using curl

**REST API:**

```bash
# Send message
curl -X POST http://localhost:5001/api/chat/message \
  -H "Content-Type: application/json" \
  -d '{
    "conversationId": null,
    "message": "I want to explore",
    "playerName": "Test Player"
  }'

# Get conversation
curl http://localhost:5001/api/chat/conversations/{id}

# Health check
curl http://localhost:5001/health
```

### Using Postman

Import this collection: (TBD - create Postman collection)

### Using JavaScript

**SignalR Client:**

```javascript
const signalR = require("@microsoft/signalr");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/chatHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.on("ReceiveMessage", console.log);

await connection.start();
await connection.invoke("SendMessage", {
    conversationId: null,
    message: "Test message",
    playerName: "Tester"
});
```

---

## API Documentation Tools

### Swagger/OpenAPI (Future)

Add Swagger UI for interactive API documentation:

```csharp
builder.Services.AddSwaggerGen();
app.UseSwagger();
app.UseSwaggerUI();
```

Access at: `http://localhost:5001/swagger`

---

## Conclusion

This API design provides:
- ✅ Real-time bidirectional communication (SignalR)
- ✅ RESTful endpoints for conversation management
- ✅ MCP protocol for LLM tool calling
- ✅ Consistent error handling
- ✅ Health checks for monitoring
- ✅ Ready for future enhancements (auth, rate limiting, versioning)

All APIs are designed to be simple, predictable, and easy to use from any client.
