import * as React from "react";
import { musicPlayerSlice } from "@/features/music-players/MusicPlayerSlice.ts";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useAppDispatch } from "@/ReduxAppHooks.ts";
import { PlayButton } from "@/components/buttons/PlayButton.tsx";

export interface RecordingPlayButtonProps {
  recording:
    | FragmentType<typeof recordingPlayButtonRecordingFragment>
    | undefined;
  renderButton?: (
    onClick: () => void,
    needsYoutubeSearch: boolean,
  ) => React.ReactNode;
  renderWhenNotPlayable?: () => React.ReactNode;
}

const recordingPlayButtonRecordingFragment = graphql(`
  fragment RecordingPlayButton_Track on Track {
    id
    release {
      artist {
        id
      }
      folderName
    }
    title
  }
`);

export const RecordingPlayButton: React.FC<RecordingPlayButtonProps> = ({
  renderButton,
  renderWhenNotPlayable,
  ...props
}) => {
  const dispatch = useAppDispatch();
  const recording = useFragment(
    recordingPlayButtonRecordingFragment,
    props.recording,
  );

  const playButtonVisible = !!recording?.id;

  const onClickPlay = () => {
    if (!recording) return;
    const artistId = recording.release.artist.id;
    const releaseFolderName = recording.release.folderName;
    // track id is a composite, but list passes trackNumber via context; fall back to index from UI as needed
    // For now this button is used in contexts where TrackItem provides number separately
    dispatch(
      musicPlayerSlice.actions.playTrack({
        artistId,
        releaseFolderName,
        trackNumber: 1, // caller should provide precise number; replace when integrated with TrackItem props
      }),
    );
  };

  if (!playButtonVisible) {
    return renderWhenNotPlayable?.() ?? null;
  }
  const needsYoutubeSearch = false;

  if (renderButton) {
    return renderButton(onClickPlay, needsYoutubeSearch);
  }

  return (
    <PlayButton
      onClick={onClickPlay}
      iconVariant={needsYoutubeSearch ? "search" : "play"}
    />
  );
};
