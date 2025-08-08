import * as React from "react";
import { RootState } from "@/Store.ts";
import { useAppSelector } from "@/ReduxAppHooks.ts";
import { LibraryAudioPlayer } from "@/features/music-players/library-audio-player/LibraryAudioPlayer.tsx";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";
import { useAppDispatch } from "@/ReduxAppHooks.ts";

export interface MusicPlayerProps {}

const selector = (state: RootState) => state.musicPlayers;

export const MusicPlayer: React.FC<MusicPlayerProps> = () => {
  const dispatch = useAppDispatch();
  const {
    currentMusicPlayer,
    isOpen,
    artistId,
    releaseFolderName,
    trackNumber,
    queue,
    currentIndex,
    isPlaying,
    positionSec,
    durationSec,
    volume,
    muted,
  } = useAppSelector(selector);

  if (!isOpen) {
    return null;
  }

  const currentQueueItem =
    currentIndex >= 0 && currentIndex < queue.length ? queue[currentIndex] : undefined;

  return (
    <div className="fixed bottom-0 left-0 right-0 bg-background/90 backdrop-blur border-t px-4 py-2">
      <div className="max-w-screen-2xl mx-auto grid grid-cols-3 items-center gap-4">
        {/* Left: Now playing info */}
        <div className="flex items-center gap-3 min-w-0">
          {currentQueueItem?.coverArtUrl && (
            <img src={currentQueueItem.coverArtUrl} className="h-12 w-12 rounded" />
          )}
          <div className="min-w-0">
            <div className="truncate font-medium">
              {currentQueueItem?.title ?? `${artistId ?? ""} - ${releaseFolderName ?? ""}`}
            </div>
            <div className="truncate text-muted-foreground text-sm">
              {currentQueueItem?.artistName ?? artistId}
            </div>
          </div>
        </div>

        {/* Center: Transport & Scrubber */}
        <div className="flex flex-col items-center gap-1">
          <div className="flex items-center gap-4">
            <button onClick={() => dispatch(musicPlayerSlice.actions.prev())}>⏮️</button>
            {isPlaying ? (
              <button onClick={() => dispatch(musicPlayerSlice.actions.pause())}>⏸️</button>
            ) : (
              <button onClick={() => dispatch(musicPlayerSlice.actions.play())}>▶️</button>
            )}
            <button onClick={() => dispatch(musicPlayerSlice.actions.next())}>⏭️</button>
          </div>
          <div className="flex items-center gap-2 w-full">
            <span className="text-xs tabular-nums w-10 text-right">
              {formatTrackLength(positionSec * 1000)}
            </span>
            <input
              type="range"
              min={0}
              max={Math.max(1, durationSec)}
              step={0.1}
              value={positionSec}
              onChange={(e) =>
                dispatch(musicPlayerSlice.actions.seekTo(parseFloat(e.target.value)))
              }
              className="w-full"
            />
            <span className="text-xs tabular-nums w-10">
              {formatTrackLength(durationSec * 1000)}
            </span>
          </div>
        </div>

        {/* Right: Volume */}
        <div className="flex items-center justify-end gap-2">
          <button onClick={() => dispatch(musicPlayerSlice.actions.setMuted(!muted))}>
            {muted ? "🔇" : "🔊"}
          </button>
          <input
            type="range"
            min={0}
            max={1}
            step={0.01}
            value={muted ? 0 : volume}
            onChange={(e) =>
              dispatch(musicPlayerSlice.actions.setVolume(parseFloat(e.target.value)))
            }
            className="w-28"
          />
        </div>
      </div>

      {currentMusicPlayer === "library" && artistId && releaseFolderName && trackNumber && (
        <LibraryAudioPlayer
          artistId={artistId}
          releaseFolderName={releaseFolderName}
          trackNumber={trackNumber}
        />
      )}
    </div>
  );
};
