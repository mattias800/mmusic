import { graphql } from "@/gql";
import * as React from "react";
import { SearchResultGroup } from "./components/SearchResultGroup";
import { ShowMoreButton } from "@/components/buttons/ShowMoreButton.tsx";
import { useLimitQuery } from "@/common/LimitQuery.ts";
import { MbArtistCard } from "@/features/artist/artist-card/MbArtistCard.tsx";

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
          ...MbArtistCard_MbArtist
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
      renderItem={(artist) => (
        <MbArtistCard mbArtist={artist} key={artist.id} />
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
