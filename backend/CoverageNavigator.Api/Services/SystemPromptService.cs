using CoverageNavigator.Contracts.Models;

namespace CoverageNavigator.Api.Services;

public class SystemPromptService : ISystemPromptService
{
    private readonly string _promptsDirectory;
    private readonly ILogger<SystemPromptService> _logger;

    public SystemPromptService(IConfiguration configuration, ILogger<SystemPromptService> logger)
    {
        var baseDirectory = configuration["PROMPTS_BASE_DIRECTORY"]
            ?? Environment.GetEnvironmentVariable("PROMPTS_BASE_DIRECTORY")
            ?? throw new InvalidOperationException("PROMPTS_BASE_DIRECTORY not found in configuration or environment variables");

        _promptsDirectory = Path.Combine(baseDirectory, "prompts");
        _logger = logger;

        if (!Directory.Exists(_promptsDirectory))
        {
            throw new DirectoryNotFoundException($"Prompts directory not found: {_promptsDirectory}");
        }

        _logger.LogInformation("SystemPromptService initialized with prompts directory: {Directory}", _promptsDirectory);
    }

    public async Task<string> GetSystemPromptAsync(ChatRequest request)
    {
        var systemPromptBuilder = new System.Text.StringBuilder();

        var defaultPromptPath = Path.Combine(_promptsDirectory, "system-default.md");
        if (!File.Exists(defaultPromptPath))
        {
            throw new FileNotFoundException($"Default system prompt not found: {defaultPromptPath}");
        }

        var defaultPrompt = await File.ReadAllTextAsync(defaultPromptPath);
        systemPromptBuilder.AppendLine(defaultPrompt);

        var additionalPrompts = await GetAdditionalSystemPromptsAsync(request);
        foreach (var additionalPrompt in additionalPrompts)
        {
            systemPromptBuilder.AppendLine();
            systemPromptBuilder.AppendLine(additionalPrompt);
        }

        var finalPrompt = systemPromptBuilder.ToString().Trim();
        _logger.LogDebug("Generated system prompt with length: {Length}", finalPrompt.Length);

        return finalPrompt;
    }

    private async Task<List<string>> GetAdditionalSystemPromptsAsync(ChatRequest request)
    {
        var additionalPrompts = new List<string>();

        var promptIdentifiers = DetermineAdditionalPrompts(request);

        foreach (var identifier in promptIdentifiers)
        {
            var promptPath = Path.Combine(_promptsDirectory, $"system-{identifier}.md");
            if (File.Exists(promptPath))
            {
                var promptContent = await File.ReadAllTextAsync(promptPath);
                additionalPrompts.Add(promptContent);
                _logger.LogDebug("Loaded additional system prompt: {Identifier}", identifier);
            }
            else
            {
                _logger.LogWarning("Additional system prompt not found: {Path}", promptPath);
            }
        }

        return additionalPrompts;
    }

    private List<string> DetermineAdditionalPrompts(ChatRequest request)
    {
        var identifiers = new List<string>();

        if (!string.IsNullOrEmpty(request.SystemPromptIdentifier))
        {
            identifiers.Add(request.SystemPromptIdentifier);
        }

        return identifiers;
    }
}

