using NetHackChatGame.LlmProxy.Models;

namespace NetHackChatGame.LlmProxy.Services;

public class LlmConfigurationService : ILlmConfigurationService
{
    private LlmConfiguration _configuration;
    private readonly IConfiguration _appConfiguration;
    private readonly ILogger<LlmConfigurationService> _logger;

    public LlmConfigurationService(IConfiguration appConfiguration, ILogger<LlmConfigurationService> logger)
    {
        _appConfiguration = appConfiguration;
        _logger = logger;

        // Initialize from appsettings
        _configuration = new LlmConfiguration
        {
            ApiKey = _appConfiguration["OpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY"),
            Endpoint = _appConfiguration["OpenAI:Endpoint"],
            Model = _appConfiguration["OpenAI:Model"] ?? "gpt-4o-mini",
            Temperature = _appConfiguration.GetValue<float?>("OpenAI:Temperature") ?? 0.7f,
            MaxTokens = _appConfiguration.GetValue<int?>("OpenAI:MaxTokens") ?? 2000,
            MCPEndpoint = _appConfiguration["MCP:Endpoint"],
        };

        _logger.LogInformation("LLM Configuration initialized: Model={0}, Endpoint={1}, HasApiKey={2}, MCP Endpoint={3}",
                _configuration.Model, _configuration.Endpoint ?? "default", !string.IsNullOrEmpty(_configuration.ApiKey), _configuration.MCPEndpoint);
    }

    public LlmConfiguration GetConfiguration()
    {
        return new LlmConfiguration
        {
            ApiKey = _configuration.ApiKey,
            Endpoint = _configuration.Endpoint,
            Model = _configuration.Model,
            Temperature = _configuration.Temperature,
            MaxTokens = _configuration.MaxTokens,
            MCPEndpoint = _configuration.MCPEndpoint,
        };
    }

    public void UpdateConfiguration(LlmConfiguration config)
    {
        _logger.LogInformation("Updating LLM configuration: Model={Model}, Endpoint={Endpoint}",
            config.Model, config.Endpoint ?? "default");

        if (!string.IsNullOrWhiteSpace(config.ApiKey))
        {
            _configuration.ApiKey = config.ApiKey;
        }

        if (!string.IsNullOrWhiteSpace(config.Endpoint))
        {
            _configuration.Endpoint = config.Endpoint;
        }

        if (!string.IsNullOrWhiteSpace(config.Model))
        {
            _configuration.Model = config.Model;
        }

        _configuration.Temperature = config.Temperature;
        _configuration.MaxTokens = config.MaxTokens;
    }

    public LlmConfigurationResponse GetConfigurationStatus()
    {
        var hasApiKey = !string.IsNullOrEmpty(_configuration.ApiKey);
        var hasEndpoint = !string.IsNullOrEmpty(_configuration.Endpoint);

        string status;
        if (hasEndpoint)
        {
            status = "Configured with custom endpoint";
        }
        else if (hasApiKey)
        {
            status = "Configured with OpenAI API key";
        }
        else
        {
            status = "Not configured (will fail on requests)";
        }
        _logger.LogInformation("endpoint: |" + _appConfiguration["OpenAI:Endpoint"] + "|");
        _logger.LogInformation("LLM Configuration initialized: Model={0}, Endpoint={1}, HasApiKey={2}, MCP Endpoint={3}",
                _configuration.Model, _configuration.Endpoint ?? "default", !string.IsNullOrEmpty(_configuration.ApiKey), _configuration.MCPEndpoint);

        return new LlmConfigurationResponse
        {
            Endpoint = _configuration.Endpoint,
            Model = _configuration.Model,
            Temperature = _configuration.Temperature,
            MaxTokens = _configuration.MaxTokens,
            HasApiKey = hasApiKey,
            Status = status,
            MCPEndpoint = _configuration.MCPEndpoint,
        };
    }
}
