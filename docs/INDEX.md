# Documentation Index

Complete documentation for the NetHack Chat Game project.

---

## 📚 Documentation Overview

This project includes comprehensive documentation covering architecture, APIs, deployment, and game design.

### Quick Links

- **[README.md](../README.md)** - Project overview and quick start
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - System architecture and design
- **[API.md](API.md)** - Complete API reference
- **[MCP_TOOLS.md](MCP_TOOLS.md)** - MCP tools specification
- **[DATABASE.md](DATABASE.md)** - Database schema and queries
- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Deployment and operations guide
- **[GAME_DESIGN.md](GAME_DESIGN.md)** - Game mechanics and design

---

## 📖 Documentation Structure

### 1. Getting Started

**For Players:**
- [README.md](../README.md) - How to run and play the game
- [GAME_DESIGN.md](GAME_DESIGN.md) - Game rules and mechanics

**For Developers:**
- [ARCHITECTURE.md](ARCHITECTURE.md) - Understanding the system
- [DEPLOYMENT.md](DEPLOYMENT.md) - Setting up development environment

### 2. System Documentation

**Architecture & Design:**
- [ARCHITECTURE.md](ARCHITECTURE.md)
  - Service architecture
  - Communication protocols
  - Data flow diagrams
  - Technology decisions
  - Scaling strategies

**API Reference:**
- [API.md](API.md)
  - SignalR WebSocket API
  - LLM Proxy REST API
  - MCP Server protocol
  - Authentication (future)
  - Error handling

**MCP Tools:**
- [MCP_TOOLS.md](MCP_TOOLS.md)
  - Tool specifications
  - Input/output schemas
  - Usage examples
  - Best practices

**Database:**
- [DATABASE.md](DATABASE.md)
  - Schema design
  - Entity relationships
  - Indexes and performance
  - Common queries
  - Backup strategies

### 3. Operations

**Deployment:**
- [DEPLOYMENT.md](DEPLOYMENT.md)
  - Docker Compose setup
  - Local development
  - Production deployment
  - Cloud platforms (Azure, AWS, K8s)
  - Monitoring and logging
  - Troubleshooting

**Configuration:**
- [.env.example](../.env.example) - Environment variables
- [docker-compose.yml](../docker-compose.yml) - Service orchestration

### 4. Game Design

**Game Mechanics:**
- [GAME_DESIGN.md](GAME_DESIGN.md)
  - Core gameplay loop
  - Combat system
  - Character progression
  - Monster stats
  - Items and equipment
  - Dungeon layout
  - Balance and difficulty

---

## 🎯 Documentation by Role

### For Players

**Want to play the game?**
1. Start with [README.md](../README.md) - Quick start guide
2. Learn game rules in [GAME_DESIGN.md](GAME_DESIGN.md)
3. Understand commands and actions

### For Developers

**Setting up for development?**
1. Read [ARCHITECTURE.md](ARCHITECTURE.md) - Understand the system
2. Follow [DEPLOYMENT.md](DEPLOYMENT.md) - Set up environment
3. Review [API.md](API.md) - Understand APIs
4. Check [DATABASE.md](DATABASE.md) - Database schema

**Building features?**
1. Review [ARCHITECTURE.md](ARCHITECTURE.md) - System design
2. Check [MCP_TOOLS.md](MCP_TOOLS.md) - Tool implementation
3. Reference [GAME_DESIGN.md](GAME_DESIGN.md) - Game mechanics

### For DevOps/SRE

**Deploying the system?**
1. Follow [DEPLOYMENT.md](DEPLOYMENT.md) - Complete guide
2. Review [.env.example](../.env.example) - Configuration
3. Check [docker-compose.yml](../docker-compose.yml) - Services

**Operating in production?**
1. [DEPLOYMENT.md](DEPLOYMENT.md) - Monitoring and logging
2. [DATABASE.md](DATABASE.md) - Backup and recovery
3. [API.md](API.md) - Health checks

### For Game Designers

**Balancing the game?**
1. Review [GAME_DESIGN.md](GAME_DESIGN.md) - All mechanics
2. Check [MCP_TOOLS.md](MCP_TOOLS.md) - Available actions
3. Understand [ARCHITECTURE.md](ARCHITECTURE.md) - How AI works

---

## 📊 Documentation Coverage

| Topic | Document | Status |
|-------|----------|--------|
| **Overview** | README.md | ✅ Complete |
| **Architecture** | ARCHITECTURE.md | ✅ Complete |
| **APIs** | API.md | ✅ Complete |
| **MCP Tools** | MCP_TOOLS.md | ✅ Complete |
| **Database** | DATABASE.md | ✅ Complete |
| **Deployment** | DEPLOYMENT.md | ✅ Complete |
| **Game Design** | GAME_DESIGN.md | ✅ Complete |
| **Configuration** | .env.example | ✅ Complete |
| **Orchestration** | docker-compose.yml | ✅ Complete |

