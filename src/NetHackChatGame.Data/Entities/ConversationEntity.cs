namespace NetHackChatGame.Data.Entities;

/// <summary>
/// Database entity for conversations/game sessions
/// </summary>
public class ConversationEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime LastMessageAt { get; set; }
    public bool IsActive { get; set; }
    
    // Navigation properties
    public UserEntity User { get; set; } = null!;
    public ICollection<MessageEntity> Messages { get; set; } = new List<MessageEntity>();
    public ICollection<GameSnapshotEntity> GameSnapshots { get; set; } = new List<GameSnapshotEntity>();
}
