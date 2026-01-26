using CoverageNavigator.Api.Models;

namespace CoverageNavigator.Api.Services;

public interface IOpenAIService
{
    Task<string> SendMessageAsync(List<ConversationMessage> messages, string? systemPrompt = null, string? model = null);
}