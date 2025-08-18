import * as React from "react";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";
import { PhotoCardBottomText } from "@/components/cards/PhotoCardBottomText.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";

export interface AppearsOnReleaseCardProps {
  release: FragmentType<
    typeof appearsOnReleaseCardArtistAppearsOnReleaseFragment
  >;
}

const appearsOnReleaseCardArtistAppearsOnReleaseFragment = graphql(`
  fragment AppearsOnReleaseCard_ArtistAppearsOnRelease on ArtistAppearsOnRelease {
    musicBrainzReleaseGroupId
    coverArtUrl
    releaseTitle
    firstReleaseYear
    primaryArtistName
  }
`);

export const AppearsOnReleaseCard: React.FC<AppearsOnReleaseCardProps> = (
  props,
) => {
  const release = useFragment(
    appearsOnReleaseCardArtistAppearsOnReleaseFragment,
    props.release,
  );

  const subtitleParts = [
    release.primaryArtistName,
    release.firstReleaseYear,
  ].filter(Boolean) as string[];

  return (
    <PhotoCard
      imageUrl={release.coverArtUrl ?? ""}
      imageAlt={release.releaseTitle}
    >
      <PhotoCardCenterHeading>{release.releaseTitle}</PhotoCardCenterHeading>
      {subtitleParts.length > 0 && (
        <PhotoCardBottomText>{subtitleParts.join(" • ")}</PhotoCardBottomText>
      )}
    </PhotoCard>
  );
};
