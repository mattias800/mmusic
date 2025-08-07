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
    mainRelease {
      id
      title
      recordings {
        id
        ...RecordingPlayButton_Track
        title
        length
        statistics {
          listeners
        }
        nameCredits {
          name
          artist {
            id
            name
          }
        }
      }
    }
  }
`);

export const AlbumTrackList: React.FC<AlbumTrackListProps> = (props) => {
  const releaseGroup = useFragment(
    albumTrackListReleaseGroupFragment,
    props.releaseGroup,
  );

  const release = releaseGroup.mainRelease;

  return (
    <div>
      <TrackListHeading />

      {release?.recordings.map((recording, idx) => (
        <TrackItem
          trackNumber={idx + 1}
          title={recording.title}
          trackLength={recording.length}
          playCount={recording.statistics?.listeners ?? 0}
          renderSubtitle={() => (
            <>
              {recording.nameCredits.map(({ artist }, index) => (
                <React.Fragment key={artist.id}>
                  {index > 0 && ", "}
                  <Link to={`/artist/${artist.id}`} className="hover:underline">
                    {artist.name}
                  </Link>
                </React.Fragment>
              ))}
            </>
          )}
          playing={false}
        />
      ))}
    </div>
  );
};
