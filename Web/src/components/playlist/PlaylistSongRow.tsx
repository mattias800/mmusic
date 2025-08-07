import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";
import { Link } from "react-router";

export interface PlaylistSongRowProps {
  song: FragmentType<typeof playlistSongRowFragment>;
  index: number;
}

const playlistSongRowFragment = graphql(`
  fragment LikedSongRow_Track on Track {
    id
    title
    length
  }
`);

export const PlaylistSongRow: React.FC<PlaylistSongRowProps> = (props) => {
  const recording = useFragment(playlistSongRowFragment, props.song);

  return (
    <div className="grid grid-cols-[40px_48px_1fr_1fr_150px_48px] gap-4 items-center px-4 py-3 hover:bg-neutral-800 rounded">
      <span className="text-neutral-400 text-right">{props.index}</span>

      {recording?.mainAlbum && (
        <img
          src={recording.mainAlbum.coverArtUri ?? undefined}
          alt="Cover"
          className="w-12 h-12 rounded"
        />
      )}

      <div className="min-w-0">
        <p className="text-white truncate">{recording?.title}</p>
        <p className="text-sm text-neutral-400 truncate">
          {recording?.nameCredits.map((a) => a.artist.name).join(", ")}
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
