import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { LikedSongRow } from "@/features/liked-songs/LikedSongRow.tsx";

export interface LikedSongsListProps {
  user: FragmentType<typeof likedSongsListUserFragment>;
}

export const likedSongsListUserFragment = graphql(`
  fragment LikedSongsList_User on User {
    id
    likedSongs {
      id
      ...LikedSongRow_LikedSong
    }
  }
`);

export const LikedSongsList: React.FC<LikedSongsListProps> = (props) => {
  const user = useFragment(likedSongsListUserFragment, props.user);

  return (
    <div>
      {user.likedSongs.map((likedSong) => (
        <div key={likedSong.id}>
          <LikedSongRow likedSong={likedSong} />
        </div>
      ))}
    </div>
  );
};
