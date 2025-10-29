package com.mmusic.desktop.pages

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Brush
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.mmusic.desktop.ui.theme.AppColors
import com.mmusic.shared.graphql.GetArtistDetailQuery
import com.mmusic.shared.services.ApolloGraphQLClient
import kotlinx.coroutines.launch

@Composable
fun ArtistDetailPage(
    artistId: String,
    apolloClient: ApolloGraphQLClient,
    audioPlayer: com.mmusic.shared.services.AudioPlayer,
    onNavigateToAlbum: (String, String) -> Unit,
    onBack: () -> Unit
) {
    var artist by remember { mutableStateOf<GetArtistDetailQuery.ArtistById?>(null) }
    var isLoading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    var selectedTab by remember { mutableStateOf(0) }
    val coroutineScope = rememberCoroutineScope()

    LaunchedEffect(artistId) {
        coroutineScope.launch {
            try {
                val response = apolloClient.getArtistDetail(artistId)
                if (response.hasErrors()) {
                    error = response.errors?.firstOrNull()?.message ?: "Unknown error"
                } else {
                    artist = response.data?.serverLibrary?.artistById
                }
                isLoading = false
            } catch (e: Exception) {
                error = e.message ?: "Failed to load artist"
                isLoading = false
            }
        }
    }

    Column(modifier = Modifier.fillMaxSize()) {
        when {
            isLoading -> {
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    CircularProgressIndicator()
                }
            }
            error != null -> {
                Box(
                    modifier = Modifier.fillMaxSize().padding(32.dp),
                    contentAlignment = Alignment.Center
                ) {
                    Column(horizontalAlignment = Alignment.CenterHorizontally) {
                        Text("Error: $error", color = MaterialTheme.colorScheme.error)
                        Spacer(modifier = Modifier.height(16.dp))
                        Button(onClick = onBack) {
                            Text("Go Back")
                        }
                    }
                }
            }
            artist != null -> {
                // Header
                ArtistHeader(artist!!)

                // Tabs
                TabRow(selectedTabIndex = selectedTab) {
                    Tab(
                        selected = selectedTab == 0,
                        onClick = { selectedTab = 0 },
                        text = { Text("Top Tracks") }
                    )
                    Tab(
                        selected = selectedTab == 1,
                        onClick = { selectedTab = 1 },
                        text = { Text("Albums") }
                    )
                    Tab(
                        selected = selectedTab == 2,
                        onClick = { selectedTab = 2 },
                        text = { Text("EPs") }
                    )
                    Tab(
                        selected = selectedTab == 3,
                        onClick = { selectedTab = 3 },
                        text = { Text("Singles") }
                    )
                }

                // Tab content
                when (selectedTab) {
                    0 -> TopTracksTab(artist!!.topTracks)
                    1 -> AlbumsTab(artist!!.albums, onNavigateToAlbum, artistId)
                    2 -> EpsTab(artist!!.eps, onNavigateToAlbum, artistId)
                    3 -> SinglesTab(artist!!.singles, onNavigateToAlbum, artistId)
                }
            }
        }
    }
}

@Composable
fun ArtistHeader(artist: GetArtistDetailQuery.ArtistById) {
    Box(
        modifier = Modifier
            .fillMaxWidth()
            .height(300.dp)
            .background(
                Brush.verticalGradient(
                    colors = listOf(
                        AppColors.primary.copy(alpha = 0.3f),
                        AppColors.background
                    )
                )
            )
            .padding(32.dp),
        contentAlignment = Alignment.BottomStart
    ) {
        Column {
            Text(
                text = artist.name,
                style = MaterialTheme.typography.displayLarge,
                color = Color.White
            )
            artist.listeners?.let { listeners ->
                Text(
                    text = "$listeners monthly listeners",
                    style = MaterialTheme.typography.titleMedium,
                    color = Color.White.copy(alpha = 0.7f)
                )
            }
        }
    }
}

@Composable
fun TopTracksTab(tracks: List<GetArtistDetailQuery.TopTrack>) {
    if (tracks.isEmpty()) {
        Box(
            modifier = Modifier.fillMaxSize().padding(32.dp),
            contentAlignment = Alignment.Center
        ) {
            Text("No top tracks available")
        }
    } else {
        Column(
            modifier = Modifier
                .fillMaxSize()
                .verticalScroll(rememberScrollState())
                .padding(16.dp),
            verticalArrangement = Arrangement.spacedBy(8.dp)
        ) {
            tracks.forEachIndexed { index, track ->
                TrackItem(
                    index = index + 1,
                    title = track.title,
                    duration = track.trackLength,
                    playCount = track.playCount
                )
            }
        }
    }
}

