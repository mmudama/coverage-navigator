# Quick Start Guide

## Setting Up for OpenAI

### Windows PowerShell
```powershell
# Navigate to project directory
cd backend\CoverageNavigator.Api

# Set environment variables
$env:AI_PROVIDER="OpenAI"
$env:OPENAI_API_KEY="your-openai-api-key-here"
$env:OPENAI_MODEL="gpt-4o-mini"  # Optional
$env:PROMPTS_BASE_DIRECTORY="<fullpath>\coverage-navigator-internal\prompts\"

# Run the application
dotnet run
```

### Linux/macOS
```bash
# Navigate to project directory
cd backend/CoverageNavigator.Api

# Set environment variables
export AI_PROVIDER="OpenAI"
export OPENAI_API_KEY="your-openai-api-key-here"
export OPENAI_MODEL="gpt-4o-mini"  # Optional
export PROMPTS_BASE_DIRECTORY="$HOME/coverage-navigator/backend"

# Run the application
dotnet run
```

## Setting Up for Anthropic

### Windows PowerShell
```powershell
$env:AI_PROVIDER="Anthropic"
$env:ANTHROPIC_API_KEY="your-anthropic-api-key-here"
$env:ANTHROPIC_MODEL="claude-3-5-sonnet-20241022"  # Optional
$env:PROMPTS_BASE_DIRECTORY="C:\Users\momud\source\repos\mmudama\coverage-navigator\backend"

dotnet run
```

### Linux/macOS
```bash
export AI_PROVIDER="Anthropic"
export ANTHROPIC_API_KEY="your-anthropic-api-key-here"
export ANTHROPIC_MODEL="claude-3-5-sonnet-20241022"  # Optional
export PROMPTS_BASE_DIRECTORY="$HOME/coverage-navigator/backend"

dotnet run
```

## Using the API

Once the application is running, access Swagger UI at: `https://localhost:5001/swagger`

### Basic Chat Request
```json
POST https://localhost:5001/api/chat
Content-Type: application/json

{
  "message": "Hello, how are you?"
}
```

### Continue Conversation
```json
POST https://localhost:5001/api/chat
Content-Type: application/json

{
  "sessionId": "your-session-id-from-previous-response",
  "message": "Tell me more about that."
}
```

### Specify Custom Model
```json
POST https://localhost:5001/api/chat
Content-Type: application/json

{
  "message": "Explain quantum computing",
  "model": "gpt-4o"
}
```

### Use Custom System Prompt
```json
POST https://localhost:5001/api/chat
Content-Type: application/json

{
  "message": "Review this code: function add(a, b) { return a + b; }",
  "systemPromptIdentifier": "prescription"
}
```

This loads the base prompt plus `system-prescription.md` for specialized code review instructions.

## Testing with cURL

### Windows
```powershell
curl.exe -X POST https://localhost:5001/api/chat `
  -H "Content-Type: application/json" `
  -d '{\"message\": \"Hello, AI!\"}'
```

### Linux/macOS
```bash
curl -X POST https://localhost:5001/api/chat \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello, AI!"}'
```

## Docker Quick Start

### Build
```bash
cd C:\Users\momud\source\repos\mmudama\coverage-navigator\backend
docker build -f CoverageNavigator.Api/Dockerfile -t coverage-navigator-api .
```

### Run with OpenAI
```bash
docker run -p 5000:80 \
  -e AI_PROVIDER="OpenAI" \
  -e OPENAI_API_KEY="your-key" \
  coverage-navigator-api
```

### Run with Anthropic
```bash
docker run -p 5000:80 \
  -e AI_PROVIDER="Anthropic" \
  -e ANTHROPIC_API_KEY="your-key" \
  coverage-navigator-api
```

## Troubleshooting

### Error: "OPENAI_API_KEY not found"
- Ensure you've set the `OPENAI_API_KEY` environment variable
- Check that `AI_PROVIDER` is set to "OpenAI"

### Error: "ANTHROPIC_API_KEY not found"
- Ensure you've set the `ANTHROPIC_API_KEY` environment variable
- Check that `AI_PROVIDER` is set to "Anthropic"

### Error: "Unsupported AI provider"
- Check that `AI_PROVIDER` is set to either "OpenAI" or "Anthropic" (case-insensitive)
- Verify the environment variable is set before running the application

### Error: "PROMPTS_BASE_DIRECTORY not found"
- Ensure you've set the `PROMPTS_BASE_DIRECTORY` environment variable
- The path should point to a directory containing a `prompts` subdirectory
- Example: If prompts are in `/opt/myapp/prompts/`, set `PROMPTS_BASE_DIRECTORY=/opt/myapp`

### Error: "Default system prompt not found"
- Verify that `system-default.md` exists in the `prompts` directory
- Check the path: `$PROMPTS_BASE_DIRECTORY/prompts/system-default.md`

### Want to switch providers?
- Stop the application (Ctrl+C)
- Set different environment variables
- Start the application again
