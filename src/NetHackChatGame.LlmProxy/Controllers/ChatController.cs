using Microsoft.AspNetCore.Mvc;
using NetHackChatGame.LlmProxy.Models;
using NetHackChatGame.LlmProxy.Services;

namespace NetHackChatGame.LlmProxy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ChatController : ControllerBase
{
    private readonly ILlmService _llmService;
    private readonly ILogger<ChatController> _logger;

    public ChatController(ILlmService llmService, ILogger<ChatController> logger)
    {
        _llmService = llmService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> PostMessage(
        [FromBody] ChatRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Processing chat message for conversation {ConversationId}", request.ConversationId);

            var response = await _llmService.ProcessChatAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing chat message");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