@Composable
fun TrackItem(
    index: Int,
    title: String,
    duration: Int?,
    playCount: Any?
) {
    Surface(
        modifier = Modifier.fillMaxWidth(),
        color = MaterialTheme.colorScheme.surface,
        shape = MaterialTheme.shapes.medium
    ) {
        Row(
            modifier = Modifier.padding(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Text(
                text = "$index",
                style = MaterialTheme.typography.bodyLarge,
                modifier = Modifier.width(32.dp)
            )
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = title,
                    style = MaterialTheme.typography.bodyLarge,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
            }
            duration?.let { dur ->
                Text(
                    text = formatDuration(dur),
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
    }
}

@Composable
fun AlbumsTab(
    releases: List<GetArtistDetailQuery.Album>,
    onNavigateToAlbum: (String, String) -> Unit,
    artistId: String
) {
    if (releases.isEmpty()) {
        Box(
            modifier = Modifier.fillMaxSize().padding(32.dp),
            contentAlignment = Alignment.Center
        ) {
            Text("No albums available")
        }
    } else {
        LazyVerticalGrid(
            columns = GridCells.Adaptive(180.dp),
            modifier = Modifier.fillMaxSize().padding(16.dp),
            horizontalArrangement = Arrangement.spacedBy(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            items(releases) { release ->
                ReleaseCardFromAlbum(
                    release = release,
                    onClick = { onNavigateToAlbum(artistId, release.folderName) }
                )
            }
        }
    }
}

@Composable
fun EpsTab(
    releases: List<GetArtistDetailQuery.Ep>,
    onNavigateToAlbum: (String, String) -> Unit,
    artistId: String
) {
    if (releases.isEmpty()) {
        Box(
            modifier = Modifier.fillMaxSize().padding(32.dp),
            contentAlignment = Alignment.Center
        ) {
            Text("No EPs available")
        }
    } else {
        LazyVerticalGrid(
            columns = GridCells.Adaptive(180.dp),
            modifier = Modifier.fillMaxSize().padding(16.dp),
            horizontalArrangement = Arrangement.spacedBy(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            items(releases) { release ->
                ReleaseCardFromEp(
                    release = release,
                    onClick = { onNavigateToAlbum(artistId, release.folderName) }
                )
            }
        }
    }
}

@Composable
fun SinglesTab(
    releases: List<GetArtistDetailQuery.Single>,
    onNavigateToAlbum: (String, String) -> Unit,
    artistId: String
) {
    if (releases.isEmpty()) {
        Box(
            modifier = Modifier.fillMaxSize().padding(32.dp),
            contentAlignment = Alignment.Center
        ) {
            Text("No singles available")
        }
    } else {
        LazyVerticalGrid(
            columns = GridCells.Adaptive(180.dp),
            modifier = Modifier.fillMaxSize().padding(16.dp),
            horizontalArrangement = Arrangement.spacedBy(16.dp),
            verticalArrangement = Arrangement.spacedBy(16.dp)
        ) {
            items(releases) { release ->
                ReleaseCardFromSingle(
                    release = release,
                    onClick = { onNavigateToAlbum(artistId, release.folderName) }
                )
            }
        }
    }
}

@Composable
fun ReleaseCardFromAlbum(
    release: GetArtistDetailQuery.Album,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .height(240.dp)
            .clickable(onClick = onClick),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.surfaceVariant
        )
    ) {
        Column(
            modifier = Modifier.fillMaxSize().padding(12.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Box(
                modifier = Modifier.size(140.dp),
                contentAlignment = Alignment.Center
            ) {
                Text("💿", style = MaterialTheme.typography.displayMedium)
            }

            Spacer(modifier = Modifier.height(8.dp))

            Text(
                text = release.title,
                style = MaterialTheme.typography.titleSmall,
                maxLines = 2,
                overflow = TextOverflow.Ellipsis
            )

            release.firstReleaseDate?.let { date ->
                Text(
                    text = date.take(4),
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
    }
}

@Composable
fun ReleaseCardFromEp(
    release: GetArtistDetailQuery.Ep,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .height(240.dp)
            .clickable(onClick = onClick),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.surfaceVariant
        )
    ) {
        Column(
            modifier = Modifier.fillMaxSize().padding(12.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Box(
                modifier = Modifier.size(140.dp),
                contentAlignment = Alignment.Center
            ) {
                Text("💿", style = MaterialTheme.typography.displayMedium)
            }

            Spacer(modifier = Modifier.height(8.dp))

            Text(
                text = release.title,
                style = MaterialTheme.typography.titleSmall,
                maxLines = 2,
                overflow = TextOverflow.Ellipsis
            )
        }
    }
}

@Composable
fun ReleaseCardFromSingle(
    release: GetArtistDetailQuery.Single,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .height(240.dp)
            .clickable(onClick = onClick),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.surfaceVariant
        )
    ) {
        Column(
            modifier = Modifier.fillMaxSize().padding(12.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            Box(
                modifier = Modifier.size(140.dp),
                contentAlignment = Alignment.Center
            ) {
                Text("💿", style = MaterialTheme.typography.displayMedium)
            }

            Spacer(modifier = Modifier.height(8.dp))

            Text(
                text = release.title,
                style = MaterialTheme.typography.titleSmall,
                maxLines = 2,
                overflow = TextOverflow.Ellipsis
            )
        }
    }
}
