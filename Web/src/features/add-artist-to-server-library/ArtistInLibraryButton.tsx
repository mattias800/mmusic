import * as React from "react";
import { Check, LoaderCircle, Plus } from "lucide-react";
import { useMutation } from "urql";
import { graphql } from "@/gql";

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
      <button
        className="border rounded-full px-1 py-1 text-sm bg-green-500 text-black transition flex gap-2 items-center"
        onClick={onClick}
      >
        <Check />
      </button>
    );
  }

  return (
    <button
      className="border border-white rounded-full px-4 py-1 text-sm hover:bg-white hover:text-black transition flex gap-2 items-center cursor-pointer"
      onClick={onClick}
    >
      {fetching ? <LoaderCircle className="animate-spin" /> : <Plus />}
      Add to Library
    </button>
  );
};
