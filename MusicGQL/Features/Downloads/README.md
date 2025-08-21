Downloads Logging
=================

Overview
--------
Per-release logs are written to help diagnose mismatches during search, provider handoff, and finalization.

Configuration
-------------
- Set `LogsFolderPath` in Server Settings via GraphQL:
  - Mutation: `updateLogsFolderPath(input: { newPath: "/path/to/Logs" })`
  - When set, logs are written under: `<LogsFolderPath>/Downloads/<Artist>/<Release>.log`
- If `LogsFolderPath` is empty or not set, file logging is disabled (no log files are created). Application console logging remains unaffected.

What gets logged
----------------
- SoulSeek: normalized query, expected/min track counts, year, candidate user attempts, and decisions.
- Prowlarr: result counts, chosen handoff (SAB/qBittorrent), validation outcomes, and errors.
- SABnzbd finalize: source/target paths, discovered files, move operations, and exceptions.

File naming & safety
--------------------
- Artist and release names are sanitized for file paths.
- Example: `/Logs/Downloads/Korn/Significant Other.log`.

Notes
-----
- Logs are appended; delete files to reset history.
- Future providers (e.g., qBittorrent finalize) can reuse the same `DownloadLogPathProvider` and `DownloadLogger`.


