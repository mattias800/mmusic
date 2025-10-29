# MMusic Desktop App - Implementation Plan

This document outlines the complete implementation plan for recreating the MMusic Web application as a Kotlin Multiplatform desktop application using Compose Multiplatform.

## Project Overview

**Goal**: Create a beautiful, fully-featured desktop music player application that connects to the MMusic GraphQL backend, providing a Spotify-like experience for self-hosted music.

**Technology Stack**:
- Kotlin Multiplatform with Compose Desktop
- Apollo Kotlin for GraphQL (queries, mutations, subscriptions)
- Ktor for HTTP client
- Compose Navigation for routing
- Media player library for audio playback
- Material 3 design system with custom theming

---

## Phase 1: Foundation & Infrastructure

### 1.1 GraphQL Client Setup

- [ ] Add Apollo Kotlin dependencies to shared module
- [ ] Generate GraphQL schema file from backend
- [ ] Configure Apollo Kotlin codegen
- [ ] Create GraphQL client with authentication (JWT Bearer token)
- [ ] Set up normalized cache configuration
- [ ] Test basic queries (Bootstrap, Artists, Albums)
- [ ] Implement subscription support over WebSocket
- [ ] Create error handling patterns for GraphQL operations

### 1.2 Design System & Theme

- [ ] Define color palette (dark theme with gradients)
  - Background colors (gray-900, gray-800, gray-950)
  - Accent colors (blue, purple, green gradients)
  - Text colors (white primary, gray-400 muted)
  - Border colors (white with 10-20% opacity)
- [ ] Create custom Material 3 theme configuration
- [ ] Implement glass morphism utilities (backdrop blur, semi-transparent backgrounds)
- [ ] Define typography scale (display, heading, body, label sizes)
- [ ] Create spacing system (4dp, 8dp, 16dp, 24dp, 32dp)
- [ ] Implement gradient utilities for backgrounds and accents
- [ ] Create icon system using Material Icons or similar

### 1.3 Core UI Components

- [ ] **LoadingSpinner** - Circular progress indicator with multiple sizes
- [ ] **ErrorDisplay** - Error message with icon and retry action
- [ ] **EmptyState** - Empty state with icon, title, and description
- [ ] **GlassCard** - Card with glass morphism effect
- [ ] **PrimaryButton** - Solid button with hover/press states
- [ ] **GhostButton** - Transparent button with hover effect
- [ ] **IconButton** - Square button with icon only
- [ ] **TextField** - Input field with label and error state
- [ ] **SearchInput** - Search field with debouncing and clear button
- [ ] **Badge** - Small label with background color
- [ ] **Tooltip** - Hover tooltip component
- [ ] **ContextMenu** - Right-click menu component
- [ ] **Dialog** - Modal dialog with backdrop
- [ ] **Sheet** - Side panel (drawer) component
- [ ] **ScrollArea** - Custom scrollable container with styled scrollbar

### 1.4 Image Loading & Caching

- [ ] Implement async image loading with Coil or Kamel
- [ ] Add placeholder and error states for images
- [ ] Implement image caching strategy
- [ ] Create **CoverArtImage** component with rounded corners and fade-in
- [ ] Create **ArtistBackgroundImage** component with gradient overlay
- [ ] Add support for various image sizes (thumbnail, medium, large)

---

## Phase 2: Navigation & Layout

### 2.1 Navigation Setup

- [ ] Add Compose Navigation dependencies
- [ ] Define navigation routes as sealed class/enum
  - Home/Dashboard route
  - Artists list route
  - Artist detail route (with artistId parameter)
  - Albums list route
  - Album detail route (with artistId and releaseFolderName)
  - Search results route (with query parameter)
  - Playlists list route
  - Playlist detail route (with playlistId)
  - Liked songs route
  - Settings route
- [ ] Implement NavHost with route handling
- [ ] Create navigation state management
- [ ] Implement deep linking support for routes
- [ ] Add back navigation support with history stack

