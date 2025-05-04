import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";

interface PopularTrackProps {
  track: FragmentType<typeof popularTrackRowLastFmTrackFragment>;
  index: number;
  active?: boolean;
}

export const popularTrackRowLastFmTrackFragment = graphql(`
  fragment PopularTrackRow_LastFmTrack on LastFmTrack {
    id
    playCount
    summary
    recording {
      id
      title
      length
      mainAlbum {
        id
        title
        coverArtUri
      }
    }
  }
`);

export const PopularTrackRow: React.FC<PopularTrackProps> = (props) => {
  const track = useFragment(popularTrackRowLastFmTrackFragment, props.track);

  if (track.recording == null) {
    return null;
  }

  return (
    <div
      className={`grid grid-cols-[40px_80px_1fr_150px_50px] items-center px-4 py-2 rounded hover:bg-neutral-800 ${
        props.active ? "text-green-400 font-semibold" : "text-white"
      }`}
    >
      <span>{props.active ? "▶" : props.index}</span>
      <img
        src={track.recording?.mainAlbum.coverArtUri}
        alt={track.recording?.mainAlbum.title}
        className={
          "h-12 w-12 object-cover transition-all hover:scale-105 aspect-square rounded-md"
        }
      />

      <span className="truncate">{track.recording?.title}</span>
      <span className="text-sm text-neutral-400 text-right">
        {track.playCount}
      </span>
      <span className="text-sm text-neutral-400 text-right">
        {formatTrackLength(track.recording?.length ?? 0)}
      </span>
    </div>
  );
};
