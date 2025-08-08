import * as React from "react";
import { LargePlayButton } from "@/components/buttons/LargePlayButton.tsx";
import { ShuffleButton } from "@/components/buttons/ShuffleButton.tsx";
import { FollowButton } from "@/components/buttons/FollowButton.tsx";
import { DotsButton } from "@/components/buttons/DotsButton.tsx";
import { graphql } from "@/gql";
import { useMutation } from "urql";
import { RefreshButton } from "@/components/buttons/RefreshButton.tsx";
import { useQuery } from "urql";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { useAppDispatch } from "@/ReduxAppHooks.ts";

export interface ArtistActionButtonsProps {
  artistId: string;
  isImporting: boolean;
}

const refreshArtistMutation = graphql(`
  mutation RefreshArtist($artistId: ID!) {
    addArtistToServerLibrary(input: { artistId: $artistId }) {
      __typename
    }
  }
`);

export const ArtistActionButtons: React.FC<ArtistActionButtonsProps> = ({
  artistId,
  isImporting,
}) => {
  const [{ fetching }, refreshArtist] = useMutation(refreshArtistMutation);
  const dispatch = useAppDispatch();

  const [{ data: topData }] = useQuery({
    query: graphql(`
      query ArtistTopTracksForQueue($artistId: ID!) {
        serverLibrary {
          artistById(id: $artistId) {
            id
            topTracks {
              title
              coverArtUrl
              track {
                trackNumber
                release { folderName artist { id name } }
              }
            }
          }
        }
      }
    `),
    variables: { artistId },
    pause: !artistId,
  });

  const onPlayTopTracks = () => {
    const tts = topData?.serverLibrary.artistById?.topTracks ?? [];
    const queue = tts
      .filter((t) => t.track)
      .map((t) => ({
        artistId: t.track!.release.artist.id,
        releaseFolderName: t.track!.release.folderName,
        trackNumber: t.track!.trackNumber,
        title: t.title,
        artistName: t.track!.release.artist.name,
        coverArtUrl: t.coverArtUrl ?? undefined,
      }));
    if (queue.length > 0) {
      dispatch(musicPlayerSlice.actions.enqueueAndPlay(queue));
    }
  };

  return (
    <div className="px-6 md:px-10 py-6 flex items-center gap-4">
      <LargePlayButton onClick={onPlayTopTracks} />
      <ShuffleButton />
      <FollowButton />
      <DotsButton />
      <RefreshButton
        loading={fetching || isImporting}
        onClick={() => !fetching && refreshArtist({ artistId })}
      />
    </div>
  );
};
