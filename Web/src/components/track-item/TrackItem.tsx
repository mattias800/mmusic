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
// import { playlists } from "@/components/playlists.ts";

export interface TrackItemProps {
  showCoverArt?: boolean;
  renderCoverArt?: () => ReactNode;
  title: string;
  renderTag?: () => ReactNode;
  renderSubtitle?: () => ReactNode;
  onClick?: () => void;
  trackNumber: number;
  playing?: boolean;
  playCount: number;
  trackLength: number | null | undefined;
  contextMenuItems?: ReactNode | ReactNode[];
  renderAddToPlaylistSubmenu?: () => ReactNode;
  draggable?: boolean;
  onDragStart?: (ev: React.DragEvent) => void;
  onDragOver?: (ev: React.DragEvent) => void;
  onDrop?: (ev: React.DragEvent) => void;
}

export const TrackItem: React.FC<TrackItemProps> = ({
  renderCoverArt,
  title,
  renderSubtitle,
  renderTag,
  onClick,
  trackNumber,
  playing,
  playCount,
  trackLength,
  contextMenuItems,
  showCoverArt,
  renderAddToPlaylistSubmenu,
  draggable,
  onDragStart,
  onDragOver,
  onDrop,
}) => {
  return (
    <ContextMenu>
      <ContextMenuTrigger>
        <div
          className={`grid ${showCoverArt ? "grid-cols-[40px_80px_1fr_150px_50px]" : "grid-cols-[40px_1fr_150px_50px]"} items-center px-4 py-2 rounded hover:bg-white/5 ${
            playing ? "text-green-400 font-semibold" : "text-white"
          }`}
          draggable={draggable}
          onDragStart={onDragStart}
          onDragOver={onDragOver}
          onDrop={onDrop}
        >
          <span>{playing ? "▶" : trackNumber}</span>
          {showCoverArt && (
            <>
              {renderCoverArt ? (
                renderCoverArt()
              ) : (
                <div className={"h-12 w-12"} />
              )}
            </>
          )}

          <div className={"flex flex-col gap-1 justify-center"}>
            <div className={"flex items-center gap-4"}>
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
              {renderTag?.()}
            </div>
            {renderSubtitle ? (
              <div className="text-white/50 text-xs">{renderSubtitle()}</div>
            ) : null}
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
            {renderAddToPlaylistSubmenu?.() ?? (
              <ContextMenuItem disabled>
                <PlusCircle className="mr-2 h-4 w-4" />
                No playlists
              </ContextMenuItem>
            )}
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
