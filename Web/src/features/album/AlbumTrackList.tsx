import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { useAppDispatch, useAppSelector } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
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
    discCount
    discs {
      discNumber
      title
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
          artist { id }
          mbArtist { id }
        }
        statistics { listeners playCount }
      }
    }
    # Flattened tracks remain for single-disc and back-compat flows
    tracks {
      id
      title
      trackLength
      isMissing
      media { id audioUrl audioQualityLabel }
      ...TrackCreditLinks_Track
      ...MusicPlayerTrackFactory_Track
      ...AlbumTrackTag_Track
      credits { artistName artist { id } mbArtist { id } }
      statistics { listeners playCount }
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

  const isMultiDisc = (release?.discCount ?? 1) > 1 && (release?.discs?.length ?? 0) > 0;

  if (!release) return null;

  const renderTrack = (track: any, trackNo: number) => {
    const isPlaying =
      player.currentTrack?.artistId === release.artist.id &&
      player.currentTrack.releaseFolderName === release.folderName &&
      player.currentTrack.trackNumber === trackNo &&
      player.isPlaying;

    return (
      <TrackItem
        key={track.id}
        trackNumber={trackNo}
        title={track.title}
        trackLength={track.trackLength}
        playCount={track.statistics?.listeners ?? 0}
        onClick={
          track.isMissing
            ? undefined
            : () =>
                dispatch(
                  musicPlayerSlice.actions.playTrack(createMusicPlayerTrack(track)),
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
              artistId: release.artist.id,
              releaseFolderName: release.folderName,
              trackNumber: trackNo,
              title: track.title,
            };
            ev.dataTransfer.setData("application/json", JSON.stringify(payload));
            ev.dataTransfer.effectAllowed = "copyMove";
          } catch (err) {
            console.error("Failed to set drag payload", err);
          }
        }}
      />
    );
  };

  return (
    <div className="space-y-6">
      {isMultiDisc
        ? release.discs!.map((disc) => (
            <div key={disc.discNumber} className="space-y-3">
              <div className="px-2 py-1 text-sm text-gray-300">{`Disc ${disc.discNumber}${disc.title ? ` — ${disc.title}` : ""}`}</div>
              <div>
                {disc.tracks.map((t: any, i: number) => renderTrack(t, i + 1))}
              </div>
            </div>
          ))
        : release.tracks.map((t, i) => renderTrack(t, i + 1))}
    </div>
  );
};
