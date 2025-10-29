package com.mmusic.desktop.pages

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.MoreVert
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.mmusic.shared.graphql.GetAlbumDetailQuery
import com.mmusic.shared.graphql.GetPlaylistsQuery
import com.mmusic.shared.services.ApolloGraphQLClient
import com.mmusic.shared.services.AudioPlayer
import com.mmusic.shared.services.Track
import kotlinx.coroutines.launch

@Composable
fun AlbumDetailPage(
    artistId: String,
    releaseFolderName: String,
    apolloClient: ApolloGraphQLClient,
    audioPlayer: AudioPlayer,
    onBack: () -> Unit
) {
    var album by remember { mutableStateOf<GetAlbumDetailQuery.ReleaseByFolderName?>(null) }
    var playlists by remember { mutableStateOf<List<GetPlaylistsQuery.Playlist>>(emptyList()) }
    var isLoading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    val coroutineScope = rememberCoroutineScope()

    LaunchedEffect(artistId, releaseFolderName) {
        coroutineScope.launch {
            try {
                // Fetch album and playlists in parallel
                val albumResponse = apolloClient.getAlbumDetail(artistId, releaseFolderName)
                val playlistsResponse = apolloClient.getPlaylists()

                if (albumResponse.hasErrors()) {
                    error = albumResponse.errors?.firstOrNull()?.message ?: "Unknown error"
                } else {
                    album = albumResponse.data?.serverLibrary?.artistById?.releaseByFolderName
                }

                if (!playlistsResponse.hasErrors()) {
                    playlists = playlistsResponse.data?.viewer?.playlists ?: emptyList()
                }

                isLoading = false
            } catch (e: Exception) {
                error = e.message ?: "Failed to load album"
                isLoading = false
            }
        }
    }

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
        album != null -> {
            Column(
                modifier = Modifier
                    .fillMaxSize()
                    .verticalScroll(rememberScrollState())
                    .padding(32.dp)
            ) {
                // Album Header
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.spacedBy(24.dp)
                ) {
                    // Cover Art
                    Box(
                        modifier = Modifier.size(240.dp),
                        contentAlignment = Alignment.Center
                    ) {
                        Card(
                            modifier = Modifier.fillMaxSize(),
                            colors = CardDefaults.cardColors(
                                containerColor = MaterialTheme.colorScheme.surfaceVariant
                            )
                        ) {
                            Box(
                                modifier = Modifier.fillMaxSize(),
                                contentAlignment = Alignment.Center
                            ) {
                                Text("💿", style = MaterialTheme.typography.displayLarge)
                            }
                        }
                    }

                    // Album Info
                    Column(
                        modifier = Modifier.weight(1f),
                        verticalArrangement = Arrangement.spacedBy(8.dp)
                    ) {
                        album!!.type?.let { type ->
                            Text(
                                text = type.toString(),
                                style = MaterialTheme.typography.labelMedium,
                                color = MaterialTheme.colorScheme.primary
                            )
                        }

                        Text(
                            text = album!!.title,
                            style = MaterialTheme.typography.displayMedium
                        )

                        Text(
                            text = album!!.artist.name,
                            style = MaterialTheme.typography.titleLarge,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )

                        Row(horizontalArrangement = Arrangement.spacedBy(16.dp)) {
                            album!!.firstReleaseYear?.let { year ->
                                Text(
                                    text = year,
                                    style = MaterialTheme.typography.bodyMedium
                                )
                            }
                            Text(
                                text = "${album!!.tracks.size} tracks",
                                style = MaterialTheme.typography.bodyMedium
                            )
                            val totalDuration = album!!.tracks.sumOf { it.trackLength ?: 0 }
                            Text(
                                text = formatDuration(totalDuration),
                                style = MaterialTheme.typography.bodyMedium
                            )
                        }

                        Spacer(modifier = Modifier.height(16.dp))

                        Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                            Button(onClick = {
                                val tracks = album!!.tracks
                                    .filter { track -> !track.isMissing }
                                    .sortedBy { it.discNumber * 1000 + it.trackNumber }
                                    .mapIndexed { _, track -> track.toAudioTrack(album!!.artist.name, album!!.title) }
                                    .filter { it.audioUrl.isNotBlank() }
                                if (tracks.isNotEmpty()) {
                                    audioPlayer.playTracks(tracks)
                                }
                            }) {
                                Text("▶ Play")
                            }
                            OutlinedButton(onClick = {
                                val tracks = album!!.tracks
                                    .filter { track -> !track.isMissing }
                                    .map { it.toAudioTrack(album!!.artist.name, album!!.title) }
                                    .filter { it.audioUrl.isNotBlank() }
                                    .shuffled()
                                if (tracks.isNotEmpty()) {
                                    audioPlayer.playTracks(tracks)
                                }
                            }) {
                                Text("🔀 Shuffle")
                            }
                        }
                    }
                }

                Spacer(modifier = Modifier.height(32.dp))

                // Track List
                Text(
                    text = "Tracks",
                    style = MaterialTheme.typography.headlineSmall,
                    modifier = Modifier.padding(bottom = 16.dp)
                )

                Column(verticalArrangement = Arrangement.spacedBy(4.dp)) {
                    val allTracks = album!!.tracks
                        .sortedBy { it.discNumber * 1000 + it.trackNumber }

                    album!!.tracks.groupBy { it.discNumber }.forEach { (discNum, discTracks) ->
                        if (album!!.tracks.maxOf { it.discNumber } > 1) {
                            Text(
                                text = "Disc $discNum",
                                style = MaterialTheme.typography.titleSmall,
                                color = MaterialTheme.colorScheme.onSurfaceVariant,
                                modifier = Modifier.padding(vertical = 8.dp)
                            )
                        }

                        discTracks.sortedBy { it.trackNumber }.forEach { track ->
                            AlbumTrackItem(
                                track = track,
                                artistId = artistId,
                                releaseFolderName = releaseFolderName,
                                playlists = playlists,
                                apolloClient = apolloClient,
                                onClick = {
                                    if (!track.isMissing) {
                                        val trackIndex = allTracks.indexOf(track)
                                        val playableTracks = allTracks
                                            .filter { t -> !t.isMissing }
                                            .map { it.toAudioTrack(album!!.artist.name, album!!.title) }
                                            .filter { it.audioUrl.isNotBlank() }
                                        val playableIndex = playableTracks.indexOfFirst {
                                            it.id == track.id
                                        }
                                        if (playableIndex >= 0) {
                                            audioPlayer.playTracks(playableTracks, playableIndex)
                                        }
                                    }
                                }
                            )
                        }
                    }
                }
            }
        }
    }
}

