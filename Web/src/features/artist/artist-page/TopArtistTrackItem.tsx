import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { ContextMenuItem } from "@/components/ui/context-menu.tsx";
import { useNavigate } from "react-router";
import { TrackItem } from "@/components/track-item/TrackItem.tsx";

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
      length
      ...RecordingPlayButton_Track
    }
  }
`);

export const TopArtistTrackItem: React.FC<TopArtistTrackItemProps> = (
  props,
) => {
  const track = useFragment(topArtistTrackItemLastFmTrackFragment, props.track);

  const navigate = useNavigate();

  const trackName = track.recording?.title ?? track.name;

  return (
    <TrackItem
      trackNumber={props.index}
      title={trackName}
      trackLength={track.recording?.length ?? 0}
      playCount={track.playCount}
      playing={props.active}
      showCoverArt
      coverArtUri={track.recording?.mainAlbum?.coverArtUri}
      contextMenuItems={
        <>
          {track.recording?.mainAlbum && (
            <ContextMenuItem
              onClick={() =>
                track.recording?.mainAlbum &&
                navigate(`/album/${track.recording.mainAlbum.id}`)
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
