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

## Migrate appsettings.development.json to server settings

* [ ] Add server settings event sourcing events and project model for all settings available in
  appsettings.development.json for the following self-hosted services:
    * [ ] SoulSeek
    * [ ] Prowlarr
    * [ ] QBittorrent
* [ ] Mutations and queries in GraphQL for changing reading and writing these settings.
* [ ] Add UI for editing server settings in the new page.
* [ ] Add ability to test these settings in the new page, so the user can easily verify that they are working.

## Show status of auth tokens

* [ ] Add a status page in server settings, that can show the status of auth tokens.
    * Tokens cannot be edited, the server admin must add these as env variables.
* [ ] Add a UI for testing connectivity to these services.

* This applies to the following external services:
    * [ ] ListenBrainz
    * [ ] YouTube
    * [ ] Spotify
    * [ ] Spotify
    * [ ] Lastfm
    * [ ] Fanart

## Installation wizard

* When starting the server for the first time, the user is presented with a couple of steps.

1. Show a welcome page with a link to the documentation, Github, etc.
2. Show status for auth tokens for external services.

* If token exists, show a check mark.
* For each service, show a box with some information about what the service is used for.
* UI should be as beautiful as the rest of the app.
* Inform the server admin that the connection to these external services can be tested from the status page.
* Also inform that integration with download services must be configured in the server settings page.

3. Ask the user to create an admin account. This page is already done, just make sure it comes last in this wizard.
