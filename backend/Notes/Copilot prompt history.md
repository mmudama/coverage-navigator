# Prompts given to Copilot to implement Coverage Navigator backend (C#)

This is a historical record of the prompts provided when creating this application.

## Prompts already given

1. createa a c# project for a multi-platform linux system. Create RESTful session-aware APIs to call either OpenAI or Anthropic's AI repeatedly, adding the previous input and API's response to each subsequent call as context. Externalize any secrets to environment variables.

1. Apply all the above changes to the README.md

1. remove any remaining unicode characters in README.md

1. The AI provider should be set as an environment variable rather than as part of the request. The AI provider should only be set at startup.

1. Reference the instructions in your .github/copilot-instructions.md
Update the code to additionally implement the following. 
The System input will come from files in a directory specified in an environment variable.
Within that directory, system inputs will always be under the "prompts" directory.
The contents of the system input will always begin with the contents of system-default.md.
In addition, in some circumstances there may be additional content appended to the system input of the API. This content will come from additional files within the "prompts" directory and will always be named in the format "system-<string>.md" where the contents of the string will be determined based on TBD logic.
For each session, store both the user inputs and API outputs and pass them back to the APIs on subsequent calls so that the models are aware of the entire chat history. Use the specific API's approach to providing this chat history.
Do not yet support session persistence through restarts, but stub out no-op classes and methods to support this functionality. Do not assume a particular data storage mechanism or format.

1. Rename everything from MultiPlatformAI to CoverageNavigator (this took a few interations

