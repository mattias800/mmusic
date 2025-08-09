import * as React from "react";
import { ShuffleButton } from "@/components/buttons/ShuffleButton.tsx";
import { DotsButton } from "@/components/buttons/DotsButton.tsx";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { useMutation, useSubscription } from "urql";
import { LargeLikeButton } from "@/components/buttons/LargeLikeButton.tsx";
import { AlbumHeader } from "@/features/album/AlbumHeader.tsx";
import { AlbumTrackList } from "@/features/album/AlbumTrackList.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { GradientContent } from "@/components/page-body/GradientContent";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { ReleaseDownloadButton } from "@/features/downloads/release-download-button/ReleaseDownloadButton.tsx";
import { PlayAlbumButton } from "@/features/album/PlayAlbumButton.tsx";

export interface AlbumPanelProps {
  release: FragmentType<typeof albumPanelReleaseGroupFragment>;
}

const albumPanelReleaseGroupFragment = graphql(`
  fragment AlbumPanel_Release on Release {
    id
    folderName
    isFullyMissing
    ...PlayAlbumButton_Release
    ...ReleaseDownloadButton_Release
    ...AlbumHeader_Release
    ...AlbumTrackList_Release
    firstReleaseYear
    artist {
      id
    }
  }
`);

const albumUpdatesSubscription = graphql(`
  subscription AlbumPanelUpdates(
    $artistId: String!
    $releaseFolderName: String!
  ) {
    libraryCacheTracksInReleaseUpdated(
      artistId: $artistId
      releaseFolderName: $releaseFolderName
    ) {
      track {
        id
        isMissing
        mediaAvailabilityStatus
      }
    }
  }
`);

const refreshReleaseMutation = graphql(`
  mutation RefreshRelease($input: RefreshReleaseInput!) {
    refreshRelease(input: $input) {
      ... on RefreshReleaseSuccess {
        release {
          id
          ...AlbumPanel_Release
        }
      }
      ... on RefreshReleaseError {
        message
      }
    }
  }
`);

export const AlbumPanel: React.FC<AlbumPanelProps> = (props) => {
  const release = useFragment(albumPanelReleaseGroupFragment, props.release);
  const [{ fetching: refreshing }, refreshRelease] = useMutation(
    refreshReleaseMutation,
  );

  useSubscription({
    query: albumUpdatesSubscription,
    variables: {
      artistId: release.artist.id,
      releaseFolderName: release.folderName,
    },
  });

  return (
    <GradientContent>
      <MainPadding>
        <div>
          <div>
            <AlbumHeader release={release} />

            <div className="flex items-center gap-3 mb-6">
              <PlayAlbumButton release={release} />
              <ShuffleButton />
              <LargeLikeButton />
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <DotsButton />
                </DropdownMenuTrigger>
                <DropdownMenuContent>
                  <DropdownMenuLabel>Refresh</DropdownMenuLabel>
                  <DropdownMenuItem
                    onSelect={() =>
                      refreshRelease({
                        input: {
                          artistId: release.artist.id,
                          releaseFolderName: release.folderName,
                        },
                      })
                    }
                  >
                    Refresh release metadata
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
              {refreshing && <Spinner size={"sm"} />}
              <ReleaseDownloadButton release={release} />
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
