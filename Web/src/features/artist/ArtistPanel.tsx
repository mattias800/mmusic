import * as React from "react";
import { LargePlayButton } from "@/components/buttons/LargePlayButton.tsx";
import { PopularArtistTracks } from "@/features/artist/PopularArtistTracks.tsx";
import { ShuffleButton } from "@/components/buttons/ShuffleButton.tsx";
import { DotsButton } from "@/components/buttons/DotsButton.tsx";
import { FollowButton } from "@/components/buttons/FollowButton.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistAlbumList } from "@/features/artist/ArtistAlbumList.tsx";
import { ArtistHeader } from "@/features/artist/ArtistHeader.tsx";

interface ArtistPanelProps {
  artist: FragmentType<typeof artistPanelArtistFragment>;
}

export const artistPanelArtistFragment = graphql(`
  fragment ArtistPanel_Artist on Artist {
    id
    name
    ...ArtistHeader_Artist
    ...PopularArtistTracks_Artist
    ...ArtistAlbumList_Artist
  }
`);

export const ArtistPanel: React.FC<ArtistPanelProps> = (props) => {
  const artist = useFragment(artistPanelArtistFragment, props.artist);

  return (
    <div className="bg-gradient-to-b from-neutral-800 to-black text-white min-h-screen pb-12">
      <ArtistHeader artist={artist} />

      {/* Controls */}
      <div className="px-6 md:px-10 py-6 flex items-center gap-4">
        <LargePlayButton />
        <ShuffleButton />
        <FollowButton />
        <DotsButton />
      </div>
      <PopularArtistTracks artist={artist} />
      <div className={"mt-12"} />

      <div className="px-6 md:px-10 mt-4">
        <h2 className="text-xl font-semibold mb-4">Albums</h2>
        <ArtistAlbumList artist={artist} />
      </div>
    </div>
  );
};
