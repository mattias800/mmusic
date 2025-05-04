import { CheckCircle } from "lucide-react";
import { PopularTrackRow } from "./PopularTrackRow";
import * as React from "react";

export const ArtistPanel: React.FC = () => {
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

  return (
    <div className="bg-gradient-to-b from-neutral-800 to-black text-white min-h-screen">
      {/* Header */}
      <div className="p-6 md:p-10 flex items-end gap-6">
        <img
          src="https://geo-media.beatport.com/image_size/590x404/c27fc408-67d0-4503-a334-e8e66d7f28b5.jpg"
          alt="U96"
          className="w-40 h-40 md:w-52 md:h-52 rounded-full shadow-lg"
        />
        <div>
          <div className="flex items-center gap-2 text-sm text-blue-400 font-medium">
            <CheckCircle className="w-4 h-4" />
            Verified Artist
          </div>
          <h1 className="text-4xl md:text-6xl font-bold mt-2">U96</h1>
          <p className="text-neutral-400 text-sm mt-1">
            1,294,847 monthly listeners
          </p>
        </div>
      </div>

      {/* Controls */}
      <div className="px-6 md:px-10 py-6 flex items-center gap-4">
        <button className="bg-green-500 hover:bg-green-600 text-black font-bold py-2 px-6 rounded-full text-sm">
          PLAY
        </button>
        <button className="text-white opacity-70 hover:opacity-100 text-xl">
          🔀
        </button>
        <button className="border border-white rounded-full px-4 py-1 text-sm hover:bg-white hover:text-black transition">
          Follow
        </button>
        <button className="text-white text-2xl opacity-70 hover:opacity-100">
          •••
        </button>
      </div>

      {/* Popular Tracks */}
      <div className="px-6 md:px-10 mt-4">
        <h2 className="text-xl font-semibold mb-4">Popular</h2>
        <div>
          {popularTracks.map((track, index) => (
            <PopularTrackRow key={track.id} index={index + 1} track={track} />
          ))}
        </div>
      </div>
    </div>
  );
};
