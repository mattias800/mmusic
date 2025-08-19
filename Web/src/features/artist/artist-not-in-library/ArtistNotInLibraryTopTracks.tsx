import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { formatLargeNumber } from "@/common/TrackLengthFormatter.ts";
import { Music, Users, TrendingUp } from "lucide-react";

export interface ArtistNotInLibraryTopTracksProps {
  lastFmArtist: FragmentType<typeof artistNotInLibraryTopTracksArtistFragment>;
}

const artistNotInLibraryTopTracksArtistFragment = graphql(`
  fragment ArtistNotInLibraryTopTracks_LastFmArtist on LastFmArtist {
    id
    topTracks {
      id
      name
      statistics {
        listeners
      }
    }
  }
`);

export const ArtistNotInLibraryTopTracks: React.FC<
  ArtistNotInLibraryTopTracksProps
> = (props) => {
  const lastFmArtist = useFragment(
    artistNotInLibraryTopTracksArtistFragment,
    props.lastFmArtist,
  );

  const topTracks = lastFmArtist.topTracks.slice(0, 3);

  return (
    <div className="space-y-4">
      {topTracks.map((track, idx) => (
        <div
          key={track.id}
          className="group relative p-6 bg-white/5 rounded-2xl border border-white/10 hover:bg-white/10 hover:border-white/20 transition-all duration-200"
        >
          {/* Track Number Badge */}
          <div className="absolute top-4 left-4">
            <div
              className={`w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold ${
                idx === 0
                  ? "bg-gradient-to-br from-yellow-500/20 to-orange-500/20 border border-yellow-500/30 text-yellow-400"
                  : idx === 1
                    ? "bg-gradient-to-br from-gray-500/20 to-gray-400/20 border border-gray-500/30 text-gray-400"
                    : "bg-gradient-to-br from-amber-500/20 to-yellow-500/20 border border-amber-500/30 text-amber-400"
              }`}
            >
              {idx + 1}
            </div>
          </div>

          {/* Track Info */}
          <div className="ml-16 space-y-3">
            <div className="flex items-center gap-3">
              <div className="p-2 bg-gradient-to-br from-purple-500/20 to-pink-500/20 rounded-lg border border-purple-500/30">
                <Music className="w-4 h-4 text-purple-400" />
              </div>
              <h4 className="text-lg font-semibold text-white group-hover:text-purple-300 transition-colors duration-200">
                {track.name}
              </h4>
            </div>

            {/* Listeners Info */}
            <div className="flex items-center gap-2 text-gray-400">
              <div className="p-1.5 bg-gradient-to-br from-blue-500/20 to-indigo-500/20 rounded-lg border border-blue-500/30">
                <Users className="w-3.5 h-3.5 text-blue-400" />
              </div>
              <span className="text-sm">
                {formatLargeNumber(track.statistics.listeners)} listeners
              </span>
            </div>
          </div>

          {/* Trending Indicator */}
          <div className="absolute top-4 right-4 opacity-0 group-hover:opacity-100 transition-opacity duration-200">
            <div className="p-2 bg-gradient-to-br from-green-500/20 to-emerald-500/20 rounded-lg border border-green-500/30">
              <TrendingUp className="w-4 h-4 text-green-400" />
            </div>
          </div>
        </div>
      ))}

      {topTracks.length === 0 && (
        <div className="text-center py-8">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-gradient-to-br from-gray-500/20 to-gray-400/20 rounded-2xl border border-gray-500/30 mb-4">
            <Music className="w-8 h-8 text-gray-400" />
          </div>
          <p className="text-gray-400">No top tracks data available</p>
        </div>
      )}
    </div>
  );
};
