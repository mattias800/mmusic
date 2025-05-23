import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { formatLargeNumber } from "@/common/TrackLengthFormatter.ts";
import { useNavigate } from "react-router";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";
import { PhotoCardBottomText } from "@/components/cards/PhotoCardBottomText.tsx";

export interface ArtistCardProps {
  artist: FragmentType<typeof artistCardArtistFragment>;
}

export const artistCardArtistFragment = graphql(`
  fragment ArtistCard_MbArtist on MbArtist {
    id
    name
    listeners
    images {
      artistThumb
    }
  }
`);

export const ArtistCard: React.FC<ArtistCardProps> = (props) => {
  const artist = useFragment(artistCardArtistFragment, props.artist);
  const imageUrl = artist.images?.artistThumb;

  const navigate = useNavigate();

  return (
    <PhotoCard
      imageUrl={imageUrl ?? ""}
      imageAlt={artist.name + " cover"}
      onClick={() => navigate(`/artist/${artist.id}`)}
    >
      <PhotoCardCenterHeading>{artist.name}</PhotoCardCenterHeading>
      <PhotoCardBottomText>
        {`${formatLargeNumber(artist.listeners)} listeners`}
      </PhotoCardBottomText>
    </PhotoCard>
  );
};
