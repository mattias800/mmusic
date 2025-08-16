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
        artistThumb
      }
      artist {
        id
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

  const onClick = () => {
    if (artist?.musicBrainzArtist?.artist?.id) {
      navigate(`/artist/${artist?.musicBrainzArtist?.artist?.id}`);
    } else if (artist?.musicBrainzArtist?.id) {
      navigate(`/mb-artist/${artist.musicBrainzArtist?.id}`);
    }
  };

  return (
    <PhotoCard
      imageUrl={imageUrl ?? ""}
      imageAlt={artist.name + " cover"}
      onClick={onClick}
    >
      <PhotoCardCenterHeading>{artist.name}</PhotoCardCenterHeading>
      <PhotoCardBottomText>
        {`${formatLargeNumber(artist.statistics.listeners)} listeners`}
      </PhotoCardBottomText>
    </PhotoCard>
  );
};
