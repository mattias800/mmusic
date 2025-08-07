import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { ContextMenuItem } from "@/components/ui/context-menu.tsx";
import { useNavigate } from "react-router";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";

interface TopArtistTrackItemProps {
  track: FragmentType<typeof topArtistTrackItemLastFmTrackFragment>;
  index: number;
  active?: boolean;
}

const topArtistTrackItemLastFmTrackFragment = graphql(`
  fragment TopArtistTrackItem_LastFmTrack on LastFmTrack {
    id
    name
    playCount
    summary
    recording {
      id
      title
      trackLength
      release {
        artist {
          id
        }
        folderName
      }
      ...RecordingPlayButton_Track
    }
  }
`);

export const TopArtistTrackItem: React.FC<TopArtistTrackItemProps> = (
  props,
) => {
  const dispatch = useAppDispatch();
  const track = useFragment(topArtistTrackItemLastFmTrackFragment, props.track);

  const navigate = useNavigate();

  const trackName = track.recording?.title ?? track.name;

  return (
    <TrackItem
      trackNumber={props.index}
      title={trackName}
      trackLength={track.recording?.trackLength ?? 0}
      playCount={track.playCount}
      playing={props.active}
      showCoverArt
      coverArtUri={""}
      onClick={() =>
        track.recording &&
        track.recording.release &&
        dispatch(
          musicPlayerSlice.actions.playTrack({
            artistId: track.recording.release.artist.id,
            releaseFolderName: track.recording.release.folderName,
            trackNumber: 1, // top tracks do not have position; adapt when source provides it
          }),
        )
      }
      contextMenuItems={
        <>
          {track.recording && (
            <ContextMenuItem
              onClick={() =>
                track.recording && navigate(`/album/${track.recording.id}`)
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
