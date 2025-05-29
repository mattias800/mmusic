import { graphql } from "@/gql";
import * as React from "react";
import { SearchResultGroup } from "./components/SearchResultGroup";
import { ArtistCard } from "@/features/artist/artist-card/ArtistCard.tsx";
import { ShowMoreButton } from "@/components/buttons/ShowMoreButton.tsx";
import { useLimitQuery } from "@/common/LimitQuery.ts";

export interface ArtistSearchResultsProps {
  searchText: string;
}

const artistSearchQueryDocument = graphql(`
  query ArtistSearchResultsSearch($searchText: String!, $limit: Int!) {
    musicBrainz {
      artist {
        searchByName(name: $searchText, limit: $limit) {
          id
          name
          ...ArtistCard_Artist
          images {
            artistThumb
          }
        }
      }
    }
  }
`);

export const ArtistSearchResults: React.FC<ArtistSearchResultsProps> = ({
  searchText,
}) => {
  const [{ data, fetching, showMoreButtonVisible, onClickMore }] =
    useLimitQuery({
      query: artistSearchQueryDocument,
      variables: { searchText },
    });

  const artists = data?.musicBrainz.artist.searchByName;

  return (
    <SearchResultGroup
      heading={"Artists"}
      fetching={fetching && !data}
      items={artists}
      renderItem={(artist) => <ArtistCard artist={artist} key={artist.id} />}
    >
      {showMoreButtonVisible && (
        <ShowMoreButton loading={fetching} onClick={onClickMore} />
      )}
    </SearchResultGroup>
  );
};
