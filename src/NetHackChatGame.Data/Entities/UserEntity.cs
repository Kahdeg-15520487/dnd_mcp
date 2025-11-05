namespace NetHackChatGame.Data.Entities;

/// <summary>
/// Database entity for user accounts
/// </summary>
public class UserEntity
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime LastLoginAt { get; set; }
    
    // Navigation properties
    public List<ConversationEntity> Conversations { get; set; } = new();
}