### 2.2 Main Layout Structure

- [ ] **AppScaffold** - Main layout container
  - Fixed sidebar on left (256dp width)
  - Search bar at top (fixed, z-index 10)
  - Main content area with proper padding
  - Bottom padding for player when visible
- [ ] **Sidebar** component
  - Gradient background (gray-900 → gray-800 → gray-900)
  - Logo/app name at top
  - Navigation sections with labels
  - Scroll support for long lists
  - Active route highlighting
- [ ] **SearchBar** - Fixed search bar with glass morphism
  - Floating above content with backdrop blur
  - Search icon on left
  - Clear button on right when text present
  - Dynamic contrast for readability
- [ ] Implement responsive layout adjustments for window size changes

### 2.3 Sidebar Navigation Items

- [ ] **Discover Section**
  - Popular Artists navigation item
  - Recommended navigation item (if implemented)
- [ ] **Library Section**
  - Artists navigation item with count badge
  - Albums navigation item with count badge
  - Tracks navigation item (optional)
- [ ] **Playlists Section**
  - List of user playlists (scrollable)
  - "Create Playlist" button
  - Playlists count badge
- [ ] **User Section** (at bottom)
  - User profile link with avatar
  - Settings link
  - Sign out button

---

## Phase 3: Core Features - Browse & Discovery

### 3.1 Artist List Page

- [ ] Create **ArtistListPage** composable
- [ ] Implement GraphQL query for artist list with pagination
- [ ] Create **ArtistCard** component
  - Circular or rounded square image (200dp x 200dp)
  - Artist name (bold, large)
  - Listener count (if available)
  - Verification badge (if applicable)
  - Hover effect with scale animation
- [ ] Implement grid layout (adaptive columns based on window width)
- [ ] Add loading state with skeleton cards
- [ ] Add error state with retry button
- [ ] Add empty state for no artists
- [ ] Implement scroll restoration on navigation back
- [ ] Add "Refresh" button in page header
- [ ] Optimize for large lists (lazy loading, virtualization)

### 3.2 Album List Page

- [ ] Create **AlbumListPage** composable
- [ ] Implement GraphQL query for album list with pagination
- [ ] Add filter toggle: "All Albums" vs "With Audio Only"
- [ ] Create **AlbumCard** component
  - Square cover art (256dp x 256dp)
  - Album title (truncate if too long)
  - Artist name (clickable link, muted color)
  - Release year badge
  - Release type badge (Album/EP/Single with color coding)
  - "Missing Audio" tag for albums without files
  - Hover effect with elevation increase
- [ ] Implement grid layout (adaptive columns)
- [ ] Add loading/error/empty states
- [ ] Implement click navigation to album detail page
- [ ] Add context menu on album card (play, shuffle, add to playlist)
- [ ] Optimize for large lists

### 3.3 Track List Component

- [ ] Create **TrackListItem** component (reusable)
  - Track number (or play icon if currently playing)
  - Optional cover art thumbnail (40dp x 40dp)
  - Track title (bold) with subtitle (featured artists, muted)
  - Optional quality badge (FLAC, 320kbps, etc.)
  - Play count (formatted: 1.2M, 345K)
  - Duration (formatted: 3:45)
  - Hover effect (background highlight)
  - Active state for currently playing track (green accent)
- [ ] Add click handler to play track
- [ ] Add double-click handler for quick play
- [ ] Implement context menu
  - Play track
  - Play next in queue
  - Add to queue
  - Add to playlist (submenu with playlist list)
  - Like/Unlike track
  - Go to artist
  - Go to album
- [ ] Support multi-disc tracks with disc separators
- [ ] Add visual indicator for current playing track (animated waveform or icon)

---

## Phase 4: Detail Pages

### 4.1 Artist Detail Page

- [ ] Create **ArtistDetailPage** composable with tabs
- [ ] Implement GraphQL query for artist details (with fragments)
- [ ] Create **ArtistHeader** component
  - Full-width background image (300-400dp height)
  - Dark gradient overlay for text readability
  - Large artist name (display typography)
  - Monthly listeners count
  - Release counts (albums, EPs, singles)
  - Verified badge if applicable
  - External service links (Spotify, Last.fm icons)
