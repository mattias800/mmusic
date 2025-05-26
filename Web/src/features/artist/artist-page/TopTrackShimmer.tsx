import * as React from "react";

export interface TopTrackShimmerProps {
  trackNumber: number;
}

export const TopTrackShimmer: React.FC<TopTrackShimmerProps> = ({
  trackNumber,
}) => {
  return (
    <div
      className={`grid grid-cols-[40px_80px_1fr_150px_50px] items-center px-4 py-2 rounded hover:bg-neutral-800 text-white animate-pulse`}
    >
      {/* Track number placeholder */}
      <div className="text-gray-500">{trackNumber}</div>

      {/* Album cover shimmer */}

      <div className="w-12 h-12 bg-gray-600 opacity-50 rounded-sm" />

      {/* Track info shimmer */}

      <div className="h-4 bg-gray-600 rounded  opacity-50 w-3/4" />

      {/* Duration shimmer */}
      <div className={"flex justify-end"}>
        <div className="h-4 w-16 bg-gray-600  opacity-50 rounded" />
      </div>

      <div className={"flex justify-end"}>
        <div className="h-4 w-8 bg-gray-600  opacity-50 rounded" />
      </div>
    </div>
  );
};
