import * as React from "react";

export interface LibraryAudioPlayerProps {
  artistId: string;
  releaseFolderName: string;
  trackNumber: number;
}

export const LibraryAudioPlayer: React.FC<LibraryAudioPlayerProps> = ({
  artistId,
  releaseFolderName,
  trackNumber,
}) => {
  const src = `/library/${encodeURIComponent(artistId)}/releases/${encodeURIComponent(
    releaseFolderName,
  )}/tracks/${trackNumber}/audio`;

  return (
    <audio src={src} controls autoPlay style={{ width: "100%" }}>
      Your browser does not support the audio element.
    </audio>
  );
};


