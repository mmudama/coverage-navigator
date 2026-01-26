using Microsoft.AspNetCore.Mvc;
using CoverageNavigator.Api.Models;
using CoverageNavigator.Api.Services;

namespace CoverageNavigator.Api.Controllers;

[ApiController]
[Route("api/chat")]
public class ChatController : ControllerBase
{
    private readonly IAIConversationService _conversationService;
    private readonly ISessionStore _sessionStore;

    public ChatController(IAIConversationService conversationService, ISessionStore sessionStore)
    {
        _conversationService = conversationService;
        _sessionStore = sessionStore;
    }

    [HttpPost]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { error = "Message cannot be empty" });
        }

        try
        {
            var response = await _conversationService.SendMessageAsync(request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("session/{sessionId}")]
    public ActionResult<ConversationSession> GetSession(string sessionId)
    {
        if (!_sessionStore.SessionExists(sessionId))
        {
            return NotFound(new { error = "Session not found" });
        }

        var session = _sessionStore.GetOrCreateSession(sessionId);
        return Ok(session);
    }

    [HttpDelete("session/{sessionId}")]
    public ActionResult DeleteSession(string sessionId)
    {
        _sessionStore.DeleteSession(sessionId);
        return NoContent();
    }

    [HttpPost("session")]
    public ActionResult<ConversationSession> CreateSession() 
    {
        var session = _sessionStore.GetOrCreateSession(null);
        return Ok(new { sessionId = session.SessionId });
    }
}