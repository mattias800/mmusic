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
import com.mmusic.shared.graphql.GetArtistsQuery
import com.mmusic.shared.services.ApolloGraphQLClient
import kotlinx.coroutines.launch

@Composable
fun ArtistListPage(
    apolloClient: ApolloGraphQLClient,
    onNavigateToArtist: (String) -> Unit
) {
    var artists by remember { mutableStateOf<List<GetArtistsQuery.AllArtist>>(emptyList()) }
    var isLoading by remember { mutableStateOf(true) }
    var error by remember { mutableStateOf<String?>(null) }
    val coroutineScope = rememberCoroutineScope()

    LaunchedEffect(Unit) {
        coroutineScope.launch {
            try {
                val response = apolloClient.getArtists()
                if (response.hasErrors()) {
                    error = response.errors?.firstOrNull()?.message ?: "Unknown error"
                } else {
                    artists = response.data?.serverLibrary?.allArtists ?: emptyList()
                }
                isLoading = false
            } catch (e: Exception) {
                error = e.message ?: "Failed to load artists"
                isLoading = false
            }
        }
    }

    Column(modifier = Modifier.fillMaxSize().padding(32.dp)) {
        Text(
            "Artists",
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
            artists.isEmpty() -> {
                Text("No artists found", style = MaterialTheme.typography.bodyLarge)
            }
            else -> {
                LazyVerticalGrid(
                    columns = GridCells.Adaptive(200.dp),
                    horizontalArrangement = Arrangement.spacedBy(16.dp),
                    verticalArrangement = Arrangement.spacedBy(16.dp)
                ) {
                    items(artists) { artist ->
                        ArtistCard(
                            artist = artist,
                            onClick = { onNavigateToArtist(artist.artistCard.id) }
                        )
                    }
                }
            }
        }
    }
}

@Composable
fun ArtistCard(
    artist: GetArtistsQuery.AllArtist,
    onClick: () -> Unit
) {
    Card(
        modifier = Modifier
            .fillMaxWidth()
            .height(220.dp)
            .clickable(onClick = onClick),
        colors = CardDefaults.cardColors(
            containerColor = MaterialTheme.colorScheme.surfaceVariant
        )
    ) {
        Column(
            modifier = Modifier.fillMaxSize().padding(16.dp),
            verticalArrangement = Arrangement.Center,
            horizontalAlignment = Alignment.CenterHorizontally
        ) {
            // Placeholder for image
            Box(
                modifier = Modifier.size(120.dp),
                contentAlignment = Alignment.Center
            ) {
                Text("🎤", style = MaterialTheme.typography.displayLarge)
            }

            Spacer(modifier = Modifier.height(12.dp))

            Text(
                text = artist.artistCard.name,
                style = MaterialTheme.typography.titleMedium,
                maxLines = 2,
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
