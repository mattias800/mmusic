import * as React from "react";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { LargePlayButton } from "@/components/buttons/LargePlayButton.tsx";

export interface PlayAlbumButtonProps {
  release: FragmentType<typeof playAlbumButtonReleaseFragment>;
}

const playAlbumButtonReleaseFragment = graphql(`
  fragment PlayAlbumButton_Release on Release {
    id
    folderName
    coverArtUrl
    artist {
      id
      name
    }
    tracks {
      id
      title
      isMissing
    }
  }
`);

export const PlayAlbumButton: React.FC<PlayAlbumButtonProps> = (props) => {
  const release = useFragment(playAlbumButtonReleaseFragment, props.release);

  const dispatch = useAppDispatch();

  const onPlayAll = () => {
    if (!release) return;
    const tracks = release.tracks ?? [];
    const queue = tracks
      .map((t, idx) => ({ track: t, idx }))
      .filter(({ track }) => !track.isMissing)
      .map(({ track, idx }) => ({
        artistId: release.artist.id,
        releaseFolderName: release.folderName,
        trackNumber: idx + 1,
        title: track.title,
        artistName: release.artist?.name,
        coverArtUrl: release.coverArtUrl,
      }));

    if (queue.length > 0) {
      dispatch(musicPlayerSlice.actions.enqueueAndPlay(queue));
    }
  };

  return <LargePlayButton onClick={onPlayAll} />;
};
