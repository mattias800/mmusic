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
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog.tsx";
import { Button } from "@/components/ui/button.tsx";
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

const deleteReleaseAudioMutation = graphql(`
  mutation DeleteReleaseAudio($input: DeleteReleaseAudioInput!) {
    deleteReleaseAudio(input: $input) {
      ... on DeleteReleaseAudioSuccess {
        release {
          id
          ...AlbumPanel_Release
        }
      }
      ... on DeleteReleaseAudioError {
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
  const [{ fetching: deleting }, deleteReleaseAudio] = useMutation(
    deleteReleaseAudioMutation,
  );

  const [confirmOpen, setConfirmOpen] = React.useState(false);
  const hasAnyAudio = React.useMemo(() => {
    // We don't have audioFilePath in this fragment; use isMissing on tracks inversely
    // If any track is not missing, we consider audio present
    return (release.tracks ?? []).some((t) => !t.isMissing);
  }, [release]);

  const onConfirmDelete = React.useCallback(() => {
    deleteReleaseAudio({
      input: { artistId: release.artist.id, releaseFolderName: release.folderName },
    });
  }, [deleteReleaseAudio, release]);

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
                  <DropdownMenuLabel>Actions</DropdownMenuLabel>
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
                  <DropdownMenuItem
                    onSelect={() => setConfirmOpen(true)}
                  >
                    Delete audio files for this release
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
              {(refreshing || deleting) && <Spinner size={"sm"} />}
              <ReleaseDownloadButton release={release} />
            </div>
          </div>

          <div>
            <AlbumTrackList releaseGroup={release} />

            <Dialog open={confirmOpen} onOpenChange={setConfirmOpen}>
              <DialogContent>
                <DialogHeader>
                  <DialogTitle>
                    {hasAnyAudio ? "Delete all audio files?" : "No audio files to delete"}
                  </DialogTitle>
                  <DialogDescription>
                    {hasAnyAudio
                      ? "Are you sure you want to delete all audio files for this release? This will remove the files from disk and clear references in release.json."
                      : "There are no audio files referenced in this release. No action will be taken."}
                  </DialogDescription>
                </DialogHeader>
                <DialogFooter>
                  <Button variant="outline" onClick={() => setConfirmOpen(false)}>
                    Close
                  </Button>
                  {hasAnyAudio && (
                    <Button
                      variant="destructive"
                      onClick={() => {
                        onConfirmDelete();
                        setConfirmOpen(false);
                      }}
                    >
                      Delete audio files
                    </Button>
                  )}
                </DialogFooter>
              </DialogContent>
            </Dialog>

            <div className="text-white/40 text-xs mt-12">
              <p>© {release.firstReleaseYear} Some label AB</p>
            </div>
          </div>
        </div>
      </MainPadding>
    </GradientContent>
  );
};
