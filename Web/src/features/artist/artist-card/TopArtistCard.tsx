import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { formatLargeNumber } from "@/common/TrackLengthFormatter.ts";
import { useNavigate } from "react-router";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";
import { PhotoCardBottomText } from "@/components/cards/PhotoCardBottomText.tsx";

export interface TopArtistCardProps {
  artist: FragmentType<typeof topArtistCardArtistFragment>;
}

const topArtistCardArtistFragment = graphql(`
  fragment TopArtistCard_Artist on LastFmArtist {
    id
    name
    musicBrainzArtist {
      id
      name
      images {
        thumbs
      }
    }
    statistics {
      listeners
    }
  }
`);

export const TopArtistCard: React.FC<TopArtistCardProps> = (props) => {
  const artist = useFragment(topArtistCardArtistFragment, props.artist);
  const imageUrl = artist.musicBrainzArtist?.images?.artistThumb;

  const navigate = useNavigate();

  return (
    <PhotoCard
      imageUrl={imageUrl ?? ""}
      imageAlt={artist.name + " cover"}
      onClick={() =>
        artist?.musicBrainzArtist?.id &&
        navigate(`/artist/${artist.musicBrainzArtist?.id}`)
      }
    >
      <PhotoCardCenterHeading>{artist.name}</PhotoCardCenterHeading>
      <PhotoCardBottomText>
        {`${formatLargeNumber(artist.statistics.listeners)} listeners`}
      </PhotoCardBottomText>
    </PhotoCard>
  );
};
