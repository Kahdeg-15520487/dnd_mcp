using NetHackChatGame.LlmProxy.Models;

namespace NetHackChatGame.LlmProxy.Services;

public interface ILlmConfigurationService
{
    LlmConfiguration GetConfiguration();
    void UpdateConfiguration(LlmConfiguration config);
    LlmConfigurationResponse GetConfigurationStatus();
}
