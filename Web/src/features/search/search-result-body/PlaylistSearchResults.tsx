import { graphql } from "@/gql";
import * as React from "react";
import { SearchResultGroup } from "./components/SearchResultGroup";
import { ShowMoreButton } from "@/components/buttons/ShowMoreButton.tsx";
import { useLimitQuery } from "@/common/LimitQuery.ts";
import { PlaylistCard } from "@/features/playlists/PlaylistCard.tsx";

export interface PlaylistSearchResultsProps {
  searchText: string;
}

const playlistSearchQueryDocument = graphql(`
  query PlaylistSearchResultsSearch($searchText: String!, $limit: Int!) {
    playlist {
      searchPlaylists(searchTerm: $searchText, limit: $limit) {
        id
        ...PlaylistCard_Playlist
      }
    }
  }
`);

export const PlaylistSearchResults: React.FC<PlaylistSearchResultsProps> = ({
  searchText,
}) => {
  const [{ data, fetching, showMoreButtonVisible, onClickMore }] =
    useLimitQuery({
      query: playlistSearchQueryDocument,
      variables: { searchText },
    });

  const playlists = data?.playlist?.searchPlaylists;

  return (
    <SearchResultGroup
      heading={"Playlists"}
      fetching={fetching && !data}
      items={playlists}
      renderItem={(playlist) => (
        <PlaylistCard playlist={playlist} key={playlist.id} />
      )}
    >
      {showMoreButtonVisible && <ShowMoreButton onClick={onClickMore} />}
    </SearchResultGroup>
  );
};
