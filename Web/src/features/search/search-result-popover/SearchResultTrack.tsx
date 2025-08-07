import { graphql } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";
import { useQuery } from "urql";
import { SearchResultGroup } from "@/features/search/search-result-popover/components/SearchResultGroup.tsx";
import { getRouteToArtist, getRouteToRelease } from "@/AppRoutes.ts";

export interface SearchResultTrackProps {
  searchText: string;
  onClickSearchResult: () => void;
}

const songSearchQuery = graphql(`
  query SearchResultTrackSearch($text: String!) {
    serverLibrary {
      searchTracks(searchTerm: $text, limit: 5) {
        id
        title
        trackLength
        release {
          id
          title
          coverArtUrl
          folderName
          artist {
            id
            name
          }
        }
      }
    }
  }
`);

export const SearchResultTrack: React.FC<SearchResultTrackProps> = ({
  onClickSearchResult,
  searchText,
}) => {
  const [{ fetching, data }] = useQuery({
    query: songSearchQuery,
    variables: { text: searchText },
  });

  const tracks = data?.serverLibrary.searchTracks;

  return (
    <SearchResultGroup
      heading={"Songs"}
      fetching={fetching}
      items={tracks}
      renderItem={(track) => (
        <div
          key={track.id}
          className="flex items-center p-2 hover:bg-white/10 rounded-md transition-colors"
        >
          {track.release.coverArtUrl ? (
            <img
              src={track.release.coverArtUrl}
              alt={track.title}
              className="w-10 h-10 object-cover mr-3"
            />
          ) : (
            <div className="w-10 h-10 bg-neutral-700 flex items-center justify-center mr-3">
              <span className="text-white text-xs">♪</span>
            </div>
          )}
          <div className="flex-grow overflow-hidden">
            <p className="text-white font-medium">{track.title}</p>
            <p
              className="text-xs text-white/60"
              style={{
                overflowWrap: "break-word",
                display: "-webkit-box",
                WebkitBoxOrient: "vertical",
                WebkitLineClamp: "3",
                overflow: "hidden",
              }}
            >
              <Link
                to={getRouteToArtist(track.release.artist.id)}
                className="hover:underline"
              >
                {track.release.artist.name}
              </Link>
              {track.release && (
                <>
                  {" "}
                  •{" "}
                  <Link
                    to={getRouteToRelease(
                      track.release.artist.id,
                      track.release.folderName,
                    )}
                    className="hover:underline"
                    onClick={onClickSearchResult}
                  >
                    {track.release.title}
                  </Link>
                </>
              )}
            </p>
          </div>
          <div className="text-xs text-white/60 ml-2">
            {track.trackLength ? formatTrackLength(track.trackLength) : ""}
          </div>
        </div>
      )}
    />
  );
};
