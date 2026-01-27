using CoverageNavigator.Contracts.Models;

namespace CoverageNavigator.Api.Services;

public interface IAIConversationService
{
    Task<ChatResponse> SendMessageAsync(ChatRequest request);
}