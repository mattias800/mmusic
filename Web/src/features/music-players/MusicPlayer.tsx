import * as React from "react";
import { RootState } from "@/Store.ts";
import { useAppSelector } from "@/ReduxAppHooks.ts";
import { LibraryAudioPlayer } from "@/features/music-players/library-audio-player/LibraryAudioPlayer.tsx";

export interface MusicPlayerProps {}

const selector = (state: RootState) => state.musicPlayers;

export const MusicPlayer: React.FC<MusicPlayerProps> = () => {
  const { currentMusicPlayer, isOpen, artistId, releaseFolderName, trackNumber } =
    useAppSelector(selector);

  if (!isOpen) {
    return null;
  }

  return (
    <div
      className={"fixed bottom-0 left-0 right-0 bg-black border-t-amber-50 p-8"}
    >
      PLAY
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
