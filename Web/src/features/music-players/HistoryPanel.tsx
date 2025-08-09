import * as React from "react";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";
import { MusicPlayerState } from "@/features/music-players/MusicPlayerSlice.ts";
import { formatDistanceToNowStrict } from "date-fns";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";

export interface HistoryPanelProps {
  history: MusicPlayerState["history"];
}

export const HistoryPanel: React.FC<HistoryPanelProps> = ({ history }) => {
  if (!history || history.length === 0) {
    return (
      <span className="p-4 text-sm text-muted-foreground">
        Nothing played yet
      </span>
    );
  }

  return (
    <ul className="divide-y">
      {history.map((h, idx) => {
        const playedAgo = h.startedAtIso
          ? formatDistanceToNowStrict(new Date(h.startedAtIso), {
              addSuffix: true,
            })
          : "";
        const item = h.track;
        return (
          <li
            key={`${item.artistId}-${item.releaseFolderName}-${item.trackNumber}-${idx}-${h.startedAtIso}`}
            className={"p-3 transition-colors"}
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
                    {item.trackLengthMs
                      ? formatTrackLength(item.trackLengthMs)
                      : "-"}
                  </div>
                </div>
                <div className="truncate text-muted-foreground text-xs">
                  {item.artistName ?? item.artistId}
                </div>
                <div className="text-[11px] text-muted-foreground mt-1">
                  {playedAgo}
                </div>
              </div>
            </div>
          </li>
        );
      })}
    </ul>
  );
};
