import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { Link } from "react-router";

export interface SearchResultArtistProps {
  artists: Array<FragmentType<typeof searchResultArtistFragment>>;
  onClickSearchResult: () => void;
}

export const searchResultArtistFragment = graphql(`
  fragment SearchResultArtist_Artist on Artist {
    id
    name
    images {
      artistThumb
    }
  }
`);

export const SearchResultArtist: React.FC<SearchResultArtistProps> = ({
  onClickSearchResult,
  ...props
}) => {
  const artists = useFragment(searchResultArtistFragment, props.artists);

  if (!artists || artists.length === 0) {
    return null;
  }

  return (
    <div className="p-4">
      <h3 className="text-lg font-semibold mb-3 text-white">Artists</h3>
      <div className="space-y-2">
        {artists.map((artist) => (
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
        ))}
      </div>
    </div>
  );
};
