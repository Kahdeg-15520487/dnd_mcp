using Microsoft.AspNetCore.Mvc;
using NetHackChatGame.LlmProxy.Models;
using NetHackChatGame.LlmProxy.Services;

namespace NetHackChatGame.LlmProxy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ConfigurationController : ControllerBase
{
    private readonly ILlmConfigurationService _configService;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(ILlmConfigurationService configService, ILogger<ConfigurationController> logger)
    {
        _configService = configService;
        _logger = logger;
    }

    /// <summary>
    /// Get current LLM configuration status (excludes sensitive API key)
    /// </summary>
    [HttpGet]
    public ActionResult<LlmConfigurationResponse> GetConfiguration()
    {
        var config = _configService.GetConfigurationStatus();
        return Ok(config);
    }

    /// <summary>
    /// Update LLM configuration at runtime
    /// </summary>
    /// <param name="config">New configuration. Only provided fields will be updated.</param>
    [HttpPost]
    public ActionResult UpdateConfiguration([FromBody] LlmConfiguration config)
    {
        try
        {
            _configService.UpdateConfiguration(config);
            _logger.LogInformation("LLM configuration updated successfully");
            
            return Ok(new
            {
                message = "Configuration updated successfully",
                status = _configService.GetConfigurationStatus()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update LLM configuration");
            return StatusCode(500, new { error = "Failed to update configuration", details = ex.Message });
        }
    }

    /// <summary>
    /// Test the LLM configuration with a simple request
    /// </summary>
    [HttpPost("test")]
    public async Task<ActionResult> TestConfiguration()
    {
        try
        {
            // TODO: Implement a simple test request to verify the configuration works
            var config = _configService.GetConfigurationStatus();
            
            return Ok(new
            {
                message = "Configuration appears valid",
                config
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Configuration test failed");
            return StatusCode(500, new { error = "Configuration test failed", details = ex.Message });
        }
    }
}
