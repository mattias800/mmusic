import * as React from "react";

export interface YoutubeMusicPlayerProps {
  youtubeId: string;
}

export const YoutubeMusicPlayer: React.FC<YoutubeMusicPlayerProps> = ({
  youtubeId,
}) => {
  return (
    <iframe
      width="560"
      height="315"
      src={`https://www.youtube.com/embed/${youtubeId}?autoplay=1`}
      title="YouTube video player"
      allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
      allowFullScreen
    />
  );
};
