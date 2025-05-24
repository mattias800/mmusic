import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import {
  formatLargeNumber,
  formatTrackLength,
} from "@/common/TrackLengthFormatter.ts";
import {
  ContextMenu,
  ContextMenuContent,
  ContextMenuItem,
  ContextMenuSeparator,
  ContextMenuSub,
  ContextMenuSubContent,
  ContextMenuSubTrigger,
  ContextMenuTrigger,
} from "@/components/ui/context-menu.tsx";
import { Play, PlusCircle, Search } from "lucide-react";
import { playlists } from "@/components/playlists.ts";
import { useNavigate } from "react-router";
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

  const navigate = useNavigate();

  if (track.recording == null) {
    return null;
  }

  return (
    <ContextMenu>
      <ContextMenuTrigger>
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
      </ContextMenuTrigger>
      <ContextMenuContent className="w-40">
        {track.recording.mainAlbum && (
          <ContextMenuItem
            onClick={() =>
              track.recording?.mainAlbum?.releaseGroup &&
              navigate(`/album/${track.recording.mainAlbum.releaseGroup.id}`)
            }
          >
            Go to album
          </ContextMenuItem>
        )}

        <ContextMenuItem>Add to Library</ContextMenuItem>
        <ContextMenuSub>
          <ContextMenuSubTrigger>Add to Playlist</ContextMenuSubTrigger>
          <ContextMenuSubContent className="w-48">
            <ContextMenuItem>
              <PlusCircle className="mr-2 h-4 w-4" />
              New Playlist
            </ContextMenuItem>
            <ContextMenuSeparator />
            {playlists.map((playlist) => (
              <ContextMenuItem key={playlist}>
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  fill="none"
                  stroke="currentColor"
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth="2"
                  className="mr-2 h-4 w-4"
                  viewBox="0 0 24 24"
                >
                  <path d="M21 15V6M18.5 18a2.5 2.5 0 1 0 0-5 2.5 2.5 0 0 0 0 5ZM12 12H3M16 6H3M12 18H3" />
                </svg>
                {playlist}
              </ContextMenuItem>
            ))}
          </ContextMenuSubContent>
        </ContextMenuSub>
        <ContextMenuSeparator />
        <ContextMenuItem>Play Next</ContextMenuItem>
        <ContextMenuItem>Play Later</ContextMenuItem>
        <ContextMenuItem>Create Station</ContextMenuItem>
        <ContextMenuSeparator />
        <ContextMenuItem>Like</ContextMenuItem>
        <ContextMenuItem>Share</ContextMenuItem>
      </ContextMenuContent>
    </ContextMenu>
  );
};
