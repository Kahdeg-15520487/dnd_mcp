namespace NetHackChatGame.Data.Entities;

/// <summary>
/// Database entity for individual messages in a conversation
/// </summary>
public class MessageEntity
{
    public Guid Id { get; set; }
    public Guid ConversationId { get; set; }
    public string Role { get; set; } = string.Empty; // user, assistant, system, tool
    public string Content { get; set; } = string.Empty;
    public string? ToolCalls { get; set; } // JSON
    public string? ToolResults { get; set; } // JSON
    public DateTime CreatedAt { get; set; }
    public int SequenceNumber { get; set; }
    
    // Navigation properties
    public ConversationEntity Conversation { get; set; } = null!;
    public ICollection<GameSnapshotEntity> GameSnapshots { get; set; } = new List<GameSnapshotEntity>();
}
