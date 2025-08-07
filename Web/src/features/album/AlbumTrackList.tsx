import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { TrackListHeading } from "@/components/track-item/TrackListHeading.tsx";

interface AlbumTrackListProps {
  releaseGroup: FragmentType<typeof albumTrackListReleaseGroupFragment>;
}

const albumTrackListReleaseGroupFragment = graphql(`
  fragment AlbumTrackList_Release on Release {
    id
    title
    tracks {
      id
      title
      trackLength
      ...RecordingPlayButton_Track
      statistics {
        listeners
        playCount
      }
    }
  }
`);

export const AlbumTrackList: React.FC<AlbumTrackListProps> = (props) => {
  const release = useFragment(
    albumTrackListReleaseGroupFragment,
    props.releaseGroup,
  );

  return (
    <div>
      <TrackListHeading />

      {release?.tracks.map((track, idx) => (
        <TrackItem
          trackNumber={idx + 1}
          title={track.title}
          trackLength={track.trackLength}
          playCount={track.statistics?.listeners ?? 0}
          renderSubtitle={() => (
            <>
              {[{ artist: { id: "1", name: "CreditArtist" } }].map(
                ({ artist }, index) => (
                  <React.Fragment key={artist.id}>
                    {index > 0 && ", "}
                    <Link
                      to={`/artist/${artist.id}`}
                      className="hover:underline"
                    >
                      {artist.name}
                    </Link>
                  </React.Fragment>
                ),
              )}
            </>
          )}
          playing={false}
        />
      ))}
    </div>
  );
};
