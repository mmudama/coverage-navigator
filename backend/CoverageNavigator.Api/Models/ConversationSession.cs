namespace CoverageNavigator.Api.Models;

public class ConversationSession
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public List<ConversationMessage> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;
}