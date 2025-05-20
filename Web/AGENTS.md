# AI Agent Development Guidelines for mmusic Frontend (Web)

This document provides guidelines for AI agents assisting with the development of the mmusic frontend (Web client).

For detailed frontend development guidelines, please refer to the existing document:

[./.junie/guidelines.md](./.junie/guidelines.md)

Key aspects to remember from those guidelines and general project structure:

- **Framework:** React with TypeScript and Vite.
- **GraphQL Client:** urql.
- **GraphQL Code Generation:** Types and document nodes are generated via `bun run types`. Ensure the backend is running and accessible.
- **Component Structure:** Strive for modular and reusable components.
- **State Management:** Primarily use `useState`, `useContext`, and urql cache. Discuss with the user for more complex global state needs.
- **Styling:** (User to confirm preferred styling approach). Be consistent with existing patterns.
- **Imports:** Use `@/*` path aliases for `src` directory.

Always ensure your work aligns with the comprehensive guidelines in [./.junie/guidelines.md](./.junie/guidelines.md). 