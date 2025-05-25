import * as React from "react";
import { Check, LoaderCircle, Plus } from "lucide-react";
import { useMutation } from "urql";
import { addArtistToServerLibraryMutation } from "@/features/add-artist-to-server-library/AddArtistToServerLibraryMutation.ts";

export interface ArtistInLibraryButtonProps {
  artistId: string;
  isInLibrary: boolean;
}

export const ArtistInLibraryButton: React.FC<ArtistInLibraryButtonProps> = ({
  artistId,
  isInLibrary,
}) => {
  const [{ fetching }, addArtistToServerLibrary] = useMutation(
    addArtistToServerLibraryMutation,
  );

  if (isInLibrary) {
    return (
      <button
        className="border rounded-full px-1 py-1 text-sm bg-green-500 text-black transition flex gap-2 items-center"
        onClick={() => !fetching && addArtistToServerLibrary({ artistId })}
      >
        <Check />
      </button>
    );
  }

  return (
    <button
      className="border border-white rounded-full px-4 py-1 text-sm hover:bg-white hover:text-black transition flex gap-2 items-center cursor-pointer"
      onClick={() => !fetching && addArtistToServerLibrary({ artistId })}
    >
      {fetching ? <LoaderCircle className="animate-spin" /> : <Plus />}
      Add to Library
    </button>
  );
};
