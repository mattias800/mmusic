import { graphql } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { useQuery } from "urql";
import { SearchResultGroup } from "@/features/search/search-result-popover/components/SearchResultGroup.tsx";
import { ListMusic } from "lucide-react";

export interface SearchResultPlaylistProps {
  searchText: string;
  onClickSearchResult: () => void;
}

const playlistSearchQuery = graphql(`
  query SearchResultPlaylistSearch($searchText: String!) {
    playlist {
      searchPlaylists(searchTerm: $searchText, limit: 5) {
        id
        name
        coverImageUrl
        createdAt
      }
    }
  }
`);

export const SearchResultPlaylist: React.FC<SearchResultPlaylistProps> = ({
  onClickSearchResult,
  searchText,
}) => {
  const [{ fetching, data }] = useQuery({
    query: playlistSearchQuery,
    variables: { searchText },
  });

  const playlists = data?.playlist?.searchPlaylists;

  return (
    <SearchResultGroup
      heading="Playlists"
      fetching={fetching}
      items={playlists}
      renderItem={(pl) => (
        <Link
          to={`/playlist/${pl.id}`}
          key={pl.id}
          className="group block p-4 rounded-xl bg-gray-800/80 hover:bg-gray-700/80 border border-white/10 hover:border-white/20 transition-all duration-200 hover:scale-[1.02]"
          onClick={onClickSearchResult}
        >
          <div className="space-y-3">
            <div className="flex items-start gap-3">
              <div className="relative flex-shrink-0">
                {pl.coverImageUrl ? (
                  <img
                    src={pl.coverImageUrl}
                    alt={pl.name ?? "Playlist"}
                    className="w-16 h-16 rounded-lg object-cover border-2 border-white/20 group-hover:border-white/30 transition-colors"
                  />
                ) : (
                  <div className="w-16 h-16 rounded-lg bg-gradient-to-br from-yellow-500/20 to-orange-500/20 border-2 border-white/20 group-hover:border-white/30 flex items-center justify-center transition-colors">
                    <ListMusic className="w-5 h-5 text-yellow-400" />
                  </div>
                )}
              </div>

              <div className="flex-1 min-w-0 space-y-2">
                <p className="text-white font-semibold text-base truncate group-hover:text-yellow-300 transition-colors">
                  {pl.name ?? "Untitled Playlist"}
                </p>
                {pl.createdAt && (
                  <div className="text-sm text-gray-300">
                    {new Date(pl.createdAt).toLocaleDateString()}
                  </div>
                )}
              </div>
            </div>
          </div>
        </Link>
      )}
    />
  );
};
