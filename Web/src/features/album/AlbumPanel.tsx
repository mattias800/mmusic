import * as React from "react";
import { LargePlayButton } from "@/components/buttons/LargePlayButton.tsx";
import { ShuffleButton } from "@/components/buttons/ShuffleButton.tsx";
import { DotsButton } from "@/components/buttons/DotsButton.tsx";
import { LargeLikeButton } from "@/components/buttons/LargeLikeButton.tsx";
import { AlbumHeader } from "@/features/album/AlbumHeader.tsx";
import { AlbumTrackList } from "@/features/album/AlbumTrackList.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { GradientContent } from "@/components/page-body/GradientContent";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { Button } from "@/components/ui/button.tsx";
import { Download } from "lucide-react";
import { useMutation } from "urql";

export interface AlbumPanelProps {
  release: FragmentType<typeof albumPanelReleaseGroupFragment>;
}

const albumPanelReleaseGroupFragment = graphql(`
  fragment AlbumPanel_Release on Release {
    id
    folderName
    isFullyMissing
    ...AlbumHeader_Release
    ...AlbumTrackList_Release
    firstReleaseYear
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
        success
      }
    }
  }
`);

export const AlbumPanel: React.FC<AlbumPanelProps> = (props) => {
  const release = useFragment(albumPanelReleaseGroupFragment, props.release);
  const [, startDownload] = useMutation(startDownloadReleaseMutation);
  const [queued, setQueued] = React.useState(false);

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

  return (
    <GradientContent>
      <MainPadding>
        <div>
          <div>
            <AlbumHeader release={release} />

            <div className="flex items-center gap-6 mb-6">
              <LargePlayButton />
              <ShuffleButton />
              <LargeLikeButton />
              {release.isFullyMissing && (
                <Button
                  variant="secondary"
                  onClick={onClickDownload}
                  className="flex items-center gap-2"
                >
                  <Download className="h-5 w-5" />
                  {queued ? "Queued" : "Download"}
                </Button>
              )}
              <DotsButton />
            </div>
          </div>

          <div>
            <AlbumTrackList releaseGroup={release} />

            <div className="text-white/40 text-xs mt-12">
              <p>© {release.firstReleaseYear} Some label AB</p>
            </div>
          </div>
        </div>
      </MainPadding>
    </GradientContent>
  );
};
