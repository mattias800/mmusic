import * as React from "react";
import { useQuery } from "urql";
import { graphql } from "@/gql";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { LargePlayButton } from "@/components/buttons/LargePlayButton.tsx";

export interface TopTracksPlayButtonProps {
  artistId: string;
}

const query = graphql(`
  query ArtistTopTracksForQueue($artistId: ID!) {
    serverLibrary {
      artistById(id: $artistId) {
        id
        topTracks {
          title
          coverArtUrl
          track {
            trackNumber
            trackLength
            release {
              folderName
              artist {
                id
                name
              }
            }
          }
        }
      }
    }
  }
`);

export const TopTracksPlayButton: React.FC<TopTracksPlayButtonProps> = ({
  artistId,
}) => {
  const dispatch = useAppDispatch();

  const [{ data }] = useQuery({
    query: query,
    variables: { artistId },
    pause: !artistId,
  });

  const onPlayTopTracks = () => {
    const tts = data?.serverLibrary.artistById?.topTracks ?? [];
    const queue = tts
      .filter((t) => t.track)
      .map((t) => ({
        artistId: t.track!.release.artist.id,
        releaseFolderName: t.track!.release.folderName,
        trackNumber: t.track!.trackNumber,
        title: t.title,
        artistName: t.track!.release.artist.name,
        coverArtUrl: t.coverArtUrl ?? undefined,
        trackLengthMs: t.track!.trackLength ?? undefined,
      }));

    if (queue.length > 0) {
      dispatch(musicPlayerSlice.actions.enqueueAndPlay(queue));
    }
  };

  return <LargePlayButton onClick={onPlayTopTracks} />;
};
