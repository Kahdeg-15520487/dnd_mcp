using Microsoft.AspNetCore.Mvc;
using NetHackChatGame.AuthService.Models;
using NetHackChatGame.AuthService.Services;

namespace NetHackChatGame.AuthService.Controllers;

[ApiController]
[Route("api/conversations")]
public class ConversationController : ControllerBase
{
    private readonly IConversationService _conversationService;
    private readonly ILogger<ConversationController> _logger;

    public ConversationController(
        IConversationService conversationService, 
        ILogger<ConversationController> logger)
    {
        _conversationService = conversationService;
        _logger = logger;
    }

    [HttpPost("users/{userId:guid}")]
    public async Task<IActionResult> CreateConversation(
        Guid userId, 
        [FromBody] CreateConversationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.PlayerName))
        {
            return BadRequest(new { message = "Player name is required" });
        }

        var conversation = await _conversationService.CreateConversationAsync(userId, request.PlayerName);
        
        if (conversation == null)
        {
            return NotFound(new { message = "User not found" });
        }

        var response = new ConversationResponse(
            conversation.Id,
            conversation.PlayerName,
            conversation.StartedAt,
            conversation.LastMessageAt,
            conversation.IsActive
        );

        return CreatedAtAction(nameof(GetConversation), new { conversationId = conversation.Id }, response);
    }

    [HttpGet("users/{userId:guid}")]
    public async Task<IActionResult> GetUserConversations(Guid userId)
    {
        var conversations = await _conversationService.GetUserConversationsAsync(userId);
        
        var response = conversations.Select(c => new ConversationResponse(
            c.Id,
            c.PlayerName,
            c.StartedAt,
            c.LastMessageAt,
            c.IsActive
        ));

        return Ok(response);
    }

    [HttpGet("{conversationId:guid}")]
    public async Task<IActionResult> GetConversation(Guid conversationId)
    {
        var conversation = await _conversationService.GetConversationAsync(conversationId);
        
        if (conversation == null)
        {
            return NotFound(new { message = "Conversation not found" });
        }

        var messages = conversation.Messages
            .OrderBy(m => m.SequenceNumber)
            .Select(m => new MessageResponse(
                m.Id,
                m.Role,
                m.Content,
                m.ToolCalls,
                m.ToolResults,
                m.CreatedAt,
                m.SequenceNumber
            ))
            .ToList();

        var gameSnapshots = conversation.GameSnapshots
            .OrderBy(gs => gs.CreatedAt)
            .Select(gs => new GameSnapshotResponse(
                gs.Id,
                gs.MessageId,
                gs.GameStateJson,
                gs.CreatedAt
            ))
            .ToList();

        var response = new ConversationDetailResponse(
            conversation.Id,
            conversation.UserId,
            conversation.PlayerName,
            conversation.StartedAt,
            conversation.LastMessageAt,
            conversation.IsActive,
            messages,
            gameSnapshots
        );

        return Ok(response);
    }

    [HttpGet("users/{userId:guid}/active")]
    public async Task<IActionResult> GetActiveConversation(Guid userId)
    {
        var conversation = await _conversationService.GetActiveConversationAsync(userId);
        
        if (conversation == null)
        {
            return NotFound(new { message = "No active conversation found" });
        }

        var response = new ConversationResponse(
            conversation.Id,
            conversation.PlayerName,
            conversation.StartedAt,
            conversation.LastMessageAt,
            conversation.IsActive
        );

        return Ok(response);
    }

    [HttpDelete("{conversationId:guid}")]
    public async Task<IActionResult> DeleteConversation(Guid conversationId)
    {
        var success = await _conversationService.DeleteConversationAsync(conversationId);
        
        if (!success)
        {
            return NotFound(new { message = "Conversation not found" });
        }

        return NoContent();
    }
}
