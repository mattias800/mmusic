import * as React from "react";

export interface TrackListHeadingProps {
  showCoverArt?: boolean;
}

export const TrackListHeading: React.FC<TrackListHeadingProps> = ({
  showCoverArt,
}) => {
  return (
    <div
      className={`grid ${showCoverArt ? "grid-cols-[40px_80px_1fr_150px_50px]" : "grid-cols-[40px_1fr_150px_50px]"} text-sm text-gray-400 border-b border-white/20 pb-2 mb-2 px-4`}
    >
      <span className="text-left text-white/60">#</span>
      {showCoverArt && <span className="text-left"></span>}
      <span className="text-left">Title</span>
      <span className="hidden md:block text-right">Plays</span>
      <span className="hidden md:block text-right pr-2">⏱️</span>
    </div>
  );
};
