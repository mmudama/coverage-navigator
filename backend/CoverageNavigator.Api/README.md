# Coverage Navigator API

A cross-platform .NET 8 REST API that provides session-aware conversations with OpenAI and Anthropic AI services.

## Features

- Session-aware conversation management
- Support for OpenAI and Anthropic AI providers
- Externalized secrets via environment variables
- Multi-platform support (Linux, macOS, Windows)
- Full conversation context in each API call
- Swagger/OpenAPI documentation

## Prerequisites

- .NET 8 SDK
- API keys from OpenAI and/or Anthropic

## Configuration

### Environment Variables

Set the following environment variables:

**Linux/macOS:**
```bash
# Required: Specify which AI provider to use
export AI_PROVIDER="OpenAI"  # Options: OpenAI, Anthropic

# Required: Path to directory containing prompts subdirectory
export PROMPTS_BASE_DIRECTORY="/path/to/base/directory"

# OpenAI Configuration (required if AI_PROVIDER=OpenAI)
export OPENAI_API_KEY="your-openai-api-key"
export OPENAI_MODEL="gpt-4o-mini"  # Optional, defaults to gpt-4o-mini

# Anthropic Configuration (required if AI_PROVIDER=Anthropic)
export ANTHROPIC_API_KEY="your-anthropic-api-key"
export ANTHROPIC_MODEL="claude-3-5-sonnet-20241022"  # Optional
```

**Windows (PowerShell):**
```powershell
# Required: Specify which AI provider to use
$env:AI_PROVIDER="OpenAI"  # Options: OpenAI, Anthropic

# Required: Path to directory containing prompts subdirectory
$env:PROMPTS_BASE_DIRECTORY="C:\path\to\base\directory"

# OpenAI Configuration (required if AI_PROVIDER=OpenAI)
$env:OPENAI_API_KEY="your-openai-api-key"
$env:OPENAI_MODEL="gpt-4o-mini"

# Anthropic Configuration (required if AI_PROVIDER=Anthropic)
$env:ANTHROPIC_API_KEY="your-anthropic-api-key"
$env:ANTHROPIC_MODEL="claude-3-5-sonnet-20241022"
```

Or copy `.env.example` to `.env` and configure your keys.

**Note:** The AI provider is configured once at application startup and cannot be changed without restarting the application. Only configure the API key for the provider you intend to use.

### System Prompts

System prompts are loaded from markdown files in the `prompts` directory. The directory structure should be:

```
<PROMPTS_BASE_DIRECTORY>/
??? prompts/
    ??? system-default.md          # Required: Base system prompt
    ??? system-<identifier>.md     # Optional: Custom prompts
```

**How it works:**
1. The `system-default.md` file is always loaded as the base prompt
2. Additional prompts can be specified using the `systemPromptIdentifier` field in requests
3. Additional prompts are appended to the default prompt
4. System prompts should include JSON format instructions per the API requirements

**Example:**
Request with `systemPromptIdentifier: "prescription"` will load:
- `system-default.md` (always)
- `system-prescription.md` (additional)

See `prompts/README.md` for detailed documentation on creating and using system prompts.

## Running the Application

### Development
```bash
cd coverage-navigator\backend\CoverageNavigator.Api
dotnet restore
dotnet run
```

The API will be available at `https://localhost:5001` (or `http://localhost:5000`).
Access Swagger UI at `https://localhost:5001/swagger`

### Production Build
```bash
# For Linux x64
dotnet publish -c Release -r linux-x64 --self-contained

# For Linux ARM64
dotnet publish -c Release -r linux-arm64 --self-contained

# For Windows
dotnet publish -c Release -r win-x64 --self-contained
```

### Docker
```bash
cd coverage-navigator\backend
docker build -f CoverageNavigator.Api/Dockerfile -t coverage-navigator-api .

# Run with OpenAI
docker run -p 5000:80 \
  -e AI_PROVIDER="OpenAI" \
  -e OPENAI_API_KEY="your-key" \
  coverage-navigator-api

# Run with Anthropic
docker run -p 5000:80 \
  -e AI_PROVIDER="Anthropic" \
  -e ANTHROPIC_API_KEY="your-key" \
  coverage-navigator-api
```

## API Endpoints

### POST /api/chat
Send a message to the configured AI provider with conversation context.

**Request:**
```json
{
  "sessionId": "optional-existing-session-id",
  "message": "Hello, how are you?",
  "model": "gpt-4o-mini",
  "systemPromptIdentifier": "prescription"
}
```

**Response:**
```json
{
  "sessionId": "generated-or-provided-session-id",
  "message": "AI response here",
  "provider": "OpenAI",
  "model": "gpt-4o-mini",
  "messageCount": 2
}
```

**Notes:**
- The `sessionId` is optional. If not provided, a new session will be created.
- The `model` is optional. If not provided, the default model for the configured provider will be used.
- The `systemPromptIdentifier` is optional. If provided, loads additional system prompt from `system-<identifier>.md`.
- The `provider` in the response indicates which AI service is configured (set at startup via environment variable).
- System prompts are loaded from the `prompts` directory specified by `PROMPTS_BASE_DIRECTORY`.

### POST /api/chat/session
Create a new session.

