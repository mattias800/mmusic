# MMusic Desktop App - Development Progress

**Last Updated**: 2025-10-28
**Status**: Foundation Phase - Apollo GraphQL Integration In Progress

---

## What We've Accomplished

### ✅ Phase 1: Foundation & Infrastructure (IN PROGRESS)

#### 1.1 GraphQL Client Setup - PARTIALLY COMPLETE
- [x] Added Apollo Kotlin plugin to root build.gradle.kts (v4.1.0)
- [x] Configured Apollo plugin in shared module
- [x] Copied GraphQL schema from Web project
- [x] Created initial GraphQL queries:
  - Bootstrap.graphql (user authentication check)
  - Artists.graphql (artist list and detail)
  - Albums.graphql (album list and detail - needs fixes)
- [x] Created ApolloGraphQLClient wrapper class
- [x] Successfully generated GraphQL types for Bootstrap query
- [ ] **BLOCKED**: Need to fix field names in Artists/Albums queries to match actual schema
- [ ] **TODO**: Complete schema validation and fix all queries
- [ ] **TODO**: Add remaining queries (Search, Playlists, Player)
- [ ] **TODO**: Configure normalized cache
- [ ] **TODO**: Set up subscription support over WebSocket

#### 1.2-1.4 Design System, UI Components, Image Loading - NOT STARTED
- [ ] Define color palette and theme
- [ ] Create core UI components
- [ ] Implement image loading

###Schema Field Mapping Issues Found

The GraphQL schema uses different field names than expected from the Web app:

**Release Type**:
- ✅ Uses: `title` (not `name`)
- ✅ Uses: `type` (not `releaseType`)
- ✅ Uses: `coverArtUrl` (not `images` array)
- ✅ Uses: `firstReleaseDate` (not `releaseDate`)
- ✅ Uses: `isFullyMissing` (not `hasAudio`)
- ✅ Uses: `folderName`

**Artist Type**:
- ✅ Uses: `listeners` (not `spotifyMonthlyListeners`)
- ✅ Uses: `images` object with fields: `backgrounds`, `banners`, `logos`, `thumbs`
- ✅ Uses: `allArtists` method (not `artists`)
- ✅ Uses: `releaseByFolderName()` method (not `release()`)

**ServerLibrarySearchRoot**:
- ✅ Uses: `allArtists` (not `artists`)
- ✅ Uses: `allReleases` (not `albums`)
- ✅ Has: `searchArtists()`, `searchReleases()`, `searchTracks()` for search
- ✅ Has: `artistById()`, `trackById()` for lookups

**Track Type** - NEEDS INVESTIGATION:
- ❌ Current query uses: `lengthMs` - may not exist
- ❌ Current query uses: `hasAudio` - may not exist
- **TODO**: Check actual Track type fields in schema

### 📂 Project Structure Created

```
App/
├── build.gradle.kts (Apollo plugin added)
├── settings.gradle.kts
├── IMPLEMENTATION_PLAN.md (200+ tasks defined)
├── PROGRESS.md (this file)
├── shared/
│   ├── build.gradle.kts (Apollo configured)
│   └── src/commonMain/
│       ├── graphql/
│       │   ├── schema.graphqls (2093 lines)
│       │   ├── Bootstrap.graphql
│       │   ├── Artists.graphql (needs fixes)
│       │   └── Albums.graphql (needs fixes)
│       └── kotlin/com/mmusic/shared/
│           └── services/
│               ├── ApolloGraphQLClient.kt (new, basic implementation)
│               ├── GraphQLClient.kt (old Ktor implementation)
│               ├── GraphQLModels.kt (old models)
│               └── StorageService.kt (JWT storage)
├── desktopApp/ (existing auth screens working)
└── androidApp/ (scaffolded, not configured)
```

---

## Current Blockers

### 1. GraphQL Schema Validation
**Issue**: Query field names don't match the actual GraphQL schema
**Impact**: Cannot generate Apollo types until queries are fixed
**Next Steps**:
1. Read Track type definition from schema (line ~400-500?)
2. Update queries with correct field names
3. Test Apollo code generation
4. Iterate until all queries generate successfully

### 2. Missing Track Type Fields
**Issue**: Don't know what fields Track actually has
**Commands to investigate**:
```bash
grep -A 30 "type Track " shared/src/commonMain/graphql/schema.graphqls
```

---

## Next Immediate Steps (Priority Order)

### HIGH PRIORITY - Complete Apollo Setup
1. **Fix Track type fields** (30 min)
   - Find Track type definition in schema
   - Update Albums.graphql and Artists.graphql queries
   - Remove or rename: `lengthMs`, `hasAudio`
   - Use correct field names

2. **Generate all GraphQL types** (15 min)
   ```bash
   ./gradlew :shared:generateApolloSources
   ```

3. **Update ApolloGraphQLClient** (1 hour)
   - Add convenience methods for all queries
   - Configure HTTP headers properly
   - Add error handling
   - Test with real backend

4. **Replace old GraphQLClient** (30 min)
   - Update desktopApp to use ApolloGraphQLClient
   - Remove old Ktor-based GraphQLClient
   - Remove old GraphQLModels.kt
   - Test authentication flow still works

