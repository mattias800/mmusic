package com.mmusic.shared.services

import kotlinx.coroutines.flow.MutableStateFlow
import kotlinx.coroutines.flow.StateFlow
import kotlinx.coroutines.flow.asStateFlow

/**
 * Core audio playback service for the application.
 * Manages audio playback state, queue, and playback controls.
 */
class AudioPlayer {
    private val _playbackState = MutableStateFlow<PlaybackState>(PlaybackState.Idle)
    val playbackState: StateFlow<PlaybackState> = _playbackState.asStateFlow()

    private val _currentTrack = MutableStateFlow<Track?>(null)
    val currentTrack: StateFlow<Track?> = _currentTrack.asStateFlow()

    private val _isPlaying = MutableStateFlow(false)
    val isPlaying: StateFlow<Boolean> = _isPlaying.asStateFlow()

    private val _currentPosition = MutableStateFlow(0L) // in milliseconds
    val currentPosition: StateFlow<Long> = _currentPosition.asStateFlow()

    private val _duration = MutableStateFlow(0L) // in milliseconds
    val duration: StateFlow<Long> = _duration.asStateFlow()

    private val _volume = MutableStateFlow(1.0f) // 0.0 to 1.0
    val volume: StateFlow<Float> = _volume.asStateFlow()

    private val _queue = MutableStateFlow<List<Track>>(emptyList())
    val queue: StateFlow<List<Track>> = _queue.asStateFlow()

    private var currentTrackIndex = 0

    /**
     * Play a single track
     */
    fun playTrack(track: Track) {
        _currentTrack.value = track
        _queue.value = listOf(track)
        currentTrackIndex = 0
        play()
    }

    /**
     * Play a list of tracks starting at a specific index
     */
    fun playTracks(tracks: List<Track>, startIndex: Int = 0) {
        if (tracks.isEmpty()) return
        _queue.value = tracks
        currentTrackIndex = startIndex.coerceIn(0, tracks.size - 1)
        _currentTrack.value = tracks[currentTrackIndex]
        play()
    }

    /**
     * Add tracks to the end of the queue
     */
    fun addToQueue(tracks: List<Track>) {
        _queue.value = _queue.value + tracks
    }

    /**
     * Start or resume playback
     */
    fun play() {
        val track = _currentTrack.value ?: return
        _isPlaying.value = true
        _playbackState.value = PlaybackState.Playing(track)
        // Platform-specific playback will be implemented in desktopApp
    }

    /**
     * Pause playback
     */
    fun pause() {
        _isPlaying.value = false
        _playbackState.value = _currentTrack.value?.let { PlaybackState.Paused(it) } ?: PlaybackState.Idle
    }

    /**
     * Stop playback and clear the current track
     */
    fun stop() {
        _isPlaying.value = false
        _currentTrack.value = null
        _queue.value = emptyList()
        _currentPosition.value = 0L
        _playbackState.value = PlaybackState.Idle
    }

    /**
     * Play the next track in the queue
     */
    fun next() {
        val nextIndex = currentTrackIndex + 1
        if (nextIndex < _queue.value.size) {
            currentTrackIndex = nextIndex
            _currentTrack.value = _queue.value[nextIndex]
            play()
        }
    }

    /**
     * Play the previous track in the queue
     */
    fun previous() {
        if (_currentPosition.value > 3000) {
            // If more than 3 seconds into the track, restart it
            seekTo(0L)
        } else {
            // Otherwise go to previous track
            val prevIndex = currentTrackIndex - 1
            if (prevIndex >= 0) {
                currentTrackIndex = prevIndex
                _currentTrack.value = _queue.value[prevIndex]
                play()
            }
        }
    }

    /**
     * Seek to a specific position in milliseconds
     */
    fun seekTo(positionMs: Long) {
        _currentPosition.value = positionMs.coerceIn(0L, _duration.value)
        // Platform-specific seek will be implemented in desktopApp
    }

    /**
     * Set the playback volume (0.0 to 1.0)
     */
    fun setVolume(volume: Float) {
        _volume.value = volume.coerceIn(0f, 1f)
        // Platform-specific volume control will be implemented in desktopApp
    }

    /**
     * Update the current playback position (called by platform-specific player)
     */
    fun updatePosition(positionMs: Long) {
        _currentPosition.value = positionMs
    }

    /**
     * Update the track duration (called by platform-specific player)
     */
    fun updateDuration(durationMs: Long) {
        _duration.value = durationMs
    }
}

/**
 * Represents the current playback state
 */
sealed class PlaybackState {
    object Idle : PlaybackState()
    data class Playing(val track: Track) : PlaybackState()
    data class Paused(val track: Track) : PlaybackState()
    data class Buffering(val track: Track) : PlaybackState()
    data class Error(val message: String) : PlaybackState()
}

/**
 * Represents a track that can be played
 */
data class Track(
    val id: String,
    val title: String,
    val artistName: String,
    val albumTitle: String?,
    val audioUrl: String,
    val durationMs: Long,
    val coverArtUrl: String?
)
