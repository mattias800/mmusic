import * as React from "react";
import { useState } from "react";
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
import { FragmentType, graphql, useFragment } from "@/gql";

export interface ArtistActionButtonsProps {
  artist: FragmentType<typeof artistActionButtonsArtistFragment>;
  loadingTopTracks: boolean;
  loadingMetaData: boolean;
  onRefreshTopTracks: () => void;
  onRefreshMetaData: () => void;
  onRefreshAllReleaseMetadata?: () => void;
  onGenerateShareFiles?: () => void;
}

const artistActionButtonsArtistFragment = graphql(`
  fragment ArtistActionButtons_Artist on Artist {
    id
    ...FixArtistMatchDialog_Artist
    name
  }
`);

export const ArtistActionButtons: React.FC<ArtistActionButtonsProps> = ({
  loadingTopTracks,
  loadingMetaData,
  onRefreshTopTracks,
  onRefreshMetaData,
  onRefreshAllReleaseMetadata,
  onGenerateShareFiles,
  ...props
}) => {
  const artist = useFragment(artistActionButtonsArtistFragment, props.artist);

  const [fixOpen, setFixOpen] = useState(false);
  const anyLoading = loadingTopTracks || loadingMetaData;

  return (
    <div className="flex items-center gap-4">
      <TopTracksPlayButton artistId={artist.id} />
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
          <DropdownMenuLabel>Share</DropdownMenuLabel>
          <DropdownMenuItem onSelect={onGenerateShareFiles}>
            Generate Soulseek share files
          </DropdownMenuItem>
          <DropdownMenuLabel>Fix</DropdownMenuLabel>
          <DropdownMenuItem onSelect={() => setFixOpen(true)}>
            Fix artist match
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
      {anyLoading && <Spinner size={"sm"} />}
      <FixArtistMatchDialog
        artist={artist}
        open={fixOpen}
        onOpenChange={setFixOpen}
      />
    </div>
  );
};
