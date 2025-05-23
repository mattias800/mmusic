import { graphql } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { useQuery } from "urql";
import { SearchResultGroup } from "@/features/search/components/SearchResultGroup.tsx";

export interface SearchResultArtistProps {
  searchText: string;
  onClickSearchResult: () => void;
}

const artistSearchQuery = graphql(`
  query SearchResultArtistSearch($text: String!) {
    musicBrainz {
      artist {
        searchByName(name: $text, limit: 5) {
          id
          name
          images {
            artistThumb
          }
        }
      }
    }
  }
`);

export const SearchResultArtist: React.FC<SearchResultArtistProps> = ({
  onClickSearchResult,
  searchText,
}) => {
  const [{ data, fetching }] = useQuery({
    query: artistSearchQuery,
    variables: { text: searchText },
  });

  const artists = data?.musicBrainz.artist.searchByName;

  return (
    <SearchResultGroup
      heading={"Artists"}
      fetching={fetching}
      items={artists}
      renderItem={(artist) => (
        <Link
          to={`/artist/${artist.id}`}
          key={artist.id}
          className="flex items-center p-2 hover:bg-white/10 rounded-md transition-colors"
          onClick={onClickSearchResult}
        >
          {artist.images?.artistThumb ? (
            <img
              src={artist.images.artistThumb}
              alt={artist.name}
              className="w-10 h-10 rounded-full object-cover mr-3"
            />
          ) : (
            <div className="w-10 h-10 rounded-full bg-neutral-700 flex items-center justify-center mr-3">
              <span className="text-white text-xs">
                {artist.name.substring(0, 2).toUpperCase()}
              </span>
            </div>
          )}
          <div>
            <p className="text-white font-medium">{artist.name}</p>
            <p className="text-xs text-white/60">Artist</p>
          </div>
        </Link>
      )}
    />
  );
};