- [ ] Implement tabbed content area
  - **Top Tracks tab** (default)
  - **Albums tab**
  - **EPs tab**
  - **Singles tab**
  - **Similar Artists tab**
- [ ] **Top Tracks Tab**
  - Query top tracks from Last.fm
  - Display with TrackListItem components
  - "Play All" button
  - "Shuffle" button
  - Loading state
- [ ] **Albums/EPs/Singles Tabs**
  - Grid of AlbumCard components
  - Filter and sort options
  - Click navigation to album detail
- [ ] **Similar Artists Tab**
  - Grid of ArtistCard components (smaller size)
  - Click navigation to artist detail
- [ ] Add floating action buttons
  - Play artist (plays top tracks)
  - Shuffle artist
  - Follow/Unfollow (if implemented)
- [ ] Implement real-time updates via GraphQL subscription for artist metadata changes
- [ ] Add error handling for missing artist data

### 4.2 Album Detail Page

- [ ] Create **AlbumDetailPage** composable
- [ ] Implement GraphQL query for album/release details
- [ ] Create **AlbumHeader** component
  - Large cover art (320dp x 320dp) with shadow
  - Release type badge (Album/EP/Single with gradient)
  - Album title (large, bold)
  - Artist name (clickable link)
  - Release year, track count, total duration
  - Additional metadata (label, catalog number)
  - Icon badges for quality, format, etc.
- [ ] Add action button row
  - **Play** button (large, primary)
  - **Shuffle** button
  - **Add to Library** button (if not already added)
  - **Like Album** button (heart icon)
  - **More Actions** dropdown menu
    - Download album
    - Share album
    - Refresh metadata
    - View on MusicBrainz
- [ ] Create **AlbumTrackList** component
  - Support for multiple discs (with disc separators)
  - Track list with TrackListItem components
  - Play count for each track
  - Click to play track
  - Context menu on tracks
- [ ] Show copyright and label information at bottom
- [ ] Implement real-time updates via subscription for:
  - Track availability changes
  - Metadata updates
- [ ] Add loading/error states

---

## Phase 5: Search Implementation

### 5.1 Search Input & Debouncing

- [ ] Create debounced search text state (1 second delay)
- [ ] Implement search input change handler
- [ ] Add clear button that appears when text is present
- [ ] Show loading indicator during search
- [ ] Add keyboard shortcuts (Ctrl/Cmd+F to focus search)

### 5.2 Search Popup Panel

- [ ] Create **SearchPopup** component (appears below search bar)
- [ ] Implement GraphQL search query (artists, albums, tracks, playlists)
- [ ] Display results in grid sections:
  - **Artists** (up to 3 results with ArtistCard mini)
  - **Albums** (up to 3 results with AlbumCard mini)
  - **Tracks** (up to 5 results with TrackListItem)
  - **Playlists** (up to 3 results, if applicable)
- [ ] Add "View All Results" button for each section
- [ ] Implement click handlers for navigation to detail pages
- [ ] Add click-outside handler to close popup
- [ ] Add backdrop with blur when popup is open
- [ ] Position popup with proper z-index (above all content)

### 5.3 Search Results Page

- [ ] Create **SearchResultsPage** composable (route: /search?q=...)
- [ ] Display full search results in sections
  - **Artists Section**: Grid of ArtistCard
  - **Albums Section**: Grid of AlbumCard
  - **Tracks Section**: List of TrackListItem
  - **Playlists Section**: List of PlaylistCard (if applicable)
- [ ] Add section headers with result counts
- [ ] Implement "Show More" pagination for each section
- [ ] Add empty state if no results found
- [ ] Highlight search term in results (optional enhancement)
- [ ] Add filter options (by type: artist/album/track)

---

## Phase 6: Music Player - Core Playback

