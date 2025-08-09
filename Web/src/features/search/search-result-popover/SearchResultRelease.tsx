import { graphql } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { useQuery } from "urql";
import { SearchResultGroup } from "@/features/search/search-result-popover/components/SearchResultGroup.tsx";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";

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
      heading={"Albums"}
      fetching={fetching}
      items={releases}
      renderItem={(release) => (
        <Link
          to={`/album/${release.id}`}
          key={release.id}
          className="flex items-center p-2 hover:bg-white/10 rounded-md transition-colors"
          onClick={onClickSearchResult}
        >
          <ReleaseCoverArt
            srcUrl={release.coverArtUrl}
            titleForPlaceholder={release.title}
            alt={release.title}
            className="w-10 h-10 object-cover mr-3"
          />
          <div>
            <p className="text-white font-medium">{release.title}</p>
            <p className="text-xs text-white/60">Album</p>
          </div>
        </Link>
      )}
    />
  );
};
