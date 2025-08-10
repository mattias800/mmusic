import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { useAppDispatch, useAppSelector } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { TrackListHeading } from "@/components/track-item/TrackListHeading.tsx";
import { AlbumTrackTag } from "@/features/album/AlbumTrackTag.tsx";
import { createMusicPlayerTrack } from "@/features/music-players/MusicPlayerTrackFactory.ts";
import { TrackCreditLinks } from "@/features/album/TrackCreditLinks.tsx";

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
      media {
        id
        audioUrl
        audioQualityLabel
      }
      ...TrackCreditLinks_Track
      ...MusicPlayerTrackFactory_Track
      ...AlbumTrackTag_Track
      credits {
        artistName
        artist {
          id
        }
        mbArtist {
          id
        }
      }
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

  const player = useAppSelector((s) => s.musicPlayers);

  return (
    <div>
      <TrackListHeading />

      {release?.tracks.map((track, idx) => {
        const isPlaying =
          player.currentTrack?.artistId === release?.artist.id &&
          player.currentTrack.releaseFolderName === release?.folderName &&
          player.currentTrack.trackNumber === idx + 1 &&
          player.isPlaying;

        return (
          <TrackItem
            key={track.id}
            trackNumber={idx + 1}
            title={track.title}
            trackLength={track.trackLength}
            playCount={track.statistics?.listeners ?? 0}
            onClick={
              track.isMissing
                ? undefined
                : () =>
                    dispatch(
                      musicPlayerSlice.actions.playTrack(
                        createMusicPlayerTrack(track),
                      ),
                    )
            }
            renderSubtitle={() => <TrackCreditLinks track={track} />}
            renderTag={() => <AlbumTrackTag track={track} />}
            playing={isPlaying}
            draggable
            onDragStart={(ev) => {
              try {
                const payload = {
                  type: "track",
                  artistId: release?.artist.id,
                  releaseFolderName: release?.folderName,
                  trackNumber: idx + 1,
                  title: track.title,
                };
                ev.dataTransfer.setData(
                  "application/json",
                  JSON.stringify(payload),
                );
                ev.dataTransfer.effectAllowed = "copyMove";
              } catch (err) {
                console.error("Failed to set drag payload", err);
              }
            }}
          />
        );
      })}
    </div>
  );
};
