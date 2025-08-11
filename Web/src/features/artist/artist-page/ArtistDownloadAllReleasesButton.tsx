import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { Download } from "lucide-react";
import { Button } from "@/components/ui/button.tsx";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu.tsx";
import { useMutation } from "urql";

export interface ArtistDownloadAllReleasesButtonProps {
  artist: FragmentType<typeof artistDownloadAllReleasesButtonArtistFragment>;
}

const artistDownloadAllReleasesButtonArtistFragment = graphql(`
  fragment ArtistDownloadAllReleasesButton_Artist on Artist {
    id
  }
`);

const startBulkMutation = graphql(`
  mutation ArtistDownloadAll_StartBulk(
    $input: StartBulkDownloadForArtistInput!
  ) {
    startBulkDownloadForArtist(input: $input) {
      __typename
      ... on StartBulkDownloadForArtistSuccess {
        queuedCount
      }
      ... on StartBulkDownloadForArtistError {
        message
      }
    }
  }
`);

export const ArtistDownloadAllReleasesButton: React.FC<
  ArtistDownloadAllReleasesButtonProps
> = (props) => {
  const artist = useFragment(
    artistDownloadAllReleasesButtonArtistFragment,
    props.artist,
  );

  const [, startBulk] = useMutation(startBulkMutation);

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="secondary" className="flex items-center gap-2">
          <Download className="h-5 w-5" />
          Download
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent>
        <DropdownMenuItem
          onSelect={() =>
            startBulk({ input: { artistId: artist.id, scope: "ALL" } })
          }
        >
          Download all releases
        </DropdownMenuItem>
        <DropdownMenuItem
          onSelect={() =>
            startBulk({ input: { artistId: artist.id, scope: "ALBUMS" } })
          }
        >
          Download all albums
        </DropdownMenuItem>
        <DropdownMenuItem
          onSelect={() =>
            startBulk({ input: { artistId: artist.id, scope: "SINGLES" } })
          }
        >
          Download all singles
        </DropdownMenuItem>
        <DropdownMenuItem
          onSelect={() =>
            startBulk({ input: { artistId: artist.id, scope: "EPS" } })
          }
        >
          Download all EPs
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  );
};
