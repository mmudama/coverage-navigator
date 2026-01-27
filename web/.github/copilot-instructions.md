# Coverage Navigator Web - Copilot Instructions

This is a React-based chat bot, passing dialog back and forth to an API.

## Project Setup

- Minimal React application with core dependencies
- Vite build tool for fast development
- Single-file entry point (App.jsx)
- Development and production build tasks

## Types source of truth
The source of truth for TypeScript types is the C# code in the contracts/CoverageNavigator.Contracts/Models directory. If they do not match the contents of web/src/types/api.ts , the Models directory is the authority. Update the api.ts file to match what's in Models.

## Development

Run `npm run dev` to start the development server.
Run `npm run build` to create a production build.

## Status

This project is in the discovery phase. We're still figuring out what we want.