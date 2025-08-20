# TODO

After each task is done, compile the backend to see that there are no errors.
If there are updates to GraphQL schema, we need to restart the backend and
run "bun types:once" while the backend is running. Ask user for help if needed.

If there are changes to the database, make sure to update migrations.

$ dotnet ef migrations add <MigrationName>
$ dotnet ef database update

This application is very much work in progress, so migrations will be deleted sometimes.

If there are no errors in backend build, type generation, frontend lint or frontend typecheck,
then the task is done.
Update the check in TODO.md.

If there are problems, ask the user for assistance.
The user will create a git commit manually.

## Search feature should search playlist names as well

* [x] Add GraphQL nodes for searching playlists names
* [x] Add a UI in search result for playlist with similar design as the existing search results.

## Artists page "appears on" tab

* [x] Artists page should have an "appears on" tab
* [x] "Appears on" tab should contain one card with playlists, and one card with other artists releases that they appear
  on

## Similar artists

* [x] Add a GraphQL node for similar artists
* [x] Similar artists should be fetched from an external service and stored in artist.json. Add JsonSimilarArtist class.
* [x] Similar artists should be self contained, like playlists. Any cover arts should be downloaded.
* [x] Similar artists should be fetched when importing the artist metadata.
* [x] If similar artist exists in local server library, that reference should be stored in the JSON as well.
* [x] Add a card "Similar artists" to artist page

## New server settings

* [x] Add user roles, so some users can be admins
* [x] Add roles for:
    * [x] Allowing users to create playlists
    * [x] Allowing users to trigger downloads
    * [x] Allowing users to change user roles on other users
    * [x] Allowing users to see current downloads and queues page
    * [x] Allowing users to edit auth tokens for external services
* [x] Add a new page for administrating users.
* [x] Add UI for adding and editing users in the new page.
* [x] Add UI for changing user roles in the new page.

## Casting

* [x] Add Public Base URL server setting for Google Cast and use it in the web player

## Migrate appsettings.development.json to server settings

* [x] Add server settings event sourcing events and project model for all settings available in
  appsettings.development.json for the following self-hosted services:
    * [x] SoulSeek
    * [x] Prowlarr
    * [x] QBittorrent
* [x] Mutations and queries in GraphQL for changing reading and writing these settings.
* [x] Add UI for editing server settings in the new page.
* [x] Add ability to test these settings in the new page, so the user can easily verify that they are working.

## Show status of auth tokens

* [x] Add a status page in server settings, that can show the status of auth tokens.
    * Tokens cannot be edited, the server admin must add these as env variables.
* [x] Add a UI for testing connectivity to these services.

* This applies to the following external services:
    * [x] ListenBrainz
    * [x] YouTube
    * [x] Spotify
    * [x] Spotify
    * [x] Lastfm
    * [x] Fanart

## Installation wizard

* When starting the server for the first time, the user is presented with a couple of steps.

* [x] Show a welcome page with a link to the documentation, Github, etc.
* [x] Show status for auth tokens for external services. This should be the same React component as in status page, so that we get the same UI and information if setup is not correct.
* [x] Inform the server admin that the connection to these external services can be tested from the status page.
* [x] Also inform that integration with download services must be configured in the server settings page.

* [x] Ask the user to create an admin account. This page is already done, just make sure it comes last in this wizard.

## Keep track of online clients

* [x] For each user, keep track of online clients. Do not store in database, just in memory.
* [x] For each client, keep track of what music it is playing. Do not store in database, just in memory.
* [ ] Add ability to set a name for the client. For example, "Chrome on Windows". Or "iPhone app". The names should have
  sane defaults, and setting them should be optional. Store this in some way that is local to the client and session.
* [x] Add an admin UI for showing clients per user, and showing online status, and what music each client is playing.

## Trigger playback of track from backend

* [ ] Backend should be able to trigger playback of a track on a client.
    * I am not sure how this should be implemented. Maybe GraphQL subscription?
* [ ] The user should be able to allow or disallow triggering playback from the backend per client.
    * The user should not have to interact with the client for playback to start, if backend triggers playback and it is
      allowed by setting.

## MCP server

* [ ] Create MCP server functionality, so that AI agents can play music using mmusic.
* [ ] Agents should be able to search artists, albums, tracks and playlists by name.
* [ ] Agents should be able to fetch all artists.
* [ ] Agents should be able to fetch all releases (albums, eps, singles) for an artists.
* [ ] Agents should be able to fetch all tracks in a release.
* [ ] Agents should be able to list all connected clients.
* [ ] Agents should be able to start playback on any connected client that allows it.
* [ ] Agents should be able to trigger downloads of all releases for an artist, all releases of a type (album, ep,
  single) for an artist, or a particular release for artist.
* [ ] Agents should be able to import new artists to the server library.
* [ ] Agents should be able to create new playlists, and give them names.
* [ ] Agents should be able to add tracks to playlists. Any track the user mentions, or currently playing track.
* [ ] Agents should be able to like a track on the users behalf.
* [ ] Agents should be able to push tracks to the playback queue.
* [ ] Agents should be able to check the currently playing track on any client.
* [ ] Agents should be able to get top tracks on an artist.
* [ ] Add a UI for the MCP server. Add anything that might be useful to the user.
* [ ] Add logging of MCP calls
* [ ] Add information to the user on how to use the MCP server.
