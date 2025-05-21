import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { LikedSongRow } from "@/features/liked-songs/LikedSongRow.tsx";
import { PlaylistHeader } from "@/components/playlist/PlaylistHeader.tsx";

export interface PlaylistProps {
  user: FragmentType<typeof playlistUserFragment>;
}

export const playlistUserFragment = graphql(`
  fragment Playlist_User on User {
    id
    likedSongs {
      id
      ...LikedSongRow_LikedSong
    }
  }
`);

export const Playlist: React.FC<PlaylistProps> = (props) => {
  const user = useFragment(playlistUserFragment, props.user);

  return (
    <div>
      <PlaylistHeader />
      {user.likedSongs.map((likedSong, i) => (
        <div key={likedSong.id}>
          <LikedSongRow likedSong={likedSong} index={i + 1} />
        </div>
      ))}
    </div>
  );
};