**Response:**
```json
{
  "sessionId": "newly-generated-session-id"
}
```

### GET /api/chat/session/{sessionId}
Retrieve conversation history for a session.

**Response:**
```json
{
  "sessionId": "session-id",
  "messages": [
    {
      "role": "user",
      "content": "Hello",
      "timestamp": "2024-01-01T12:00:00Z"
    },
    {
      "role": "assistant",
      "content": "Hi there!",
      "timestamp": "2024-01-01T12:00:01Z"
    }
  ],
  "createdAt": "2024-01-01T12:00:00Z",
  "lastAccessedAt": "2024-01-01T12:00:01Z"
}
```

### DELETE /api/chat/session/{sessionId}
Delete a session and its conversation history.

## Example Usage

**Note:** The AI provider is configured at application startup via the `AI_PROVIDER` environment variable. All requests will use the configured provider.

### Using cURL (Linux/macOS/Windows)

**Start a new conversation:**
```bash
curl -X POST https://localhost:5001/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "What is the capital of France?"
  }'
```

**Continue the conversation:**
```bash
curl -X POST https://localhost:5001/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "sessionId": "your-session-id-from-first-response",
    "message": "What is its population?"
  }'
```

**Specify a custom model:**
```bash
curl -X POST https://localhost:5001/api/chat \
  -H "Content-Type: application/json" \
  -d '{
    "message": "Explain quantum computing",
    "model": "gpt-4o"
  }'
```

**To use a different AI provider, restart the application with a different AI_PROVIDER environment variable:**
```bash
# For Anthropic
export AI_PROVIDER="Anthropic"
export ANTHROPIC_API_KEY="your-anthropic-key"
dotnet run
```

### Using PowerShell

```powershell
$body = @{
    message = "Hello, AI!"
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/chat" `
  -Method Post `
  -ContentType "application/json" `
  -Body $body
```

## Architecture

- **Session Management**: In-memory session store (24-hour timeout)
- **Context Preservation**: All previous messages are sent with each request
- **Provider Configuration**: AI provider is set via environment variable at startup
- **Single Provider**: Application uses one AI provider per instance (OpenAI or Anthropic)
- **System Prompts**: Loaded from external markdown files for flexibility and maintainability
- **Session Persistence**: Stubbed for future implementation (currently no-op)
- **Security**: All API keys externalized to environment variables

### Project Structure
```
CoverageNavigator.Api\
??? Controllers/
?   ??? ChatController.cs          # REST API endpoints
??? Models/
?   ??? ChatRequest.cs             # Request DTOs
?   ??? ChatResponse.cs            # Response DTOs
?   ??? ConversationMessage.cs     # Message model
?   ??? ConversationSession.cs     # Session model
??? Services/
?   ??? AIConversationService.cs   # Main conversation orchestrator
?   ??? AnthropicService.cs        # Anthropic API integration
?   ??? OpenAIService.cs           # OpenAI API integration
?   ??? InMemorySessionStore.cs    # Session storage
?   ??? Interfaces/                # Service contracts
??? Program.cs                     # Application entry point
??? appsettings.json              # Configuration
??? Dockerfile                     # Docker container config
??? README.md                      # This file
```

## Supported AI Models

### OpenAI
- `gpt-4o` - Latest GPT-4 Optimized
- `gpt-4o-mini` - Cost-effective GPT-4 (default)
- `gpt-4-turbo`
- `gpt-3.5-turbo`

### Anthropic
- `claude-3-5-sonnet-20241022` - Latest Claude 3.5 Sonnet (default)
- `claude-3-opus-20240229` - Most capable
- `claude-3-sonnet-20240229`
- `claude-3-haiku-20240307` - Fastest

## Development

### Running Tests
```bash
dotnet test
```

### Building for Specific Platforms
The project supports cross-compilation for multiple platforms:
- `linux-x64` - Linux 64-bit
- `linux-arm64` - Linux ARM64 (Raspberry Pi, etc.)
- `win-x64` - Windows 64-bit
- `osx-x64` - macOS Intel
- `osx-arm64` - macOS Apple Silicon

## Deployment

### Linux Systemd Service
Create `/etc/systemd/system/ai-api.service`:
```ini
[Unit]
Description=Coverage Navigator AI API
After=network.target

[Service]
Type=notify
WorkingDirectory=/opt/ai-api
ExecStart=/opt/ai-api/CoverageNavigator.Api
Restart=always
User=www-data
Environment=AI_PROVIDER=OpenAI
Environment=OPENAI_API_KEY=your-key

[Install]
WantedBy=multi-user.target
```

Enable and start:
```bash
sudo systemctl enable ai-api
sudo systemctl start ai-api
```

## Security Considerations

- Never commit API keys to version control
- Use environment variables or secure secret management
- Enable HTTPS in production
- Implement rate limiting for production use
- Consider adding authentication/authorization
- Sessions expire after 24 hours of inactivity
- AI provider is configured at startup and cannot be changed without restart
- Only the API key for the configured provider is required

## Contributing

This project is part of the coverage-navigator repository.
Repository: https://github.com/mmudama/coverage-navigator

## License

All content is proprietary to Monique Y Mudama. No license is granted to copy, fork, modify, distribute, or incorporate this code in any form without prior written permission.