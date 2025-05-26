import * as React from "react";
import { CircleAlert } from "lucide-react";
import { ArtistInLibraryButton } from "@/features/add-artist-to-server-library/ArtistInLibraryButton.tsx";

export interface AddArtistToLibraryBoxProps {
  artistId: string;
  artistName: string;
  isInLibrary: boolean;
}

export const AddArtistToLibraryBox: React.FC<AddArtistToLibraryBoxProps> = ({
  artistName,
  artistId,
  isInLibrary,
}) => {
  return (
    <div
      className={
        "flex flex-col gap-4 p-4 bg-black/70 rounded-md justify-between"
      }
    >
      <div className={"flex gap-4 items-center"}>
        <CircleAlert />
        <span className={"text-2xl"}>Not in library</span>
      </div>
      <span>{artistName} is not in your library, click to add.</span>
      <div className={"flex justify-end"}>
        <ArtistInLibraryButton artistId={artistId} isInLibrary={isInLibrary} />
      </div>
    </div>
  );
};
