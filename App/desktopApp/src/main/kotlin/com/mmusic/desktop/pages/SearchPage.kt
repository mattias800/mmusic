package com.mmusic.desktop.pages

import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.foundation.rememberScrollState
import androidx.compose.foundation.verticalScroll
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.mmusic.desktop.components.SearchBar
import com.mmusic.shared.graphql.SearchQuery
import com.mmusic.shared.services.ApolloGraphQLClient
import kotlinx.coroutines.launch

@Composable
fun SearchPage(
    apolloClient: ApolloGraphQLClient,
    onNavigateToArtist: (String) -> Unit,
    onNavigateToAlbum: (String, String) -> Unit
) {
    var searchQuery by remember { mutableStateOf("") }
    var searchResults by remember { mutableStateOf<SearchQuery.Data?>(null) }
    var isLoading by remember { mutableStateOf(false) }
    var error by remember { mutableStateOf<String?>(null) }
    val coroutineScope = rememberCoroutineScope()

    fun performSearch(query: String) {
        if (query.isBlank()) {
            searchResults = null
            isLoading = false
            error = null
            return
        }

        isLoading = true
        error = null
        coroutineScope.launch {
            try {
                val response = apolloClient.search(query, 20)
                if (response.hasErrors()) {
                    error = response.errors?.firstOrNull()?.message ?: "Search failed"
                } else {
                    searchResults = response.data
                }
                isLoading = false
            } catch (e: Exception) {
                error = e.message ?: "Failed to search"
                isLoading = false
            }
        }
    }

    Column(modifier = Modifier.fillMaxSize().padding(32.dp)) {
        Text(
            "Search",
            style = MaterialTheme.typography.headlineLarge,
            modifier = Modifier.padding(bottom = 24.dp)
        )

        SearchBar(
            value = searchQuery,
            onValueChange = { searchQuery = it },
            onSearch = { performSearch(it) },
            modifier = Modifier.padding(bottom = 24.dp)
        )

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
                Text(
                    "Error: $error",
                    color = MaterialTheme.colorScheme.error,
                    style = MaterialTheme.typography.bodyLarge
                )
            }
            searchResults == null -> {
                Box(
                    modifier = Modifier.fillMaxSize(),
                    contentAlignment = Alignment.Center
                ) {
                    Text(
                        "Search for artists, albums, or tracks",
                        style = MaterialTheme.typography.bodyLarge,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
            else -> {
                Column(
                    modifier = Modifier
                        .fillMaxSize()
                        .verticalScroll(rememberScrollState()),
                    verticalArrangement = Arrangement.spacedBy(24.dp)
                ) {
                    val results = searchResults!!.serverLibrary

                    // Artists
                    if (results.searchArtists.isNotEmpty()) {
                        SearchSection(
                            title = "Artists (${results.searchArtists.size})",
                            content = {
                                Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    results.searchArtists.take(5).forEach { artist ->
                                        SearchArtistItem(
                                            artist = artist,
                                            onClick = { onNavigateToArtist(artist.artistCard.id) }
                                        )
                                    }
                                }
                            }
                        )
                    }

                    // Albums
                    if (results.searchReleases.isNotEmpty()) {
                        SearchSection(
                            title = "Albums (${results.searchReleases.size})",
                            content = {
                                Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    results.searchReleases.take(5).forEach { release ->
                                        SearchAlbumItem(
                                            release = release,
                                            onClick = {
                                                onNavigateToAlbum(
                                                    release.albumCard.artist.id,
                                                    release.albumCard.folderName
                                                )
                                            }
                                        )
                                    }
                                }
                            }
                        )
                    }

                    // Tracks
                    if (results.searchTracks.isNotEmpty()) {
                        SearchSection(
                            title = "Tracks (${results.searchTracks.size})",
                            content = {
                                Column(verticalArrangement = Arrangement.spacedBy(8.dp)) {
                                    results.searchTracks.take(10).forEach { track ->
                                        SearchTrackItem(track = track)
                                    }
                                }
                            }
                        )
                    }

                    // No results
                    if (results.searchArtists.isEmpty() &&
                        results.searchReleases.isEmpty() &&
                        results.searchTracks.isEmpty()
                    ) {
                        Box(
                            modifier = Modifier.fillMaxWidth().padding(32.dp),
                            contentAlignment = Alignment.Center
                        ) {
                            Text(
                                "No results found for \"$searchQuery\"",
                                style = MaterialTheme.typography.bodyLarge,
                                color = MaterialTheme.colorScheme.onSurfaceVariant
                            )
                        }
                    }
                }
            }
        }
    }
}

