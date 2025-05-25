import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { CheckCircle } from "lucide-react";
import { formatLargeNumber } from "@/common/TrackLengthFormatter.ts";
import { ArtistServerStatus } from "@/features/artist/artist-server-status/ArtistServerStatus.tsx";

export interface ArtistHeaderProps {
  artist: FragmentType<typeof artistHeaderArtistFragment>;
}

export const artistHeaderArtistFragment = graphql(`
  fragment ArtistHeader_Artist on Artist {
    id
    ...ArtistServerStatus_Artist
    name
    listeners
    images {
      artistBackground
    }
  }
`);

export const ArtistHeader: React.FC<ArtistHeaderProps> = (props) => {
  const artist = useFragment(artistHeaderArtistFragment, props.artist);

  return (
    <div className="relative">
      {/* Artist info positioned above the image */}
      <div className="absolute inset-x-0 inset-y-0 top-0 z-10 p-6 md:p-10 flex flex-col justify-end">
        <div className={"flex gap-4 justify-between"}>
          <div className={"flex flex-col gap-4"}>
            <div className="flex items-center gap-2 text-sm text-white font-medium">
              <CheckCircle className="w-4 h-4 text-purple-300" />
              Verified Artist
            </div>
            <h1 className="text-4xl md:text-7xl font-bold  text-white drop-shadow-lg">
              {artist.name}
            </h1>
            <p className="text-white text-sm">
              {formatLargeNumber(artist.listeners)} monthly listeners
            </p>
          </div>
          <div className={"flex items-end"}>
            <ArtistServerStatus artist={artist} />
          </div>
        </div>
      </div>

      {/* Full-width artist image as background */}
      <div className="w-full h-[300px] md:h-[400px] overflow-hidden relative">
        {artist.images?.artistBackground && (
          <img
            src={artist.images.artistBackground}
            alt={artist.name + " background image"}
            className="w-full h-full object-cover"
          />
        )}
        {/* Gradient overlay to ensure text visibility */}
        <div className="absolute inset-0 bg-gradient-to-b from-transparent to-black/70"></div>
      </div>
    </div>
  );
};
