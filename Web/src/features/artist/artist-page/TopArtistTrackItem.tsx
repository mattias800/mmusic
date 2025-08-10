import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { ContextMenuItem } from "@/components/ui/context-menu.tsx";
import { useNavigate } from "react-router";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { getRouteToRelease } from "@/AppRoutes.ts";
import { AlbumTrackTag } from "@/features/album/AlbumTrackTag.tsx";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import { createMusicPlayerTrack } from "@/features/music-players/MusicPlayerTrackFactory.ts";

interface TopArtistTrackItemProps {
  artistTopTrack: FragmentType<typeof topArtistTrackItemArtistTopTrackFragment>;
  index: number;
  active?: boolean;
}

const topArtistTrackItemArtistTopTrackFragment = graphql(`
  fragment TopArtistTrackItem_ArtistTopTrack on ArtistTopTrack {
    title
    releaseTitle
    playCount
    coverArtUrl
    track {
      id
      ...MusicPlayerTrackFactory_Track
      ...AlbumTrackTag_Track
      trackLength
      trackNumber
      isMissing
      release {
        id
        folderName
        artist {
          id
          images {
            thumbs
          }
        }
      }
    }
  }
`);

export const TopArtistTrackItem: React.FC<TopArtistTrackItemProps> = (
  props,
) => {
  const dispatch = useAppDispatch();
  const artistTopTrack = useFragment(
    topArtistTrackItemArtistTopTrackFragment,
    props.artistTopTrack,
  );

  const navigate = useNavigate();

  return (
    <TrackItem
      trackNumber={props.index}
      title={artistTopTrack.title}
      trackLength={artistTopTrack.track?.trackLength ?? 0}
      playCount={artistTopTrack.playCount}
      playing={props.active}
      showCoverArt
      renderCoverArt={() => (
        <ReleaseCoverArt
          srcUrl={artistTopTrack.coverArtUrl ?? undefined}
          artistThumbUrl={
            artistTopTrack.track?.release.artist.images?.thumbs?.[0]
          }
          titleForPlaceholder={
            artistTopTrack.releaseTitle ?? artistTopTrack.title
          }
          alt={artistTopTrack.title}
          className={
            "h-12 w-12 object-cover transition-all hover:scale-105 aspect-square rounded-md"
          }
        />
      )}
      renderTag={() => <AlbumTrackTag track={artistTopTrack?.track} />}
      onClick={() =>
        artistTopTrack.track &&
        dispatch(
          musicPlayerSlice.actions.playTrack(
            createMusicPlayerTrack(artistTopTrack.track),
          ),
        )
      }
      contextMenuItems={
        <>
          {artistTopTrack.track && (
            <ContextMenuItem
              onClick={() =>
                artistTopTrack.track &&
                navigate(
                  getRouteToRelease(
                    artistTopTrack.track.release.artist.id,
                    artistTopTrack.track.release.folderName,
                  ),
                )
              }
            >
              Go to album
            </ContextMenuItem>
          )}
        </>
      }
      draggable={!!artistTopTrack.track}
      onDragStart={(ev) => {
        if (!artistTopTrack.track) return;
        try {
          const payload = {
            type: "track",
            artistId: artistTopTrack.track.release.artist.id,
            releaseFolderName: artistTopTrack.track.release.folderName,
            trackNumber: artistTopTrack.track.trackNumber,
            title: artistTopTrack.title,
          };
          ev.dataTransfer.setData("application/json", JSON.stringify(payload));
          ev.dataTransfer.effectAllowed = "copyMove";
        } catch {
          // ignore drag payload error
        }
      }}
    />
  );
};
