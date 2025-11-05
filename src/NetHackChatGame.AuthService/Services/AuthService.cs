using Microsoft.EntityFrameworkCore;
using NetHackChatGame.Data;
using NetHackChatGame.Data.Entities;

namespace NetHackChatGame.AuthService.Services;

public class AuthService : IAuthService
{
    private readonly NetHackDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;

    public AuthService(NetHackDbContext dbContext, ILogger<AuthService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<UserEntity?> RegisterAsync(string username, string email, string password)
    {
        // Check if username already exists
        var existingUser = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username || u.Email == email);

        if (existingUser != null)
        {
            _logger.LogWarning("Registration failed: Username or email already exists");
            return null;
        }

        // Hash the password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

        var user = new UserEntity
        {
            Username = username,
            Email = email,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User registered successfully: {Username}", username);
        return user;
    }

    public async Task<UserEntity?> LoginAsync(string username, string password)
    {
        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            _logger.LogWarning("Login failed: User not found - {Username}", username);
            return null;
        }

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed: Invalid password for user {Username}", username);
            return null;
        }

        // Update last login time
        user.LastLoginAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("User logged in successfully: {Username}", username);
        return user;
    }

    public async Task<UserEntity?> GetUserByIdAsync(Guid userId)
    {
        return await _dbContext.Users
            .Include(u => u.Conversations)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<UserEntity?> GetUserByUsernameAsync(string username)
    {
        return await _dbContext.Users
            .Include(u => u.Conversations)
            .FirstOrDefaultAsync(u => u.Username == username);
    }
}
