import * as React from "react";
import { Button } from "@/components/ui/button.tsx";
import { Users } from "lucide-react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";

export interface MissingArtistsInPlaylistBoxProps {
  playlist: FragmentType<typeof missingArtistsInPlaylistBoxPlaylistFragment>;
}

const missingArtistsInPlaylistBoxPlaylistFragment = graphql(`
  fragment MissingArtistsInPlaylistBox_Playlist on Playlist {
    id
    items {
      id
      artist {
        id
      }
    }
  }
`);

const enqueueMissingArtistsMutation = graphql(`
  mutation EnqueueMissingArtistsFromPlaylist($playlistId: ID!) {
    enqueueMissingArtistsFromPlaylist(playlistId: $playlistId)
  }
`);

export const MissingArtistsInPlaylistBox: React.FC<
  MissingArtistsInPlaylistBoxProps
> = (props) => {
  const playlist = useFragment(
    missingArtistsInPlaylistBoxPlaylistFragment,
    props.playlist,
  );

  const [, enqueueMissing] = useMutation(enqueueMissingArtistsMutation);

  const enqueueAllMissing = async () => {
    await enqueueMissing({ playlistId: playlist.id });
  };

  const anyMissingArtists = playlist.items.some((p) => p.artist == null);

  if (!anyMissingArtists) {
    return null;
  }

  return (
    <div className="mt-4 p-4 rounded-md border bg-muted/30 flex items-center justify-between">
      <div>
        <div className="font-medium">Missing artists detected</div>
        <div className="text-sm text-zinc-400">
          Queue all missing artists from this playlist for import.
        </div>
      </div>
      <Button size="sm" iconLeft={Users} onClick={enqueueAllMissing}>
        Enqueue missing artists
      </Button>
    </div>
  );
};
