import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { Link } from "react-router";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { TrackListHeading } from "@/components/track-item/TrackListHeading.tsx";
import { Tag } from "@/components/text/Tag.tsx";

interface AlbumTrackListProps {
  releaseGroup: FragmentType<typeof albumTrackListReleaseGroupFragment>;
}

const albumTrackListReleaseGroupFragment = graphql(`
  fragment AlbumTrackList_Release on Release {
    id
    title
    folderName
    artist {
      id
    }
    tracks {
      id
      title
      trackLength
      isMissing
      ...RecordingPlayButton_Track
      statistics {
        listeners
        playCount
      }
    }
  }
`);

export const AlbumTrackList: React.FC<AlbumTrackListProps> = (props) => {
  const dispatch = useAppDispatch();
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
          onClick={
            track.isMissing
              ? undefined
              : () =>
                  dispatch(
                    musicPlayerSlice.actions.playTrack({
                      artistId: (release?.artist.id as string) ?? "",
                      releaseFolderName: release?.folderName as string,
                      trackNumber: idx + 1,
                    }),
                  )
          }
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
          renderTag={track.isMissing ? () => <Tag>Missing</Tag> : undefined}
          playing={false}
        />
      ))}
    </div>
  );
};
