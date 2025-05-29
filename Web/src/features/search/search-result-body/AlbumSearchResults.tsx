import { graphql } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { useQuery } from "urql";
import { SearchResultGroup } from "./components/SearchResultGroup";

export interface AlbumSearchResultsProps {
  searchText: string;
}

const albumSearchQueryDocument = graphql(`
  query AlbumSearchResultsSearch($text: String!) {
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

export const AlbumSearchResults: React.FC<AlbumSearchResultsProps> = ({
  searchText,
}) => {
  const [{ fetching, data }] = useQuery({
    query: albumSearchQueryDocument,
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
          to={`/album/${releaseGroup.id}`}
          key={releaseGroup.id}
          className="flex items-center p-2 hover:bg-white/10 rounded-md transition-colors"
        >
          {releaseGroup.mainRelease?.coverArtUri ? (
            <img
              src={releaseGroup.mainRelease.coverArtUri}
              alt={releaseGroup.title ?? ""}
              className="w-10 h-10 object-cover mr-3"
            />
          ) : (
            <div className="w-10 h-10 bg-neutral-700 flex items-center justify-center mr-3">
              <span className="text-white text-xs">🎵</span>
            </div>
          )}
          <div>
            <p className="text-white font-medium">
              {releaseGroup.title ?? "Unknown Album"}
            </p>
            <p className="text-xs text-white/60">Album</p>
          </div>
        </Link>
      )}
    />
  );
};
