import { graphql } from "@/gql";
import * as React from "react";
import { SearchResultGroup } from "./components/SearchResultGroup";
import { useLimitQuery } from "@/common/LimitQuery.ts";
import { ShowMoreButton } from "@/components/buttons/ShowMoreButton.tsx";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";

export interface AlbumSearchResultsProps {
  searchText: string;
}

const albumSearchQueryDocument = graphql(`
  query AlbumSearchResultsSearch($text: String!, $limit: Int!) {
    musicBrainz {
      releaseGroup {
        searchByName(name: $text, limit: $limit) {
          id
          title
          ...AlbumCard_ReleaseGroup
        }
      }
    }
  }
`);

export const AlbumSearchResults: React.FC<AlbumSearchResultsProps> = ({
  searchText,
}) => {
  const [{ fetching, data, showMoreButtonVisible, onClickMore }] =
    useLimitQuery({
      query: albumSearchQueryDocument,
      variables: { text: searchText },
    });

  const releaseGroups = data?.musicBrainz.releaseGroup.searchByName;

  return (
    <SearchResultGroup
      heading={"Releases"}
      fetching={fetching}
      items={releaseGroups}
      renderItem={(releaseGroup) => (
        <AlbumCard releaseGroup={releaseGroup} key={releaseGroup.id} />
      )}
    >
      {showMoreButtonVisible && (
        <div>
          <ShowMoreButton loading={fetching} onClick={onClickMore} />
        </div>
      )}
    </SearchResultGroup>
  );
};
