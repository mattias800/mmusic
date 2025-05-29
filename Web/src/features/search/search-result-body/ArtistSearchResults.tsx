import { graphql } from "@/gql";
import * as React from "react";
import { useQuery } from "urql";
import { SearchResultGroup } from "./components/SearchResultGroup";
import { ArtistCard } from "@/features/artist/artist-card/ArtistCard.tsx";
import { ShowMoreButton } from "@/components/buttons/ShowMoreButton.tsx";

export interface ArtistSearchResultsProps {
  searchText: string;
}

const artistSearchQueryDocument = graphql(`
  query ArtistSearchResultsSearch($text: String!) {
    musicBrainz {
      artist {
        searchByName(name: $text, limit: 5) {
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
  const [{ data, fetching }] = useQuery({
    query: artistSearchQueryDocument,
    variables: { text: searchText },
  });

  const artists = data?.musicBrainz.artist.searchByName;

  return (
    <SearchResultGroup
      heading={"Artists"}
      fetching={fetching}
      items={artists}
      renderItem={(artist) => <ArtistCard artist={artist} key={artist.id} />}
    >
      <ShowMoreButton />
    </SearchResultGroup>
  );
};
