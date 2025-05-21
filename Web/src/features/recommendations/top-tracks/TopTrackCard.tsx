import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { useNavigate } from "react-router";
import { PhotoCardTrackTitle } from "@/components/cards/PhotoCardTrackTitle.tsx";

export interface TopTrackCardProps {
  track: FragmentType<typeof topTrackCardLastFmTrackFragment>;
}

const topTrackCardLastFmTrackFragment = graphql(`
  fragment TopTrackCard_LastFmTrack on LastFmTrack {
    id
    playCount
    name
    recording {
      id
      mainAlbum {
        id
        coverArtUri
      }
    }
    artist {
      id
      name
      artist {
        id
        images {
          artistThumb
        }
      }
    }
  }
`);

export const TopTrackCard: React.FC<TopTrackCardProps> = (props) => {
  const track = useFragment(topTrackCardLastFmTrackFragment, props.track);
  const navigate = useNavigate();

  return (
    <PhotoCard
      imageUrl={
        track.recording?.mainAlbum?.coverArtUri ??
        track.artist.artist?.images?.artistThumb ??
        ""
      }
      onClick={() =>
        track.artist.artist?.id &&
        navigate(`/artist/${track.artist.artist?.id}`)
      }
    >
      <PhotoCardTrackTitle
        trackName={track.name}
        artistName={track.artist.name}
      />
    </PhotoCard>
  );
};
