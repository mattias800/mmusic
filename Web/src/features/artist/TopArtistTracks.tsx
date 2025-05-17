import * as React from "react";
import { TopArtistTrackItem } from "@/features/artist/TopArtistTrackItem.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";

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

  return (
    <div className="px-6 md:px-10 mt-4">
      <h2 className="text-xl font-semibold mb-4">Popular</h2>
      <div>
        {artist.topTracks.map((track, index) => (
          <TopArtistTrackItem key={track.id} index={index + 1} track={track} />
        ))}
      </div>
    </div>
  );
};
