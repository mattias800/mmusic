# mmusic Flutter App Implementation Plan

This plan outlines the complete implementation of a multiplatform Flutter music player app for the mmusic service.

## Phase 0: Environment Setup

### Flutter Installation & Tools
- [x] Install Flutter SDK (https://docs.flutter.dev/get-started/install)
  - macOS: `brew install --cask flutter` or download from flutter.dev
  - Verify: `flutter doctor`
- [x] Install Xcode (for iOS/macOS development)
  - Install from App Store
  - Run: `sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer`
  - Run: `sudo xcodebuild -runFirstLaunch`
- [x] Install Android Studio (for Android development)
  - Download from developer.android.com
  - Install Android SDK, Android SDK Platform-Tools
  - Accept licenses: `flutter doctor --android-licenses`
- [x] Install IDE plugins
  - VS Code: Install "Flutter" and "Dart" extensions
  - OR Android Studio: Install Flutter plugin
- [x] Verify all platforms: `flutter doctor -v`
- [x] Enable desktop platforms:
  - `flutter config --enable-macos-desktop`
  - `flutter config --enable-windows-desktop`
  - `flutter config --enable-linux-desktop`

## Phase 1: Project Setup

### Create & Initialize Project
- [x] Create Flutter project: `flutter create mmusic_app --org com.mmusic --platforms android,ios,macos,windows,linux`
- [x] Navigate to project: `cd mmusic_app`
- [x] Test run on current platform: `flutter run -d macos` (or your platform)
- [x] Initialize git repo inside mmusic_app
- [x] Create `.gitignore` (Flutter template should include one)
- [x] Create `README.md` for the app

### Project Structure Setup
- [x] Create directory structure:
  ```
  lib/
    core/
      config/
      theme/
      utils/
    data/
      models/
      repositories/
      graphql/
    domain/
      entities/
      usecases/
    presentation/
      screens/
      widgets/
      providers/
    services/
      audio/
      storage/
  ```
- [x] Create `ARCHITECTURE.md` documenting app architecture

## Phase 2: Core Dependencies

### Add Essential Packages
- [x] Add dependencies to `pubspec.yaml`:
  ```yaml
  dependencies:
    # State Management
    provider: ^6.1.0
    riverpod: ^2.4.0  # or provider - choose one

    # GraphQL
    graphql_flutter: ^5.1.0

    # Audio
    audio_service: ^0.18.12
    just_audio: ^0.9.36
    audio_session: ^0.1.18

    # UI
    cached_network_image: ^3.3.1
    flutter_svg: ^2.0.10

    # Storage
    shared_preferences: ^2.2.2
    hive: ^2.2.3
    hive_flutter: ^1.1.0
    path_provider: ^2.1.2

    # Networking
    dio: ^5.4.0

    # Utilities
    equatable: ^2.0.5
    freezed_annotation: ^2.4.1
    json_annotation: ^4.8.1

  dev_dependencies:
    # Code Generation
    build_runner: ^2.4.8
    freezed: ^2.4.6
    json_serializable: ^6.7.1
    hive_generator: ^2.0.1

    # Testing
    flutter_test:
      sdk: flutter
    mocktail: ^1.0.3
  ```
- [x] Run: `flutter pub get`
- [x] Verify all packages installed successfully

## Phase 3: Configuration & Setup

### App Configuration
- [x] Create `lib/core/config/app_config.dart` for environment configuration
  - Server URL configuration
  - API endpoints
  - Feature flags
- [x] Create `lib/core/config/environment.dart` for dev/staging/prod environments
- [ ] Set up app name and bundle IDs in platform-specific files:
  - Android: `android/app/build.gradle` (applicationId)
  - iOS: `ios/Runner.xcodeproj` (bundle identifier)
  - macOS: `macos/Runner.xcodeproj` (bundle identifier)
  - Windows: `windows/runner/main.cpp` (app name)
  - Linux: `linux/my_application.cc` (app name)

### Theme & Design System
- [x] Create `lib/core/theme/app_theme.dart`
  - Light theme
  - Dark theme
  - Color scheme matching web frontend
- [x] Create `lib/core/theme/text_styles.dart`
- [x] Create `lib/core/theme/app_colors.dart`

## Phase 4: GraphQL Integration

### GraphQL Client Setup
- [x] Create `lib/data/graphql/graphql_client.dart`
  - Configure HTTP link to backend
  - Set up authentication interceptor
  - Configure cache policies
- [x] Create `lib/core/providers/graphql_provider.dart`
- [ ] Create `lib/data/graphql/queries/` directory
- [ ] Create `lib/data/graphql/mutations/` directory
- [x] Port essential GraphQL operations from Web frontend:
  - [x] User authentication queries
  - [x] Library queries (artists, albums, tracks)
  - [ ] Playlist queries
  - [x] Search queries
- [x] Create data models matching GraphQL schema (User model)
- [ ] Set up code generation for GraphQL (if using graphql_codegen)

### Authentication
- [x] Implement authentication repository: `lib/data/repositories/auth_repository.dart`
- [x] Create sign-in mutation
- [x] Create sign-up mutation (if applicable)
- [x] Implement secure token storage (using flutter_secure_storage)
- [x] Create auth state provider/notifier
- [x] Implement auto-login on app start
- [x] Implement logout functionality

## Phase 5: Audio Service Implementation

### Audio Service Core
- [x] Create `lib/services/audio/audio_handler.dart` extending `BaseAudioHandler`
  - Implement play/pause/stop
  - Implement skip next/previous
  - Implement seek
  - Implement queue management
- [x] Create `lib/services/audio/audio_service_manager.dart`
  - Initialize audio_service
  - Configure audio session
  - Handle platform-specific audio setup
- [x] Create `lib/presentation/providers/player_provider.dart`
  - Track current track
  - Track playback position
  - Track queue
  - Track repeat/shuffle state
- [x] Implement streaming from mmusic backend
  - Handle authentication headers for audio requests
  - Implement buffering strategy (via just_audio)
  - Handle network errors/retries

### Media Controls & OS Integration
- [x] Implement lock screen controls (Android/iOS)
- [x] Implement Control Center integration (iOS/macOS)
- [x] Implement media notification (Android)
- [x] Implement MPRIS integration (Linux)
- [x] Implement system media controls (Windows)
- [x] Implement hardware button controls (play/pause/skip)
- [x] Implement Bluetooth headset controls
- [x] Implement "Now Playing" info updates
  - Track title
  - Artist name
  - Album art
  - Duration
  - Position

### Background Playback
- [x] Configure background execution (via audio_service)
- [x] Configure foreground service (Android)
- [ ] Test background playback on all platforms
- [x] Implement proper audio interruption handling (via audio_service)

## Phase 6: Data Layer

### Models & Entities
- [x] Create domain entities in `lib/domain/entities/`:
  - [x] `track.dart`
  - [x] `artist.dart`
  - [x] `album.dart`
  - [x] `playlist.dart`
  - [x] `user.dart`
- [x] Create data models in `lib/data/models/` (GraphQL response models)
- [x] Create mappers between data models and domain entities

### Repositories
- [x] Create `lib/data/repositories/library_repository.dart`
  - Fetch artists
  - Fetch albums
  - Fetch tracks
  - Search functionality
- [ ] Create `lib/data/repositories/playlist_repository.dart`
  - Fetch playlists
  - Create playlist
  - Add/remove tracks
  - Reorder tracks
- [ ] Create `lib/data/repositories/user_repository.dart`
  - Get user profile
  - Get liked songs
  - Update preferences

### Local Storage
- [ ] Set up Hive boxes for caching:
  - [ ] Recent searches
  - [ ] Playback history
  - [ ] Downloaded tracks (future)
  - [ ] App preferences
- [ ] Implement cache invalidation strategy
- [ ] Create storage service: `lib/services/storage/storage_service.dart`

## Phase 7: UI Implementation - Core Screens

### Authentication Screens
- [x] Create `lib/presentation/screens/auth/login_screen.dart`
  - Server URL input
  - Username input
  - Password input
  - Sign in button
  - Remember me toggle
- [x] Create `lib/presentation/screens/auth/server_setup_screen.dart`
  - Server discovery/manual entry
  - Connection testing
- [x] Create auth loading states
- [x] Create error handling UI

### Home/Library Screen
- [x] Create `lib/presentation/screens/home/home_screen.dart`
  - Bottom navigation bar
  - Library tab (placeholder)
  - Search tab (placeholder)
  - Playlists tab (placeholder)
  - Settings tab (with sign out)
- [x] Create `lib/presentation/screens/library/library_screen.dart`
  - Artists list
  - Albums list
  - Tracks list
  - Tabs or sections
- [ ] Implement pull-to-refresh
- [ ] Implement infinite scroll/pagination

### Artist Detail Screen
- [ ] Create `lib/presentation/screens/artist/artist_detail_screen.dart`
  - Artist header with image
  - Albums list
  - Top tracks
  - Play all button
  - Shuffle button

### Album Detail Screen
- [ ] Create `lib/presentation/screens/album/album_detail_screen.dart`
  - Album artwork
  - Track list
  - Play button
  - Add to playlist button
  - Album metadata

### Playlist Screens
- [ ] Create `lib/presentation/screens/playlist/playlists_screen.dart`
  - User playlists list
  - Create playlist button
- [ ] Create `lib/presentation/screens/playlist/playlist_detail_screen.dart`
  - Track list
  - Edit mode
  - Reorder tracks
  - Remove tracks

### Search Screen
- [ ] Create `lib/presentation/screens/search/search_screen.dart`
  - Search bar
  - Recent searches
  - Search results (artists, albums, tracks)
  - Search filters

## Phase 8: UI Implementation - Player UI

### Now Playing Screen
- [x] Create `lib/presentation/screens/player/now_playing_screen.dart`
  - Full-screen album art
  - Track title
  - Artist name
  - Progress bar with time
  - Play/pause button
  - Skip next/previous
  - Shuffle button
  - Repeat button
  - [ ] Volume control
  - [ ] Queue button
  - [ ] Like/favorite button
  - [ ] More options menu

### Mini Player
- [x] Create `lib/presentation/widgets/mini_player.dart`
  - Thumbnail artwork
  - Track title
  - Artist name
  - Play/pause button
  - Skip next button
  - Tap to expand to full player
  - [ ] Swipe up gesture

### Queue Screen
- [ ] Create `lib/presentation/screens/player/queue_screen.dart`
  - Current track
  - Up next list
  - Reorder queue
  - Clear queue
  - Add to queue

## Phase 9: State Management

### Player State
- [x] Create `lib/presentation/providers/player_provider.dart`
  - Current track state
  - Playback state (playing/paused/stopped)
  - Position state
  - Queue state
  - Repeat/shuffle state
  - [ ] Volume state

### Library State
- [x] Create `lib/presentation/providers/library_provider.dart`
  - Artists state
  - Albums state
  - Tracks state
  - Loading states
  - Error states

### Playlist State
- [ ] Create `lib/presentation/providers/playlist_provider.dart`
  - Playlists list state
  - Current playlist state
  - Edit mode state

### Auth State
- [x] Create `lib/presentation/providers/auth_provider.dart`
  - User state
  - Authentication status
  - Server configuration

## Phase 10: Shared Widgets

### Create Reusable Components
- [x] Create `lib/presentation/widgets/track_list_item.dart`
  - Track thumbnail
  - Track title
  - Artist name
  - Duration
  - More menu
  - Play on tap
- [x] Create `lib/presentation/widgets/album_card.dart`
- [x] Create `lib/presentation/widgets/artist_card.dart`
- [ ] Create `lib/presentation/widgets/playlist_card.dart`
- [ ] Create `lib/presentation/widgets/loading_indicator.dart`
- [ ] Create `lib/presentation/widgets/error_view.dart`
- [ ] Create `lib/presentation/widgets/empty_state.dart`
- [ ] Create `lib/presentation/widgets/artwork_image.dart` (with caching)

## Phase 11: Platform-Specific Configuration

### Android
- [ ] Configure permissions in `android/app/src/main/AndroidManifest.xml`:
  - Internet permission
  - Wake lock permission
  - Foreground service permission
  - Audio focus permission
- [ ] Configure notification channels for media playback
- [ ] Set up ProGuard rules if needed
- [ ] Configure minimum SDK version (API 21+)
- [ ] Test on physical Android device
- [ ] Test on Android emulator

### iOS
- [ ] Configure permissions in `ios/Runner/Info.plist`:
  - Network usage description
- [ ] Enable background modes in Xcode:
  - Audio, AirPlay, and Picture in Picture
- [ ] Configure audio session category
- [ ] Test on physical iOS device
- [ ] Test on iOS simulator

### macOS
- [ ] Configure permissions in `macos/Runner/Info.plist`:
  - Network client entitlement
- [ ] Configure signing & capabilities in Xcode
- [ ] Test network access
- [ ] Test media keys integration

### Windows
- [ ] Configure app icon
- [ ] Test system media controls
- [ ] Verify audio output
- [ ] Test on Windows 10 and Windows 11

### Linux
- [ ] Configure desktop entry file
- [ ] Test MPRIS integration
- [ ] Verify audio output with PulseAudio/PipeWire
- [ ] Test on Ubuntu/Debian-based distro

## Phase 12: Advanced Features

### Offline Support (Future)
- [ ] Design download management system
- [ ] Implement track download
- [ ] Implement offline playback
- [ ] Implement storage management
- [ ] Implement sync status UI

### Queue Management
- [ ] Implement add to queue
- [ ] Implement play next
- [ ] Implement play last
- [ ] Implement queue reordering
- [ ] Implement queue persistence
- [ ] Implement smart queue features

### Settings & Preferences
- [ ] Create `lib/presentation/screens/settings/settings_screen.dart`
  - Account settings
  - Audio quality settings
  - Download settings
  - Appearance settings (theme)
  - About section
- [ ] Implement theme switching (light/dark)
- [ ] Implement audio quality selection
- [ ] Implement cache management

### Additional Features
- [ ] Implement lyrics display (if backend supports)
- [ ] Implement crossfade between tracks
- [ ] Implement gapless playback
- [ ] Implement equalizer (platform-dependent)
- [ ] Implement sleep timer
- [ ] Implement playback speed control
- [ ] Implement casting support (Chromecast/AirPlay)

## Phase 13: Testing

### Unit Tests
- [ ] Test audio service
- [ ] Test repositories
- [ ] Test providers/state management
- [ ] Test models and mappers
- [ ] Test utilities

### Widget Tests
- [ ] Test authentication screens
- [ ] Test player UI
- [ ] Test list screens
- [ ] Test shared widgets

### Integration Tests
- [ ] Test authentication flow
- [ ] Test playback flow
- [ ] Test playlist management
- [ ] Test search functionality

### Platform Testing
- [ ] Test on Android (multiple devices/versions)
- [ ] Test on iOS (multiple devices/versions)
- [ ] Test on iPad
- [ ] Test on macOS
- [ ] Test on Windows
- [ ] Test on Linux

## Phase 14: Polish & Optimization

### Performance
- [ ] Profile app performance with Flutter DevTools
- [ ] Optimize image loading and caching
- [ ] Optimize list scrolling performance
- [ ] Reduce app bundle size
- [ ] Optimize GraphQL queries
- [ ] Implement proper loading states everywhere

### UX Improvements
- [ ] Add haptic feedback (where appropriate)
- [ ] Add animations and transitions
- [ ] Implement gesture controls (swipe, long-press)
- [ ] Add tooltips and onboarding
- [ ] Improve error messages
- [ ] Add pull-to-refresh indicators

### Accessibility
- [ ] Add semantic labels for screen readers
- [ ] Test with TalkBack (Android)
- [ ] Test with VoiceOver (iOS)
- [ ] Ensure sufficient color contrast
- [ ] Support dynamic font sizes

## Phase 15: Deployment Preparation

### App Icons & Branding
- [ ] Design app icon (1024x1024 master)
- [ ] Generate all required icon sizes
- [ ] Add icons to all platforms:
  - Android: `android/app/src/main/res/mipmap-*/`
  - iOS: `ios/Runner/Assets.xcassets/AppIcon.appiconset/`
  - macOS: `macos/Runner/Assets.xcassets/AppIcon.appiconset/`
  - Windows: Update icon in CMakeLists.txt
  - Linux: Update desktop file
- [ ] Create splash screen
- [ ] Add app branding/colors

### Documentation
- [ ] Update README.md with setup instructions
- [ ] Document architecture decisions
- [ ] Create user guide/help documentation
- [ ] Document build process
- [ ] Create CONTRIBUTING.md (if open-source)

### Build & Release
- [ ] Configure versioning strategy
- [ ] Set up release build configurations
- [ ] Android: Set up signing key
- [ ] iOS: Configure provisioning profiles
- [ ] Create release builds for all platforms
- [ ] Test release builds thoroughly

### Distribution
- [ ] Android: Prepare for Google Play Store
  - Create store listing
  - Screenshots for all device sizes
  - Privacy policy
- [ ] iOS: Prepare for App Store
  - Create store listing
  - Screenshots for iPhone/iPad
  - Privacy policy
- [ ] macOS: Prepare for Mac App Store OR notarization
- [ ] Windows: Prepare installer (MSIX or traditional)
- [ ] Linux: Prepare packages (AppImage, snap, flatpak)

## Phase 16: Launch & Iteration

### Pre-launch
- [ ] Beta test with small group
- [ ] Fix critical bugs
- [ ] Gather feedback
- [ ] Final QA pass

### Launch
- [ ] Submit to app stores
- [ ] Release for desktop platforms
- [ ] Announce release
- [ ] Monitor crash reports
- [ ] Monitor user feedback

### Post-launch
- [ ] Address user-reported issues
- [ ] Plan feature roadmap
- [ ] Regular updates
- [ ] Performance monitoring

---

## Notes

### Development Tips
- Use `flutter run --release` to test performance
- Use `flutter logs` to debug on physical devices
- Use Flutter DevTools for profiling
- Test on real devices early and often

### Key Architecture Decisions
- **State Management**: Riverpod (or Provider) for simplicity and testability
- **Audio**: audio_service + just_audio for cross-platform media controls
- **GraphQL**: graphql_flutter for queries/mutations
- **Storage**: Hive for fast local storage, shared_preferences for simple settings
- **Navigation**: Go_router or Navigator 2.0 for deep linking support

### Estimated Timeline
- Phase 0-2: 1-2 days (setup)
- Phase 3-5: 1 week (core infrastructure)
- Phase 6-7: 2 weeks (data layer & core UI)
- Phase 8-10: 2 weeks (player UI & state)
- Phase 11: 1 week (platform-specific)
- Phase 12-14: 2-3 weeks (features & polish)
- Phase 15-16: 1 week (release prep)

**Total: 8-10 weeks for full implementation**

### Resources
- Flutter documentation: https://docs.flutter.dev
- audio_service package: https://pub.dev/packages/audio_service
- just_audio package: https://pub.dev/packages/just_audio
- graphql_flutter: https://pub.dev/packages/graphql_flutter
- Flutter samples: https://github.com/flutter/samples
