# NetHack Chat Game

A real-time chat-based dungeon crawler game powered by LLM and Model Context Protocol (MCP). Players interact with an AI game master through natural language to explore dungeons, fight monsters, and collect treasures.

## 🎮 Overview

This project combines:
- **SignalR WebSocket API** for real-time communication
- **LLM Proxy Service** for AI-powered game master
- **MCP Server** for game logic and state management
- **PostgreSQL** for conversation and game state persistence
- **OpenAI-compatible API** for natural language understanding

## 🏗️ Architecture

```
┌──────────────────────────────────────────────────────────┐
│                    Docker Network                        │
├──────────────────────────────────────────────────────────┤
│                                                          │
│  ┌────────────────┐         ┌─────────────────┐        │
│  │  SignalR API   │◄────────┤   Web Client    │        │
│  │   (Port 5000)  │         │   (Browser)     │        │
│  └────────┬───────┘         └─────────────────┘        │
│           │                                              │
│           │ HTTP REST                                    │
│           ▼                                              │
│  ┌────────────────┐                                     │
│  │  LLM Proxy     │──────────────► OpenAI API          │
│  │   (Port 5001)  │                (External)           │
│  └────────┬───────┘                                     │
│           │                                              │
│           │ MCP Protocol (SSE)                          │
│           ▼                                              │
│  ┌────────────────┐                                     │
│  │   MCP Server   │                                     │
│  │   (Port 5002)  │                                     │
│  └────────┬───────┘                                     │
│           │                                              │
│           │ EF Core                                      │
│           ▼                                              │
│  ┌────────────────┐                                     │
│  │  PostgreSQL    │                                     │
│  │   (Port 5432)  │                                     │
│  └────────────────┘                                     │
│                                                          │
└──────────────────────────────────────────────────────────┘
```

## 📦 Project Structure

```
NetHackChatGame/
│
├── docker-compose.yml              # Docker orchestration
├── .env.example                    # Environment variables template
├── NetHackChatGame.sln            # Solution file
│
├── docs/                           # Documentation
│   ├── ARCHITECTURE.md            # Detailed architecture
│   ├── API.md                     # API specifications
│   ├── MCP_TOOLS.md              # MCP tools documentation
│   ├── DATABASE.md               # Database schema
│   ├── DEPLOYMENT.md             # Deployment guide
│   └── GAME_DESIGN.md            # Game mechanics
│
├── src/
│   ├── NetHackChatGame.SignalRApi/    # WebSocket API server
│   ├── NetHackChatGame.LlmProxy/      # LLM orchestration
│   ├── NetHackChatGame.McpServer/     # MCP server & game logic
│   ├── NetHackChatGame.Core/          # Shared models
│   └── NetHackChatGame.Data/          # Data access layer
│
└── client/
    └── index.html                  # Simple test client
```

## 🚀 Quick Start

### Prerequisites

- Docker & Docker Compose
- .NET 8 SDK (for local development)
- OpenAI API key or compatible endpoint

### Running with Docker Compose

1. Clone the repository:
```bash
git clone <repository-url>
cd NetHackChatGame
```

2. Create `.env` file from template:
```bash
cp .env.example .env
```

3. Edit `.env` and add your OpenAI API key:
```env
OPENAI_API_ENDPOINT=https://api.openai.com/v1
OPENAI_API_KEY=sk-your-key-here
OPENAI_MODEL=gpt-4
```

4. Start all services:
```bash
docker-compose up -d
```

5. Open the client:
```
http://localhost:8080
```

### Local Development

See [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) for detailed local development setup.

## 🎯 Features

### Current Features
- ✅ Real-time chat interface via SignalR
- ✅ AI-powered game master using LLM
- ✅ MCP-based tool calling for game actions
- ✅ Turn-based combat system
- ✅ Treasure and loot mechanics
- ✅ Conversation persistence
- ✅ Game state snapshots for replay

### Planned Features
- 🔲 Procedural dungeon generation
- 🔲 Conversation sharing with shareable links
- 🔲 Multiple character classes
- 🔲 Advanced combat mechanics (spells, abilities)
- 🔲 Multiplayer support

## 🛠️ Technology Stack

| Component | Technology |
|-----------|-----------|
| **Backend Services** | .NET 8 C# |
| **WebSocket** | ASP.NET Core SignalR |
| **MCP Framework** | [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk) |
| **Database** | PostgreSQL 16 |
| **ORM** | Entity Framework Core 8 |
| **LLM Provider** | OpenAI-compatible API |
| **Containerization** | Docker & Docker Compose |
| **Frontend** | Vanilla JavaScript + SignalR Client |

## 📚 Documentation

- [Architecture Overview](docs/ARCHITECTURE.md) - Detailed system architecture
- [API Specification](docs/API.md) - REST and SignalR APIs
- [MCP Tools Reference](docs/MCP_TOOLS.md) - Available game actions
- [Database Schema](docs/DATABASE.md) - Database design and models
- [Deployment Guide](docs/DEPLOYMENT.md) - How to deploy and run
- [Game Design](docs/GAME_DESIGN.md) - Game mechanics and rules

## 🎮 How to Play

1. Connect to the chat interface
2. Type natural language commands like:
   - "I want to explore the dungeon"
   - "Look around the room"
   - "Attack the goblin"
   - "Check my inventory"
   - "Go north"
   - "Take the treasure"

3. The AI game master will:
   - Describe your surroundings
   - Narrate combat outcomes
   - Guide you through the dungeon
   - Track your progress

## 🔧 Configuration

### Environment Variables

See [.env.example](.env.example) for all configuration options.

Key variables:
- `OPENAI_API_ENDPOINT` - LLM API endpoint
- `OPENAI_API_KEY` - API authentication key
- `OPENAI_MODEL` - Model name (e.g., gpt-4, gpt-3.5-turbo)
- `POSTGRES_*` - Database connection settings

### Service Ports

- **5000** - SignalR API (client connections)
- **5001** - LLM Proxy (internal)
- **5002** - MCP Server (internal)
- **5432** - PostgreSQL (internal)
- **8080** - Web Client (optional)

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific project tests
dotnet test src/NetHackChatGame.McpServer.Tests
```

## 📝 License

MIT License - See [LICENSE](LICENSE) file

## 🤝 Contributing

This is a personal learning project. Feel free to fork and experiment!

## 🐛 Known Issues

- MCP C# SDK is in early development - may need workarounds
- Streaming responses not yet implemented
- No authentication/authorization (single user only)

## 📞 Support

For issues and questions, please open a GitHub issue.

---

**Status:** 🚧 In Development

Built with ❤️ using .NET, MCP, and AI
