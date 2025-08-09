import * as React from "react";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";
import { MusicPlayerState } from "@/features/music-players/MusicPlayerSlice.ts";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";

export interface QueuePanelProps {
  queue: MusicPlayerState["queue"];
  currentIndex: number;
  onSelect: (index: number) => void;
}

export const QueuePanel: React.FC<QueuePanelProps> = ({
  queue,
  currentIndex,
  onSelect,
}) => {
  if (!queue || queue.length === 0) {
    return (
      <div className="p-4 text-sm text-muted-foreground">
        Nothing in the queue yet
      </div>
    );
  }

  return (
    <ul className="divide-y">
      {queue.map((item, idx) => {
        const isCurrent = idx === currentIndex;
        return (
          <li
            key={`${item.artistId}-${item.releaseFolderName}-${item.trackNumber}-${idx}`}
            className={
              "p-3 hover:bg-accent/60 cursor-pointer transition-colors " +
              (isCurrent ? "bg-accent" : "")
            }
            onClick={() => onSelect(idx)}
          >
            <div className="flex items-center gap-3">
              <ReleaseCoverArt
                srcUrl={item.coverArtUrl}
                titleForPlaceholder={item.title ?? `#${item.trackNumber}`}
                className="h-10 w-10 rounded"
              />
              <div className="min-w-0 flex-1">
                <div className="flex items-center justify-between gap-3">
                  <div className="truncate font-medium">
                    {item.title ?? `#${item.trackNumber}`}
                  </div>
                  <div className="shrink-0 text-xs tabular-nums text-muted-foreground">
                    {item.trackLengthMs ? formatTrackLength(item.trackLengthMs) : "-"}
                  </div>
                </div>
                <div className="truncate text-muted-foreground text-xs">
                  {item.artistName ?? item.artistId}
                </div>
              </div>
            </div>
          </li>
        );
      })}
    </ul>
  );
};

