using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CoverageNavigator.Api.Models;

namespace CoverageNavigator.Api.Services;

public class AnthropicService : IAnthropicService, IAIProviderService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _defaultModel;

    public AnthropicService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["ANTHROPIC_API_KEY"] 
            ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY") 
            ?? throw new InvalidOperationException("ANTHROPIC_API_KEY not found in configuration or environment variables");
        
        _defaultModel = configuration["ANTHROPIC_MODEL"] 
            ?? Environment.GetEnvironmentVariable("ANTHROPIC_MODEL") 
            ?? "claude-3-5-sonnet-20241022";

        _httpClient.BaseAddress = new Uri("https://api.anthropic.com/v1/");
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
    }

    public async Task<string> SendMessageAsync(List<ConversationMessage> messages, string? systemPrompt = null, string? model = null)
    {
        var requestBody = new
        {
            model = model ?? _defaultModel,
            max_tokens = 4096,
            system = systemPrompt,
            messages = messages
                .Where(m => m.Role != "system")
                .Select(m => new
                {
                    role = m.Role,
                    content = m.Content
                }).ToList()
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("messages", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseObj = JsonDocument.Parse(responseJson);
        
        var messageContent = responseObj.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString();

        return messageContent ?? string.Empty;
    }

    public string GetProviderName() => "Anthropic";

    public string GetDefaultModel() => _defaultModel;
}