### MEDIUM PRIORITY - Start Building UI
5. **Create design system basics** (2 hours)
   - Define `AppTheme` with dark colors
   - Create `AppColors` object
   - Create `AppTypography` object
   - Add gradient utilities

6. **Build navigation structure** (3 hours)
   - Add Compose Navigation dependency
   - Define Routes sealed class
   - Create NavHost
   - Create Sidebar component
   - Create AppScaffold layout

7. **Create first feature: Artist List** (4 hours)
   - Create ArtistListPage composable
   - Create ArtistCard component
   - Wire up GetArtists query
   - Display grid of artists
   - Add loading/error states

---

## Technology Decisions Made

### Core Stack
- **UI Framework**: Compose Multiplatform (Desktop)
- **GraphQL Client**: Apollo Kotlin 4.1.0
- **HTTP Client**: Ktor (via Apollo)
- **State Management**: StateFlow/ViewModel (TBD)
- **Navigation**: Compose Navigation (TBD)
- **Image Loading**: Coil/Kamel (TBD)
- **Audio Playback**: JVM media library (TBD - needs research)

### Build Configuration
- **Kotlin**: 2.1.0
- **Gradle**: 8.11
- **Java Target**: 17 (OpenJDK 17)
- **Android SDK**: Not configured (desktop-only focus)

---

## Lessons Learned

### GraphQL Schema Exploration
- The backend schema is different from what the Web app queries suggest
- Need to carefully validate each field against the actual schema
- Apollo is strict about type checking (good for catching errors!)

### Scope Management
- Original plan: 12 phases, 200+ tasks, 3-5 months of work
- Reality: Need to build incrementally, one feature at a time
- Approach: Get MVP working first, then iterate

### Development Workflow
1. Define GraphQL queries carefully (validate against schema)
2. Generate types with Apollo
3. Build UI components
4. Wire up data fetching
5. Test with real backend
6. Iterate and refine

---

## Resources & References

### Documentation
- **Apollo Kotlin Docs**: https://www.apollographql.com/docs/kotlin/
- **Compose Multiplatform Docs**: https://www.jetbrains.com/lp/compose-multiplatform/
- **Kotlin Docs**: https://kotlinlang.org/docs/home.html

### Backend Integration
- **GraphQL Schema**: `shared/src/commonMain/graphql/schema.graphqls`
- **Backend URL**: `http://localhost:5095/graphql`
- **Web App Reference**: `../Web/` (for query examples)

### Project Plans
- **Full Implementation Plan**: `IMPLEMENTATION_PLAN.md`
- **Current Progress**: This file (`PROGRESS.md`)

---

## How to Continue Development

### Immediate Next Session
1. Run this command to find Track fields:
   ```bash
   grep -A 30 "type Track " shared/src/commonMain/graphql/schema.graphqls
   ```

2. Fix all GraphQL queries to match schema

3. Generate Apollo types successfully:
   ```bash
   export JAVA_HOME=/opt/homebrew/opt/openjdk@17/libexec/openjdk.jdk/Contents/Home
   ./gradlew :shared:generateApolloSources
   ```

4. Create a simple test page to verify GraphQL queries work

### After Apollo is Working
5. Start implementing Phase 2 (Navigation) from IMPLEMENTATION_PLAN.md
6. Build one complete feature flow (e.g., browse artists → view artist details)
7. Add music player functionality
8. Iterate and add more features

### Long-term Approach
- Work through phases in order from IMPLEMENTATION_PLAN.md
- Check off tasks as completed
- Update this PROGRESS.md file after each session
- Commit code frequently with descriptive messages

---

## Estimated Remaining Effort

Based on the implementation plan:

- **Phase 1 Completion**: 4-6 hours
- **Phases 2-3 (Navigation + Browse)**: 2-3 weeks
- **Phases 4-5 (Details + Search)**: 1-2 weeks
- **Phases 6-7 (Music Player)**: 2-3 weeks
- **Phase 8 (Playlists)**: 1-2 weeks
- **Phases 9-10 (Polish + Advanced)**: 2-3 weeks
- **Phases 11-12 (Testing + Docs)**: 1-2 weeks

**Total**: 10-16 weeks (2.5-4 months) for full implementation

---

## Success Metrics

### Phase 1 Complete When:
- [ ] All GraphQL queries generate types successfully
- [ ] ApolloGraphQLClient tested and working
- [ ] Can fetch artists/albums from backend
- [ ] Authentication still works with new client

### MVP Complete When:
- [ ] Can browse list of artists
- [ ] Can view artist details
- [ ] Can browse list of albums
- [ ] Can view album details with track list
- [ ] Can play a single track
- [ ] Basic navigation works
- [ ] App looks decent (dark theme, proper layout)

### V1.0 Complete When:
- [ ] All features from IMPLEMENTATION_PLAN.md implemented
- [ ] Music player with queue, volume, seeking works perfectly
- [ ] Search works across artists/albums/tracks
- [ ] Playlists can be created and managed
- [ ] Real-time updates from backend working
- [ ] App is polished and bug-free
- [ ] Documentation complete

---

**Next Developer**: Start with the "Next Immediate Steps" section above. Good luck! 🚀
