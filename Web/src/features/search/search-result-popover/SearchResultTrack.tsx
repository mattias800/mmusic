import { graphql } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { useQuery } from "urql";
import { SearchResultGroup } from "@/features/search/search-result-popover/components/SearchResultGroup.tsx";
import { Play, Music, Clock } from "lucide-react";
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
          <div className="flex items-center gap-3">
            {/* Track Cover */}
            <div className="relative">
              {track.release.coverArtUrl ? (
                <img
                  src={track.release.coverArtUrl}
                  alt={track.release.title}
                  className="w-12 h-12 rounded-lg object-cover border-2 border-white/20 group-hover:border-white/30 transition-colors"
                />
              ) : (
                <div className="w-12 h-12 rounded-lg bg-gradient-to-br from-purple-500/20 to-pink-500/20 border-2 border-white/20 group-hover:border-white/30 flex items-center justify-center transition-colors">
                  <Play className="w-5 h-5 text-purple-400" />
                </div>
              )}
              <div className="absolute -bottom-1 -right-1 p-1 bg-purple-500/80 rounded-full border border-white/20">
                <Music className="w-3 h-3 text-white" />
              </div>
            </div>
            
            {/* Track Info */}
            <div className="flex-1 min-w-0">
              <p className="text-white font-medium truncate group-hover:text-purple-300 transition-colors">
                {track.title}
              </p>
              <div className="text-xs text-gray-400 flex items-center gap-1 mt-1">
                <span className="w-2 h-2 bg-purple-400 rounded-full"></span>
                <Link
                  to={getRouteToArtist(track.release.artist.id)}
                  className="hover:underline hover:text-purple-300 transition-colors"
                >
                  {track.release.artistName !== track.release.artist.name 
                    ? `${track.release.artistName} (${track.release.artist.name})`
                    : track.release.artistName
                  }
                </Link>
                {track.release && (
                  <>
                    <span className="text-gray-500">•</span>
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
                  </>
                )}
              </div>
            </div>
            
            {/* Track Length */}
            {track.trackLength && (
              <div className="flex items-center gap-1 text-xs text-gray-400 bg-white/5 px-2 py-1 rounded-full border border-white/10">
                <Clock className="w-3 h-3" />
                {formatTrackLength(track.trackLength)}
              </div>
            )}
          </div>
        </div>
      )}
    />
  );
};
