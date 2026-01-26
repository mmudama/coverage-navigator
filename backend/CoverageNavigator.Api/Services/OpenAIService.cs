using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CoverageNavigator.Api.Models;

namespace CoverageNavigator.Api.Services;

public class OpenAIService : IOpenAIService, IAIProviderService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _defaultModel;

    public OpenAIService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OPENAI_API_KEY"] 
            ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY") 
            ?? throw new InvalidOperationException("OPENAI_API_KEY not found in configuration or environment variables");
        
        _defaultModel = configuration["OPENAI_MODEL"] 
            ?? Environment.GetEnvironmentVariable("OPENAI_MODEL") 
            ?? "gpt-4o-mini";

        _httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
    }

    public async Task<string> SendMessageAsync(List<ConversationMessage> messages, string? systemPrompt = null, string? model = null)
    {
        var messagesList = new List<object>();

        if (!string.IsNullOrEmpty(systemPrompt))
        {
            messagesList.Add(new
            {
                role = "system",
                content = systemPrompt
            });
        }

        messagesList.AddRange(messages.Select(m => new
        {
            role = m.Role,
            content = m.Content
        }));

        var requestBody = new
        {
            model = model ?? _defaultModel,
            messages = messagesList
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync("chat/completions", content);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync();
        var responseObj = JsonDocument.Parse(responseJson);
        
        var messageContent = responseObj.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        return messageContent ?? string.Empty;
    }

    public string GetProviderName() => "OpenAI";

    public string GetDefaultModel() => _defaultModel;
}