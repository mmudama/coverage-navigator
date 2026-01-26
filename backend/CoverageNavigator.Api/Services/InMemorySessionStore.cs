using System.Collections.Concurrent;
using CoverageNavigator.Api.Models;

namespace CoverageNavigator.Api.Services;

public class InMemorySessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, ConversationSession> _sessions = new();
    private readonly TimeSpan _sessionTimeout = TimeSpan.FromHours(24);

    public ConversationSession GetOrCreateSession(string? sessionId)
    {
        CleanupExpiredSessions();

        if (string.IsNullOrEmpty(sessionId) || !_sessions.TryGetValue(sessionId, out var session))
        {
            session = new ConversationSession();
            _sessions[session.SessionId] = session;
        }
        else
        {
            session.LastAccessedAt = DateTime.UtcNow;
        }

        return session;
    }

    public void UpdateSession(ConversationSession session)
    {
        session.LastAccessedAt = DateTime.UtcNow;
        _sessions[session.SessionId] = session;
    }

    public void DeleteSession(string sessionId)
    {
        _sessions.TryRemove(sessionId, out _);
    }

    public bool SessionExists(string sessionId)
    {
        return !string.IsNullOrEmpty(sessionId) && _sessions.ContainsKey(sessionId);
    }

    private void CleanupExpiredSessions()
    {
        var expiredSessions = _sessions
            .Where(kvp => DateTime.UtcNow - kvp.Value.LastAccessedAt > _sessionTimeout)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var sessionId in expiredSessions)
        {
            _sessions.TryRemove(sessionId, out _);
        }
    }
}