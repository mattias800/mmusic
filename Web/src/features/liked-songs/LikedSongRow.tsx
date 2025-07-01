import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";
import { Link } from "react-router";

export interface LikedSongRowProps {
  likedSong: FragmentType<typeof likedSongRowFragment>;
  index: number;
}

const likedSongRowFragment = graphql(`
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
        artists {
          id
        }
      }
    }
  }
`);

export const LikedSongRow: React.FC<LikedSongRowProps> = (props) => {
  const { recording } = useFragment(likedSongRowFragment, props.likedSong);

  if (recording == null) {
    return null;
  }

  return (
    <div className="grid grid-cols-[40px_48px_1fr_1fr_150px_48px] gap-4 items-center px-4 py-3 hover:bg-neutral-800 rounded">
      <span className="text-neutral-400 text-right">{props.index}</span>

      {recording?.mainAlbum && (
        <img
          src={recording?.mainAlbum.coverArtUri}
          alt="Cover"
          className="w-12 h-12 rounded"
        />
      )}

      <div className="min-w-0">
        <p className="text-white truncate">{recording?.title}</p>
        <p className="text-sm text-neutral-400 truncate">
          {recording?.artists.map((a) => (
            <Link to={"/artist/" + a.id} className="hover:underline">
              {a.name}
            </Link>
          ))}
        </p>
      </div>

      <div className="text-neutral-400 truncate hidden md:block">
        {recording.mainAlbum && (
          <Link
            to={"/album/" + recording.mainAlbum.id}
            className="hover:underline"
          >
            {recording?.mainAlbum.title}
          </Link>
        )}
      </div>

      <span className="text-sm text-neutral-400 whitespace-nowrap hidden sm:inline">
        21 hours ago
      </span>

      <span className="text-sm text-neutral-400 text-right whitespace-nowrap">
        {recording?.length ? formatTrackLength(recording.length) : ""}
      </span>
    </div>
  );
};
