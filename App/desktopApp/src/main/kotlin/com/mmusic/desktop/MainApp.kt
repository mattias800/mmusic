package com.mmusic.desktop

import androidx.compose.foundation.layout.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.unit.dp
import com.mmusic.desktop.navigation.Route
import com.mmusic.desktop.pages.*
import com.mmusic.desktop.ui.theme.AppTheme
import com.mmusic.desktop.components.PlayerBar
import com.mmusic.desktop.audio.DesktopAudioEngine
import com.mmusic.shared.services.ApolloGraphQLClient
import com.mmusic.shared.services.StorageService
import com.mmusic.shared.services.AudioPlayer
import com.mmusic.shared.graphql.BootstrapQuery
import kotlinx.coroutines.launch

@Composable
fun MainApp(
    storageService: StorageService,
    apolloClient: ApolloGraphQLClient
) {
    // Keep old client for auth screens temporarily
    val graphQLClient = remember { com.mmusic.shared.services.GraphQLClient(storageService) }
    var currentRoute by remember { mutableStateOf<String>("loading") }
    val coroutineScope = rememberCoroutineScope()

    // Audio player setup
    val audioPlayer = remember { AudioPlayer() }
    val audioEngine = remember { DesktopAudioEngine(audioPlayer, coroutineScope) }

    // Check authentication on startup
    LaunchedEffect(Unit) {
        if (!storageService.hasServerUrl()) {
            currentRoute = "serverSetup"
        } else {
            try {
                val result = apolloClient.bootstrap()
                currentRoute = when {
                    result.hasErrors() -> "serverSetup"
                    result.data?.viewer != null -> "artists"
                    else -> "login"
                }
            } catch (e: Exception) {
                currentRoute = "serverSetup"
            }
        }
    }

    AppTheme {
        Surface(
            modifier = Modifier.fillMaxSize(),
            color = MaterialTheme.colorScheme.background
        ) {
            when (currentRoute) {
                "loading" -> LoadingScreen()
                "serverSetup" -> ServerSetupScreen(
                    onServerConfigured = { currentRoute = "login" },
                    storageService = storageService,
                    graphQLClient = graphQLClient,
                    coroutineScope = coroutineScope
                )
                "login" -> LoginScreen(
                    onLoginSuccess = { currentRoute = "artists" },
                    onChangeServer = {
                        storageService.clearAll()
                        currentRoute = "serverSetup"
                    },
                    storageService = storageService,
                    graphQLClient = graphQLClient,
                    coroutineScope = coroutineScope
                )
                "artists" -> AppScaffold(
                    currentRoute = currentRoute,
                    onNavigate = { currentRoute = it },
                    storageService = storageService,
                    apolloClient = apolloClient,
                    audioPlayer = audioPlayer
                )
                else -> AppScaffold(
                    currentRoute = currentRoute,
                    onNavigate = { currentRoute = it },
                    storageService = storageService,
                    apolloClient = apolloClient,
                    audioPlayer = audioPlayer
                )
            }
        }
    }
}

@Composable
fun LoadingScreen() {
    Box(
        modifier = Modifier.fillMaxSize(),
        contentAlignment = androidx.compose.ui.Alignment.Center
    ) {
        CircularProgressIndicator()
    }
}