@Composable
fun SearchSection(
    title: String,
    content: @Composable () -> Unit
) {
    Column {
        Text(
            text = title,
            style = MaterialTheme.typography.titleLarge,
            modifier = Modifier.padding(bottom = 12.dp)
        )
        content()
    }
}

@Composable
fun SearchArtistItem(
    artist: SearchQuery.SearchArtist,
    onClick: () -> Unit
) {
    Surface(
        onClick = onClick,
        modifier = Modifier.fillMaxWidth(),
        color = MaterialTheme.colorScheme.surface,
        shape = MaterialTheme.shapes.medium
    ) {
        Row(
            modifier = Modifier.padding(16.dp),
            horizontalArrangement = Arrangement.spacedBy(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(
                modifier = Modifier.size(48.dp),
                contentAlignment = Alignment.Center
            ) {
                Text("🎤", style = MaterialTheme.typography.headlineMedium)
            }
            Column {
                Text(
                    text = artist.artistCard.name,
                    style = MaterialTheme.typography.titleMedium,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                artist.artistCard.listeners?.let { listeners ->
                    Text(
                        text = "$listeners listeners",
                        style = MaterialTheme.typography.bodySmall,
                        color = MaterialTheme.colorScheme.onSurfaceVariant
                    )
                }
            }
        }
    }
}

@Composable
fun SearchAlbumItem(
    release: SearchQuery.SearchRelease,
    onClick: () -> Unit
) {
    Surface(
        onClick = onClick,
        modifier = Modifier.fillMaxWidth(),
        color = MaterialTheme.colorScheme.surface,
        shape = MaterialTheme.shapes.medium
    ) {
        Row(
            modifier = Modifier.padding(16.dp),
            horizontalArrangement = Arrangement.spacedBy(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(
                modifier = Modifier.size(48.dp),
                contentAlignment = Alignment.Center
            ) {
                Text("💿", style = MaterialTheme.typography.headlineMedium)
            }
            Column {
                Text(
                    text = release.albumCard.title,
                    style = MaterialTheme.typography.titleMedium,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                Text(
                    text = release.albumCard.artist.name,
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
    }
}

@Composable
fun SearchTrackItem(
    track: SearchQuery.SearchTrack
) {
    Surface(
        modifier = Modifier.fillMaxWidth(),
        color = MaterialTheme.colorScheme.surface,
        shape = MaterialTheme.shapes.medium
    ) {
        Row(
            modifier = Modifier.padding(16.dp),
            horizontalArrangement = Arrangement.spacedBy(16.dp),
            verticalAlignment = Alignment.CenterVertically
        ) {
            Box(
                modifier = Modifier.size(40.dp),
                contentAlignment = Alignment.Center
            ) {
                Text("🎵", style = MaterialTheme.typography.titleLarge)
            }
            Column(modifier = Modifier.weight(1f)) {
                Text(
                    text = track.title,
                    style = MaterialTheme.typography.bodyLarge,
                    maxLines = 1,
                    overflow = TextOverflow.Ellipsis
                )
                Text(
                    text = track.release?.artist?.name ?: "Unknown Artist",
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
            track.trackLength?.let { duration ->
                Text(
                    text = formatDuration(duration),
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
    }
}
