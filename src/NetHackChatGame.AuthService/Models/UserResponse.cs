namespace NetHackChatGame.AuthService.Models;

public record UserResponse(
    Guid Id,
    string Username,
    string Email,
    DateTime CreatedAt,
    DateTime LastLoginAt
);
