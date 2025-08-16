import * as React from "react";
import { Clock, Hash, Play } from "lucide-react";

export interface TrackListHeadingProps {
  showCoverArt?: boolean;
}

export const TrackListHeading: React.FC<TrackListHeadingProps> = ({
  showCoverArt,
}) => {
  return (
    <div className="relative">
      {/* Beautiful Background */}
      <div className="absolute inset-0 bg-gradient-to-r from-white/5 via-white/3 to-white/5 rounded-t-xl border border-white/10 backdrop-blur-sm" />

      {/* Content */}
      <div
        className={`relative grid ${showCoverArt ? "grid-cols-[40px_80px_1fr_150px_50px]" : "grid-cols-[40px_1fr_150px_50px]"} items-center text-sm text-gray-300 py-4 px-4`}
      >
        {/* Track Number */}
        <div className="flex items-center gap-2">
          <Hash className="w-3.5 h-3.5 text-blue-400" />
        </div>

        {/* Cover Art Column Header */}
        {showCoverArt && <div className="flex items-center gap-2"></div>}

        {/* Title */}
        <div className="flex items-center gap-2">
          <span className="font-semibold text-white/80">Title</span>
        </div>

        {/* Plays */}
        <div className="hidden md:flex items-center justify-end gap-2">
          <Play className="w-3.5 h-3.5 text-yellow-400" />
          <span className="font-semibold text-white/80">Plays</span>
        </div>

        {/* Duration */}
        <div className="hidden md:flex items-center justify-end gap-2 pr-2">
          <span className="font-semibold text-white/80">
            <Clock className="w-3.5 h-3.5 text-indigo-400" />
          </span>
        </div>
      </div>

      {/* Subtle Bottom Border */}
      <div className="absolute bottom-0 left-0 right-0 h-px bg-gradient-to-r from-transparent via-white/20 to-transparent" />
    </div>
  );
};
