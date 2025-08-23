import * as React from "react";
import { useCallback, useState } from "react";
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
import { FixMatchDialog } from "@/features/album/components/FixMatchDialog.tsx";
import { LargeLikeButton } from "@/components/buttons/LargeLikeButton.tsx";
import { AlbumHeader } from "@/features/album/AlbumHeader.tsx";
import { AlbumTrackList } from "@/features/album/AlbumTrackList.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ReleaseDownloadButton } from "@/features/downloads/release-download-button/ReleaseDownloadButton.tsx";
import { PlayAlbumButton } from "@/features/album/PlayAlbumButton.tsx";
import { ConfirmDeleteReleaseAudioDialog } from "@/features/album/components/ConfirmDeleteReleaseAudioDialog.tsx";
import { GlassCard, PageLayout } from "@/components/ui";
import { FolderOpen, RefreshCw, Settings, Trash2 } from "lucide-react";

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
    labels {
      name
      id
      catalogNumber
      disambiguation
    }
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

const releaseMetadataUpdatedSubscription = graphql(`
  subscription AlbumPanelReleaseMetadataUpdated(
    $artistId: String!
    $releaseFolderName: String!
  ) {
    libraryReleaseMetadataUpdated(
      artistId: $artistId
      releaseFolderName: $releaseFolderName
    ) {
      id
      ...AlbumPanel_Release
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

const scanReleaseFolderForMediaMutation = graphql(`
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

export const AlbumPanel: React.FC<AlbumPanelProps> = (props) => {
  const release = useFragment(albumPanelReleaseGroupFragment, props.release);
  const [{ fetching: refreshing }, refreshRelease] = useMutation(
    refreshReleaseMutation,
  );
  const [{ fetching: deleting }, deleteReleaseAudio] = useMutation(
    deleteReleaseAudioMutation,
  );
  const [{ fetching: scanning }, scanReleaseFolderForMedia] = useMutation(
    scanReleaseFolderForMediaMutation,
  );

  const [confirmOpen, setConfirmOpen] = useState(false);
  const [fixOpen, setFixOpen] = useState(false);
  const [fixing, setFixing] = useState(false);

  const onConfirmDelete = useCallback(async () => {
    await deleteReleaseAudio({
      input: {
        artistId: release.artist.id,
        releaseFolderName: release.folderName,
      },
    });
  }, [deleteReleaseAudio, release]);

  useSubscription({
    query: albumUpdatesSubscription,
    variables: {
      artistId: release.artist.id,
      releaseFolderName: release.folderName,
    },
  });

  // When backend rebuilds metadata (e.g., after download or manual override),
  // this subscription delivers the updated Release object. urql cache will merge it.
  useSubscription({
    query: releaseMetadataUpdatedSubscription,
    variables: {
      artistId: release.artist.id,
      releaseFolderName: release.folderName,
    },
  });

  return (
    <PageLayout addSearchPadding>
      <div className="space-y-8">
        {/* Album Header with Cover Art */}
        <AlbumHeader release={release} />

        {/* Action Buttons */}
        <GlassCard>
          <div className={"flex items-center justify-between gap-4"}>
            <div className="flex items-center gap-4 flex-wrap">
              <PlayAlbumButton release={release} />
              <ShuffleButton />
              <LargeLikeButton />

              {/* Actions Dropdown */}
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <DotsButton />
                </DropdownMenuTrigger>
                <DropdownMenuContent>
                  <DropdownMenuLabel>Release Actions</DropdownMenuLabel>
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
                    <RefreshCw className="w-4 h-4 mr-2" />
                    Refresh release metadata
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    onSelect={() =>
                      scanReleaseFolderForMedia({
                        artistId: release.artist.id,
                        releaseFolderName: release.folderName,
                      })
                    }
                  >
                    <FolderOpen className="w-4 h-4 mr-2" />
                    Scan folder for media files
                  </DropdownMenuItem>
                  <DropdownMenuItem onSelect={() => setConfirmOpen(true)}>
                    <Trash2 className="w-4 h-4 mr-2" />
                    Delete audio files for this release
                  </DropdownMenuItem>
                  <DropdownMenuItem onSelect={() => setFixOpen(true)}>
                    <Settings className="w-4 h-4 mr-2" />
                    Fix match
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>

              {/* Loading Spinner */}
              {(refreshing || deleting || scanning || fixing) && (
                <div className="flex items-center gap-2 text-blue-400">
                  <Spinner size={"sm"} />
                  <span className="text-sm">
                    {refreshing && "Refreshing..."}
                    {deleting && "Deleting..."}
                    {scanning && "Scanning..."}
                    {fixing && "Fixing..."}
                  </span>
                </div>
              )}
            </div>
            <div>
              <ReleaseDownloadButton release={release} />
            </div>
          </div>
        </GlassCard>

        {/* Track List */}
        <GlassCard>
<AlbumTrackList releaseGroup={release} />
        </GlassCard>

        {/* Copyright Info */}
        <div className="text-center py-6">
          <div className="inline-flex items-center gap-2 text-gray-400 text-sm bg-white/5 px-4 py-2 rounded-full border border-white/10">
            <span>© {release.firstReleaseYear}</span>
            {release.labels && release.labels.length > 0 && (
              <>
                <span>•</span>
                <span>{release.labels[0].name}</span>
                {release.labels[0].catalogNumber && (
                  <>
                    <span>•</span>
                    <span className="text-xs opacity-75">
                      {release.labels[0].catalogNumber}
                    </span>
                  </>
                )}
              </>
            )}
            {(!release.labels || release.labels.length === 0) && (
              <>
                <span>•</span>
                <span className="opacity-50">
                  Label information unavailable
                </span>
              </>
            )}
          </div>
        </div>
      </div>

      <ConfirmDeleteReleaseAudioDialog
        open={confirmOpen}
        onOpenChange={setConfirmOpen}
        hasAnyAudio={!release.isFullyMissing}
        onConfirmDelete={onConfirmDelete}
      />

      <FixMatchDialog
        open={fixOpen}
        onOpenChange={setFixOpen}
        artistId={release.artist.id}
        releaseFolderName={release.folderName}
        onBeginFix={() => setFixing(true)}
        onEndFix={() => setFixing(false)}
      />
    </PageLayout>
  );
};
