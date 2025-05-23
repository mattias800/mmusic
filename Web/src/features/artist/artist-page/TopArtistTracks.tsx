import * as React from "react";
import { TopArtistTrackItem } from "@/features/artist/artist-page/TopArtistTrackItem.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";

export interface TopArtistTracksProps {
  artist: FragmentType<typeof topArtistTracksArtistFragment>;
}

export const topArtistTracksArtistFragment = graphql(`
  fragment TopArtistTracks_MbArtist on MbArtist {
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
      <SectionHeading>Popular</SectionHeading>
      <div>
        {artist.topTracks.map((track, index) => (
          <TopArtistTrackItem key={track.id} index={index + 1} track={track} />
        ))}
      </div>
    </div>
  );
};
