# 🚀 Quick Start Guide

## Prerequisites

- Docker Desktop (for Docker Compose)
- OR .NET 9 SDK + PostgreSQL (for local development)
- Ollama (optional, for LLM server)

## 🏃‍♂️ Fastest Way to Run (3 Steps)

### 1. Start Services

```bash
docker-compose up -d
```

### 2. Apply Database Migrations

```bash
# Wait for postgres to be ready (check docker-compose logs)
docker-compose logs -f postgres

# Once ready, apply migrations
docker exec -it nethack-auth-service dotnet ef database update --project /app
```

### 3. Open the Game

The web client is automatically served by nginx at:

```
http://localhost:8080
```

Just open your browser and navigate to that URL!

## 🎮 How to Play

1. **Register** a new account (username + password)
2. **Login** with your credentials
3. **Enter character name** (e.g., "Gandalf")
4. **Start playing!** Type commands like:
   - "Look around" - See your current room
   - "Check my stats" - View health, inventory
   - "Move north" - Explore the dungeon
   - "Attack goblin" - Engage in combat
   - "Pick up sword" - Loot items

## 🔧 Troubleshooting

### Services Not Starting?

Check service health:
```bash
curl http://localhost:5003/health  # Auth Service
curl http://localhost:5002/health  # MCP Server  
curl http://localhost:5001/health  # LLM Proxy
curl http://localhost:5000/health  # SignalR API
```

### Database Connection Failed?

```bash
# Check if PostgreSQL is running
docker-compose ps postgres

# View logs
docker-compose logs postgres

# Restart if needed
docker-compose restart postgres
```

### LLM Not Responding?

The system needs an OpenAI-compatible LLM server. Options:

**Option 1: Ollama (Local, Free)**
```bash
# Install from https://ollama.ai
ollama serve
ollama pull llama3.2
```

**Option 2: OpenAI (Cloud, Paid)**

Edit `src/NetHackChatGame.LlmProxy/appsettings.json`:
```json
{
  "LlmApi": {
    "Endpoint": "https://api.openai.com/v1/chat/completions",
    "Model": "gpt-4",
    "ApiKey": "your-openai-key"
  }
}
```

**Option 3: Mock Mode (No LLM)**

For testing without LLM, you can call the REST API directly:

```bash
# Get current room
curl http://localhost:5002/api/game/current-room?conversationId=YOUR_CONVERSATION_ID

# Move north
curl -X POST http://localhost:5002/api/game/move \
  -H "Content-Type: application/json" \
  -d '{"conversationId":"YOUR_ID","direction":"North"}'
```

### CORS Errors?

Make sure you're accessing the client via `http://localhost` (not `file://`):

```bash
# Serve the client folder
cd client
python -m http.server 8080
```

## 📝 Configuration

All services use `appsettings.json` for configuration:

- **Auth Service** (`src/NetHackChatGame.AuthService/appsettings.json`)
  - Database connection string
  
- **MCP Server** (`src/NetHackChatGame.McpServer/appsettings.json`)
  - Database connection string
  
- **LLM Proxy** (`src/NetHackChatGame.LlmProxy/appsettings.json`)
  - LLM API endpoint and model
  - MCP Server URL
  - Database connection string
  
- **SignalR API** (`src/NetHackChatGame.SignalRApi/appsettings.json`)
  - LLM Proxy URL

For Docker Compose, these are overridden by environment variables in `docker-compose.yml`.

## 🐛 Common Issues

**Issue**: "Conversation not found"
- **Solution**: Make sure you created a conversation via Auth Service first

**Issue**: "Tool execution failed"
- **Solution**: Check MCP Server is running and healthy

**Issue**: SignalR connection keeps dropping
- **Solution**: Check CORS settings, ensure SignalR API is running

**Issue**: Password hash too long
- **Solution**: Migration already fixes this (PasswordHash column is 500 chars)

## 📚 More Information

- Full documentation: See `docs/` folder
- Architecture: `docs/ARCHITECTURE.md`
- API Reference: `docs/API.md`
- Game Design: `docs/GAME_DESIGN.md`
- Deployment: `docs/DEPLOYMENT.md`

---

**Need Help?** Check `IMPLEMENTATION_SUMMARY.md` for complete system overview.
