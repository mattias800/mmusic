import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { YoutubeMusicPlayer } from "@/features/music-players/youtube-music-player/YoutubeMusicPlayer.tsx";

export interface YoutubeVideoSearchProps {
  recordingId: string;
}

export const youtubeVideoSearchQuery = graphql(`
  query YoutubeVideoSearch($recordingId: ID!) {
    musicBrainz {
      recording {
        byId(id: $recordingId) {
          id
          streamingServiceInfo {
            id
            youtubeSearchVideoId
          }
        }
      }
    }
  }
`);

export const YoutubeVideoSearch: React.FC<YoutubeVideoSearchProps> = ({
  recordingId,
}) => {
  const [{ data, fetching }] = useQuery({
    query: youtubeVideoSearchQuery,
    variables: { recordingId },
  });

  if (fetching) {
    return (
      <SpinnerSpacing>
        <Spinner />
      </SpinnerSpacing>
    );
  }

  if (
    !data?.musicBrainz.recording.byId?.streamingServiceInfo.youtubeSearchVideoId
  ) {
    return <div>No Youtube Video Found</div>;
  }

  return (
    <YoutubeMusicPlayer
      youtubeVideoId={
        data?.musicBrainz.recording.byId?.streamingServiceInfo
          .youtubeSearchVideoId
      }
    />
  );
};
