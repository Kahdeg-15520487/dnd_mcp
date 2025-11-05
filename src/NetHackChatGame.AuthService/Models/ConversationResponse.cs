namespace NetHackChatGame.AuthService.Models;

public record ConversationResponse(
    Guid Id,
    string PlayerName,
    DateTime StartedAt,
    DateTime LastMessageAt,
    bool IsActive
);

public record ConversationDetailResponse(
    Guid Id,
    Guid UserId,
    string PlayerName,
    DateTime StartedAt,
    DateTime LastMessageAt,
    bool IsActive,
    List<MessageResponse> Messages,
    List<GameSnapshotResponse> GameSnapshots
);

public record MessageResponse(
    Guid Id,
    string Role,
    string Content,
    string? ToolCalls,
    string? ToolResults,
    DateTime CreatedAt,
    int SequenceNumber
);

public record GameSnapshotResponse(
    Guid Id,
    Guid MessageId,
    string GameStateJson,
    DateTime CreatedAt
);
