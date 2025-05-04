import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { formatTrackLength } from "@/common/TrackLengthFormatter.ts";

export interface SearchResultRecordingProps {
  recordings: Array<FragmentType<typeof searchResultRecordingFragment>>;
}

export const searchResultRecordingFragment = graphql(`
  fragment SearchResultRecording_Recording on Recording {
    id
    title
    length
    artists {
      id
      name
    }
    mainAlbum {
      id
      title
      coverArtUri
    }
  }
`);

export const SearchResultRecording: React.FC<SearchResultRecordingProps> = (
  props,
) => {
  const recordings = useFragment(
    searchResultRecordingFragment,
    props.recordings,
  );

  if (!recordings || recordings.length === 0) {
    return null;
  }

  return (
    <div className="p-4">
      <h3 className="text-lg font-semibold mb-3 text-white">Songs</h3>
      <div className="space-y-2">
        {recordings.map((recording) => (
          <div
            key={recording.id}
            className="flex items-center p-2 hover:bg-white/10 rounded-md transition-colors"
          >
            {recording.mainAlbum?.coverArtUri ? (
              <img
                src={recording.mainAlbum.coverArtUri}
                alt={recording.title}
                className="w-10 h-10 object-cover mr-3"
              />
            ) : (
              <div className="w-10 h-10 bg-neutral-700 flex items-center justify-center mr-3">
                <span className="text-white text-xs">♪</span>
              </div>
            )}
            <div className="flex-grow">
              <p className="text-white font-medium">{recording.title}</p>
              <p className="text-xs text-white/60">
                {recording.artists.map((artist, index) => (
                  <React.Fragment key={artist.id}>
                    {index > 0 && ", "}
                    <Link
                      to={`/artist/${artist.id}`}
                      className="hover:underline"
                    >
                      {artist.name}
                    </Link>
                  </React.Fragment>
                ))}
                {recording.mainAlbum && (
                  <>
                    {" "}
                    •{" "}
                    <Link
                      to={`/album/${recording.mainAlbum.id}`}
                      className="hover:underline"
                    >
                      {recording.mainAlbum.title}
                    </Link>
                  </>
                )}
              </p>
            </div>
            <div className="text-xs text-white/60 ml-2">
              {recording.length ? formatTrackLength(recording.length) : ""}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};
