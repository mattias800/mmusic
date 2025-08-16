import { graphql } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { useQuery } from "urql";
import { SearchResultGroup } from "@/features/search/search-result-popover/components/SearchResultGroup.tsx";
import { Disc3, Music } from "lucide-react";

export interface SearchResultReleaseProps {
  searchText: string;
  onClickSearchResult: () => void;
}

const albumSearchQuery = graphql(`
  query SearchResultReleaseSearch($text: String!) {
    serverLibrary {
      searchReleases(searchTerm: $text, limit: 5) {
        id
        title
        coverArtUrl
        artistName
        artist {
          id
          name
        }
      }
    }
  }
`);

export const SearchResultRelease: React.FC<SearchResultReleaseProps> = ({
  onClickSearchResult,
  searchText,
}) => {
  const [{ fetching, data }] = useQuery({
    query: albumSearchQuery,
    variables: { text: searchText },
  });

  const releases = data?.serverLibrary.searchReleases;

  return (
    <SearchResultGroup
      heading="Albums"
      fetching={fetching}
      items={releases}
      renderItem={(release) => (
        <Link
          to={`/album/${release.id}`}
          key={release.id}
          className="group block p-4 rounded-xl bg-gray-800/80 hover:bg-gray-700/80 border border-white/10 hover:border-white/20 transition-all duration-200 hover:scale-[1.02]"
          onClick={onClickSearchResult}
        >
          <div className="space-y-3">
            {/* Album Header */}
            <div className="flex items-start gap-3">
              {/* Album Cover */}
              <div className="relative flex-shrink-0">
                {release.coverArtUrl ? (
                  <img
                    src={release.coverArtUrl}
                    alt={release.title}
                    className="w-16 h-16 rounded-lg object-cover border-2 border-white/20 group-hover:border-white/30 transition-colors"
                  />
                ) : (
                  <div className="w-16 h-16 rounded-lg bg-gradient-to-br from-green-500/20 to-blue-500/20 border-2 border-white/20 group-hover:border-white/30 flex items-center justify-center transition-colors">
                    <Disc3 className="w-5 h-5 text-green-400" />
                  </div>
                )}
                <div className="absolute -bottom-1 -right-1 p-1.5 bg-green-500/80 rounded-full border border-white/20">
                  <Music className="w-3.5 h-3.5 text-white" />
                </div>
              </div>
              
              {/* Album Info */}
              <div className="flex-1 min-w-0 space-y-2">
                <p className="text-white font-semibold text-base truncate group-hover:text-green-300 transition-colors">
                  {release.title}
                </p>
                <div className="text-sm text-gray-300">
                  {release.artistName !== release.artist.name 
                    ? `${release.artistName} (${release.artist.name})`
                    : release.artistName
                  }
                </div>
              </div>
            </div>
          </div>
        </Link>
      )}
    />
  );
};
