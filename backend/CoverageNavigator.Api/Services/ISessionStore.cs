using CoverageNavigator.Contracts.Models;

namespace CoverageNavigator.Api.Services;

public interface ISessionStore
{
    ConversationSession GetOrCreateSession(string? sessionId);
    void UpdateSession(ConversationSession session);
    void DeleteSession(string sessionId);
    bool SessionExists(string sessionId);
}