# Quick Reference: System Prompts & Sessions

## Environment Setup

```bash
# Required variables
export AI_PROVIDER="OpenAI"                                    # or "Anthropic"
export PROMPTS_BASE_DIRECTORY="/path/to/backend"              # Contains prompts/ subdirectory
export OPENAI_API_KEY="sk-..."                                # Your OpenAI key

# Windows PowerShell
$env:AI_PROVIDER="OpenAI"
$env:PROMPTS_BASE_DIRECTORY="C:\path\to\backend"
$env:OPENAI_API_KEY="sk-..."
```

## Directory Structure

```
backend/
??? prompts/
    ??? system-default.md           ? Required
    ??? system-<youridentifier>.md  ? Optional
```

## API Request Examples

### Basic Request (default prompt only)
```json
POST /api/chat
{
  "message": "My procedure was denied, and I don't know what to do"
}
```

### With Session (continues conversation)
```json
POST /api/chat
{
  "sessionId": "abc-123-def",
  "message": "What did I just ask?"
}
```

### With Custom System Prompt
```json
POST /api/chat
{
  "message": "Help me get my prescription",
  "systemPromptIdentifier": "prescription"
}
```
Loads: `system-default.md` + `system-prescription.md`

### Full Example
```json
POST /api/chat
{
  "sessionId": "abc-123-def",
  "message": "Help me get my prescription",
  "model": "gpt-4o",
  "systemPromptIdentifier": "prescription"
}
```

## Creating New System Prompts

1. Create file: `prompts/system-myfeature.md`
2. Add markdown content with instructions
3. Include JSON format requirements
4. Use in requests: `"systemPromptIdentifier": "myfeature"`

**Example: prompts/system-myfeature.md**
```markdown
# My Feature Prompt

## Instructions
- Do X, Y, and Z
- Return JSON format with fields: a, b, c

## Response Structure
{
  "result": "...",
  "details": []
}
```

## Session Management

### Create Session
```json
POST /api/chat/session
Response: { "sessionId": "new-id" }
```

### Get Session History
```json
GET /api/chat/session/{sessionId}
Response: {
  "sessionId": "...",
  "messages": [...],
  "createdAt": "...",
  "lastAccessedAt": "..."
}
```

### Delete Session
```json
DELETE /api/chat/session/{sessionId}
Response: 204 No Content
```

## How Context Works

1. **First Message**: Creates new session, loads system prompt, sends to AI
2. **Follow-up Messages**: Loads session history, appends new message, sends ALL to AI
3. **AI Response**: Added to session history for next message

**Example Flow:**
```
Message 1: "What is 2+2?"
  ? AI: "2+2 equals 4"

Message 2: "What about 3+3?" (with same sessionId)
  ? AI sees: ["What is 2+2?", "4", "What about 3+3?"]
  ? AI: "3+3 equals 6"
```

## System Prompt Behavior

### OpenAI Format
```json
{
  "messages": [
    {"role": "system", "content": "System prompt here"},
    {"role": "user", "content": "First user message"},
    {"role": "assistant", "content": "First AI response"},
    {"role": "user", "content": "Second user message"}
  ]
}
```

### Anthropic Format
```json
{
  "system": "System prompt here",
  "messages": [
    {"role": "user", "content": "First user message"},
    {"role": "assistant", "content": "First AI response"},
    {"role": "user", "content": "Second user message"}
  ]
}
```

## Troubleshooting

| Error | Solution |
|-------|----------|
| "PROMPTS_BASE_DIRECTORY not found" | Set environment variable to parent of `prompts/` directory |
| "Default system prompt not found" | Create `prompts/system-default.md` |
| "Additional system prompt not found" | Warning only, check filename: `system-<identifier>.md` |
| Session not found | Use `POST /api/chat/session` to create new session first |
| No conversation context | Ensure you're passing the same `sessionId` in all requests |

## Best Practices

1. **Always use sessions** for multi-turn conversations
2. **Keep system prompts focused** on specific tasks
3. **Include JSON format requirements** in all prompts
4. **Test prompts with both providers** (OpenAI & Anthropic)
5. **Document custom prompt identifiers** in your team docs
6. **Version control prompts** separately from code if needed
7. **Set appropriate model** for your use case (speed vs capability)

## Limits

- Sessions expire after **24 hours** of inactivity
- Sessions are **in-memory** (lost on restart)
- No pagination for session history (all messages returned)
- System prompt max size depends on AI provider token limits

## Next Steps

- Read: `SYSTEM_PROMPTS_GUIDE.md` for detailed implementation
- Read: `IMPLEMENTATION_SUMMARY.md` for architecture overview
- Read: `prompts/README.md` for prompt creation guidelines
