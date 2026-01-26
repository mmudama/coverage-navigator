# Instructions for GitHub Copilot

## Instructions specifically for CoverageNavigator.Api project

### The instructions in this section of the document are only to be applied to the CoverageNavigator.Api project

* In documentation, always use relative paths. Never use a path that reveals a username.
* Secrets such as API keys must never be included in the code or documentation. They should be stored in environment variables.
* The specific API provider and model to be used should be configurable via environment variables.

### Guidance regarding API invocations
All calls to the AI APIs should follow the correct API input and output formats for the chosen AI provider. For example, the system input will use  the "role":"system" block in OpenAI while it will use the "system": <content> approach in the Anthropic endpoint.
* All AI API system inputs must include instructions for a JSON format in the response.

* 
### Purpose of this application
The purpose of this service is to provide a back-and-forth between the caller API and an AI API. For each call to the AI API, the service may choose to modify the system prompt (as defined by "Guidance regarding API invocations" in this document) based on specific fields of the JSON-formatted response, or based on the contents of the LLM's chat response, or based on additional fields provided in the caller's request.

## Proprietary contents - CoverageNavigator.Internal

Proprietary contents will be managed in the CoverageNavigator.Internal project. The directory for this file will be specified in an environment variable. Prompt text will be stored in a "prompts" subdirectory.

### Caution around editing proprietary contents

Never make changes to anything in the CoverageNavigator.Internal project without explicit instructions to do so. Always assume that your instructions will only apply to other projects in this solution.

## System Prompts - CoverageNavigator.Internal

The CoverageNavigator.Api service loads system prompts from the CoverageNavigator.Internal project at runtime.

### Prompt File Location and Structure

Prompt files are stored in: `CoverageNavigator.Internal/prompts/`

**Required files:**
- `system-default.md` - Base system prompt loaded for every request

**Optional prompt identifiers:**
The `ChatRequest.SystemPromptIdentifier` field can be set to load additional prompts. Valid identifiers are:
- `"prescription"` → loads `system-prescription.md` (in addition to default)
- (Add other identifiers as they are created)

### How Prompts Are Used

1. The base `system-default.md` is always loaded
2. If `systemPromptIdentifier` is provided in the request, the corresponding `system-{identifier}.md` file is appended
3. The prompts are combined and sent to the AI provider as the system prompt

### About Editing Prompts

Prompt files should be edited in the CoverageNavigator.Internal repository, not in CoverageNavigator.Api. Never hardcode prompt text in the API code.