### 6.1 Player State Management

- [ ] Create **MusicPlayerState** data class
  ```kotlin
  data class MusicPlayerState(
    val isOpen: Boolean = false,
    val currentTrack: PlayerTrack? = null,
    val queue: List<PlayerTrack> = emptyList(),
    val currentIndex: Int = 0,
    val isPlaying: Boolean = false,
    val positionMs: Long = 0,
    val durationMs: Long = 0,
    val volume: Float = 1f,
    val isMuted: Boolean = false,
    val history: List<HistoryEntry> = emptyList()
  )
  ```
- [ ] Create **PlayerTrack** data class
  ```kotlin
  data class PlayerTrack(
    val artistId: String,
    val releaseFolderName: String,
    val trackNumber: Int,
    val title: String,
    val artistName: String,
    val coverArtUrl: String?,
    val durationMs: Long,
    val qualityLabel: String?
  )
  ```
- [ ] Create **MusicPlayerViewModel** with StateFlow
- [ ] Implement player actions:
  - `playTrack(track)` - Play single track immediately
  - `playTracks(tracks, startIndex)` - Load queue and play from index
  - `play()` - Resume playback
  - `pause()` - Pause playback
  - `next()` - Skip to next track
  - `previous()` - Previous track (or restart if >3s played)
  - `seekTo(positionMs)` - Seek to position
  - `setVolume(volume)` - Set volume (0f to 1f)
  - `toggleMute()` - Mute/unmute
  - `playAtIndex(index)` - Jump to specific queue position
  - `removeFromQueue(index)` - Remove track from queue
  - `addToQueue(track)` - Add track to end of queue
  - `playNext(track)` - Add track to play next

### 6.2 Audio Playback Engine

- [ ] Research and select audio library for desktop (JavaFX Media, JLayer, or native)
- [ ] Create **AudioPlayer** class wrapping the audio library
- [ ] Implement basic playback controls
  - Load audio from URL
  - Play/pause
  - Seek to position
  - Get current position
  - Get duration
  - Set volume
- [ ] Implement audio URL construction
  ```kotlin
  fun buildTrackUrl(artistId: String, releaseFolderName: String, trackNumber: Int): String {
    val base = storageService.getServerUrl()
    val encodedArtist = URLEncoder.encode(artistId, "UTF-8")
    val encodedRelease = URLEncoder.encode(releaseFolderName, "UTF-8")
    return "$base/library/$encodedArtist/releases/$encodedRelease/tracks/$trackNumber/audio"
  }
  ```
- [ ] Add authentication header to audio requests (JWT token)
- [ ] Implement error handling for audio loading failures
- [ ] Add playback state listeners (onReady, onPlaying, onPaused, onEnded, onError)
- [ ] Implement position update callbacks (emit every 100-500ms)

### 6.3 Queue Management

- [ ] Implement queue manipulation methods in ViewModel
- [ ] Support auto-advance to next track on track end
- [ ] Handle end of queue (stop playback or loop)
- [ ] Store and restore queue on app restart (optional)
- [ ] Implement shuffle mode
  - Generate shuffled queue order
  - Maintain current track position
  - Toggle shuffle on/off
- [ ] Implement repeat mode
  - Repeat off (stop at end)
  - Repeat queue (loop entire queue)
  - Repeat one (loop current track)

### 6.4 History Tracking

- [ ] Create **HistoryEntry** data class
  ```kotlin
  data class HistoryEntry(
    val track: PlayerTrack,
    val playedAtMs: Long
  )
  ```
- [ ] Add track to history when playback starts
- [ ] Limit history to 500 entries (FIFO)
- [ ] Format relative timestamps ("5 minutes ago", "2 hours ago")
- [ ] Make history persistent across app restarts (optional)

---

## Phase 7: Music Player - UI Components

### 7.1 Player Bar (Bottom Fixed)

