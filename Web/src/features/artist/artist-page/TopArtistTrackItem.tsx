import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { ContextMenuItem } from "@/components/ui/context-menu.tsx";
import { useNavigate } from "react-router";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { getRouteToRelease } from "@/AppRoutes.ts";
import { Tag } from "@/components/text/Tag.tsx";

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
      trackLength
      trackNumber
      release {
        id
        folderName
        artist {
          id
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
      coverArtUri={artistTopTrack.coverArtUrl}
      renderTag={!artistTopTrack.track ? () => <Tag>Missing</Tag> : undefined}
      onClick={() =>
        artistTopTrack.track &&
        dispatch(
          musicPlayerSlice.actions.playTrack({
            artistId: artistTopTrack.track.release.artist.id,
            releaseFolderName: artistTopTrack.track.release.folderName,
            trackNumber: artistTopTrack.track.trackNumber,
          }),
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
    />
  );
};
