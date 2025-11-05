using NetHackChatGame.Data.Entities;

namespace NetHackChatGame.AuthService.Services;

public interface IConversationService
{
    Task<ConversationEntity?> CreateConversationAsync(Guid userId, string playerName);
    Task<List<ConversationEntity>> GetUserConversationsAsync(Guid userId);
    Task<ConversationEntity?> GetConversationAsync(Guid conversationId);
    Task<bool> DeleteConversationAsync(Guid conversationId);
    Task<ConversationEntity?> GetActiveConversationAsync(Guid userId);
}