- [ ] Create **MusicPlayerBar** composable (fixed at bottom)
- [ ] Implement three-column layout:
  - **Left**: Now playing info (30% width)
  - **Center**: Transport controls (40% width)
  - **Right**: Volume & queue button (30% width)
- [ ] Add glass morphism background (backdrop blur, 90% opacity)
- [ ] Add elevation/shadow effect
- [ ] Show/hide animation when track starts/stops
- [ ] Adjust main content bottom padding when player visible

### 7.2 Now Playing Info (Left Section)

- [ ] Display small cover art (48dp x 48dp) with rounded corners
- [ ] Display track title (bold, truncate if needed)
- [ ] Display artist name (muted color, clickable to artist page)
- [ ] Display quality badge (FLAC, 320kbps, etc.)
- [ ] Add click handler to expand player (optional future feature)
- [ ] Implement fade-in animation on track change

### 7.3 Transport Controls (Center Section)

- [ ] Create control button row:
  - **Previous button** (skip back icon)
  - **Play/Pause button** (large, circular, primary color)
  - **Next button** (skip forward icon)
- [ ] Add keyboard shortcuts:
  - Space: Play/pause
  - Left arrow: Seek backward 5s
  - Right arrow: Seek forward 5s
  - Ctrl/Cmd + Left: Previous track
  - Ctrl/Cmd + Right: Next track
- [ ] Create **ProgressSlider** component
  - Scrubber showing current position
  - Hover tooltip showing time at hover position
  - Drag to seek (update position on release)
  - Visual style: thin line with rounded thumb
- [ ] Display time labels
  - Current position (left): "2:34"
  - Total duration (right): "5:12"
  - Format: M:SS or H:MM:SS
- [ ] Update position in real-time while playing
- [ ] Implement optimistic UI for seeking (immediate visual feedback)

### 7.4 Volume & Queue Controls (Right Section)

- [ ] Create **VolumeControl** component
  - Mute/unmute button (speaker icon, changes based on state)
  - Volume slider (0-100%, appears on hover or always visible)
  - Store volume preference in local storage
- [ ] Create **QueueButton** component
  - Queue icon with badge showing number of tracks
  - Click to open queue/history sheet
  - Highlight when queue is active
- [ ] Add keyboard shortcuts for volume (Up/Down arrows)

### 7.5 Queue & History Sheet

- [ ] Create **QueueSheet** side panel (right side, 400dp width)
- [ ] Implement tabs: "Queue" and "History"
- [ ] **Queue Tab**
  - List of upcoming tracks
  - Highlight current track with accent color
  - Show cover art, title, artist, duration
  - Click track to jump to that position
  - Drag to reorder tracks
  - Swipe to remove track (or X button)
  - "Clear Queue" button at top
- [ ] **History Tab**
  - List of recently played tracks
  - Show cover art, title, artist
  - Show relative play time ("5 minutes ago")
  - Click track to play again
  - "Clear History" button at top
- [ ] Add slide-in/out animation for sheet
- [ ] Add backdrop with click-outside to close

---

## Phase 8: Playlists

### 8.1 Playlists List

- [ ] Implement GraphQL query for user playlists
- [ ] Display playlist items in sidebar
  - Playlist name
  - Track count badge
  - Playlist icon or cover art (mosaic of first 4 tracks)
- [ ] Add "Create Playlist" button in sidebar
- [ ] Implement playlist creation dialog
  - Name input field
  - Description input field (optional)
  - Create button
  - Cancel button
- [ ] Handle create playlist mutation and update cache
- [ ] Navigate to newly created playlist on success

### 8.2 Playlist Detail Page

- [ ] Create **PlaylistDetailPage** composable
- [ ] Implement GraphQL query for playlist details
- [ ] Create **PlaylistHeader** component
  - Playlist cover art (mosaic or custom image)
  - Playlist name (editable on click)
  - Track count and total duration
  - Creator name
  - Description (editable on click)
- [ ] Add action buttons
  - **Play** button (play all tracks)
  - **Shuffle** button
  - **Edit** button (enter edit mode)
  - **Delete Playlist** button (with confirmation)
