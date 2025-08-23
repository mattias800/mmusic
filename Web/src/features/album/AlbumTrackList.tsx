import { FragmentType, useFragment } from "@/gql";
import * as React from "react";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { useAppDispatch, useAppSelector } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { createMusicPlayerTrack } from "@/features/music-players/MusicPlayerTrackFactory.ts";
import { TrackCreditLinks } from "@/features/album/TrackCreditLinks.tsx";
import { AlbumTrackTag } from "@/features/album/AlbumTrackTag.tsx";
import { AlbumTrackList_ReleaseFragmentDoc, MusicPlayerTrackFactory_TrackFragmentDoc } from "@/gql/graphql.ts";

const albumTrackListReleaseFragment = AlbumTrackList_ReleaseFragmentDoc;

interface AlbumTrackListProps {
  releaseGroup: FragmentType<typeof albumTrackListReleaseFragment>;
}

// Presentational subcomponents (no GraphQL data in props)
interface TrackRowProps {
  id: string;
  trackNo: number;
  title: string;
  trackLength: number | null | undefined;
  playCount: number;
  isMissing: boolean;
  playing: boolean;
  onClick?: () => void;
  onDragStart?: (ev: React.DragEvent) => void;
  subtitle: React.ReactNode;
  tag: React.ReactNode;
}

const TrackRow: React.FC<TrackRowProps> = ({
  id,
  trackNo,
  title,
  trackLength,
  playCount,
  isMissing,
  playing,
  onClick,
  onDragStart,
  subtitle,
  tag,
}) => (
  <TrackItem
    key={id}
    trackNumber={trackNo}
    title={title}
    trackLength={trackLength}
    playCount={playCount}
    onClick={isMissing ? undefined : onClick}
    renderSubtitle={() => <>{subtitle}</>}
    renderTag={() => <>{tag}</>}
    playing={playing}
    draggable
    onDragStart={onDragStart}
  />
);

const DiscSection: React.FC<{ discNumber: number; title?: string | null; children: React.ReactNode }> = ({ discNumber, title, children }) => (
  <div className="space-y-3">
    <div className="px-2 py-1 text-sm text-gray-300">{`Disc ${discNumber}${title ? ` — ${title}` : ""}`}</div>
    <div>{children}</div>
  </div>
);

export const AlbumTrackList: React.FC<AlbumTrackListProps> = (props) => {
  const release = useFragment(
    albumTrackListReleaseFragment,
    props.releaseGroup,
  );
  const dispatch = useAppDispatch();
  const player = useAppSelector((s) => s.musicPlayers);
  if (!release) return null;

  const isMultiDisc =
    (release.discCount ?? 1) > 1 && (release.discs?.length ?? 0) > 0;

  const buildOnDragStart = (trackNo: number, title: string) => (ev: React.DragEvent) => {
    try {
      const payload = {
        type: "track" as const,
        artistId: release.artist.id,
        releaseFolderName: release.folderName,
        trackNumber: trackNo,
        title,
      };
      ev.dataTransfer.setData("application/json", JSON.stringify(payload));
      ev.dataTransfer.effectAllowed = "copyMove";
    } catch (err) {
      console.error("Failed to set drag payload", err);
    }
  };

  const buildOnClick = (track: FragmentType<typeof MusicPlayerTrackFactory_TrackFragmentDoc>) => () =>
    dispatch(musicPlayerSlice.actions.playTrack(createMusicPlayerTrack(track)));

  const isPlaying = (trackNo: number) =>
    player.currentTrack?.artistId === release.artist.id &&
    player.currentTrack.releaseFolderName === release.folderName &&
    player.currentTrack.trackNumber === trackNo &&
    player.isPlaying;

  return (
    <div className="space-y-6">
      {isMultiDisc
        ? release.discs!.map((disc) => (
            <DiscSection key={disc.discNumber} discNumber={disc.discNumber} title={disc.title}>
              {disc.tracks.map((t, i) => (
                <TrackRow
                  key={t.id}
                  id={t.id}
                  trackNo={i + 1}
                  title={t.title}
                  trackLength={t.trackLength}
                  playCount={t.statistics?.listeners ?? 0}
                  isMissing={!!t.isMissing}
                  playing={isPlaying(i + 1)}
                  onClick={buildOnClick(t)}
                  onDragStart={buildOnDragStart(i + 1, t.title)}
                  subtitle={<TrackCreditLinks track={t} />}
                  tag={<AlbumTrackTag track={t} />}
                />
              ))}
            </DiscSection>
          ))
        : release.tracks.map((t, i) => (
            <TrackRow
              key={t.id}
              id={t.id}
              trackNo={i + 1}
              title={t.title}
              trackLength={t.trackLength}
              playCount={t.statistics?.listeners ?? 0}
              isMissing={!!t.isMissing}
              playing={isPlaying(i + 1)}
              onClick={buildOnClick(t)}
              onDragStart={buildOnDragStart(i + 1, t.title)}
              subtitle={<TrackCreditLinks track={t} />}
              tag={<AlbumTrackTag track={t} />}
            />
          ))}
    </div>
  );
};
