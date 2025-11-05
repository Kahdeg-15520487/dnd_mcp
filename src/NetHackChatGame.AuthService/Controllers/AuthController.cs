using Microsoft.AspNetCore.Mvc;
using NetHackChatGame.AuthService.Models;
using NetHackChatGame.AuthService.Services;

namespace NetHackChatGame.AuthService.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Email) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username, email, and password are required" });
        }

        if (request.Password.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters" });
        }

        var user = await _authService.RegisterAsync(request.Username, request.Email, request.Password);
        
        if (user == null)
        {
            return Conflict(new { message = "Username or email already exists" });
        }

        var response = new UserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.CreatedAt,
            user.LastLoginAt
        );

        return CreatedAtAction(nameof(GetUser), new { userId = user.Id }, response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || 
            string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Username and password are required" });
        }

        var user = await _authService.LoginAsync(request.Username, request.Password);
        
        if (user == null)
        {
            return Unauthorized(new { message = "Invalid username or password" });
        }

        var response = new UserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.CreatedAt,
            user.LastLoginAt
        );

        return Ok(response);
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetUser(Guid userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var response = new UserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.CreatedAt,
            user.LastLoginAt
        );

        return Ok(response);
    }

    [HttpGet("users/username/{username}")]
    public async Task<IActionResult> GetUserByUsername(string username)
    {
        var user = await _authService.GetUserByUsernameAsync(username);
        
        if (user == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var response = new UserResponse(
            user.Id,
            user.Username,
            user.Email,
            user.CreatedAt,
            user.LastLoginAt
        );

        return Ok(response);
    }
}
