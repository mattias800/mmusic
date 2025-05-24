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

export const recordingPlayButtonRecordingFragment = graphql(`
  fragment RecordingPlayButton_Recording on Recording {
    id
    streamingServiceInfo {
      id
      youtubeVideoId
    }
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

  const playButtonVisible =
    recording?.streamingServiceInfo.youtubeVideoId || recording?.id;

  const onClickPlay = () => {
    if (recording?.streamingServiceInfo.youtubeVideoId) {
      dispatch(
        musicPlayerSlice.actions.openYoutubeVideoId({
          youtubeVideoId: recording.streamingServiceInfo.youtubeVideoId,
        }),
      );
    } else if (recording) {
      dispatch(
        musicPlayerSlice.actions.openRecordingId({
          recordingId: recording.id,
        }),
      );
    }
  };

  if (!playButtonVisible) {
    return renderWhenNotPlayable?.() ?? null;
  }
  const needsYoutubeSearch = !recording?.streamingServiceInfo.youtubeVideoId;

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
