import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { formatLargeNumber } from "@/common/TrackLengthFormatter.ts";
import { useNavigate } from "react-router";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";
import { PhotoCardBottomText } from "@/components/cards/PhotoCardBottomText.tsx";

export interface MbArtistCardProps {
  mbArtist: FragmentType<typeof mbArtistCardArtistFragment>;
}

const mbArtistCardArtistFragment = graphql(`
  fragment MbArtistCard_MbArtist on MbArtist {
    id
    name
    listeners
    images {
      artistThumb
    }
  }
`);

export const MbArtistCard: React.FC<MbArtistCardProps> = (props) => {
  const mbArtist = useFragment(mbArtistCardArtistFragment, props.mbArtist);
  const imageUrl = mbArtist.images?.artistThumb;

  const navigate = useNavigate();

  return (
    <PhotoCard
      imageUrl={imageUrl ?? ""}
      imageAlt={mbArtist.name + " cover"}
      onClick={() => navigate(`/mb-artist/${mbArtist.id}`)}
    >
      <PhotoCardCenterHeading>{mbArtist.name}</PhotoCardCenterHeading>
      <PhotoCardBottomText>
        {`${formatLargeNumber(mbArtist.listeners)} listeners`}
      </PhotoCardBottomText>
    </PhotoCard>
  );
};
