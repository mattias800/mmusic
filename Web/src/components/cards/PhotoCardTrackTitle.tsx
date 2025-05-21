import * as React from "react";

export interface PhotoCardTrackTitleProps {
  trackName: string;
  artistName: string;
}

export const PhotoCardTrackTitle: React.FC<PhotoCardTrackTitleProps> = ({
  trackName,
  artistName,
}) => {
  return (
    <>
      <div className="absolute top-0 bottom-0 left-0 right-0 bg-gradient-to-b from-transparent via-black/60 to-transparent"></div>
      <div className="absolute inset-0 flex flex-col items-center justify-center p-4 z-20">
        <h3 className="text-white text-2xl font-bold text-center drop-shadow-md">
          {trackName}
        </h3>
        <span className="text-white text-sm font-medium text-center opacity-80 drop-shadow-sm mt-1">
          {artistName}
        </span>
      </div>
    </>
  );
};
