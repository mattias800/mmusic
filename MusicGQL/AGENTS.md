# AI Agent Development Guidelines for MusicGQL (Backend)

This document provides guidelines for AI agents assisting with the development of the MusicGQL backend.

## 1. Understanding the Backend

- **Familiarize Yourself:** Before making changes, try to understand the relevant parts of the project structure, focusing on the C# backend (ASP.NET Core, HotChocolate for GraphQL, Entity Framework Core).
- **Key Areas:** Pay attention to `Program.cs` for service and GraphQL registrations, `Features` for business logic, `Db/Postgres` for database models and event sourcing, and `Integration` for external service communications.

## 2. Code Style and Conventions (C# & ASP.NET Core)

- **Naming Conventions:** Follow standard C# naming conventions (e.g., PascalCase for classes, methods, properties).
- **Async/Await:** Use `async` and `await` appropriately for all I/O-bound operations.
- **Dependency Injection:** Utilize ASP.NET Core's built-in dependency injection. Register services in `Program.cs`.
- **LINQ:** Use LINQ for data manipulation where appropriate, ensuring queries are efficient.
- **File Organization:** Keep code organized within the `Features` directory structure.
- **Comments:** Comment on non-obvious logic or complex sections of code.

## 3. GraphQL Development (HotChocolate)

- **Code-First Approach:** The project uses a code-first approach. GraphQL types are generated from C# classes.
- **Naming:** HotChocolate typically converts C# method names to camelCase for GraphQL field names. Use `[GraphQLName("explicitName")]` if a different GraphQL field name is desired or to maintain consistency.
- **Type Extensions:** Extend the root `Query`, `Mutation`, or `Subscription` types by creating classes that use attributes like `[ExtendObjectType(typeof(Query))]`.
- **Registration:** Ensure new Query/Mutation/Subscription type extensions (and any related services or types) are correctly registered in `Program.cs`. For example: `.AddGraphQLServer().AddQueryType<Query>().AddMutationType<Mutation>().AddTypeExtension<MyNewQueryExtensions>()`.
- **DTOs:** Use Data Transfer Objects (DTOs) for GraphQL inputs and outputs where appropriate to decouple from database entities.
- **Error Handling:** Return appropriate GraphQL errors from resolvers. Handle exceptions gracefully and avoid leaking sensitive information.
- **Authorization/Authentication:** (User to specify details if/when applicable). Be mindful of security.

### GraphQL entities

GraphQL entities are records.
The GraphQL entities typically take one argument, the API model or database DTO model.  
The GraphQL entity is then responsible for mapping the API model to the database model. This allows for a clean separation of concerns and makes it easier to maintain the codebase.
The incoming model is never exposed in the API, always add GraphQLIgnore.

Here is an example:
```csharp
public record NameCredit([property: GraphQLIgnore] MbNameCredit Model)
{
    public string Name => Model.Name;
    public Artist.Artist Artist => new(Model.Artist);
    public string? JoinPhrase => Model.JoinPhrase;
}
```

### GraphQL search roots

The search roots are the entry points for searching for data in the GraphQL API.
They use the services to search for data and return the results.

Here is an example:
```csharp
public record ArtistSearchRoot
{
    public async Task<IEnumerable<Artist>> SearchByName(
        [Service] MusicBrainzService mbService,
        string name,
        int limit = 25,
        int offset = 0
    )
    {
        var artists = await mbService.SearchArtistByNameAsync(name, limit, offset);
        return artists.Select(a => new Artist(a));
    }

    public async Task<Artist?> ById([Service] MusicBrainzService mbService, [ID] string id)
    {
        var artist = await mbService.GetArtistByIdAsync(id);
        return artist != null ? new Artist(artist) : null;
    }
}
```


## 4. Database and Entity Framework Core

### Migrations

This application is still in early development, so while we are using EF Core migrations, we are not yet using them in production.
You can freely change the database models and create migrations. You can also delete all the migrations if needed.

### Event sourcing

#### Events

The internal data in the application (such as users, playlists, liked songs, etc) is event sourced.
You can add more events, just make sure they extend the abstract class `Event`.
See `MusicGQL/Db/Postgres/Models/Events/`.

#### Projections

Read models (projections) are used for querying. Ensure these are updated if underlying events or data structures change.

## 5. Backend Development Workflow

- **Building:** `dotnet build` from the `MusicGQL` directory.
- **Running:** `dotnet run` from the `MusicGQL` directory. The server typically runs on `http://localhost:5095`.
- **Testing:** (User to specify backend testing strategy, frameworks like xUnit or NUnit, and where tests are located). Strive to write testable code, potentially using an in-memory database for testing EF Core interactions.
- **Debugging:** Utilize debugging tools in your IDE.

## 6. Communication with User

- **Clarification:** Backend changes can have wide-ranging effects. If a request is unclear or its implications are not fully understood, ask for clarification.
- **Impact Assessment:** Briefly mention potential impacts of proposed changes (e.g., "This will require a database migration").
- **Summaries:** Provide concise summaries of backend changes made.

This is a starting point. Please feel free to update and expand these guidelines as the project evolves! 