@Composable
fun AppScaffold(
    currentRoute: String,
    onNavigate: (String) -> Unit,
    storageService: StorageService,
    apolloClient: ApolloGraphQLClient,
    audioPlayer: AudioPlayer
) {
    Column(modifier = Modifier.fillMaxSize()) {
        Row(modifier = Modifier.weight(1f).fillMaxWidth()) {
        // Sidebar (hide on detail pages only)
        val showSidebar = !currentRoute.startsWith("artist/") || currentRoute == "artists"
        val showSidebarForAlbums = !currentRoute.startsWith("album/") || currentRoute == "albums"
        val showSidebarForPlaylists = !currentRoute.startsWith("playlist/") || currentRoute == "playlists"

        if (showSidebar && showSidebarForAlbums && showSidebarForPlaylists) {
            AppSidebar(
                currentRoute = currentRoute,
                onNavigate = onNavigate,
                onSignOut = {
                    storageService.clearAuthToken()
                    onNavigate("login")
                }
            )
        }

        // Main content
        Box(modifier = Modifier.weight(1f)) {
            when {
                currentRoute == "artists" -> ArtistListPage(
                    apolloClient = apolloClient,
                    onNavigateToArtist = { artistId -> onNavigate("artist/$artistId") }
                )
                currentRoute == "albums" -> AlbumListPage(
                    apolloClient = apolloClient,
                    onNavigateToAlbum = { artistId, folderName -> onNavigate("album/$artistId/$folderName") }
                )
                currentRoute == "playlists" -> PlaylistListPage(
                    apolloClient = apolloClient,
                    onNavigateToPlaylist = { playlistId -> onNavigate("playlist/$playlistId") }
                )
                currentRoute == "search" -> SearchPage(
                    apolloClient = apolloClient,
                    onNavigateToArtist = { artistId -> onNavigate("artist/$artistId") },
                    onNavigateToAlbum = { artistId, folderName -> onNavigate("album/$artistId/$folderName") }
                )
                currentRoute.startsWith("artist/") && currentRoute.contains("/album/") -> {
                    // Album detail: artist/{artistId}/album/{folderName}
                    val parts = currentRoute.split("/")
                    if (parts.size >= 4) {
                        val artistId = parts[1]
                        val folderName = parts.drop(3).joinToString("/")
                        AlbumDetailPage(
                            artistId = artistId,
                            releaseFolderName = folderName,
                            apolloClient = apolloClient,
                            audioPlayer = audioPlayer,
                            onBack = { onNavigate("artist/$artistId") }
                        )
                    }
                }
                currentRoute.startsWith("playlist/") -> {
                    // Playlist detail
                    val playlistId = currentRoute.substring("playlist/".length)
                    PlaylistDetailPage(
                        playlistId = playlistId,
                        apolloClient = apolloClient,
                        audioPlayer = audioPlayer,
                        onBack = { onNavigate("playlists") }
                    )
                }
                currentRoute.startsWith("artist/") -> {
                    // Artist detail
                    val artistId = currentRoute.substring("artist/".length)
                    ArtistDetailPage(
                        artistId = artistId,
                        apolloClient = apolloClient,
                        audioPlayer = audioPlayer,
                        onNavigateToAlbum = { artId, folderName ->
                            onNavigate("album/$artId/$folderName")
                        },
                        onBack = { onNavigate("artists") }
                    )
                }
                else -> {
                    // Fallback to artists
                    ArtistListPage(
                        apolloClient = apolloClient,
                        onNavigateToArtist = { artistId -> onNavigate("artist/$artistId") }
                    )
                }
            }
        }
        }

        // Player bar at the bottom
        PlayerBar(audioPlayer = audioPlayer)
    }
}

@Composable
fun AppSidebar(
    currentRoute: String,
    onNavigate: (String) -> Unit,
    onSignOut: () -> Unit
) {
    Surface(
        modifier = Modifier.width(256.dp).fillMaxHeight(),
        color = MaterialTheme.colorScheme.surfaceVariant
    ) {
        Column(
            modifier = Modifier.fillMaxSize().padding(16.dp),
            verticalArrangement = Arrangement.SpaceBetween
        ) {
            Column {
                Text(
                    "🎵 MMusic",
                    style = MaterialTheme.typography.headlineMedium,
                    modifier = Modifier.padding(bottom = 32.dp)
                )

                SidebarButton(
                    text = "Search",
                    isActive = currentRoute == "search",
                    onClick = { onNavigate("search") }
                )

                Spacer(modifier = Modifier.height(8.dp))

                SidebarButton(
                    text = "Artists",
                    isActive = currentRoute == "artists",
                    onClick = { onNavigate("artists") }
                )

                SidebarButton(
                    text = "Albums",
                    isActive = currentRoute == "albums",
                    onClick = { onNavigate("albums") }
                )

                SidebarButton(
                    text = "Playlists",
                    isActive = currentRoute == "playlists",
                    onClick = { onNavigate("playlists") }
                )
            }

            Button(
                onClick = onSignOut,
                modifier = Modifier.fillMaxWidth()
            ) {
                Text("Sign Out")
            }
        }
    }
}

@Composable
fun SidebarButton(text: String, isActive: Boolean, onClick: () -> Unit) {
    Button(
        onClick = onClick,
        modifier = Modifier.fillMaxWidth().padding(vertical = 4.dp),
        colors = ButtonDefaults.buttonColors(
            containerColor = if (isActive)
                MaterialTheme.colorScheme.primary
            else
                MaterialTheme.colorScheme.surface
        )
    ) {
        Text(text)
    }
}
