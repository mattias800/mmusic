## Bugs

## Missing manifest

When installing server, no library path has been set.
When setting the library path in server settings,
if there is already a manifest in the selected folder,
the UI should report everything is OK.
But it doesn't, it says the manifest file is missing.

## Refreshing all artist releases

When refreshing all artist releases, every release is pushed
to import queue separately. This makes the progress bar show 0/1 for every release.

Instead, the artist should be pushed to the queue, so the progress bar shows correct progress.

## Importing a playlist

When importing a playlist, it should appear in realtime in the playlist list.
The should be implemented with subscriptions, there can be no query refreshes.
