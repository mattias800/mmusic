package com.mmusic.desktop.audio

import com.mmusic.shared.services.AudioPlayer
import com.mmusic.shared.services.Track
import kotlinx.coroutines.*
import java.io.BufferedInputStream
import java.net.URL
import javax.sound.sampled.*

/**
 * Desktop-specific audio engine implementation using javax.sound.sampled
 *
 * Note: This implementation has basic audio support. For production use,
 * consider using a library like JLayer (MP3) or vlcj (full media support).
 */
class DesktopAudioEngine(
    private val audioPlayer: AudioPlayer,
    private val coroutineScope: CoroutineScope
) {
    private var clip: Clip? = null
    private var positionUpdateJob: Job? = null
    private var currentAudioUrl: String? = null

    init {
        // Listen to playback state changes
        coroutineScope.launch {
            audioPlayer.currentTrack.collect { track ->
                if (track != null && track.audioUrl != currentAudioUrl) {
                    loadAndPlay(track)
                }
            }
        }

        coroutineScope.launch {
            audioPlayer.isPlaying.collect { isPlaying ->
                if (isPlaying) {
                    clip?.start()
                    startPositionUpdates()
                } else {
                    clip?.stop()
                    stopPositionUpdates()
                }
            }
        }

        coroutineScope.launch {
            audioPlayer.volume.collect { volume ->
                setClipVolume(volume)
            }
        }
    }

    private suspend fun loadAndPlay(track: Track) {
        withContext(Dispatchers.IO) {
            try {
                // Clean up existing clip
                clip?.stop()
                clip?.close()

                // Try to load the audio file
                val url = URL(track.audioUrl)
                val audioInputStream = AudioSystem.getAudioInputStream(BufferedInputStream(url.openStream()))

                // Get a clip
                clip = AudioSystem.getClip()
                clip?.open(audioInputStream)

                // Update duration
                val durationMs = (clip!!.microsecondLength / 1000)
                audioPlayer.updateDuration(durationMs)

                // Set volume
                setClipVolume(audioPlayer.volume.value)

                // Add listener for when track ends
                clip?.addLineListener { event ->
                    if (event.type == LineEvent.Type.STOP && clip?.microsecondPosition!! >= clip?.microsecondLength!!) {
                        coroutineScope.launch {
                            audioPlayer.next()
                        }
                    }
                }

                currentAudioUrl = track.audioUrl

                // Start playing if needed
                if (audioPlayer.isPlaying.value) {
                    clip?.start()
                    startPositionUpdates()
                }
            } catch (e: Exception) {
                println("Error loading audio: ${e.message}")
                // For now, just print error. In production, update playback state to error
            }
        }
    }

    private fun setClipVolume(volume: Float) {
        val currentClip = clip ?: return

        try {
            val gainControl = currentClip.getControl(FloatControl.Type.MASTER_GAIN) as? FloatControl
            if (gainControl != null) {
                // Convert linear volume (0.0-1.0) to decibels
                val dB = if (volume == 0f) {
                    gainControl.minimum
                } else {
                    (kotlin.math.ln(volume.toDouble()) / kotlin.math.ln(10.0) * 20.0).toFloat()
                        .coerceIn(gainControl.minimum, gainControl.maximum)
                }
                gainControl.value = dB
            }
        } catch (e: Exception) {
            println("Error setting volume: ${e.message}")
        }
    }

    private fun startPositionUpdates() {
        stopPositionUpdates()
        positionUpdateJob = coroutineScope.launch {
            while (isActive) {
                val position = clip?.microsecondPosition?.div(1000) ?: 0L
                audioPlayer.updatePosition(position)
                delay(100) // Update every 100ms
            }
        }
    }

    private fun stopPositionUpdates() {
        positionUpdateJob?.cancel()
        positionUpdateJob = null
    }

    fun cleanup() {
        stopPositionUpdates()
        clip?.stop()
        clip?.close()
    }
}
