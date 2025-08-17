import * as React from "react";
import { useState } from "react";
import { useMutation, useSubscription } from "urql";
import { FragmentType, graphql, useFragment } from "@/gql";
import { Download } from "lucide-react";
import { ReleaseDownloadStatus } from "@/gql/graphql.ts";
import { Tag } from "@/components/text/Tag.tsx";
import { GradientButton } from "@/components/ui";

export interface ReleaseDownloadButtonProps {
  release: FragmentType<typeof releaseDownloadButtonReleaseFragment>;
}

const releaseDownloadButtonReleaseFragment = graphql(`
  fragment ReleaseDownloadButton_Release on Release {
    id
    isFullyMissing
    folderName
    downloadStatus
    artist {
      id
    }
  }
`);

const startDownloadReleaseMutation = graphql(`
  mutation AlbumPanel_StartDownloadRelease(
    $artistId: String!
    $releaseFolderName: String!
  ) {
    startDownloadRelease(
      input: { artistId: $artistId, releaseFolderName: $releaseFolderName }
    ) {
      __typename
      ... on StartDownloadReleaseSuccess {
        release {
          id
        }
      }
      ... on StartDownloadReleaseAccepted {
        artistId
        releaseFolderName
      }
    }
  }
`);

const subscription = graphql(`
  subscription ReleaseDownloadButton(
    $artistId: String!
    $releaseFolderName: String!
  ) {
    libraryReleaseDownloadStatusUpdated(
      artistId: $artistId
      releaseFolderName: $releaseFolderName
    ) {
      release {
        id
        downloadStatus
        isFullyMissing
      }
    }
  }
`);

export const ReleaseDownloadButton: React.FC<ReleaseDownloadButtonProps> = (
  props,
) => {
  const release = useFragment(
    releaseDownloadButtonReleaseFragment,
    props.release,
  );

  const [queued, setQueued] = useState(false);

  const [, startDownload] = useMutation(startDownloadReleaseMutation);

  useSubscription({
    query: subscription,
    variables: {
      artistId: release.artist.id,
      releaseFolderName: release.folderName,
    },
  });

  const onClickDownload = async () => {
    try {
      const res = await startDownload({
        artistId: release.artist.id,
        releaseFolderName: release.folderName,
      });

      if (
        res.data?.startDownloadRelease?.__typename ===
        "StartDownloadReleaseSuccess"
      ) {
        setQueued(true);
        setTimeout(() => setQueued(false), 4000);
      }
    } catch {
      /* empty */
    }
  };

  if (
    release.downloadStatus === ReleaseDownloadStatus.Idle &&
    !release.isFullyMissing
  ) {
    return null;
  }

  switch (release.downloadStatus) {
    case ReleaseDownloadStatus.Downloading:
      return (
        <GradientButton
          onClick={onClickDownload}
          loading={true}
          disabled={true}
        >
          Downloading...
        </GradientButton>
      );

    case ReleaseDownloadStatus.Searching:
      return (
        <GradientButton
          onClick={onClickDownload}
          loading={true}
          disabled={true}
        >
          Searching...
        </GradientButton>
      );

    case ReleaseDownloadStatus.Idle:
      return (
        <GradientButton
          onClick={onClickDownload}
          iconLeft={Download}
        >
          {queued ? "Queued" : "Download"}
        </GradientButton>
      );

    case ReleaseDownloadStatus.NotFound:
      return (
        <div className={"flex items-center gap-4 text-red-500"}>
          <Tag variant={"error"}>Could not find release.</Tag>
          <GradientButton
            onClick={onClickDownload}
            iconLeft={Download}
          >
            {queued ? "Queued" : "Download"}
          </GradientButton>
        </div>
      );
  }

  return null;
};
