import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { GradientContent } from "@/components/page-body/GradientContent";
import { useMutation } from "urql";
import { PlaylistHeader } from "@/features/playlists/PlaylistHeader.tsx";
import { TrackListHeading } from "@/components/track-item/TrackListHeading.tsx";
import { Button } from "@/components/ui/button.tsx";
import { Users } from "lucide-react";
import { PlaylistTrackItem } from "@/features/playlists/PlaylistTrackItem.tsx";

export interface PlaylistPanelProps {
  playlist: FragmentType<typeof playlistPanelPlaylistFragment>;
}

const playlistPanelPlaylistFragment = graphql(`
  fragment PlaylistPanel_Playlist on Playlist {
    id
    ...PlaylistHeader_Playlist
    name
    items {
      id
      ...PlaylistTrackItem_PlaylistItem
    }
  }
`);

const enqueueMissingArtistsMutation = graphql(`
  mutation EnqueueMissingArtistsFromPlaylist($playlistId: ID!) {
    enqueueMissingArtistsFromPlaylist(playlistId: $playlistId)
  }
`);

export const PlaylistPanel: React.FC<PlaylistPanelProps> = (props) => {
  const playlist = useFragment(playlistPanelPlaylistFragment, props.playlist);

  const [, enqueueMissing] = useMutation(enqueueMissingArtistsMutation);

  const enqueueAllMissing = async () => {
    await enqueueMissing({ playlistId: playlist.id });
  };

  return (
    <GradientContent>
      <MainPadding>
        <PlaylistHeader playlist={playlist} />
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
        <div className="mb-12" />

        <TrackListHeading showCoverArt />

        {playlist.items.map((item, index) => (
          <PlaylistTrackItem
            playlistId={playlist.id}
            playlistItem={item}
            key={item.id}
            trackNumber={index + 1}
          />
        ))}
      </MainPadding>
    </GradientContent>
  );
};
