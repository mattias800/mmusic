package com.mmusic.desktop.pages

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.itemsIndexed
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Delete
import androidx.compose.material.icons.filled.Edit
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.mmusic.shared.graphql.GetPlaylistsWithDetailsQuery
import com.mmusic.shared.services.ApolloGraphQLClient
import com.mmusic.shared.services.AudioPlayer
import com.mmusic.shared.services.Track
import kotlinx.coroutines.launch

@Composable
fun PlaylistDetailPage(
    playlistId: String,
    apolloClient: ApolloGraphQLClient,
    audioPlayer: AudioPlayer,
    onBack: () -> Unit
) {
    var playlist by remember { mutableStateOf<GetPlaylistsWithDetailsQuery.Playlist?>(null) }
    var isLoading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    var showRenameDialog by remember { mutableStateOf(false) }
    var showDeleteDialog by remember { mutableStateOf(false) }
    val coroutineScope = rememberCoroutineScope()

    fun loadPlaylist() {
        coroutineScope.launch {
            try {
                isLoading = true
                val response = apolloClient.getPlaylistsWithDetails()
                if (response.hasErrors()) {
                    error = response.errors?.firstOrNull()?.message ?: "Unknown error"
                } else {
                    // Find the specific playlist by ID
                    playlist = response.data?.viewer?.playlists?.find { it.id == playlistId }
                    if (playlist == null) {
                        error = "Playlist not found"
                    }
                }
                isLoading = false
            } catch (e: Exception) {
                error = e.message ?: "Failed to load playlist"
                isLoading = false
            }
        }
    }

    LaunchedEffect(playlistId) {
        loadPlaylist()
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
        playlist != null -> {
            Column(modifier = Modifier.fillMaxSize().padding(32.dp)) {
                // Header
                Row(
                    modifier = Modifier.fillMaxWidth(),
                    horizontalArrangement = Arrangement.SpaceBetween,
                    verticalAlignment = Alignment.CenterVertically
                ) {
                    Column(modifier = Modifier.weight(1f)) {
                        Text(
                            text = playlist!!.name ?: "Untitled Playlist",
                            style = MaterialTheme.typography.displayMedium
                        )
                        Text(
                            text = "${playlist!!.items.size} tracks",
                            style = MaterialTheme.typography.bodyLarge,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }

                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        IconButton(onClick = { showRenameDialog = true }) {
                            Icon(Icons.Default.Edit, contentDescription = "Rename Playlist")
                        }
                        IconButton(onClick = { showDeleteDialog = true }) {
                            Icon(Icons.Default.Delete, contentDescription = "Delete Playlist")
                        }
                    }
                }

                Spacer(modifier = Modifier.height(24.dp))

                // Play/Shuffle buttons
                if (playlist!!.items.isNotEmpty()) {
                    Row(horizontalArrangement = Arrangement.spacedBy(8.dp)) {
                        Button(onClick = {
                            val tracks = playlist!!.items
                                .mapNotNull { it.toAudioTrack() }
                                .filter { it.audioUrl.isNotBlank() }
                            if (tracks.isNotEmpty()) {
                                audioPlayer.playTracks(tracks)
                            }
                        }) {
                            Text("▶ Play")
                        }
                        OutlinedButton(onClick = {
                            val tracks = playlist!!.items
                                .mapNotNull { it.toAudioTrack() }
                                .filter { it.audioUrl.isNotBlank() }
                                .shuffled()
                            if (tracks.isNotEmpty()) {
                                audioPlayer.playTracks(tracks)
                            }
                        }) {
                            Text("🔀 Shuffle")
                        }
                    }

                    Spacer(modifier = Modifier.height(24.dp))
                }

                // Playlist items
                if (playlist!!.items.isEmpty()) {
                    Box(
                        modifier = Modifier.fillMaxSize(),
                        contentAlignment = Alignment.Center
                    ) {
                        Text(
                            "This playlist is empty",
                            style = MaterialTheme.typography.bodyLarge,
                            color = MaterialTheme.colorScheme.onSurfaceVariant
                        )
                    }
                } else {
                    LazyColumn(
                        verticalArrangement = Arrangement.spacedBy(4.dp)
                    ) {
                        itemsIndexed(playlist!!.items) { index, item ->
                            PlaylistItemRow(
                                item = item,
                                index = index,
                                onClick = {
                                    val tracks = playlist!!.items
                                        .mapNotNull { it.toAudioTrack() }
                                        .filter { it.audioUrl.isNotBlank() }
                                    val playIndex = tracks.indexOfFirst { it.id == item.playlistItemDetail.track?.id }
                                    if (playIndex >= 0) {
                                        audioPlayer.playTracks(tracks, playIndex)
                                    }
                                },
                                onRemove = {
                                    coroutineScope.launch {
                                        try {
                                            val response = apolloClient.removeItemFromPlaylist(playlistId, item.playlistItemDetail.id)
                                            if (!response.hasErrors()) {
                                                loadPlaylist()
                                            } else {
                                                error = response.errors?.firstOrNull()?.message ?: "Failed to remove item"
                                            }
                                        } catch (e: Exception) {
                                            error = e.message ?: "Failed to remove item"
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

    // Rename Dialog
    if (showRenameDialog) {
        var newName by remember { mutableStateOf(playlist?.name ?: "") }
        AlertDialog(
            onDismissRequest = { showRenameDialog = false },
            title = { Text("Rename Playlist") },
            text = {
                Column {
                    Text("Enter a new name:")
                    Spacer(modifier = Modifier.height(16.dp))
                    OutlinedTextField(
                        value = newName,
                        onValueChange = { newName = it },
                        label = { Text("Playlist Name") },
                        singleLine = true,
                        modifier = Modifier.fillMaxWidth()
                    )
                }
            },
            confirmButton = {
                Button(
                    onClick = {
                        coroutineScope.launch {
                            try {
                                val response = apolloClient.renamePlaylist(playlistId, newName)
                                if (!response.hasErrors()) {
                                    showRenameDialog = false
                                    loadPlaylist()
                                } else {
                                    error = response.errors?.firstOrNull()?.message ?: "Failed to rename playlist"
                                }
                            } catch (e: Exception) {
                                error = e.message ?: "Failed to rename playlist"
                            }
                        }
                    },
                    enabled = newName.isNotBlank()
                ) {
                    Text("Rename")
                }
            },
            dismissButton = {
                TextButton(onClick = { showRenameDialog = false }) {
                    Text("Cancel")
                }
            }
        )
    }

    // Delete Dialog
    if (showDeleteDialog) {
        AlertDialog(
            onDismissRequest = { showDeleteDialog = false },
            title = { Text("Delete Playlist") },
            text = {
                Text("Are you sure you want to delete \"${playlist?.name ?: "this playlist"}\"? This action cannot be undone.")
            },
            confirmButton = {
                Button(
                    onClick = {
                        coroutineScope.launch {
                            try {
                                val response = apolloClient.deletePlaylist(playlistId)
                                if (!response.hasErrors()) {
                                    onBack()
                                } else {
                                    error = response.errors?.firstOrNull()?.message ?: "Failed to delete playlist"
                                    showDeleteDialog = false
                                }
                            } catch (e: Exception) {
                                error = e.message ?: "Failed to delete playlist"
                                showDeleteDialog = false
                            }
                        }
                    },
                    colors = ButtonDefaults.buttonColors(
                        containerColor = MaterialTheme.colorScheme.error
                    )
                ) {
                    Text("Delete")
                }
            },
            dismissButton = {
                TextButton(onClick = { showDeleteDialog = false }) {
                    Text("Cancel")
                }
            }
        )
    }
}

@Composable
fun PlaylistItemRow(
    item: GetPlaylistsWithDetailsQuery.Item,
    index: Int,
    onClick: () -> Unit,
    onRemove: () -> Unit
) {
    Surface(
        onClick = onClick,
        modifier = Modifier.fillMaxWidth(),
        color = MaterialTheme.colorScheme.surface,
        shape = MaterialTheme.shapes.small
    ) {
        Row(
            modifier = Modifier.padding(12.dp),
            verticalAlignment = Alignment.CenterVertically,
            horizontalArrangement = Arrangement.spacedBy(12.dp)
        ) {
            // Index
            Text(
                text = "${index + 1}",
                style = MaterialTheme.typography.bodyMedium,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                modifier = Modifier.width(32.dp)
            )

            // Track info
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = item.playlistItemDetail.title ?: "Unknown Track",
                    style = MaterialTheme.typography.bodyLarge,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                Text(
                    text = item.playlistItemDetail.artistName ?: "Unknown Artist",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
            }

            // Duration
            item.playlistItemDetail.trackLengthMs?.let { durationMs ->
                val durationSec = durationMs / 1000
                Text(
                    text = formatDuration(durationSec),
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant,
                    modifier = Modifier.width(48.dp)
                )
            }

            // Remove button
            IconButton(onClick = onRemove) {
                Icon(
                    Icons.Default.Delete,
                    contentDescription = "Remove from playlist",
                    tint = MaterialTheme.colorScheme.error
                )
            }
        }
    }
}

fun GetPlaylistsWithDetailsQuery.Item.toAudioTrack(): Track? {
    val track = this.playlistItemDetail.track ?: return null
    val audioUrl = track.media?.audioUrl ?: return null
    if (audioUrl.isBlank()) return null

    return Track(
        id = track.id,
        title = track.title,
        artistName = track.release.artist.name,
        albumTitle = track.release.title,
        audioUrl = audioUrl,
        durationMs = (track.trackLength ?: 0) * 1000L,
        coverArtUrl = track.release.coverArtUrl
    )
}
