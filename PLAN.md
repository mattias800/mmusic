Here’s a concrete, incremental plan to add first-class multi-disc support across the stack while preserving full backward compatibility and keeping the UI unchanged for single-disc albums.

Scope and guiding principles
- [ ] Backward compatible JSON: existing release.json with a flat tracks[] continues to work unchanged.
- [ ] Forward-compatible JSON: when multi-disc is present, discs[] is the source of truth; we can optionally materialize a flattened view for compatibility.
- [ ] API and UI remain identical for single-disc releases; for multi-disc, discs show as sections (no layout changes otherwise).
- [ ] Importers and downloaders become disc-aware (MusicBrainz mediums, filesystem “CD1/Disc 2/Digital Media 03”, etc.).
- [ ] Track identity and availability tracking include disc number for correctness.

Proposed data model changes
- [ ] JSON (release.json):
- [ ] Add an optional discs array: discs: [{ discNumber: number, title?: string, tracks: Track[] }]
- [ ] Optional JSON track extension: each track may have discNumber to aid flattened compatibility.
- [ ] Back-compat:
  - [ ] If discs is absent, use top-level tracks (current behavior).
  - [ ] If discs is present, treat discs as the source of truth. Optionally still include flattened tracks, with discNumber populated, for older readers.
- [ ] C# DTOs:
- [ ] JsonRelease: add List<JsonDisc>? Discs { get; set; }
- [ ] JsonDisc: int DiscNumber; string? Title; List<JsonTrack> Tracks
- [ ] JsonTrack: optional int? DiscNumber (for flat view).
- [ ] Cache:
- [ ] CachedTrack gains DiscNumber (default 1 if absent).
- [ ] Availability keys include disc number (ArtistId|Release|Disc|Track).

Backend implementation steps
- [ ] 1) Design multi-disc JSON schema + compatibility
- [ ] Add discs[] as described, with clear precedence rules: discs > tracks.
- [ ] Decide write rules:
- [ ] Single-disc write only tracks[] (unchanged).
- [ ] Multi-disc write discs[]; optionally also write flattened tracks with discNumber for compatibility.
- [ ] Migration is not required, but we’ll support an optional migration tool later.

- [ ] 2) Update C# JSON models
- [ ] Add JsonDisc and DiscNumber on JsonTrack.
- [ ] System.Text.Json attributes:
- [ ] Maintain null-tolerant handling, no breaking changes for missing fields.
- [ ] Unit tests for:
- [ ] legacy single-disc JSON
- [ ] multi-disc JSON (only discs)
- [ ] hybrid JSON (discs + tracks) to ensure precedence is respected.

- [ ] 3) ServerLibraryCache
- [ ] Parse discs if present:
- [ ] Build CachedRelease.Tracks from discs by flattening with DiscNumber preserved.
- [ ] Preserve media availability status by keys that include DiscNumber.
- [ ] Update UpdateReleaseFromJsonAsync similarly.

- [ ] 4) ServerLibraryJsonWriter
- [ ] Update UpdateReleaseAsync to support writing audio file paths under discs.
- [ ] For single-disc, behavior unchanged.
- [ ] For multi-disc, modify only the disc’s tracks.
- [ ] Update DeleteReleaseAudioMutation and any writers to iterate discs when present.

- [ ] 5) MusicBrainz import (IImportExecutor/MusicBrainzImportExecutor)
- [ ] Map MB mediums to discs:
- [ ] For each medium: discNumber = medium.position, title = medium.title, tracks = medium.tracks with correct numbering.
- [ ] Persist as discs[]; optionally include flattened tracks[] with discNumber for compatibility.
- [ ] Update track count logic to handle per-disc and totals.

- [ ] 6) Filesystem scanning/maintenance
- [ ] Detect discs from folder patterns:
- [ ] “cd N”, “disc N”, “disk N”, “digital media N”, “digital N”, “dvd N” (for audio-only mediums)
- [ ] Roman numerals (I, II, III) as fallback when obvious
- [ ] Parse track numbers in common patterns:
- [ ] Leading “NN …”
- [ ] Embedded “- NN -” segment (e.g., “Artist - Album - 01 - Title”)
- [ ] Set DiscNumber on tracks accordingly.

- [ ] 7) Download pipeline (SoulSeek + Prowlarr/SAB)
- [ ] DownloadQueueFactory:
- [ ] Disc detection: same folder patterns as above and “Digital Media N”.
- [ ] Track-number parsing: support embedded “- NN -”.
- [ ] Group per-disc; build queue(s) that match allowed totals (e.g., 26=two discs).
- [ ] Avoid assembling partial discs if strict allowed counts exist; allow relaxed mode when we have no authoritative counts.
- [ ] Prefer sources that offer a consistent disc set (1+2) over mixed or extras discs.
- [ ] SoulSeekReleaseDownloader:
- [ ] Use disc-aware queue from factory.
- [ ] After download, write release.json as discs[] when there are multiple discs detected.
- [ ] SAB watcher/finalizer:
- [ ] When moving/updating JSON, identify disc and assign file path in the corresponding disc’s track.

- [ ] 8) GraphQL schema and resolvers
- [ ] Extend Release type:
- [ ] discs: [Disc!]!
- [ ] discCount: Int!
- [ ] tracks: [Track!]! remains (flattened; single-disc identical to today)
- [ ] Extend Track:
- [ ] discNumber: Int!
- [ ] Back-compat:
- [ ] Existing queries using release.tracks keep working.
- [ ] New UI code can use release.discs when discCount > 1.
- [ ] Subscriptions:
- [ ] Track/Release updated events include discNumber where applicable; existing topics continue working.

- [ ] 9) Frontend UI
- [ ] AlbumTrackList:
- [ ] If discCount > 1, render sections per disc with optional disc title header; otherwise unchanged.
- [ ] Play buttons/queue: iterate discs in order (1..N) and tracks in each disc.
- [ ] No change to single-disc releases; all current pages remain the same visually.

- [ ] 10) Tests and fixtures
- [ ] Add fixtures:
- [ ] Sample multi-disc JSON and expected flattened/tracks view in GraphQL.
- [ ] Import tests that MB mediums produce discs correctly.
- [ ] SoulSeek parsing tests for disc detection and “- NN -” numbering.
- [ ] SAB finalizer JSON update tests disc-aware path assignment.

- [ ] 11) Docs and migration
- [ ] Document the schema and precedence rules.
- [ ] Optional script/command to reorganize an existing flat JSON with disc markers into discs[].

Rollout and compatibility
- [ ] Phase 1 (backend only): complete JSON, cache, importers, and downloaders. GraphQL returns discCount and discs (can be hidden from UI initially).
- [ ] Phase 2 (UI): enable disc grouping in AlbumTrackList when discCount > 1.
- [ ] No breaking changes for existing content. Single-disc remains identical.

