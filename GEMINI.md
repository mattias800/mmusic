
# Gemini Code Assistant Documentation

This document provides a comprehensive overview of the mmusic project, designed to assist the Gemini code assistant in understanding the project's structure, architecture, and functionality.

## Project Structure

The mmusic project is a monorepo containing three main projects:

- **MusicGQL:** A .NET/C# GraphQL API that serves as the backend for the application.
- **Web:** A React/TypeScript single-page application that provides the user interface.
- **Database:** A .NET project for managing the PostgreSQL database schema.

## Backend (MusicGQL)

The `MusicGQL` project is a .NET 8 application built with ASP.NET Core and the Hot Chocolate GraphQL server. It follows a CQRS (Command Query Responsibility Segregation) and event-sourcing architecture.

### Technology Stack

- **Framework:** .NET 8, ASP.NET Core
- **GraphQL:** Hot Chocolate
- **Event Sourcing/Service Bus:** Rebus
- **Databases:**
    - **PostgreSQL:** For event sourcing and application data.
    - **Neo4j:** For graph-based data, modeling relationships between artists, releases, and other music entities.
    - **Redis:** For caching and real-time GraphQL subscriptions.
- **External Integrations:**
    - **MusicBrainz:** For comprehensive music metadata.
    - **Last.fm:** For artist and track recommendations and statistics.
    - **Spotify:** For playlist import and music data.
    - **Soulseek:** For downloading music releases.
    - **YouTube:** For finding music videos and audio streams.

### Architecture

The backend is designed around the principles of CQRS and event sourcing.

- **CQRS:** The application separates commands (write operations) from queries (read operations). Commands are handled by dedicated handlers, and queries are resolved through the GraphQL API.
- **Event Sourcing:** State changes are captured as a sequence of events, which are stored in the PostgreSQL database using Rebus. These events are then processed by event handlers to update read models and trigger side effects.

### Features

- **User Authentication:** User registration and sign-in with password hashing.
- **Music Library Management:**
    - Add artists and release groups to the server library.
    - Track the import status of artists and releases.
    - Real-time updates on the status of the music library.
- **Playlist Management:**
    - Create, rename, and delete playlists.
    - Import playlists from Spotify.
- **Music Discovery:**
    - Search for artists, releases, and recordings on MusicBrainz.
    - Get recommendations from Last.fm (top artists, tracks, and tags).
- **Downloading:**
    - Download music releases from Soulseek.
    - Track the status of downloads in real-time.
- **Liked Songs:**
    - Users can like and unlike songs.

### CQRS Handlers

This application follows the CQRS pattern, where commands can be issued.
These events can invoke code that causes side-effects, and it can store events in the database.

Handlers are located in the `Features` directory, organized by feature. 
For example, the `Features/Users/Handlers` directory contains handlers for creating users and managing user-related events.

Rebus is not used for triggering commands.

## Frontend (Web)

The `Web` project is a modern React application built with TypeScript, Vite, and Redux.

### Technology Stack

- **Framework:** React 19
- **Language:** TypeScript
- **Build Tool:** Vite
- **State Management:** Redux Toolkit
- **GraphQL Client:** Urql with GraphCache for normalized caching.
- **Styling:** Tailwind CSS with Radix UI components.
- **Testing:** Vitest and React Testing Library.

### Features

- **User Interface:** A responsive and interactive UI for browsing the music library, managing playlists, and monitoring downloads.
- **Real-time Updates:** Leverages GraphQL subscriptions to provide real-time updates for features like download status and library import progress.
- **GraphQL Code Generation:** Uses `graphql-codegen` to generate TypeScript types from the GraphQL schema, ensuring type safety between the frontend and backend.

## GraphQL API

The GraphQL API is the primary interface between the frontend and backend. The schema is defined in the `Web/schema.graphql` file and is used to generate types for both the client and server.

### Queries

- **`artist`:** Search for artists and retrieve artist details.
- **`release`:** Search for releases and retrieve release details.
- **`recording`:** Search for recordings and retrieve recording details.
- **`playlist`:** Manage and view playlists.
- **`viewer`:** Get information about the currently authenticated user.
- **`recommendations`:** Get music recommendations from Last.fm.
- **`musicBrainz`:** Search the MusicBrainz database.

### Mutations

- **`createUser`:** Register a new user.
- **`signIn` / `signOut`:** Authenticate and de-authenticate users.
- **`addArtistToServerLibrary` / `addReleaseGroupToServerLibrary`:** Add music to the library.
- **`createPlaylist` / `renamePlaylist` / `deletePlaylist`:** Manage playlists.
- **`likeSong` / `unlikeSong`:** Like and unlike songs.
- **`startDownloadRelease`:** Initiate a download of a music release.

### Subscriptions

- **`artistServerStatusUpdated`:** Get real-time updates on the import status of an artist.
- **`downloadStatusUpdated`:** Get real-time updates on the status of a download.
- **`soulSeekStatusUpdated`:** Monitor the connection status of Soulseek.

## Databases

The application uses a polyglot persistence approach, leveraging multiple databases for different purposes.

- **PostgreSQL:** Managed by the `Database` project using Entity Framework Core migrations. It stores:
    - **Event Store:** All events generated by the application via Rebus.
    - **Application Data:** Read models and other application-specific data.
- **Neo4j:** A graph database used to model the complex relationships between artists, releases, recordings, and other music entities. This allows for powerful and efficient queries on the music graph.
- **Redis:** An in-memory data store used for:
    - **Caching:** Caching frequently accessed data to improve performance.
    - **GraphQL Subscriptions:** Handling real-time updates for GraphQL subscriptions.

## Event Sourcing

Event sourcing is a core architectural pattern in the `MusicGQL` backend, implemented using the Rebus service bus.

- **Events:** Domain events are defined in the `Features` directory (e.g., `UserCreated`, `LikedSong`, `AddArtistToServerLibrary`). These events represent state changes in the application.
- **Event Handlers:** Event processors (e.g., `UserEventProcessor`, `LikedSongsEventProcessor`) consume these events to update read models, trigger notifications, and perform other side effects.
- **Sagas:** Long-running processes, such as downloading a release, are managed using Rebus sagas. The `DownloadReleaseSaga` coordinates the steps involved in looking up a release on MusicBrainz, finding it on Soulseek, and downloading the tracks.
