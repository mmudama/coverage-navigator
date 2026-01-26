using CoverageNavigator.Api.Models;

namespace CoverageNavigator.Api.Services;

public interface ISessionPersistenceService
{
    Task<ConversationSession?> LoadSessionAsync(string sessionId);
    Task SaveSessionAsync(ConversationSession session);
    Task DeleteSessionAsync(string sessionId);
}