@Composable
fun AlbumTrackItem(
    track: GetAlbumDetailQuery.Track,
    artistId: String,
    releaseFolderName: String,
    playlists: List<GetPlaylistsQuery.Playlist>,
    apolloClient: ApolloGraphQLClient,
    onClick: () -> Unit = {}
) {
    var showMenu by remember { mutableStateOf(false) }
    val coroutineScope = rememberCoroutineScope()

    Surface(
        onClick = onClick,
        modifier = Modifier.fillMaxWidth(),
        enabled = !track.isMissing,
        color = if (track.isMissing) {
            MaterialTheme.colorScheme.surface.copy(alpha = 0.5f)
        } else {
            MaterialTheme.colorScheme.surface
        },
        shape = MaterialTheme.shapes.small
    ) {
        Row(
            modifier = Modifier.padding(12.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            // Track number
            Text(
                text = "${track.trackNumber}",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                modifier = Modifier.width(32.dp)
            )

            // Track title
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = track.title,
                    style = MaterialTheme.typography.bodyLarge,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis,
                    color = if (track.isMissing) {
                        MaterialTheme.colorScheme.onSurface.copy(alpha = 0.5f)
                    } else {
                        MaterialTheme.colorScheme.onSurface
                    }
                )
                if (track.isMissing) {
                    Text(
                        text = "Missing audio",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.error
                    )
                }
            }

            // Play count
            track.playCount?.let { count ->
                Text(
                    text = formatPlayCount(count),
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }

            // Duration
            track.trackLength?.let { duration ->
                Text(
                    text = formatDuration(duration),
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.width(48.dp)
                )
            }

            // Menu button
            Box {
                IconButton(
                    onClick = { showMenu = true },
                    enabled = !track.isMissing
                ) {
                    Icon(
                        Icons.Default.MoreVert,
                        contentDescription = "More options",
                        tint = if (track.isMissing) {
                            MaterialTheme.colorScheme.onSurface.copy(alpha = 0.3f)
                        } else {
                            MaterialTheme.colorScheme.onSurfaceVariant
                        }
                    )
                }

                DropdownMenu(
                    expanded = showMenu,
                    onDismissRequest = { showMenu = false }
                ) {
                    if (playlists.isEmpty()) {
                        DropdownMenuItem(
                            text = { Text("No playlists available") },
                            onClick = { },
                            enabled = false
                        )
                    } else {
                        playlists.forEach { playlist ->
                            DropdownMenuItem(
                                text = { Text(playlist.playlistCard.name ?: "Untitled") },
                                onClick = {
                                    coroutineScope.launch {
                                        try {
                                            apolloClient.addTrackToPlaylist(
                                                playlistId = playlist.playlistCard.id,
                                                artistId = artistId,
                                                releaseFolderName = releaseFolderName,
                                                trackNumber = track.trackNumber
                                            )
                                            showMenu = false
                                        } catch (e: Exception) {
                                            // Could show a snackbar here
                                            showMenu = false
                                        }
                                    }
                                }
                            )
                        }
                    }
                }
            }
        }
    }
}

fun formatPlayCount(count: Any): String {
    val num = when (count) {
        is Long -> count
        is Int -> count.toLong()
        else -> return count.toString()
    }
    return when {
        num >= 1_000_000 -> "%.1fM".format(num / 1_000_000.0)
        num >= 1_000 -> "%.1fK".format(num / 1_000.0)
        else -> num.toString()
    }
}

/**
 * Convert a GraphQL track to an AudioPlayer Track
 */
fun GetAlbumDetailQuery.Track.toAudioTrack(artistName: String, albumTitle: String): Track {
    val audioUrl = media?.audioUrl ?: ""
    val durationMs = (trackLength ?: 0) * 1000L

    return Track(
        id = id,
        title = title,
        artistName = artistName,
        albumTitle = albumTitle,
        audioUrl = audioUrl,
        durationMs = durationMs,
        coverArtUrl = null // Could be passed from album if needed
    )
}

fun formatDuration(seconds: Int): String {
    val mins = seconds / 60
    val secs = seconds % 60
    return "%d:%02d".format(mins, secs)
}
