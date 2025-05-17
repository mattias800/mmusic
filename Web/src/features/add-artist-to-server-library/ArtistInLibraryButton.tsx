import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { Check, LoaderCircle, Plus } from "lucide-react";
import { useMutation } from "urql";
import { addArtistToServerLibraryMutation } from "@/features/add-artist-to-server-library/AddArtistToServerLibraryMutation.ts";

export interface ArtistInLibraryButtonProps {
  artist: FragmentType<typeof artistInLibraryButtonArtistFragment>;
}

export const artistInLibraryButtonArtistFragment = graphql(`
  fragment ArtistInLibraryButton_Artist on Artist {
    id
    serverAvailability {
      id
      isInServerLibrary
    }
  }
`);

export const ArtistInLibraryButton: React.FC<ArtistInLibraryButtonProps> = (
  props,
) => {
  const [{ fetching }, addArtistToServerLibrary] = useMutation(
    addArtistToServerLibraryMutation,
  );

  const artist = useFragment(artistInLibraryButtonArtistFragment, props.artist);

  const inLibrary = artist.serverAvailability.isInServerLibrary;

  if (inLibrary) {
    return (
      <button
        className="border rounded-full px-1 py-1 text-sm bg-green-500 text-black transition flex gap-2 items-center"
        onClick={() =>
          !fetching && addArtistToServerLibrary({ artistId: artist.id })
        }
      >
        <Check />
      </button>
    );
  }

  return (
    <button
      className="border border-white rounded-full px-4 py-1 text-sm hover:bg-white hover:text-black transition flex gap-2 items-center cursor-pointer"
      onClick={() =>
        !fetching && addArtistToServerLibrary({ artistId: artist.id })
      }
    >
      {fetching ? <LoaderCircle className="animate-spin" /> : <Plus />}
      Add to Library
    </button>
  );
};
