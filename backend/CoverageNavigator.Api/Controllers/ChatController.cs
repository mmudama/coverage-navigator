using Microsoft.AspNetCore.Mvc;
using CoverageNavigator.Contracts.Models;
using CoverageNavigator.Api.Services;

namespace CoverageNavigator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[Consumes("application/json")]
public class ChatController : ControllerBase
{
    private readonly IAIConversationService _conversationService;
    private readonly ISessionStore _sessionStore;

    public ChatController(IAIConversationService conversationService, ISessionStore sessionStore)
    {
        _conversationService = conversationService;
        _sessionStore = sessionStore;
    }

    /// <summary>
    /// Send a message to the AI and get a response
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

    /// <summary>
    /// Get conversation history for a session
    /// </summary>
    [HttpGet("session/{sessionId}")]
    [ProducesResponseType(typeof(ConversationSession), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult<ConversationSession> GetSession(string sessionId)
    {
        if (!_sessionStore.SessionExists(sessionId))
        {
            return NotFound(new { error = "Session not found" });
        }

        var session = _sessionStore.GetOrCreateSession(sessionId);
        return Ok(session);
    }

    /// <summary>
    /// Delete a session and its conversation history
    /// </summary>
    [HttpDelete("session/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public ActionResult DeleteSession(string sessionId)
    {
        _sessionStore.DeleteSession(sessionId);
        return NoContent();
    }

    /// <summary>
    /// Create a new session
    /// </summary>
    [HttpPost("session")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public ActionResult<object> CreateSession() 
    {
        var session = _sessionStore.GetOrCreateSession(null);
        return Ok(new { sessionId = session.SessionId });
    }
}