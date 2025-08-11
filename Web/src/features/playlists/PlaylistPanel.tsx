import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { GradientContent } from "@/components/page-body/GradientContent";
import { PlaylistHeader } from "@/features/playlists/PlaylistHeader.tsx";
import { TrackListHeading } from "@/components/track-item/TrackListHeading.tsx";
import { PlaylistTrackItem } from "@/features/playlists/PlaylistTrackItem.tsx";
import { MissingArtistsInPlaylistBox } from "@/features/playlists/MissingArtistsInPlaylistBox.tsx";

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
    <GradientContent>
      <MainPadding>
        <PlaylistHeader playlist={playlist} />
        <MissingArtistsInPlaylistBox playlist={playlist} />
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
