using Microsoft.EntityFrameworkCore;
using NetHackChatGame.Data;
using NetHackChatGame.Data.Entities;

namespace NetHackChatGame.AuthService.Services;

public class ConversationService : IConversationService
{
    private readonly NetHackDbContext _dbContext;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(NetHackDbContext dbContext, ILogger<ConversationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ConversationEntity?> CreateConversationAsync(Guid userId, string playerName)
    {
        var user = await _dbContext.Users.FindAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Cannot create conversation: User {UserId} not found", userId);
            return null;
        }

        var conversation = new ConversationEntity
        {
            UserId = userId,
            PlayerName = playerName,
            StartedAt = DateTime.UtcNow,
            LastMessageAt = DateTime.UtcNow,
            IsActive = true
        };

        _dbContext.Conversations.Add(conversation);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Conversation created: {ConversationId} for user {UserId}", 
            conversation.Id, userId);
        
        return conversation;
    }

    public async Task<List<ConversationEntity>> GetUserConversationsAsync(Guid userId)
    {
        return await _dbContext.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.LastMessageAt)
            .ToListAsync();
    }

    public async Task<ConversationEntity?> GetConversationAsync(Guid conversationId)
    {
        return await _dbContext.Conversations
            .Include(c => c.Messages)
            .Include(c => c.GameSnapshots)
            .FirstOrDefaultAsync(c => c.Id == conversationId);
    }

    public async Task<bool> DeleteConversationAsync(Guid conversationId)
    {
        var conversation = await _dbContext.Conversations.FindAsync(conversationId);
        if (conversation == null)
        {
            return false;
        }

        _dbContext.Conversations.Remove(conversation);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Conversation deleted: {ConversationId}", conversationId);
        return true;
    }

    public async Task<ConversationEntity?> GetActiveConversationAsync(Guid userId)
    {
        return await _dbContext.Conversations
            .Where(c => c.UserId == userId && c.IsActive)
            .OrderByDescending(c => c.LastMessageAt)
            .FirstOrDefaultAsync();
    }
}
