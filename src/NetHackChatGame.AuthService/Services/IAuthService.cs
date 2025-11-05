using NetHackChatGame.Data.Entities;

namespace NetHackChatGame.AuthService.Services;

public interface IAuthService
{
    Task<UserEntity?> RegisterAsync(string username, string email, string password);
    Task<UserEntity?> LoginAsync(string username, string password);
    Task<UserEntity?> GetUserByIdAsync(Guid userId);
    Task<UserEntity?> GetUserByUsernameAsync(string username);
}
