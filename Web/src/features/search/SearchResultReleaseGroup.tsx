import { graphql } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { useQuery } from "urql";
import { SearchResultGroup } from "@/features/search/components/SearchResultGroup.tsx";

export interface SearchResultReleaseGroupProps {
  searchText: string;
  onClickSearchResult: () => void;
}

const albumSearchQuery = graphql(`
  query SearchResultAlbumSearch($text: String!) {
    musicBrainz {
      releaseGroup {
        searchByName(name: $text, limit: 5) {
          id
          title
          mainRelease {
            id
            title
            coverArtUri
          }
        }
      }
    }
  }
`);

export const SearchResultReleaseGroup: React.FC<
  SearchResultReleaseGroupProps
> = ({ onClickSearchResult, searchText }) => {
  const [{ fetching, data }] = useQuery({
    query: albumSearchQuery,
    variables: { text: searchText },
  });

  const releaseGroups = data?.musicBrainz.releaseGroup.searchByName;

  return (
    <SearchResultGroup
      heading={"Albums"}
      fetching={fetching}
      items={releaseGroups}
      renderItem={(releaseGroup) => (
        <Link
          to={`/album/${releaseGroup.mainRelease?.id || releaseGroup.id}`}
          key={releaseGroup.id}
          className="flex items-center p-2 hover:bg-white/10 rounded-md transition-colors"
          onClick={onClickSearchResult}
        >
          {releaseGroup.mainRelease?.coverArtUri ? (
            <img
              src={releaseGroup.mainRelease.coverArtUri}
              alt={releaseGroup.title}
              className="w-10 h-10 object-cover mr-3"
            />
          ) : (
            <div className="w-10 h-10 bg-neutral-700 flex items-center justify-center mr-3">
              <span className="text-white text-xs">🎵</span>
            </div>
          )}
          <div>
            <p className="text-white font-medium">{releaseGroup.title}</p>
            <p className="text-xs text-white/60">Album</p>
          </div>
        </Link>
      )}
    />
  );
};
