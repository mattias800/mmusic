import { CheckCircle } from "lucide-react";
import * as React from "react";
import { LargePlayButton } from "@/components/buttons/LargePlayButton.tsx";
import { PopularArtistTracks } from "@/features/artist/PopularArtistTracks.tsx";
import { ShuffleButton } from "@/components/buttons/ShuffleButton.tsx";
import { DotsButton } from "@/components/buttons/DotsButton.tsx";
import { FollowButton } from "@/components/buttons/FollowButton.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistAlbumList } from "@/features/artist/ArtistAlbumList.tsx";

interface ArtistPanelProps {
  artist: FragmentType<typeof artistPanelArtistFragment>;
}

export const artistPanelArtistFragment = graphql(`
  fragment ArtistPanel_Artist on Artist {
    id
    name
    ...ArtistAlbumList_Artist
  }
`);

export const ArtistPanel: React.FC<ArtistPanelProps> = (props) => {
  const artist = useFragment(artistPanelArtistFragment, props.artist);

  return (
    <div className="bg-gradient-to-b from-neutral-800 to-black text-white min-h-screen">
      {/* Header */}
      <div className="relative">
        {/* Artist info positioned above the image */}
        <div className="absolute inset-x-0 inset-y-0 top-0 z-10 p-6 md:p-10 flex flex-col justify-end gap-4">
          <div className="flex items-center gap-2 text-sm text-white font-medium">
            <CheckCircle className="w-4 h-4 text-purple-300" />
            Verified Artist
          </div>
          <h1 className="text-4xl md:text-7xl font-bold  text-white drop-shadow-lg">
            {artist.name}
          </h1>
          <p className="text-white text-sm">1,294,847 monthly listeners</p>
        </div>

        {/* Full-width artist image as background */}
        <div className="w-full h-[300px] md:h-[400px] overflow-hidden relative">
          <img
            src="https://geo-media.beatport.com/image_size/590x404/c27fc408-67d0-4503-a334-e8e66d7f28b5.jpg"
            alt="U96"
            className="w-full h-full object-cover"
          />
          {/* Gradient overlay to ensure text visibility */}
          <div className="absolute inset-0 bg-gradient-to-b from-black/70 to-transparent"></div>
        </div>
      </div>

      {/* Controls */}
      <div className="px-6 md:px-10 py-6 flex items-center gap-4">
        <LargePlayButton />
        <ShuffleButton />
        <FollowButton />
        <DotsButton />
      </div>
      <PopularArtistTracks />
      <ArtistAlbumList artist={artist} />
    </div>
  );
};
