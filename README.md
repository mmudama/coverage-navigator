# Coverage Navigator

## Structure

This is a monorepo containing the back end, front end, and shared code for a POC of an AI-based tool. It cannot be built on its own - it requires an additional, private repository.

### Back end
The back end is implemented in C#
* Open `backend\CoverageNavigator.sln` in Visual Studio (not VS Code). This solution assumes a particular directory structure, which can be inferred from the solution file. The solution and project builds require an additional project not present in this repository. Note: Visual Studio Copilot requires that `.github\copilot-instructions.md` will be in the same directory as the solution file, and will only use that one file for the whole solution.

### Front end
This is currently an extremely basic driver for the back end, implemented in React. It is intended to be opened via the VS Code workspace `coverage-navigator.code-workspace`. The workspace includes the `contracts` directory, which contains some models implemented in C#. The only reason to include it in the workspace is to make it easy to create TypeScript types from the C# models. 