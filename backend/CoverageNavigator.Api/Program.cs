using CoverageNavigator.Api.Models;
using CoverageNavigator.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Get AI provider from environment variable
var aiProvider = builder.Configuration["AI_PROVIDER"] 
    ?? Environment.GetEnvironmentVariable("AI_PROVIDER") 
    ?? "OpenAI";

// Configure HTTP clients and register the appropriate AI provider
if (aiProvider.Equals("Anthropic", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IAnthropicService, AnthropicService>();
    builder.Services.AddScoped<IAIProviderService>(sp => 
        sp.GetRequiredService<IAnthropicService>() as AnthropicService 
        ?? throw new InvalidOperationException("Failed to resolve AnthropicService"));
}
else if (aiProvider.Equals("OpenAI", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.AddHttpClient<IOpenAIService, OpenAIService>();
    builder.Services.AddScoped<IAIProviderService>(sp => 
        sp.GetRequiredService<IOpenAIService>() as OpenAIService 
        ?? throw new InvalidOperationException("Failed to resolve OpenAIService"));
}
else
{
    throw new InvalidOperationException($"Unsupported AI provider: {aiProvider}. Supported providers are: OpenAI, Anthropic");
}

// Register session store and AI services
builder.Services.AddSingleton<ISessionStore, InMemorySessionStore>();
builder.Services.AddScoped<IAIConversationService, AIConversationService>();
builder.Services.AddScoped<ISystemPromptService, SystemPromptService>();
builder.Services.AddSingleton<ISessionPersistenceService, NoOpSessionPersistenceService>();

// Add CORS for development
// TODO: Restrict CORS policy in production
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Log the configured AI provider
app.Logger.LogInformation("Application started with AI Provider: {Provider}", aiProvider);

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();