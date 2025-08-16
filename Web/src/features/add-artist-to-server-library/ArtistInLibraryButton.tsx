import * as React from "react";
import { Check } from "lucide-react";
import { useMutation } from "urql";
import { graphql } from "@/gql";
import { GradientButton } from "@/components/ui";

export interface ArtistInLibraryButtonProps {
  artistId: string;
  isInLibrary: boolean;
}

const importArtistMutation = graphql(`
  mutation ImportArtist($musicBrainzArtistId: String!) {
    importArtist(input: { musicBrainzArtistId: $musicBrainzArtistId }) {
      __typename
      ... on ImportArtistSuccess {
        artist {
          id
          name
        }
      }
      ... on ImportArtistError {
        message
      }
    }
  }
`);

export const ArtistInLibraryButton: React.FC<ArtistInLibraryButtonProps> = ({
  artistId,
  isInLibrary,
}) => {
  const [{ fetching }, importArtist] = useMutation(importArtistMutation);

  const onClick = () =>
    !fetching && importArtist({ musicBrainzArtistId: artistId });

  if (isInLibrary) {
    return (
      <GradientButton onClick={onClick} loading={fetching}>
        <Check className="w-5 h-5 text-green-400" />
        Add to Library
      </GradientButton>
    );
  }

  return (
    <GradientButton onClick={onClick} loading={fetching}>
      Add to Library
    </GradientButton>
  );
};
