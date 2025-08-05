import * as React from "react";
import { ReactNode } from "react";
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
import { PlusCircle } from "lucide-react";
import {
  formatLargeNumber,
  formatTrackLength,
} from "@/common/TrackLengthFormatter.ts";
import { playlists } from "@/components/playlists.ts";

export interface TrackItemProps {
  coverArtUri?: string | null | undefined;
  showCoverArt?: boolean;
  title: string;
  renderSubtitle?: () => ReactNode;
  onClick?: () => void;
  trackNumber: number;
  playing?: boolean;
  playCount: number;
  trackLength: number | null | undefined;
  contextMenuItems?: ReactNode | ReactNode[];
}

export const TrackItem: React.FC<TrackItemProps> = ({
  coverArtUri,
  title,
  renderSubtitle,
  onClick,
  trackNumber,
  playing,
  playCount,
  trackLength,
  contextMenuItems,
  showCoverArt,
}) => {
  return (
    <ContextMenu>
      <ContextMenuTrigger>
        <div
          className={`grid ${showCoverArt ? "grid-cols-[40px_80px_1fr_150px_50px]" : "grid-cols-[40px_1fr_150px_50px]"} items-center px-4 py-2 rounded hover:bg-neutral-800 ${
            playing ? "text-green-400 font-semibold" : "text-white"
          }`}
        >
          <span>{playing ? "▶" : trackNumber}</span>

          {showCoverArt && (
            <>
              {coverArtUri ? (
                <img
                  src={coverArtUri}
                  alt={title ?? ""}
                  className={
                    "h-12 w-12 object-cover transition-all hover:scale-105 aspect-square rounded-md"
                  }
                />
              ) : (
                <div className={"h-12 w-12"} />
              )}
            </>
          )}

          <div className={"flex gap-4 items-center"}>
            <div>
              {onClick ? (
                <button
                  className={
                    "truncate cursor-pointer hover:underline flex items-center gap-4"
                  }
                  onClick={onClick}
                >
                  <span>{title}</span>
                </button>
              ) : (
                <span>{title}</span>
              )}
              {renderSubtitle ? (
                <div className="text-white/50 text-xs">{renderSubtitle()}</div>
              ) : null}
            </div>
          </div>

          <span className="text-sm text-neutral-400 text-right">
            {formatLargeNumber(playCount)}
          </span>
          <span className="text-sm text-neutral-400 text-right">
            {trackLength == null ? "" : formatTrackLength(trackLength)}
          </span>
        </div>
      </ContextMenuTrigger>
      <ContextMenuContent className="w-40">
        <ContextMenuItem>Add to Library</ContextMenuItem>
        <ContextMenuSub>
          {contextMenuItems}
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
