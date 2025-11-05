namespace NetHackChatGame.AuthService.Models;

public record ConversationResponse(
    Guid Id,
    string PlayerName,
    DateTime StartedAt,
    DateTime LastMessageAt,
    bool IsActive
);
