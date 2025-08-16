import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { PlaylistHeader } from "@/features/playlists/PlaylistHeader.tsx";
import { TrackListHeading } from "@/components/track-item/TrackListHeading.tsx";
import { PlaylistTrackItem } from "@/features/playlists/PlaylistTrackItem.tsx";
import { MissingArtistsInPlaylistBox } from "@/features/playlists/MissingArtistsInPlaylistBox.tsx";
import { GlassCard, PageLayout } from "@/components/ui";

export interface PlaylistPanelProps {
  playlist: FragmentType<typeof playlistPanelPlaylistFragment>;
}

const playlistPanelPlaylistFragment = graphql(`
  fragment PlaylistPanel_Playlist on Playlist {
    id
    ...PlaylistHeader_Playlist
    ...MissingArtistsInPlaylistBox_Playlist
    name
    items {
      id
      ...PlaylistTrackItem_PlaylistItem
    }
  }
`);

export const PlaylistPanel: React.FC<PlaylistPanelProps> = (props) => {
  const playlist = useFragment(playlistPanelPlaylistFragment, props.playlist);

  return (
    <PageLayout addSearchPadding>
      <PlaylistHeader playlist={playlist} />
      <MissingArtistsInPlaylistBox playlist={playlist} />
      <div className="mb-8" />

      <GlassCard>
        <TrackListHeading showCoverArt />

        {playlist.items.map((item, index) => (
          <PlaylistTrackItem
            playlistId={playlist.id}
            playlistItem={item}
            key={item.id}
            trackNumber={index + 1}
          />
        ))}
      </GlassCard>
    </PageLayout>
  );
};
