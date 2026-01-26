using CoverageNavigator.Api.Models;

namespace CoverageNavigator.Api.Services;

public interface IAIProviderService
{
    Task<string> SendMessageAsync(List<ConversationMessage> messages, string? systemPrompt = null, string? model = null);
    string GetProviderName();
    string GetDefaultModel();
}
