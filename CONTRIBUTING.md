# Contributing to NetHack Chat Game

Thank you for your interest in contributing! This is a personal learning project, but contributions are welcome.

---

## Getting Started

1. **Fork the repository**
2. **Clone your fork**:
   ```bash
   git clone https://github.com/yourusername/NetHackChatGame.git
   cd NetHackChatGame
   ```
3. **Set up development environment**: Follow [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md)
4. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

---

## Development Workflow

### 1. Make Your Changes

- Follow the existing code style
- Write clear commit messages
- Add tests if applicable
- Update documentation

### 2. Test Your Changes

```bash
# Run all services
docker-compose up -d

# Test manually through the client
# Run unit tests (when available)
dotnet test

# Check logs for errors
docker-compose logs
```

### 3. Update Documentation

If your changes affect:
- **Architecture**: Update [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)
- **APIs**: Update [docs/API.md](docs/API.md)
- **MCP Tools**: Update [docs/MCP_TOOLS.md](docs/MCP_TOOLS.md)
- **Database**: Update [docs/DATABASE.md](docs/DATABASE.md)
- **Game Mechanics**: Update [docs/GAME_DESIGN.md](docs/GAME_DESIGN.md)

### 4. Commit Your Changes

```bash
git add .
git commit -m "feat: add new combat mechanic"
```

**Commit Message Format**:
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation only
- `refactor:` - Code refactoring
- `test:` - Adding tests
- `chore:` - Maintenance tasks

### 5. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Then create a pull request on GitHub.

---

## Code Style

### C# Guidelines

- Use **PascalCase** for classes, methods, properties
- Use **camelCase** for local variables, parameters
- Use **meaningful names**
- Add **XML comments** for public APIs
- Follow **.NET conventions**

**Example**:

```csharp
/// <summary>
/// Executes a combat action between player and monster.
/// </summary>
/// <param name="action">The combat action to perform.</param>
/// <returns>The result of the combat action.</returns>
public async Task<CombatResult> ExecuteCombatActionAsync(CombatAction action)
{
    // Implementation
}
```

### File Organization

```
Service/
├── Controllers/      # API controllers
├── Services/         # Business logic
├── Models/           # Data models
├── Repositories/     # Data access
└── Program.cs        # Entry point
```

---

## Testing Guidelines

### Unit Tests (Future)

```csharp
[Fact]
public void CombatSystem_AttackAction_DealsDamage()
{
    // Arrange
    var player = new Player { Level = 1, WeaponDamage = 5 };
    var monster = new Monster { HP = 20, Defense = 1 };
    
    // Act
    var result = CombatSystem.ExecuteAttack(player, monster);
    
    // Assert
    Assert.True(result.DamageDealt > 0);
}
```

### Integration Tests

Test complete workflows:
- Create conversation
- Send message
- Execute tool call
- Verify game state

---

## Areas for Contribution

### High Priority

- [ ] **Unit Tests**: Add test coverage
- [ ] **Error Handling**: Improve error messages
- [ ] **Performance**: Optimize database queries
- [ ] **Documentation**: Add more examples

### Features

- [ ] **Procedural Generation**: Random dungeon generator
- [ ] **Character Classes**: Warrior, Mage, Rogue
- [ ] **Magic System**: Spells and abilities
- [ ] **Achievements**: Track player accomplishments
- [ ] **Leaderboards**: Compare scores

### Polish

- [ ] **Better UI**: Improve web client
- [ ] **Animations**: Combat visualizations
- [ ] **Sound Effects**: Audio feedback
- [ ] **Mobile Support**: Responsive design

---

## Bug Reports

### Before Reporting

1. Check existing issues
2. Verify it's reproducible
3. Test with latest version

### Report Template

```markdown
**Description**
Clear description of the bug

**Steps to Reproduce**
1. Start conversation
2. Send message "I attack"
3. Observe error

**Expected Behavior**
What should happen

**Actual Behavior**
What actually happened

**Environment**
- OS: Windows 11
- Docker version: 24.0.0
- .NET version: 8.0

**Logs**
```
Paste relevant logs here
```
```

---

## Feature Requests

### Template

```markdown
**Feature Description**
What feature would you like to see?

**Use Case**
Why is this feature needed?

**Proposed Solution**
How might this work?

**Alternatives**
Other approaches considered?
```

---

## Pull Request Guidelines

### PR Checklist

- [ ] Code follows project style
- [ ] Tests added/updated
- [ ] Documentation updated
- [ ] Commits are clear and logical
- [ ] No merge conflicts
- [ ] Builds successfully
- [ ] All services run without errors

### PR Template

```markdown
**Description**
What does this PR do?

**Motivation**
Why is this change needed?

**Changes**
- Added X
- Modified Y
- Fixed Z

**Testing**
How was this tested?

**Screenshots**
If applicable

**Related Issues**
Fixes #123
```

---

## Code Review Process

1. **Automated Checks** (future):
   - Build succeeds
   - Tests pass
   - Code style check

2. **Manual Review**:
   - Code quality
   - Documentation
   - Architecture fit

3. **Feedback**:
   - Address comments
   - Update PR
   - Request re-review

4. **Merge**:
   - Squash commits
   - Update changelog
   - Close related issues

---

## Communication

### Channels

- **GitHub Issues**: Bug reports, features
- **Pull Requests**: Code contributions
- **Discussions**: General questions

### Response Time

This is a personal project, so responses may take:
- **Issues**: 1-3 days
- **PRs**: 1-7 days
- **Questions**: As available

---

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

---

## Recognition

Contributors will be:
- Added to CONTRIBUTORS.md (when created)
- Mentioned in release notes
- Credited in documentation

---

## Questions?

Open an issue with the "question" label.

Thank you for contributing! 🎮
