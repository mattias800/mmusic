import * as React from "react";
import { LargePlayButton } from "@/components/buttons/LargePlayButton.tsx";
import { ShuffleButton } from "@/components/buttons/ShuffleButton.tsx";
import { FollowButton } from "@/components/buttons/FollowButton.tsx";
import { DotsButton } from "@/components/buttons/DotsButton.tsx";
import { graphql } from "@/gql";
import { useMutation } from "urql";
import { RefreshButton } from "@/components/buttons/RefreshButton.tsx";

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

  return (
    <div className="px-6 md:px-10 py-6 flex items-center gap-4">
      <LargePlayButton />
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
