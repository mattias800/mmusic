import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";
import { Link } from "react-router";

export interface LikedSongRowProps {
  likedSong: FragmentType<typeof likedSongRowFragment>;
}

export const likedSongRowFragment = graphql(`
  fragment LikedSongRow_LikedSong on LikedSong {
    id
    recording {
      id
      title
      length
      artists {
        id
        name
      }
      mainAlbum {
        id
        title
        coverArtUri
      }
    }
  }
`);

export const LikedSongRow: React.FC<LikedSongRowProps> = (props) => {
  const { recording } = useFragment(likedSongRowFragment, props.likedSong);

  return (
    <div className="flex items-center justify-between px-4 py-3 hover:bg-neutral-800 rounded">
      <div className="flex items-center gap-4 min-w-0">
        <span className="text-neutral-400 w-6 text-right">1</span>
        <img
          src={recording?.mainAlbum.coverArtUri}
          alt="Cover"
          className="w-12 h-12 rounded"
        />
        <div className="min-w-0">
          <p className="text-white truncate">{recording?.title}</p>
          <p className="text-sm text-neutral-400 truncate">
            {recording?.artists.map((a) => a.name).join(", ")}
          </p>
        </div>
      </div>

      <div className="hidden md:block md:flex-1 px-4 text-neutral-400 truncate">
        {recording && (
          <Link
            to={"/album/" + recording.mainAlbum.id}
            className={"text-neutral-400 hover:underline"}
          >
            {recording?.mainAlbum.title}
          </Link>
        )}
      </div>

      <div className="flex items-center gap-8 text-sm text-neutral-400">
        <span className="whitespace-nowrap hidden sm:inline">21 hours ago</span>
        <span className="whitespace-nowrap">
          {recording?.length ? formatTrackLength(recording.length) : ""}
        </span>
      </div>
    </div>
  );
};
