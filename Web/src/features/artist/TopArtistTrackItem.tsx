import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import {
  formatLargeNumber,
  formatTrackLength,
} from "@/common/TrackLengthFormatter.ts";
import { Play, Search } from "lucide-react";
import { RecordingPlayButton } from "@/features/music-players/RecordingPlayButton.tsx";

interface TopArtistTrackItemProps {
  track: FragmentType<typeof topArtistTrackItemLastFmTrackFragment>;
  index: number;
  active?: boolean;
}

export const topArtistTrackItemLastFmTrackFragment = graphql(`
  fragment TopArtistTrackItem_LastFmTrack on LastFmTrack {
    id
    playCount
    summary
    recording {
      id
      title
      length
      ...RecordingPlayButton_Recording
      relations {
        attributes
        url {
          id
          resource
        }
        direction
        end
        begin
        typeId
        targetType
        type
      }
      mainAlbum {
        id
        title
        coverArtUri
        releaseGroup {
          id
        }
      }
    }
  }
`);

export const TopArtistTrackItem: React.FC<TopArtistTrackItemProps> = (
  props,
) => {
  const track = useFragment(topArtistTrackItemLastFmTrackFragment, props.track);

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

      {track.recording?.mainAlbum && (
        <img
          src={track.recording?.mainAlbum.coverArtUri}
          alt={track.recording?.mainAlbum.title}
          className={
            "h-12 w-12 object-cover transition-all hover:scale-105 aspect-square rounded-md"
          }
        />
      )}
      <div className={"flex gap-4 items-center"}>
        <RecordingPlayButton
          recording={track.recording}
          renderButton={(onClick, needsYoutubeSearch) => (
            <button
              className={
                "truncate cursor-pointer hover:underline flex items-center gap-4"
              }
              onClick={onClick}
            >
              <span>{track.recording?.title}</span>
              {needsYoutubeSearch ? (
                <Search className={"h-4 w-4"} />
              ) : (
                <Play className={"h-4 w-4"} />
              )}
            </button>
          )}
          renderWhenNotPlayable={() => (
            <span className="truncate">{track.recording?.title}</span>
          )}
        />
      </div>

      <span className="text-sm text-neutral-400 text-right">
        {formatLargeNumber(track.playCount)}
      </span>
      <span className="text-sm text-neutral-400 text-right">
        {formatTrackLength(track.recording?.length ?? 0)}
      </span>
    </div>
  );
};
