import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { LikedSongRow } from "@/features/liked-songs/LikedSongRow.tsx";
import { LikedSongsHeader } from "@/features/liked-songs/LikedSongsHeader.tsx";

export interface LikedSongsListProps {
  user: FragmentType<typeof likedSongsListUserFragment>;
}

const likedSongsListUserFragment = graphql(`
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
      <LikedSongsHeader />
      {user.likedSongs.map((likedSong, i) => (
        <div key={likedSong.id}>
          <LikedSongRow likedSong={likedSong} index={i + 1} />
        </div>
      ))}
    </div>
  );
};
