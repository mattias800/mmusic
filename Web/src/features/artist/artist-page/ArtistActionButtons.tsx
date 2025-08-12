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
import { FixArtistMatchDialog } from "@/features/artist/components/FixArtistMatchDialog.tsx";

export interface ArtistActionButtonsProps {
  artistId: string;
  artistName?: string;
  loadingTopTracks: boolean;
  loadingMetaData: boolean;
  onRefreshTopTracks: () => void;
  onRefreshMetaData: () => void;
  onRefreshAllReleaseMetadata?: () => void;
}

export const ArtistActionButtons: React.FC<ArtistActionButtonsProps> = ({
  artistId,
  artistName,
  loadingTopTracks,
  loadingMetaData,
  onRefreshTopTracks,
  onRefreshMetaData,
  onRefreshAllReleaseMetadata,
}) => {
  const [fixOpen, setFixOpen] = React.useState(false);
  const anyLoading = loadingTopTracks || loadingMetaData;

  return (
    <div className="flex items-center gap-4">
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
          <DropdownMenuItem onSelect={onRefreshAllReleaseMetadata}>
            Refresh all release metadata
          </DropdownMenuItem>
          <DropdownMenuLabel>Fix</DropdownMenuLabel>
          <DropdownMenuItem onSelect={() => setFixOpen(true)}>
            Fix artist match
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      {anyLoading && <Spinner size={"sm"} />}
      <FixArtistMatchDialog
        open={fixOpen}
        onOpenChange={setFixOpen}
        artistId={artistId}
        artistName={artistName ?? ""}
      />
    </div>
  );
};
