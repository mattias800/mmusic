import * as React from "react";
import { RootState } from "@/Store.ts";
import { useAppSelector } from "@/ReduxAppHooks.ts";
import { YoutubeMusicPlayer } from "@/features/music-players/youtube-music-player/YoutubeMusicPlayer.tsx";
import { YoutubeVideoSearch } from "@/features/music-players/youtube-music-player/YoutubeVideoSearch.tsx";

export interface MusicPlayerProps {}

const selector = (state: RootState) => state.musicPlayers;

export const MusicPlayer: React.FC<MusicPlayerProps> = () => {
  const { currentMusicPlayer, isOpen, youtubeVideoId, recordingId } =
    useAppSelector(selector);

  if (!isOpen) {
    return null;
  }

  return (
    <div
      className={"fixed bottom-0 left-0 right-0 bg-black border-t-amber-50 p-8"}
    >
      PLAY
      {currentMusicPlayer === "youtube-video-id" && youtubeVideoId && (
        <YoutubeMusicPlayer youtubeVideoId={youtubeVideoId} />
      )}
      {currentMusicPlayer === "recording" && recordingId && (
        <YoutubeVideoSearch recordingId={recordingId} />
      )}
    </div>
  );
};