- [ ] Create **PlaylistTrackList** component
  - List of tracks with TrackListItem
  - Show "Added At" date for each track
  - Drag to reorder tracks (persist order)
  - Swipe/button to remove track
  - Context menu with additional actions
- [ ] Implement edit mode
  - Checkbox selection for bulk actions
  - Remove selected tracks
  - Move selected tracks to another playlist
- [ ] Handle empty playlist state with "Add Songs" prompt

### 8.3 Add to Playlist Feature

- [ ] Create **AddToPlaylistDialog** component
  - List of user playlists with checkboxes
  - "Create New Playlist" option
  - Add button (disabled until selection)
  - Cancel button
- [ ] Implement from track context menu
- [ ] Implement from album page ("Add Album to Playlist")
- [ ] Handle mutation and show success/error toast
- [ ] Update playlist cache after adding tracks

---

## Phase 9: Polish & UX Enhancements

### 9.1 Loading States

- [ ] Implement skeleton loaders for:
  - Artist card grid
  - Album card grid
  - Track lists
  - Playlist items
- [ ] Add shimmer animation effect to skeletons
- [ ] Ensure smooth transitions between loading and loaded states
- [ ] Add progressive loading for images (blur-up effect)

### 9.2 Error Handling & Retry

- [ ] Create consistent error UI components
- [ ] Implement retry buttons on network errors
- [ ] Add error boundaries to prevent full app crashes
- [ ] Show user-friendly error messages (not technical stack traces)
- [ ] Implement offline detection and messaging
- [ ] Add error logging (optional: send to backend)

### 9.3 Animations & Transitions

- [ ] Add page transition animations (fade, slide)
- [ ] Implement hover effects on interactive elements
  - Scale on hover for cards
  - Background color change on hover for buttons
  - Elevation increase on hover
- [ ] Add click/press animations (ripple effect or scale down)
- [ ] Animate player bar show/hide
- [ ] Animate sheet open/close
- [ ] Add smooth scrolling behavior
- [ ] Implement parallax effect on artist/album headers (optional)

### 9.4 Keyboard Shortcuts

- [ ] Implement global keyboard shortcuts:
  - `Ctrl/Cmd + F`: Focus search
  - `Space`: Play/pause
  - `Ctrl/Cmd + →`: Next track
  - `Ctrl/Cmd + ←`: Previous track
  - `Ctrl/Cmd + ↑`: Volume up
  - `Ctrl/Cmd + ↓`: Volume down
  - `Ctrl/Cmd + M`: Toggle mute
  - `Ctrl/Cmd + Shift + Q`: Show queue
  - `Ctrl/Cmd + ,`: Open settings
  - `Escape`: Close dialogs/sheets
- [ ] Display keyboard shortcuts in a help dialog (Ctrl/Cmd + /)
- [ ] Ensure shortcuts don't conflict with system shortcuts

### 9.5 Tooltips & Hints

- [ ] Add tooltips to all icon buttons
- [ ] Show keyboard shortcuts in tooltips (e.g., "Play (Space)")
- [ ] Add helpful hints for first-time users
- [ ] Implement contextual help (? icon with explanations)

### 9.6 Performance Optimizations

- [ ] Implement lazy loading for long lists (virtual scrolling)
- [ ] Optimize image loading and caching
- [ ] Debounce expensive operations (search, filters)
- [ ] Use Compose remember and derivedStateOf appropriately
- [ ] Profile and optimize recompositions
- [ ] Implement pagination for large data sets
- [ ] Add loading indicators for slow operations

---

## Phase 10: Advanced Features

### 10.1 Real-time Updates

- [ ] Implement GraphQL subscriptions for:
  - Artist metadata updates (refresh artist page when metadata changes)
  - Album metadata updates (refresh album page)
  - Track availability changes (update UI when new tracks added)
  - Playlist changes (update when another device modifies playlist)
