namespace NetHackChatGame.Data.Entities;

/// <summary>
/// Database entity for game state snapshots
/// </summary>
public class GameSnapshotEntity
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public Guid MessageId { get; set; }
    public string GameStateJson { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    // Navigation properties
    public ConversationEntity Conversation { get; set; } = null!;
    public MessageEntity Message { get; set; } = null!;
}
