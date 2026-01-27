using CoverageNavigator.Contracts.Models;

namespace CoverageNavigator.Api.Services;

public interface ISystemPromptService
{
    Task<string> GetSystemPromptAsync(ChatRequest request);
}