- [ ] Handle subscription lifecycle (connect, disconnect, reconnect)
- [ ] Update local cache when subscription events received
- [ ] Show subtle notifications for important updates ("New tracks available!")

### 10.2 Settings Page

- [ ] Create **SettingsPage** composable with sections
- [ ] **Appearance Section**
  - Theme selection (if multiple themes)
  - Font size adjustment
  - Sidebar width adjustment
- [ ] **Playback Section**
  - Crossfade duration (0-12 seconds)
  - Normalize volume toggle
  - Gapless playback toggle
  - Audio quality preference
- [ ] **Account Section**
  - Display username and email
  - Change password button
  - Sign out button
  - Delete account button (with confirmation)
- [ ] **About Section**
  - App version
  - Backend version and URL
  - Credits and licenses
  - Check for updates button
- [ ] Persist settings in local storage
- [ ] Apply settings changes immediately

### 10.3 Liked Songs

- [ ] Implement GraphQL query for liked songs
- [ ] Create **LikedSongsPage** composable
- [ ] Display as track list with TrackListItem
- [ ] Add "Play All" and "Shuffle" buttons
- [ ] Implement like/unlike mutation
- [ ] Update heart icon state in track list and player
- [ ] Sort options (recently added, artist, album, title)
- [ ] Filter by artist or album

### 10.4 Context Menus

- [ ] Implement right-click context menus for:
  - **Track items**
    - Play track
    - Play next / Add to queue
    - Add to playlist (submenu)
    - Like/Unlike
    - Go to artist
    - Go to album
    - Share (copy link)
  - **Album cards**
    - Play album
    - Shuffle album
    - Add to library
    - Add to playlist
    - Share
  - **Artist cards**
    - Play artist (top tracks)
    - Shuffle artist
    - Follow/Unfollow
    - Share
  - **Playlist items** (in sidebar)
    - Play playlist
    - Rename
    - Delete
    - Share
- [ ] Implement keyboard navigation in context menus (up/down arrows, enter)
- [ ] Add icons to context menu items
- [ ] Position menus properly (avoid screen edges)

### 10.5 Drag & Drop

- [ ] Implement drag & drop for:
  - Reordering tracks in playlists
  - Reordering playlists in sidebar
  - Dragging tracks to playlists (in sidebar)
  - Dragging albums to playlists
- [ ] Add visual feedback during drag (drag preview, drop target highlight)
- [ ] Implement drop zones with clear visual indicators
- [ ] Handle edge cases (invalid drop targets, permissions)

---

## Phase 11: Testing & Quality Assurance

### 11.1 Unit Tests

- [ ] Write tests for ViewModels
  - Player state transitions
  - Queue management logic
  - Search debouncing
- [ ] Write tests for utility functions
  - Time formatting
  - URL construction
  - Data transformations
- [ ] Write tests for GraphQL operations (mock responses)
- [ ] Aim for >70% code coverage on business logic

### 11.2 Integration Tests

- [ ] Test navigation flows
  - Home → Artists → Artist Detail → Album Detail
  - Search → Results → Detail pages
- [ ] Test player integration
  - Play track → next → previous → seek
  - Queue manipulation
  - Volume control
- [ ] Test playlist operations
  - Create → add tracks → reorder → delete

### 11.3 Manual Testing

- [ ] Test all navigation routes
- [ ] Test all player controls and states
- [ ] Test search with various queries
- [ ] Test context menus on all item types
- [ ] Test keyboard shortcuts
- [ ] Test with slow network (loading states)
- [ ] Test with network failures (error states)
- [ ] Test with empty data (empty states)
- [ ] Test with large data sets (performance)
- [ ] Test window resizing (responsive layout)
- [ ] Test drag & drop interactions
- [ ] Test long-running playback (memory leaks)

### 11.4 Bug Fixes & Refinements

- [ ] Fix all critical bugs (crashes, data loss)
- [ ] Fix high-priority bugs (broken features, visual glitches)
- [ ] Polish animations and transitions
- [ ] Refine spacing and alignment
- [ ] Improve loading times
- [ ] Optimize memory usage
- [ ] Fix accessibility issues

