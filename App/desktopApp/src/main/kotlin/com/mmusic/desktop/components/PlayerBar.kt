package com.mmusic.desktop.components

import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Clear
import androidx.compose.material.icons.filled.PlayArrow
import androidx.compose.material.icons.filled.Search
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.text.style.TextOverflow
import androidx.compose.ui.unit.dp
import com.mmusic.shared.services.AudioPlayer
import com.mmusic.shared.services.Track

/**
 * Player bar component displayed at the bottom of the app
 * Shows current track, playback controls, progress bar, and volume
 */
@Composable
fun PlayerBar(
    audioPlayer: AudioPlayer,
    modifier: Modifier = Modifier
) {
    val currentTrack by audioPlayer.currentTrack.collectAsState()
    val isPlaying by audioPlayer.isPlaying.collectAsState()
    val currentPosition by audioPlayer.currentPosition.collectAsState()
    val duration by audioPlayer.duration.collectAsState()
    val volume by audioPlayer.volume.collectAsState()

    if (currentTrack == null) return

    Surface(
        modifier = modifier
            .fillMaxWidth()
            .height(90.dp),
        color = MaterialTheme.colorScheme.surfaceVariant,
        shadowElevation = 8.dp
    ) {
        Column(
            modifier = Modifier.fillMaxSize()
        ) {
            // Progress bar
            PlayerProgressBar(
                currentPosition = currentPosition,
                duration = duration,
                onSeek = { position -> audioPlayer.seekTo(position) }
            )

            // Main controls row
            Row(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(horizontal = 16.dp, vertical = 8.dp),
                horizontalArrangement = Arrangement.SpaceBetween,
                verticalAlignment = Alignment.CenterVertically
            ) {
                // Track info
                TrackInfo(
                    track = currentTrack!!,
                    modifier = Modifier.weight(1f)
                )

                // Playback controls
                PlaybackControls(
                    isPlaying = isPlaying,
                    onPlayPause = {
                        if (isPlaying) audioPlayer.pause() else audioPlayer.play()
                    },
                    onPrevious = { audioPlayer.previous() },
                    onNext = { audioPlayer.next() },
                    modifier = Modifier.weight(1f)
                )

                // Volume control
                VolumeControl(
                    volume = volume,
                    onVolumeChange = { audioPlayer.setVolume(it) },
                    modifier = Modifier.weight(1f)
                )
            }
        }
    }
}

@Composable
private fun PlayerProgressBar(
    currentPosition: Long,
    duration: Long,
    onSeek: (Long) -> Unit
) {
    var isDragging by remember { mutableStateOf(false) }
    var dragPosition by remember { mutableStateOf(0f) }

    val progress = if (duration > 0) {
        if (isDragging) dragPosition else currentPosition.toFloat() / duration.toFloat()
    } else 0f

    Column(
        modifier = Modifier
            .fillMaxWidth()
            .background(MaterialTheme.colorScheme.surface.copy(alpha = 0.5f))
    ) {
        Slider(
            value = progress,
            onValueChange = { value ->
                isDragging = true
                dragPosition = value
            },
            onValueChangeFinished = {
                onSeek((dragPosition * duration).toLong())
                isDragging = false
            },
            modifier = Modifier
                .fillMaxWidth()
                .height(4.dp)
        )

        // Time labels
        Row(
            modifier = Modifier
                .fillMaxWidth()
                .padding(horizontal = 16.dp),
            horizontalArrangement = Arrangement.SpaceBetween
        ) {
            Text(
                text = formatTime(currentPosition),
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
            Text(
                text = formatTime(duration),
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant
            )
        }
    }
}

@Composable
private fun TrackInfo(
    track: Track,
    modifier: Modifier = Modifier
) {
    Row(
        modifier = modifier,
        horizontalArrangement = Arrangement.spacedBy(12.dp),
        verticalAlignment = Alignment.CenterVertically
    ) {
        // Album art placeholder
        Box(
            modifier = Modifier.size(56.dp),
            contentAlignment = Alignment.Center
        ) {
            Text("🎵", style = MaterialTheme.typography.headlineMedium)
        }

        // Track details
        Column(
            modifier = Modifier.weight(1f)
        ) {
            Text(
                text = track.title,
                style = MaterialTheme.typography.bodyLarge,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )
            Text(
                text = track.artistName,
                style = MaterialTheme.typography.bodySmall,
                color = MaterialTheme.colorScheme.onSurfaceVariant,
                maxLines = 1,
                overflow = TextOverflow.Ellipsis
            )
        }
    }
}

@Composable
private fun PlaybackControls(
    isPlaying: Boolean,
    onPlayPause: () -> Unit,
    onPrevious: () -> Unit,
    onNext: () -> Unit,
    modifier: Modifier = Modifier
) {
    Row(
        modifier = modifier,
        horizontalArrangement = Arrangement.Center,
        verticalAlignment = Alignment.CenterVertically
    ) {
        Button(onClick = onPrevious) {
            Text("⏮", style = MaterialTheme.typography.titleLarge)
        }

        Spacer(modifier = Modifier.width(8.dp))

        FilledTonalButton(
            onClick = onPlayPause,
            modifier = Modifier.size(56.dp)
        ) {
            Text(
                if (isPlaying) "⏸" else "▶",
                style = MaterialTheme.typography.headlineMedium
            )
        }

        Spacer(modifier = Modifier.width(8.dp))

        Button(onClick = onNext) {
            Text("⏭", style = MaterialTheme.typography.titleLarge)
        }
    }
}

@Composable
private fun VolumeControl(
    volume: Float,
    onVolumeChange: (Float) -> Unit,
    modifier: Modifier = Modifier
) {
    Row(
        modifier = modifier,
        horizontalArrangement = Arrangement.End,
        verticalAlignment = Alignment.CenterVertically
    ) {
        Text(
            text = when {
                volume == 0f -> "🔇"
                volume < 0.5f -> "🔉"
                else -> "🔊"
            },
            style = MaterialTheme.typography.titleMedium
        )

        Spacer(modifier = Modifier.width(8.dp))

        Slider(
            value = volume,
            onValueChange = onVolumeChange,
            modifier = Modifier.width(120.dp)
        )
    }
}

private fun formatTime(milliseconds: Long): String {
    val totalSeconds = milliseconds / 1000
    val minutes = totalSeconds / 60
    val seconds = totalSeconds % 60
    return "%d:%02d".format(minutes, seconds)
}
