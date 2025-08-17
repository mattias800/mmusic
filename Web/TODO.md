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

* [ ] Artists page should have an "appears on" tab
* [ ] "Appears on" tab should contain one card with playlists, and one card with other artists releases that they appear
  on

## Similar artists

* [ ] Add a GraphQL node for similar artists
* [ ] Similar artists should be fetched from an external service and stored in artist.json. Add JsonSimilarArtist class.
* [ ] Similar artists should be self contained, like playlists. Any cover arts should be downloaded.
* [ ] Similar artists should be fetched when importing the artist metadata.
* [ ] If similar artist exists in local library, that reference should be stored in the JSON as well.
* [ ] Add a card "Similar artists" to artist page

## New server settings

* [ ] Add user roles, so some users can be admins
* [ ] Add roles for:
    * [ ] Allowing users to create playlists
    * [ ] Allowing users to trigger downloads
    * [ ] Allowing users to change user roles on other users
    * [ ] Allowing users to see current downloads and queues page
    * [ ] Allowing users to edit auth tokens for external services
* [ ] Add UI for adding new users
* [ ] Add UI for changing user roles
* [ ] Move all appsettings.development.json to server settings, while still allowing appsettings.development.json to
  override server settings, for local development. This should be done in ServerSettingsAccessor.
* [ ] Limit access to server settings to users with admin role. Both in UI, and in the API. Authenticate the user in
  both queries and mutations.
* [ ] Add UI for editing the new server settings from appsettings.development.json
* [ ] Connection strings should be read from a config file, and not be stored in server settings.
