# AI Agent Development Guidelines for MusicGQL (Backend)

This document provides guidelines for AI agents assisting with the development of the MusicGQL backend.

## 1. Understanding the Backend

- **Familiarize Yourself:** Before making changes, try to understand the relevant parts of the project structure,
  focusing on the C# backend (ASP.NET Core, HotChocolate for GraphQL, Entity Framework Core).
- **Key Areas:** Pay attention to `Program.cs` for service and GraphQL registrations, `Features` for business logic,
  `Db/Postgres` for database models and event sourcing, and `Integration` for external service communications.

## 2. Code Style and Conventions (C# & ASP.NET Core)

- **Naming Conventions:** Follow standard C# naming conventions (e.g., PascalCase for classes, methods, properties).
- **Async/Await:** Use `async` and `await` appropriately for all I/O-bound operations.
- **Dependency Injection:** Utilize ASP.NET Core's built-in dependency injection. Register services in `Program.cs`.
- **LINQ:** Use LINQ for data manipulation where appropriate, ensuring queries are efficient.
- **File Organization:** Keep code organized within the `Features` directory structure.
- **Comments:** Comment on non-obvious logic or complex sections of code.

### Comments

Use comments sparingly and only when necessary.
Any comment should be in English, and they should explain things that are not obvious from the code itself.
There should never be comments on using statements.

### Folder structure

This application uses vertical slice architecture.
Features are logically organized in the `Features` folder.
Each feature has its own folder, which contains all the files related to that feature.

The feature folder can contain the following files:

* GraphQL entities, for example `Artist.cs`
* GraphQL search roots, for example `ArtistSearchRoot.cs`

The feature folder can also contain subfolders:

* `Commands` for commands, for example `LikeSongCommand.cs`
* `Mutations` for mutations, for example `LikeSongMutation.cs`
* `Sagas` for sagas, for example `LikeSongSaga.cs`
* `Aggregates` for read model projections, based on event sourcing. For example `LikeSongAggregate.cs`
* `Services` for services, for example `SpotifyService.cs`
* `Mappers` for mappers, for example `UserMapper.cs`

The feature folder can also contain more features, for logical grouping of features.
For example the `Playlist` feature contains the `Spotify` feature, which contains the `SpotifyPlaylist`

### Commands

Commands are the main entry point for writes into the application.
Mutations should always use commands to perform the actual work.
This is only relevent for data that is owned by the application, for example users, playlists, liked songs, etc.
This is not relevant for imported data, such as MusicBrainz, Spotify, etc.

### Services

Services are the main entry point for reads from external services, such as Spotify and MusicBrainz.
They are used to fetch data from external services and return it to the application.
These services often use HybridCache to cache the data, since these external services often have strict
rate limits.

## 3. GraphQL Development (HotChocolate)

- **Code-First Approach:** The project uses a code-first approach. GraphQL types are generated from C# classes.
- **Naming:** HotChocolate typically converts C# method names to camelCase for GraphQL field names. Use
  `[GraphQLName("explicitName")]` if a different GraphQL field name is desired or to maintain consistency.
- **Type Extensions:** Extend the root `Query`, `Mutation`, or `Subscription` types by creating classes that use
  attributes like `[ExtendObjectType(typeof(Query))]`.
- **Registration:** Ensure new Query/Mutation/Subscription type extensions (and any related services or types) are
  correctly registered in `Program.cs`. For example:
  `.AddGraphQLServer().AddQueryType<Query>().AddMutationType<Mutation>().AddTypeExtension<MyNewQueryExtensions>()`.
- **DTOs:** Use Data Transfer Objects (DTOs) for GraphQL inputs and outputs where appropriate to decouple from database
  entities.
- **Error Handling:** Return appropriate GraphQL errors from resolvers. Handle exceptions gracefully and avoid leaking
  sensitive information.
- **Authorization/Authentication:** (User to specify details if/when applicable). Be mindful of security.

### GraphQL mutations

GraphQL mutations are used to modify data. They always return a result type that indicates success or failure.
It should have an input type that represents the input to the operation.
The result type should be a union type that can represent different outcomes.
It should be modeled with an abstract class and concrete classes for each possible outcome.
Both input and results should be named after the mutation.

Here is an example for the LikeSongMutation class.

```csharp
public record LikeSongInput(string RecordingId);

[UnionType("LikeSongResult")]
public abstract record LikeSongResult
{
    public record LikeSongSuccess(User.User Viewer) : LikeSongResult;

    public record LikeSongAlreadyLiked(string Message) : LikeSongResult;

    public record LikeSongSongDoesNotExist(string Message) : LikeSongResult;
}
```

There should only be one mutation per file and class.
Create one class per mutation and name the class after the operation that is being made.
All mutation classes should use the `[ExtendObjectType(typeof(Mutation))]` annotation.

### GraphQL entities

GraphQL entities are records.
The GraphQL entities typically take one argument, the API model or database DTO model.  
The GraphQL entity is then responsible for mapping the API model to the database model. 
This allows for a clean separation of concerns and makes it easier to maintain the codebase.
The incoming model is never exposed in the API, always add GraphQLIgnore.
We should never expose projection models in the GraphQL API, always use GraphQL entities.

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

All search roots should be added to the `Query` class as a field. They should not use the `ExtendObjectType` annotation.

## Code quality

Always prefer primary constructors over normal constructor method.
Always use records for data models.

## 4. Database and Entity Framework Core

### Migrations

This application is still in early development, so while we are using EF Core migrations, we are not yet using them in
production.
You can freely change the database models and create migrations. You can also delete all the migrations if needed.

### Event sourcing

#### Events

The internal data in the application (such as users, playlists, liked songs, etc) is event sourced.
You can add more events, just make sure they extend the abstract class `Event`.
See `MusicGQL/Db/Postgres/Models/Events/`.

#### Projections

Read models (projections) are used for querying. Ensure these are updated if underlying events or data structures
change.

## 5. Backend Development Workflow

- **Building:** `dotnet build` from the `MusicGQL` directory.
- **Running:** `dotnet run` from the `MusicGQL` directory. The server typically runs on `http://localhost:5095`.
- **Testing:** (User to specify backend testing strategy, frameworks like xUnit or NUnit, and where tests are located).
  Strive to write testable code, potentially using an in-memory database for testing EF Core interactions.
- **Debugging:** Utilize debugging tools in your IDE.

## 6. Communication with User

- **Clarification:** Backend changes can have wide-ranging effects. If a request is unclear or its implications are not
  fully understood, ask for clarification.
- **Impact Assessment:** Briefly mention potential impacts of proposed changes (e.g., "This will require a database
  migration").
- **Summaries:** Provide concise summaries of backend changes made.

This is a starting point. Please feel free to update and expand these guidelines as the project evolves! 