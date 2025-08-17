import * as React from "react";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";
import { PhotoCardBottomText } from "@/components/cards/PhotoCardBottomText.tsx";
import type { ArtistAppearsOn } from "@/gql/graphql";

export interface AppearsOnReleaseCardProps {
  item: ArtistAppearsOn;
}

export const AppearsOnReleaseCard: React.FC<AppearsOnReleaseCardProps> = ({
  item,
}) => {
  const subtitleParts = [item.primaryArtistName, item.firstReleaseYear].filter(
    Boolean,
  ) as string[];

  return (
    <PhotoCard imageUrl={item.coverArtUrl ?? ""} imageAlt={item.releaseTitle}>
      <PhotoCardCenterHeading>{item.releaseTitle}</PhotoCardCenterHeading>
      {subtitleParts.length > 0 && (
        <PhotoCardBottomText>{subtitleParts.join(" • ")}</PhotoCardBottomText>
      )}
    </PhotoCard>
  );
};


