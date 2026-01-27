namespace CoverageNavigator.Contracts.Models;

public class ChatRequest
{
    public string? SessionId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Model { get; set; }
    public string? SystemPromptIdentifier { get; set; }
}