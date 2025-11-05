# Project Roadmap

NetHack Chat Game - Development roadmap and feature planning

---

## Project Vision

Create an engaging text-based dungeon crawler where players interact with an AI Game Master through natural language, combining classic roguelike mechanics with modern LLM-powered storytelling.

---

## Development Phases

### ✅ Phase 0: Planning (Complete)

**Status**: ✅ Complete

**Goals**:
- [x] Architecture design
- [x] Technology stack selection
- [x] Documentation structure
- [x] Game mechanics design
- [x] Database schema design

**Deliverables**:
- Complete documentation
- Docker Compose configuration
- Project structure defined

---

### 🚧 Phase 1: Core Infrastructure (Current)

**Status**: 🚧 In Progress  
**Target**: 2-3 weeks

**Goals**:
- [ ] Set up all project files and solution
- [ ] Implement basic SignalR API
- [ ] Implement basic LLM Proxy
- [ ] Implement basic MCP Server
- [ ] Database setup with EF Core
- [ ] Docker containers working

**Milestones**:
1. **Week 1**: Project structure, basic services
   - [ ] Create all .csproj files
   - [ ] Implement Program.cs for each service
   - [ ] Basic health check endpoints
   - [ ] Docker builds successfully

2. **Week 2**: Database and basic communication
   - [ ] EF Core entities and DbContext
   - [ ] Database migrations
   - [ ] SignalR hub basic echo
   - [ ] LLM Proxy calls OpenAI
   
3. **Week 3**: Integration
   - [ ] SignalR → LLM Proxy communication
   - [ ] LLM Proxy → OpenAI integration
   - [ ] Basic conversation persistence
   - [ ] All services communicate

**Deliverables**:
- Running Docker Compose setup
- Services communicate successfully
- Basic conversation flow works
- Database stores conversations

---

### 📋 Phase 2: MCP Integration

**Status**: 🔜 Planned  
**Target**: 2 weeks

**Goals**:
- [ ] Implement MCP C# SDK integration
- [ ] Create one working tool (get_current_room)
- [ ] LLM successfully calls tools via MCP
- [ ] Test tool calling end-to-end

**Milestones**:
1. **Week 1**: MCP Server setup
   - [ ] MCP SDK integration
   - [ ] Implement get_current_room tool
   - [ ] MCP server exposes tool correctly

2. **Week 2**: LLM integration
   - [ ] LLM Proxy MCP client setup
   - [ ] Tool call execution
   - [ ] Result passing back to LLM
   - [ ] End-to-end test

**Deliverables**:
- Working MCP server with 1 tool
- LLM successfully calls tool
- Game state created and persisted

---

### 📋 Phase 3: Game Logic

**Status**: 🔜 Planned  
**Target**: 3 weeks

**Goals**:
- [ ] Implement all 5 MCP tools
- [ ] Static dungeon generator
- [ ] Combat system
- [ ] Inventory system
- [ ] Complete game loop

**Milestones**:
1. **Week 1**: Basic game logic
   - [ ] Player model and stats
   - [ ] Static dungeon layout
   - [ ] Room navigation
   - [ ] get_player_stats tool
   - [ ] move_to_room tool

2. **Week 2**: Combat system
   - [ ] Combat mechanics implementation
   - [ ] Monster stats and behaviors
   - [ ] combat_action tool
   - [ ] Damage calculation
   - [ ] Victory/defeat logic

3. **Week 3**: Items and polish
   - [ ] Inventory system
   - [ ] loot_treasure tool
   - [ ] Item effects
   - [ ] Complete game loop
   - [ ] Balance tuning

**Deliverables**:
- Playable dungeon crawler
- All MCP tools working
- Combat system functional
- Items and looting works

---

### 📋 Phase 4: Persistence & Replay

**Status**: 🔜 Planned  
**Target**: 1 week

**Goals**:
- [ ] Save conversation history
- [ ] Save game state snapshots
- [ ] Replay endpoint
- [ ] Conversation listing

**Milestones**:
- [ ] Message persistence working
- [ ] Game snapshots created
- [ ] Replay API endpoint
- [ ] List conversations

**Deliverables**:
- Full conversation replay
- Game state at each turn
- Browse past games

---

### 📋 Phase 5: Web Client

**Status**: 🔜 Planned  
**Target**: 1 week

**Goals**:
- [ ] Simple HTML/CSS/JS client
- [ ] SignalR connection
- [ ] Chat interface
- [ ] Basic styling

**Milestones**:
- [ ] SignalR client integration
- [ ] Message display
- [ ] Input handling
- [ ] Responsive design

**Deliverables**:
- Working web client
- Good user experience
- Mobile-friendly

---

### 📋 Phase 6: Polish & Testing

**Status**: 🔜 Planned  
**Target**: 1 week