---

## Phase 12: Documentation & Deployment

### 12.1 Code Documentation

- [ ] Add KDoc comments to public APIs
- [ ] Document architecture decisions in README
- [ ] Create component library documentation
- [ ] Document GraphQL schema and operations
- [ ] Add inline comments for complex logic

### 12.2 User Documentation

- [ ] Create user guide with screenshots
  - Getting started
  - Navigating the app
  - Playing music
  - Creating playlists
  - Using search
  - Keyboard shortcuts
- [ ] Create FAQ document
- [ ] Create troubleshooting guide

### 12.3 Build & Distribution

- [ ] Configure desktop packaging (DMG for macOS, MSI for Windows, DEB for Linux)
- [ ] Set up application icons and branding
- [ ] Configure code signing (for macOS/Windows)
- [ ] Test installers on all target platforms
- [ ] Create release notes template
- [ ] Set up auto-update mechanism (optional)
- [ ] Publish first release

### 12.4 Future Enhancements (Backlog)

- [ ] Equalizer with presets
- [ ] Lyrics display (synced lyrics)
- [ ] Visualizer (audio visualization)
- [ ] Scrobbling to Last.fm
- [ ] Import from Spotify (playlists, liked songs)
- [ ] Social features (share listening activity)
- [ ] Smart playlists (auto-generated based on criteria)
- [ ] Desktop notifications for track changes
- [ ] System media controls integration (macOS/Windows)
- [ ] Mini player mode (compact window)
- [ ] Multi-device sync (play queue across devices)
- [ ] Offline mode (download for offline listening)

---

## Implementation Guidelines

### Code Quality Standards

- **Kotlin Style**: Follow official Kotlin coding conventions
- **Compose Best Practices**: Use remember, derivedStateOf, LaunchedEffect appropriately
- **State Management**: Prefer StateFlow for ViewModels, State for UI
- **Error Handling**: Use Result/Either patterns, never ignore errors
- **Null Safety**: Leverage Kotlin's null safety, avoid !! operator
- **Comments**: Comment why, not what; keep code self-documenting

### UI/UX Principles

- **Consistency**: Maintain consistent spacing, colors, typography
- **Feedback**: Provide immediate visual feedback for all interactions
- **Performance**: Keep UI responsive, move heavy work to background
- **Accessibility**: Support keyboard navigation, screen readers
- **Beauty**: Use gradients, glass morphism, smooth animations thoughtfully

### Git Workflow

- Create feature branches for each phase/task
- Commit frequently with descriptive messages
- Write meaningful PR descriptions
- Test thoroughly before merging to main
- Tag releases with semantic versioning

---

## Success Criteria

The desktop app will be considered feature-complete when:

✅ Users can browse artists, albums, and tracks
✅ Search works across all content types with instant results
✅ Music playback is stable with queue management
✅ Playlists can be created, edited, and deleted
✅ UI is beautiful, responsive, and performant
✅ Real-time updates keep UI in sync with backend
✅ Keyboard shortcuts enable power user workflows
✅ Error states are handled gracefully
✅ All critical bugs are fixed
✅ App can be packaged and distributed

---

## Estimated Timeline

**Phase 1-2 (Foundation)**: 1-2 weeks
**Phase 3-4 (Browse & Detail)**: 2-3 weeks
**Phase 5 (Search)**: 1 week
**Phase 6-7 (Music Player)**: 2-3 weeks
**Phase 8 (Playlists)**: 1-2 weeks
**Phase 9 (Polish)**: 1-2 weeks
**Phase 10 (Advanced)**: 2-3 weeks
**Phase 11 (Testing)**: 1-2 weeks
**Phase 12 (Documentation)**: 1 week

**Total**: 12-20 weeks (3-5 months) for a single developer

---

**Last Updated**: 2025-10-28
**Status**: Planning Phase - Ready to Begin Implementation
