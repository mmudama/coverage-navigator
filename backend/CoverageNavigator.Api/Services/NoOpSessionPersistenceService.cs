using CoverageNavigator.Contracts.Models;

namespace CoverageNavigator.Api.Services;

public class NoOpSessionPersistenceService : ISessionPersistenceService
{
    private readonly ILogger<NoOpSessionPersistenceService> _logger;

    public NoOpSessionPersistenceService(ILogger<NoOpSessionPersistenceService> logger)
    {
        _logger = logger;
        _logger.LogInformation("NoOpSessionPersistenceService initialized - sessions will not persist through restarts");
    }

    public Task<ConversationSession?> LoadSessionAsync(string sessionId)
    {
        _logger.LogDebug("LoadSessionAsync called for session {SessionId} - no-op implementation", sessionId);
        return Task.FromResult<ConversationSession?>(null);
    }

    public Task SaveSessionAsync(ConversationSession session)
    {
        _logger.LogDebug("SaveSessionAsync called for session {SessionId} - no-op implementation", session.SessionId);
        return Task.CompletedTask;
    }

    public Task DeleteSessionAsync(string sessionId)
    {
        _logger.LogDebug("DeleteSessionAsync called for session {SessionId} - no-op implementation", sessionId);
        return Task.CompletedTask;
    }
}

