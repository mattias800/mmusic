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
    artist {
      id
      musicBrainzArtist {
        id
        name
        images {
          thumbs
        }
      }
    }
    album {
      id
      imageUrl
    }
    images {
      thumbs
    }
  }
`);

export const TopTrackCard: React.FC<TopTrackCardProps> = (props) => {
  const track = useFragment(topTrackCardLastFmTrackFragment, props.track);
  const navigate = useNavigate();

  return (
    <PhotoCard
      imageUrl={
        track.images?.artistThumb ??
        track.album?.imageUrl ??
        track.artist.musicBrainzArtist?.images?.artistThumb ??
        ""
      }
      onClick={() =>
        track.artist.musicBrainzArtist?.id &&
        navigate(`/artist/${track.artist.musicBrainzArtist?.id}`)
      }
    >
      <PhotoCardTrackTitle
        trackName={track.name}
        artistName={track.artist.musicBrainzArtist?.name ?? ""}
      />
    </PhotoCard>
  );
};
