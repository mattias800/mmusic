import * as React from "react";
import { PopularTrackRow } from "@/features/artist/PopularTrackRow.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";

export interface PopularArtistTracksProps {
  artist: FragmentType<typeof popularArtistTracksArtistFragment>;
}

export const popularArtistTracksArtistFragment = graphql(`
  fragment PopularArtistTracks_Artist on Artist {
    id
    topTracks {
      id
      ...PopularTrackRow_LastFmTrack
    }
  }
`);

export const PopularArtistTracks: React.FC<PopularArtistTracksProps> = (
  props,
) => {
  const artist = useFragment(popularArtistTracksArtistFragment, props.artist);

  return (
    <div className="px-6 md:px-10 mt-4">
      <h2 className="text-xl font-semibold mb-4">Popular</h2>
      <div>
        {artist.topTracks.map((track, index) => (
          <PopularTrackRow key={track.id} index={index + 1} track={track} />
        ))}
      </div>
    </div>
  );
};
