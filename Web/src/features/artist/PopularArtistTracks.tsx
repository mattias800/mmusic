import * as React from "react";
import { PopularTrackRow } from "@/features/artist/PopularTrackRow.tsx";

export interface PopularArtistTracksProps {}

const popularTracks = [
  {
    id: 1,
    title: "Club Bizarre",
    plays: "31,526,462",
    duration: "5:00",
    active: false,
  },
  {
    id: 2,
    title: "Das Boot",
    plays: "35,090,208",
    duration: "5:14",
    active: false,
  },
  {
    id: 3,
    title: "Heaven",
    plays: "12,153,930",
    duration: "3:39",
    active: true,
  },
  {
    id: 4,
    title: "Club Bizarre - Steve Baltes Remix",
    plays: "889,487",
    duration: "4:57",
    active: false,
  },
  {
    id: 5,
    title: "Love Religion - Video Edit",
    plays: "5,260,253",
    duration: "3:34",
    active: false,
  },
];

export const PopularArtistTracks: React.FC<PopularArtistTracksProps> = () => {
  return (
    <div className="px-6 md:px-10 mt-4">
      <h2 className="text-xl font-semibold mb-4">Popular</h2>
      <div>
        {popularTracks.map((track, index) => (
          <PopularTrackRow key={track.id} index={index + 1} track={track} />
        ))}
      </div>
    </div>
  );
};
