package com.mmusic.desktop.pages

import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.grid.GridCells
import androidx.compose.foundation.lazy.grid.LazyVerticalGrid
import androidx.compose.foundation.lazy.grid.items
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.mmusic.shared.graphql.GetAlbumsQuery
import com.mmusic.shared.services.ApolloGraphQLClient
import kotlinx.coroutines.launch

@Composable
fun AlbumListPage(
    apolloClient: ApolloGraphQLClient,
    onNavigateToAlbum: (String, String) -> Unit
) {
    var albums by remember { mutableStateOf<List<GetAlbumsQuery.AllRelease>>(emptyList()) }
    var isLoading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    val coroutineScope = rememberCoroutineScope()

    LaunchedEffect(Unit) {
        coroutineScope.launch {
            try {
                val response = apolloClient.getAlbums()
                if (response.hasErrors()) {
                    error = response.errors?.firstOrNull()?.message ?: "Unknown error"
                } else {
                    albums = response.data?.serverLibrary?.allReleases ?: emptyList()
                }
                isLoading = false
            } catch (e: Exception) {
                error = e.message ?: "Failed to load albums"
                isLoading = false
            }
        }
    }

    Column(modifier = Modifier.fillMaxSize().padding(32.dp)) {
        Text(
            "Albums",
            style = MaterialTheme.typography.headlineLarge,
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
            albums.isEmpty() -> {
                Text("No albums found", style = MaterialTheme.typography.bodyLarge)
            }
            else -> {
                LazyVerticalGrid(
                    columns = GridCells.Adaptive(200.dp),
                    horizontalArrangement = Arrangement.spacedBy(16.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    items(albums) { album ->
                        AlbumCard(
                            album = album,
                            onClick = {
                                onNavigateToAlbum(
                                    album.albumCard.artist.id,
                                    album.albumCard.folderName
                                )
                            }
                        )
                    }
                }
            }
        }
    }
}

@Composable
fun AlbumCard(
    album: GetAlbumsQuery.AllRelease,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .height(260.dp)
            .clickable(onClick = onClick),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.surfaceVariant
        )
    ) {
        Column(
            modifier = Modifier.fillMaxSize().padding(16.dp),
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            // Placeholder for cover art
            Box(
                modifier = Modifier.size(160.dp),
                contentAlignment = Alignment.Center
            ) {
                Text("💿", style = MaterialTheme.typography.displayLarge)
            }

            Spacer(modifier = Modifier.height(12.dp))

            Text(
                text = album.albumCard.title,
                style = MaterialTheme.typography.titleMedium,
                maxLines = 2,
                overflow = TextOverflow.Ellipsis
            )

            Text(
                text = album.albumCard.artist.name,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )

            album.albumCard.firstReleaseDate?.let { date ->
                Text(
                    text = date.take(4), // Just year
                    style = MaterialTheme.typography.bodySmall,
                    color = MaterialTheme.colorScheme.onSurfaceVariant
                )
            }
        }
    }
}
