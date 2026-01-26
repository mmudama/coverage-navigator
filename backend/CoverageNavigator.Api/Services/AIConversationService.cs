using CoverageNavigator.Api.Models;

namespace CoverageNavigator.Api.Services;

public class AIConversationService : IAIConversationService
{
    private readonly ISessionStore _sessionStore;
    private readonly IAIProviderService _aiProviderService;
    private readonly ISystemPromptService _systemPromptService;
    private readonly ISessionPersistenceService _sessionPersistenceService;

    public AIConversationService(
        ISessionStore sessionStore,
        IAIProviderService aiProviderService,
        ISystemPromptService systemPromptService,
        ISessionPersistenceService sessionPersistenceService)
    {
        _sessionStore = sessionStore;
        _aiProviderService = aiProviderService;
        _systemPromptService = systemPromptService;
        _sessionPersistenceService = sessionPersistenceService;
    }

    public async Task<ChatResponse> SendMessageAsync(ChatRequest request)
    {
        var session = _sessionStore.GetOrCreateSession(request.SessionId);

        var systemPrompt = await _systemPromptService.GetSystemPromptAsync(request);

        var userMessage = new ConversationMessage
        {
            Role = "user",
            Content = request.Message,
            Timestamp = DateTime.UtcNow
        };
        session.Messages.Add(userMessage);

        string aiResponse = await _aiProviderService.SendMessageAsync(session.Messages, systemPrompt, request.Model);

        var assistantMessage = new ConversationMessage
        {
            Role = "assistant",
            Content = aiResponse,
            Timestamp = DateTime.UtcNow
        };
        session.Messages.Add(assistantMessage);

        _sessionStore.UpdateSession(session);
        await _sessionPersistenceService.SaveSessionAsync(session);

        return new ChatResponse
        {
            SessionId = session.SessionId,
            Message = aiResponse,
            Provider = _aiProviderService.GetProviderName(),
            Model = request.Model ?? _aiProviderService.GetDefaultModel(),
            MessageCount = session.Messages.Count
        };
    }
}