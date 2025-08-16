import { graphql } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { useQuery } from "urql";
import { SearchResultGroup } from "@/features/search/search-result-popover/components/SearchResultGroup.tsx";
import { Play, Clock } from "lucide-react";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";
import { getRouteToArtist, getRouteToRelease } from "@/AppRoutes.ts";

export interface SearchResultTrackProps {
  searchText: string;
  onClickSearchResult: () => void;
}

const songSearchQuery = graphql(`
  query SearchResultTrackSearch($text: String!) {
    serverLibrary {
      searchTracks(searchTerm: $text, limit: 5) {
        id
        title
        trackLength
        release {
          id
          title
          coverArtUrl
          folderName
          artistName
          artist {
            id
            name
          }
        }
      }
    }
  }
`);

export const SearchResultTrack: React.FC<SearchResultTrackProps> = ({
  onClickSearchResult,
  searchText,
}) => {
  const [{ fetching, data }] = useQuery({
    query: songSearchQuery,
    variables: { text: searchText },
  });

  const tracks = data?.serverLibrary.searchTracks;

  return (
    <SearchResultGroup
      heading="Songs"
      fetching={fetching}
      items={tracks}
      renderItem={(track) => (
        <div
          key={track.id}
          className="group block p-4 rounded-xl bg-gray-800/80 hover:bg-gray-700/80 border border-white/10 hover:border-white/20 transition-all duration-200 hover:scale-[1.02]"
        >
          <div className="space-y-3">
            {/* Track Header */}
            <div className="flex items-start gap-3">
              {/* Track Cover with Runtime Below */}
              <div className="relative flex-shrink-0">
                {track.release.coverArtUrl ? (
                  <img
                    src={track.release.coverArtUrl}
                    alt={track.release.title}
                    className="w-16 h-16 rounded-lg object-cover border-2 border-white/20 group-hover:border-white/30 transition-colors"
                  />
                ) : (
                  <div className="w-16 h-16 rounded-lg bg-gradient-to-br from-purple-500/20 to-pink-500/20 border-2 border-white/20 group-hover:border-white/30 flex items-center justify-center transition-colors">
                    <Play className="w-5 h-5 text-purple-400" />
                  </div>
                )}
                
                {/* Runtime Badge Below Cover */}
                {track.trackLength && (
                  <div className="mt-2 flex items-center justify-center gap-1 text-xs text-gray-400 bg-gray-700/80 px-2 py-1 rounded-full border border-white/10">
                    <Clock className="w-3 h-3" />
                    {formatTrackLength(track.trackLength)}
                  </div>
                )}
              </div>
              
              {/* Track Info */}
              <div className="flex-1 min-w-0 space-y-2">
                {/* Song Title */}
                <p className="text-white font-semibold text-base truncate group-hover:text-purple-300 transition-colors">
                  {track.title}
                </p>
                
                {/* Artist Name */}
                <div className="text-sm text-gray-300">
                  <Link
                    to={getRouteToArtist(track.release.artist.id)}
                    className="hover:underline hover:text-purple-300 transition-colors"
                  >
                    {track.release.artistName !== track.release.artist.name 
                      ? `${track.release.artistName} (${track.release.artist.name})`
                      : track.release.artistName
                    }
                  </Link>
                </div>
                
                {/* Album Name */}
                {track.release && (
                  <div className="text-xs text-gray-400">
                    <Link
                      to={getRouteToRelease(
                        track.release.artist.id,
                        track.release.folderName,
                      )}
                      className="hover:underline hover:text-purple-300 transition-colors"
                      onClick={onClickSearchResult}
                    >
                      {track.release.title}
                    </Link>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      )}
    />
  );
};
