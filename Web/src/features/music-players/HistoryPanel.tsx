import * as React from "react";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";
import { MusicPlayerState } from "@/features/music-players/MusicPlayerSlice.ts";
import { formatDistanceToNowStrict } from "date-fns";

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
      {history.map((item, idx) => {
        const playedAgo = item.startedAtIso
          ? formatDistanceToNowStrict(new Date(item.startedAtIso), {
              addSuffix: true,
            })
          : "";
        return (
          <li
            key={`${item.artistId}-${item.releaseFolderName}-${item.trackNumber}-${idx}-${item.startedAtIso}`}
            className={"p-3 transition-colors"}
          >
            <div className="flex items-center gap-3">
              {item.coverArtUrl ? (
                <img src={item.coverArtUrl} className="h-10 w-10 rounded" />
              ) : (
                <div className="h-10 w-10 rounded bg-muted flex items-center justify-center text-xs text-muted-foreground">
                  {item.trackNumber}
                </div>
              )}
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
