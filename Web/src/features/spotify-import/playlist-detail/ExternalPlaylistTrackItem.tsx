import * as React from "react";
import { ReactNode } from "react";
import {
  formatLargeNumber,
  formatTrackLength,
} from "@/common/TrackLengthFormatter.ts";

// import { playlists } from "@/components/playlists.ts";

export interface ExternalPlaylistTrackItemProps {
  showCoverArt?: boolean;
  renderCoverArt?: () => ReactNode;
  renderCheckbox?: () => ReactNode;
  title: string;
  renderTag?: () => ReactNode;
  renderSubtitle?: () => ReactNode;
  onClick?: () => void;
  trackNumber: number;
  playing?: boolean;
  playCount: number;
  trackLength: number | null | undefined;
}

export const ExternalPlaylistTrackItem: React.FC<
  ExternalPlaylistTrackItemProps
> = ({
  renderCoverArt,
  title,
  renderSubtitle,
  renderTag,
  onClick,
  trackNumber,
  playing,
  playCount,
  trackLength,
  showCoverArt,
  renderCheckbox,
}) => {
  return (
    <div
      className={`grid ${showCoverArt ? "grid-cols-[40px_40px_80px_1fr_150px_50px]" : "grid-cols-[40px_40px_1fr_150px_50px]"} items-center px-4 py-2 rounded hover:bg-neutral-800 ${
        playing ? "text-green-400 font-semibold" : "text-white"
      }`}
    >
      <span>{renderCheckbox?.()}</span>
      <span>{playing ? "▶" : trackNumber}</span>
      {showCoverArt && (
        <>
          {renderCoverArt ? renderCoverArt() : <div className={"h-12 w-12"} />}
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
  );
};
