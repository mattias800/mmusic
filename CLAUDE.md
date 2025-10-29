# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

mmusic is a full-stack music player application with:
- **Backend**: ASP.NET Core 9.0 with HotChocolate GraphQL server
- **Frontend**: React 19 + TypeScript + Vite with urql GraphQL client
- **Architecture**: Vertical slice architecture with event sourcing for domain data

## Common Commands

### Backend (MusicGQL)
```bash
cd MusicGQL
dotnet build                    # Build the backend
dotnet run                      # Run the backend (default: http://localhost:5095)
dotnet test                     # Run tests
dotnet format                   # Format C# code
```

### Frontend (Web)
```bash
cd Web
bun install                     # Install dependencies
bun dev                         # Start dev server
bun build                       # Build for production
bun run types:once              # Generate GraphQL types once (requires backend running)
bun run types                   # Generate GraphQL types in watch mode
bun run prettier                # Format code
bun test                        # Run tests
bun test:watch                  # Run tests in watch mode
```

### Testing
```bash
cd MusicGQL
dotnet test                     # Run backend tests (xUnit)

cd Web
bun test                        # Run frontend tests (Vitest + React Testing Library)
```

## Architecture Overview

### Backend Architecture

**Vertical Slice Architecture**: Features are organized in `MusicGQL/Features/` with each feature containing all related code:
- GraphQL entities (e.g., `Artist.cs`)
- GraphQL search roots (e.g., `ArtistSearchRoot.cs`)
- `Commands/` - Command handlers for writes
- `Mutations/` - GraphQL mutations (one per file)
- `Services/` - Business logic and external service integration
- `Aggregates/` - Event-sourced read model projections

**Event Sourcing**: Internal domain data (users, playlists, liked songs) is event-sourced:
- Events are in `MusicGQL/Db/Postgres/Models/Events/`
- Commands apply events to the event store
- Aggregates project events into read models
- External data (MusicBrainz, Spotify) is NOT event-sourced

**Key Features**:
- Authentication (cookie-based)
- Server library management (local music files)
- Downloads (via Prowlarr, qBittorrent, Sabnzbd, SoulSeek)
- Playlists (event-sourced)
- Spotify/LastFM/ListenBrainz integration
- MusicBrainz metadata

### Frontend Architecture

**Feature-based structure**: `Web/src/features/` organized by domain:
- Each feature contains components, hooks, GraphQL operations
- Shared UI components in `Web/src/components/`
- Redux Toolkit for global state
- urql for GraphQL client with normalized cache

**GraphQL Integration**:
- GraphQL operations defined with `graphql` tag at module scope
- Types generated via `graphql-codegen` in `Web/src/gql/`
- Always use generated `*Document` constants and types

## GraphQL Conventions

### Mutation Pattern (Mandatory)

Every mutation follows this pattern:

```csharp
[ExtendObjectType(typeof(Mutation))]
public class SignInMutation
{
    public async Task<SignInResult> SignIn(SignInInput input, [Service] ...)
    {
        // implementation
        return new SignInSuccess(new(user));
    }
}

public record SignInInput(string Username, string Password);

[UnionType]
public abstract record SignInResult;
public record SignInSuccess(User User) : SignInResult;
public record SignInError(string Message) : SignInResult;
```

**Rules**:
- Single `input` parameter of type `OperationNameInput`
- Union result type `OperationNameResult` with `[UnionType]`
- Success records return the modified entity (e.g., updated `Playlist`, `Artist`)
- All result records declared at top level (NOT nested)
- One mutation per file in feature's `Mutations/` folder
- Register in `Program.cs` with `.AddTypeExtension<OperationNameMutation>()`

### GraphQL Entities

GraphQL entities wrap database/API models:

```csharp
public record NameCredit([property: GraphQLIgnore] MbNameCredit Model)
{
    public string Name => Model.Name;
    public Artist.Artist Artist => new(Model.Artist);
}
```

**Rules**:
- Use records with primary constructors
- Mark input model with `[GraphQLIgnore]`
- Never expose projection models directly in GraphQL

### Search Roots

Search roots provide query entry points:
- Registered as fields on `Query` type (not `ExtendObjectType`)
- Use services to fetch and transform data
- Return GraphQL entities, not raw models

## Code Style

### C# (Backend)
- Use primary constructors and records for data models
- Async/await for all I/O operations
- LINQ for data manipulation
- Dependency injection via ASP.NET Core
- Comments only for non-obvious logic (no comments on using statements)

### TypeScript (Frontend)
- Functional React components with hooks
- PascalCase for component files
- Absolute imports with `@/` alias
- Use generated GraphQL types from `@/gql/graphql`
- Define GraphQL operations at module scope, not inside components

## Database

**PostgreSQL with Entity Framework Core**:
- Still in early development - migrations can be deleted/recreated
- Event-sourced data in `EventDbContext`
- Create migrations: `cd MusicGQL && dotnet ef migrations add MigrationName`

## Important Patterns

1. **Commands for Writes**: Mutations should use commands for application-owned data
2. **Services for Reads**: External services (Spotify, MusicBrainz) use service classes with HybridCache
3. **Frontend GraphQL**: After schema changes, run `bun run types:once` in Web directory
4. **Cache Updates**: Update specific entities in cache, not entire cache
5. **Confirmations**: Use dedicated dialog components for destructive actions

## Additional Resources

See project-specific guidelines:
- Backend: `MusicGQL/.cursorrules` and `MusicGQL/AGENTS.md`
- Root: `.cursorrules` for full project guidelines
- Feature READMEs in `MusicGQL/Features/*/README.md`
