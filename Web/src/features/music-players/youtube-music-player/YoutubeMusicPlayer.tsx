import * as React from "react";

export interface YoutubeMusicPlayerProps {
  youtubeVideoId: string;
}

export const YoutubeMusicPlayer: React.FC<YoutubeMusicPlayerProps> = ({
  youtubeVideoId,
}) => {
  return (
    <iframe
      width="560"
      height="315"
      src={`https://www.youtube.com/embed/${youtubeVideoId}?autoplay=1`}
      title="YouTube video player"
      allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
      allowFullScreen
    />
  );
};