---

## 🔍 Documentation Features

### Comprehensive Coverage

- **Architecture**: Every service documented with diagrams
- **APIs**: Every endpoint with request/response examples
- **Tools**: Every MCP tool with input/output schemas
- **Database**: Every table with ER diagrams and queries
- **Deployment**: Multiple platforms with step-by-step guides
- **Game**: Every mechanic with formulas and examples

### Practical Examples

Each document includes:
- ✅ Code examples
- ✅ Configuration samples
- ✅ Command-line snippets
- ✅ API request/response examples
- ✅ Troubleshooting tips

### Visual Aids

- ASCII diagrams for architecture
- ER diagrams for database
- Flow charts for processes
- Tables for comparisons
- Maps for dungeon layout

---

## 📝 Documentation Standards

### Style Guide

**Code Blocks:**
```csharp
// Always specify language
public class Example { }
```

**Environment Variables:**
```env
# Always include comments
VARIABLE_NAME=value
```

**API Examples:**
```http
GET /api/endpoint
Content-Type: application/json
```

**Shell Commands:**
```bash
# Include explanatory comments
docker-compose up -d
```

### Organization

1. **Heading Hierarchy**: Consistent use of #, ##, ###
2. **Table of Contents**: For documents >500 lines
3. **Cross-References**: Links between related docs
4. **Examples**: After every concept
5. **Troubleshooting**: Dedicated sections

---

## 🔄 Keeping Documentation Updated

### When to Update

**Architecture Changes:**
- Update [ARCHITECTURE.md](ARCHITECTURE.md)
- Update relevant diagrams

**API Changes:**
- Update [API.md](API.md)
- Update request/response examples

**New MCP Tools:**
- Update [MCP_TOOLS.md](MCP_TOOLS.md)
- Add tool specification

**Database Changes:**
- Update [DATABASE.md](DATABASE.md)
- Update ER diagram

**Game Balance:**
- Update [GAME_DESIGN.md](GAME_DESIGN.md)
- Update balance tables

### Version History

Track major documentation changes:

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2025-11-04 | Initial complete documentation |

---

## 🤝 Contributing to Documentation

### Guidelines

1. **Clarity**: Write for someone unfamiliar with the project
2. **Examples**: Include practical, working examples
3. **Completeness**: Document all parameters and options
4. **Accuracy**: Test all commands and examples
5. **Maintenance**: Update when code changes

### Documentation Checklist

When adding new features:
- [ ] Update relevant .md files
- [ ] Add code examples
- [ ] Update diagrams if needed
- [ ] Test all examples
- [ ] Update this index if adding new docs

---

## 📚 External Resources

### Technologies

- **ASP.NET Core**: https://learn.microsoft.com/aspnet/core
- **SignalR**: https://learn.microsoft.com/aspnet/core/signalr
- **Entity Framework Core**: https://learn.microsoft.com/ef/core
- **PostgreSQL**: https://www.postgresql.org/docs
- **Docker**: https://docs.docker.com
- **Docker Compose**: https://docs.docker.com/compose

### Model Context Protocol

- **MCP Specification**: https://modelcontextprotocol.io
- **MCP C# SDK**: https://github.com/modelcontextprotocol/csharp-sdk
- **MCP Documentation**: https://modelcontextprotocol.io/docs

### OpenAI

- **API Reference**: https://platform.openai.com/docs/api-reference
- **Chat Completions**: https://platform.openai.com/docs/guides/chat
- **Function Calling**: https://platform.openai.com/docs/guides/function-calling

---

## 🔗 Quick Reference

### Essential Commands

```bash
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Rebuild services
docker-compose up --build -d

# Stop services
docker-compose down

# Database access
docker exec -it nethack-postgres psql -U nethack_user -d nethack_chat

# Health checks
curl http://localhost:5000/health
curl http://localhost:5001/health
curl http://localhost:5002/health
```

### Essential URLs

| Service | URL | Purpose |
|---------|-----|---------|
| Web Client | http://localhost:8080 | Play the game |
| SignalR API | http://localhost:5000 | WebSocket server |
| LLM Proxy | http://localhost:5001 | LLM orchestration |
| MCP Server | http://localhost:5002 | Game logic |
| PostgreSQL | localhost:5432 | Database |

### Key Files

| File | Purpose |
|------|---------|
| `.env` | Environment configuration |
| `docker-compose.yml` | Service orchestration |
| `appsettings.json` | App configuration |
| `Program.cs` | Service entry point |
| `DbContext.cs` | Database context |

---

## 📧 Support

For questions about the documentation:
1. Check the relevant document first
2. Search for keywords in all docs
3. Open a GitHub issue if unclear
4. Suggest improvements via pull request

---

**Documentation Status**: ✅ Complete and up-to-date as of 2025-11-04

**Last Updated**: November 4, 2025

**Maintainer**: Project Team