**Goals**:
- [ ] Unit tests
- [ ] Integration tests
- [ ] Error handling improvements
- [ ] Performance optimization
- [ ] Documentation updates

**Deliverables**:
- Test coverage >70%
- All error cases handled
- Performance benchmarks
- Updated documentation

---

## Future Phases (v2+)

### Phase 7: Advanced Features

**Target**: TBD

**Features**:
- [ ] Procedural dungeon generation
- [ ] Character classes (Warrior, Mage, Rogue)
- [ ] Magic system
- [ ] Advanced combat mechanics
- [ ] Quests and objectives
- [ ] NPCs and dialogue

### Phase 8: Social Features

**Target**: TBD

**Features**:
- [ ] Conversation sharing
- [ ] Leaderboards
- [ ] Achievements
- [ ] User accounts
- [ ] Authentication

### Phase 9: Scaling & Production

**Target**: TBD

**Features**:
- [ ] Multi-user support
- [ ] Horizontal scaling
- [ ] Redis backplane
- [ ] Rate limiting
- [ ] API versioning
- [ ] Monitoring dashboard

---

## Technology Improvements

### Short Term
- [ ] Add Swagger/OpenAPI docs
- [ ] Implement structured logging
- [ ] Add health check dashboard
- [ ] Docker Compose production config

### Medium Term
- [ ] CI/CD pipeline (GitHub Actions)
- [ ] Automated testing
- [ ] Code coverage reporting
- [ ] Performance monitoring

### Long Term
- [ ] Kubernetes deployment
- [ ] Microservices architecture
- [ ] Event-driven architecture
- [ ] CQRS pattern

---

## Known Issues & Technical Debt

### High Priority
- [ ] MCP C# SDK is early stage - may need custom implementation
- [ ] No authentication/authorization
- [ ] No rate limiting
- [ ] Error handling needs improvement

### Medium Priority
- [ ] No streaming responses
- [ ] No connection pooling optimization
- [ ] No caching strategy
- [ ] No backup automation

### Low Priority
- [ ] No metrics/telemetry
- [ ] No admin interface
- [ ] No log aggregation
- [ ] No automated backups

---

## Research & Experiments

### To Investigate
- [ ] MCP C# SDK capabilities and limitations
- [ ] Alternative LLM providers (Claude, local models)
- [ ] Streaming responses with SignalR
- [ ] WebAssembly client
- [ ] Voice interface (speech-to-text)

### Proof of Concepts
- [ ] Procedural generation algorithm
- [ ] Character class system
- [ ] Magic spell system
- [ ] Real-time multiplayer

---

## Release Planning

### v0.1.0 - MVP (End of Phase 3)
**Target**: 6-8 weeks from start

**Features**:
- ✅ Complete documentation
- 🚧 Basic chat interface
- 🔜 Working game loop
- 🔜 Combat system
- 🔜 Static dungeon

**Status**: Playable but minimal features

---

### v0.2.0 - Polish (End of Phase 6)
**Target**: +2 weeks

**Features**:
- Conversation replay
- Web client
- Tests
- Better error handling
- Performance improvements

**Status**: Feature-complete v1

---

### v1.0.0 - Production Ready (End of Phase 9)
**Target**: +4-6 weeks

**Features**:
- All v0.2.0 features
- Authentication
- User accounts
- Sharing
- Production deployment
- Monitoring

**Status**: Ready for public use

---

### v2.0.0 - Advanced Features (Future)
**Target**: TBD

**Features**:
- Procedural generation
- Character classes
- Magic system
- Multi-user
- Advanced mechanics

**Status**: Future vision

---

## Success Metrics

### Technical Metrics
- [ ] 100% service uptime
- [ ] < 3s response time (end-to-end)
- [ ] > 70% test coverage
- [ ] Zero critical bugs

### User Experience Metrics
- [ ] Average session length > 20 minutes
- [ ] Conversation completion rate > 60%
- [ ] Positive feedback from testers

### Learning Goals (Personal)
- [x] Understand MCP protocol
- [ ] Master SignalR
- [ ] Learn EF Core advanced features
- [ ] Practice microservices architecture
- [ ] Improve Docker skills

---

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md) for how to contribute to this roadmap.

**High-value contributions**:
1. MCP C# SDK implementation help
2. Game balance feedback
3. UI/UX improvements
4. Test coverage
5. Documentation improvements

---

## Updates

This roadmap is updated regularly as the project progresses.

**Last Updated**: November 4, 2025  
**Current Phase**: Phase 1 - Core Infrastructure  
**Progress**: Planning complete, beginning implementation

---

## Contact

For questions about the roadmap or to suggest features:
- Open a GitHub issue with "roadmap" label
- Propose changes via pull request

---

**Status Legend**:
- ✅ Complete
- 🚧 In Progress
- 🔜 Planned
- 💡 Idea
- ❌ Cancelled/Blocked
