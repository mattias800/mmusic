import { graphql } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";
import { useQuery } from "urql";
import { SearchResultGroup } from "./components/SearchResultGroup";

export interface RecordingSearchResultsProps {
  searchText: string;
}

const recordingSearchQueryDocument = graphql(`
  query RecordingSearchResultsSearch($text: String!) {
    musicBrainz {
      recording {
        searchByName(name: $text, limit: 5) {
          id
          title
          length
          nameCredits {
            artist {
              id
              name
            }
          }
          mainAlbum {
            id
            title
            coverArtUri
          }
        }
      }
    }
  }
`);

export const RecordingSearchResults: React.FC<RecordingSearchResultsProps> = ({
  searchText,
}) => {
  const [{ fetching, data }] = useQuery({
    query: recordingSearchQueryDocument,
    variables: { text: searchText },
  });

  const recordings = data?.musicBrainz.recording.searchByName;

  return (
    <SearchResultGroup
      heading={"Songs"}
      fetching={fetching}
      items={recordings} // Cast to any[] for now
      renderItem={(recording) => (
        <div // Changed Link to div to avoid nested interactive elements if we add play button later
          key={recording.id}
          className="flex items-center p-2 hover:bg-white/10 rounded-md transition-colors"
        >
          {recording.mainAlbum?.coverArtUri ? (
            <img
              src={recording.mainAlbum.coverArtUri}
              alt={recording.title ?? ""}
              className="w-10 h-10 object-cover mr-3"
            />
          ) : (
            <div className="w-10 h-10 bg-neutral-700 flex items-center justify-center mr-3">
              <span className="text-white text-xs">♪</span>
            </div>
          )}
          <div className="flex-grow overflow-hidden">
            <p className="text-white font-medium">
              {recording.title ?? "Unknown Song"}
            </p>
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
              {(recording.nameCredits ?? []).map(
                ({ artist }, index: number) => (
                  <React.Fragment key={artist.id}>
                    {index > 0 && ", "}
                    <Link
                      to={`/artist/${artist.id}`}
                      className="hover:underline"
                    >
                      {artist.name ?? "Unknown Artist"}
                    </Link>
                  </React.Fragment>
                ),
              )}
              {recording.mainAlbum && (
                <>
                  {" "}
                  •{" "}
                  <Link
                    to={`/album/${recording.mainAlbum.id}`}
                    className="hover:underline"
                  >
                    {recording.mainAlbum.title ?? "Unknown Album"}
                  </Link>
                </>
              )}
            </p>
          </div>
          <div className="text-xs text-white/60 ml-2">
            {recording.length ? formatTrackLength(recording.length) : ""}
          </div>
        </div>
      )}
    />
  );
};
