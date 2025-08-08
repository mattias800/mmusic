import * as React from "react";
import { useEffect, useRef } from "react";
import { useAppDispatch, useAppSelector } from "@/ReduxAppHooks.ts";
import { RootState } from "@/Store.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";

export interface LibraryAudioPlayerProps {
  artistId: string;
  releaseFolderName: string;
  trackNumber: number;
}

export const LibraryAudioPlayer: React.FC<LibraryAudioPlayerProps> = ({
  artistId,
  releaseFolderName,
  trackNumber,
}) => {
  const src = `/library/${encodeURIComponent(artistId)}/releases/${encodeURIComponent(
    releaseFolderName,
  )}/tracks/${trackNumber}/audio`;

  const audioRef = useRef<HTMLAudioElement | null>(null);
  const dispatch = useAppDispatch();
  const { isPlaying, volume, muted, positionSec } = useAppSelector(
    (s: RootState) => s.musicPlayers,
  );

  useEffect(() => {
    if (!audioRef.current) return;
    audioRef.current.volume = volume;
    audioRef.current.muted = muted;
  }, [volume, muted]);

  useEffect(() => {
    if (!audioRef.current) return;
    if (isPlaying) audioRef.current.play().catch(() => {});
    else audioRef.current.pause();
  }, [isPlaying, src]);

  useEffect(() => {
    if (!audioRef.current) return;
    if (Math.abs((audioRef.current.currentTime ?? 0) - positionSec) > 1) {
      audioRef.current.currentTime = positionSec;
    }
  }, [positionSec]);

  return (
    <audio
      ref={audioRef}
      src={src}
      autoPlay
      onTimeUpdate={(e) =>
        dispatch(
          musicPlayerSlice.actions.updatePlaybackTime({
            positionSec: (e.target as HTMLAudioElement).currentTime ?? 0,
            durationSec: (e.target as HTMLAudioElement).duration ?? 0,
          }),
        )
      }
      onEnded={() => dispatch(musicPlayerSlice.actions.next())}
      style={{ width: "100%" }}
    />
  );
};


