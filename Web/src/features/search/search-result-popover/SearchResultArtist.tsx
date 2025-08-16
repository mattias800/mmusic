import { graphql } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { useQuery } from "urql";
import { SearchResultGroup } from "@/features/search/search-result-popover/components/SearchResultGroup.tsx";
import { Users, Mic } from "lucide-react";

export interface SearchResultArtistProps {
  searchText: string;
  onClickSearchResult: () => void;
}

const artistSearchQuery = graphql(`
  query SearchResultArtistSearch($text: String!) {
    serverLibrary {
      searchArtists(searchTerm: $text, limit: 5) {
        id
        name
        images {
          thumbs
        }
      }
    }
  }
`);

export const SearchResultArtist: React.FC<SearchResultArtistProps> = ({
  onClickSearchResult,
  searchText,
}) => {
  const [{ data, fetching }] = useQuery({
    query: artistSearchQuery,
    variables: { text: searchText },
  });

  const artists = data?.serverLibrary.searchArtists;

  return (
    <SearchResultGroup
      heading="Artists"
      fetching={fetching}
      items={artists}
      renderItem={(artist) => (
        <Link
          to={`/artist/${artist.id}`}
          key={artist.id}
          className="group block p-4 rounded-xl bg-gray-800/80 hover:bg-gray-700/80 border border-white/10 hover:border-white/20 transition-all duration-200 hover:scale-[1.02]"
          onClick={onClickSearchResult}
        >
          <div className="space-y-3">
            {/* Artist Header */}
            <div className="flex items-center gap-3">
              {/* Artist Image */}
              {artist.images?.thumbs?.[0] ? (
                <div className="relative">
                  <img
                    src={artist.images.thumbs[0]}
                    alt={artist.name}
                    className="w-16 h-16 rounded-full object-cover border-2 border-white/20 group-hover:border-white/30 transition-colors"
                  />
                  <div className="absolute -bottom-1 -right-1 p-1.5 bg-blue-500/80 rounded-full border border-white/20">
                    <Users className="w-3.5 h-3.5 text-white" />
                  </div>
                </div>
              ) : (
                <div className="relative">
                  <div className="w-16 h-16 rounded-full bg-gradient-to-br from-blue-500/20 to-purple-500/20 border-2 border-white/20 group-hover:border-white/30 flex items-center justify-center transition-colors">
                    <Mic className="w-6 h-6 text-blue-400" />
                  </div>
                  <div className="absolute -bottom-1 -right-1 p-1.5 bg-blue-500/80 rounded-full border border-white/20">
                    <Users className="w-3.5 h-3.5 text-white" />
                  </div>
                </div>
              )}
              
              {/* Artist Info */}
              <div className="flex-1 min-w-0">
                <p className="text-white font-semibold text-base truncate group-hover:text-blue-300 transition-colors">
                  {artist.name}
                </p>
                <p className="text-xs text-gray-400 flex items-center gap-1 mt-1">
                  <span className="w-2 h-2 bg-blue-400 rounded-full"></span>
                  Artist
                </p>
              </div>
            </div>

            {/* Artist Description */}
            <div className="pt-2 border-t border-white/10">
              <p className="text-xs text-gray-400 leading-relaxed">
                Click to view artist profile, releases, and tracks
              </p>
            </div>
          </div>
        </Link>
      )}
    />
  );
};
