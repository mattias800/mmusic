import * as React from "react";
import { ReactNode } from "react";
import { CheckCircle } from "lucide-react";
import { formatLargeNumber } from "@/common/TrackLengthFormatter.ts";

export interface ArtistHeaderProps {
  artistName: string;
  listeners: number;
  artistBackgroundUrl: string | undefined;
  renderServerStatus?: () => ReactNode;
  renderConnections?: () => ReactNode;
}

export const ArtistHeader: React.FC<ArtistHeaderProps> = ({
  renderServerStatus,
  artistName,
  artistBackgroundUrl,
  listeners,
  renderConnections,
}) => {
  return (
    <div className="relative">
      {/* Artist info positioned above the image */}
      <div className="absolute inset-x-0 inset-y-0 top-0 z-10 p-6 md:p-10 flex flex-col justify-end">
        <div className={"flex gap-4 justify-between"}>
          <div className={"flex flex-col gap-4"}>
            <div className="flex items-center gap-2 text-sm text-white font-medium">
              <CheckCircle className="w-4 h-4 text-purple-300" />
              Verified Artist
            </div>
            <h1 className="text-4xl md:text-7xl font-bold  text-white drop-shadow-lg">
              {artistName}
            </h1>
            <p className="text-white text-sm">
              {formatLargeNumber(listeners)} monthly listeners
            </p>
          </div>
          <div className={"flex items-end gap-4"}>
            {renderConnections?.()}
            {renderServerStatus?.()}
          </div>
        </div>
      </div>

      {/* Full-width artist image as background */}
      <div className="w-full h-[300px] md:h-[400px] overflow-hidden relative">
        {artistBackgroundUrl && (
          <img
            src={artistBackgroundUrl}
            alt={artistName + " background image"}
            className="w-full h-full object-cover"
          />
        )}
        {/* Gradient overlay to ensure text visibility */}
        <div className="absolute inset-0 bg-gradient-to-b from-transparent to-black/70"></div>
      </div>
    </div>
  );
};
