import * as React from "react";
import { useState } from "react";
import { TopArtistTrackItem } from "@/features/artist/artist-page/TopArtistTrackItem.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { SecondaryButton } from "@/components/buttons/SecondaryButton.tsx";

export interface TopArtistTracksProps {
  artist: FragmentType<typeof topArtistTracksArtistFragment>;
}

export const topArtistTracksArtistFragment = graphql(`
  fragment TopArtistTracks_Artist on Artist {
    id
    topTracks {
      id
      ...TopArtistTrackItem_LastFmTrack
    }
  }
`);

export const TopArtistTracks: React.FC<TopArtistTracksProps> = (props) => {
  const artist = useFragment(topArtistTracksArtistFragment, props.artist);

  const [showingMore, setShowingMore] = useState(false);

  const visibleTracks = artist.topTracks.slice(0, showingMore ? 20 : 10);

  return (
    <div>
      <div>
        {visibleTracks.map((track, index) => (
          <TopArtistTrackItem key={track.id} index={index + 1} track={track} />
        ))}
      </div>
      {!showingMore && (
        <div className={"pl-14 pt-8"}>
          <SecondaryButton
            label={"Show more"}
            onClick={() => setShowingMore(true)}
          />
        </div>
      )}
    </div>
  );
};
