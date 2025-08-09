import * as React from "react";
import { ShuffleButton } from "@/components/buttons/ShuffleButton.tsx";
import { FollowButton } from "@/components/buttons/FollowButton.tsx";
import { DotsButton } from "@/components/buttons/DotsButton.tsx";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { TopTracksPlayButton } from "@/features/artist/artist-page/TopTracksPlayButton.tsx";

export interface ArtistActionButtonsProps {
  artistId: string;
  loadingTopTracks: boolean;
  loadingMetaData: boolean;
  onRefreshTopTracks: () => void;
  onRefreshMetaData: () => void;
}

export const ArtistActionButtons: React.FC<ArtistActionButtonsProps> = ({
  artistId,
  loadingTopTracks,
  loadingMetaData,
  onRefreshTopTracks,
  onRefreshMetaData,
}) => {
  const anyLoading = loadingTopTracks || loadingMetaData;

  return (
    <div className="px-6 md:px-10 py-6 flex items-center gap-4">
      <TopTracksPlayButton artistId={artistId} />
      <ShuffleButton />
      <FollowButton />
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <DotsButton />
        </DropdownMenuTrigger>
        <DropdownMenuContent>
          <DropdownMenuLabel>Refresh</DropdownMenuLabel>
          <DropdownMenuItem onSelect={onRefreshTopTracks}>
            Refresh top tracks
          </DropdownMenuItem>
          <DropdownMenuItem onSelect={onRefreshMetaData}>
            Refresh artist metadata
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      {anyLoading && <Spinner size={"sm"} />}
    </div>
  );
};
