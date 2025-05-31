import * as React from "react";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";

export interface SpotifyPlaylistCardProps {
  imageUrl: string;
  title: string;
}

export const SpotifyPlaylistCard: React.FC<SpotifyPlaylistCardProps> = ({
  imageUrl,
  title,
}) => {
  return (
    <PhotoCard imageUrl={imageUrl}>
      <PhotoCardCenterHeading>{title}</PhotoCardCenterHeading>
    </PhotoCard>
  );
};
