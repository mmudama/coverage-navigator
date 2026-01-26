using CoverageNavigator.Api.Models;

namespace CoverageNavigator.Api.Services;

public interface IAnthropicService
{
    Task<string> SendMessageAsync(List<ConversationMessage> messages, string? systemPrompt = null, string? model = null);
}