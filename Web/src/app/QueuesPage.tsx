import React from "react";
import { graphql } from "@/gql";
import { useMutation, useQuery, useSubscription } from "urql";
import { Button } from "@/components/ui/button";
import { Link } from "react-router";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import { ProgressIndicator } from "@/components/progress/ProgressIndicator";
import { DownloadStatus } from "@/gql/graphql.ts";
import {
  PageLayout,
  PageHeader,
  GlassCard,
  InfoSection,
} from "@/components/ui";
import {
  Download,
  Clock,
  ListOrdered,
  CheckCircle,
  XCircle,
  AlertTriangle,
} from "lucide-react";

// Type for the actual query result structure
type QueuesPageDownloadSlot = {
  id: number;
  isActive: boolean;
  isWorking: boolean;
  currentWork?: {
    artistId: string;
    releaseFolderName: string;
  } | null;
  currentProgress?: {
    id: string;
    artistId: string;
    releaseFolderName: string;
    status: string;
    totalTracks: number;
    completedTracks: number;
    errorMessage?: string | null;
    artistName?: string | null;
    releaseTitle?: string | null;
    coverArtUrl?: string | null;
    currentTrackProgressPercent?: number | null;
    currentDownloadSpeedKbps?: number | null;
    currentProvider?: string | null;
    currentProviderIndex?: number | null;
    totalProviders?: number | null;
  } | null;
  startedAt?: string | null;
  lastActivityAt?: string | null;
  status?: string | null;
};

type QueuesPageImport = {
  id: string;
  artistName: string;
  songTitle?: string | null;
  statusInfo: {
    id: string;
    text: string;
  };
  totalReleases: number;
  completedReleases: number;
  errorMessage?: string | null;
};

type QueuesPageQueueItem = {
  id: string;
  artistId?: string | null;
  artistName?: string | null;
  releaseFolderName?: string | null;
  songTitle?: string | null;
  queueKey?: string | null;
};

type QueuesPageHistoryItem = {
  timestampUtc: string;
  artistId?: string | null;
  artistName?: string | null;
  releaseFolderName?: string | null;
  releaseTitle?: string | null;
  localArtistId?: string | null;
  songTitle?: string | null;
  success: boolean;
  errorMessage?: string | null;
  providerUsed?: string | null;
  jobKind?: string | null;
  musicBrainzArtistId?: string | null;
};

const query = graphql(`
  query QueuesPage_Query {
    downloads {
      downloadSlots {
        id
        isActive
        isWorking
        currentWork {
          artistId
          releaseFolderName
        }
        currentProgress {
          id
          artistId
          releaseFolderName
          status
          totalTracks
          completedTracks
          errorMessage
          artistName
          releaseTitle
          coverArtUrl
          currentTrackProgressPercent
          currentDownloadSpeedKbps
          currentProvider
          currentProviderIndex
          totalProviders
        }
        startedAt
        lastActivityAt
        status
      }
      downloadQueue {
        id
        queueLength
        items {
          id
          artistId
          releaseFolderName
          queueKey
        }
      }
      downloadHistory {
        timestampUtc
        artistId
        releaseFolderName
        artistName
        releaseTitle
        success
        errorMessage
        providerUsed
      }
    }
    artistImport {
      currentArtistImport {
        id
        artistName
        statusInfo {
          id
          text
        }
        totalReleases
        completedReleases
        errorMessage
      }
      artistImportQueue {
        id
        queueLength
        items {
          id
          artistName
          songTitle
          releaseFolderName
          queueKey
        }
      }
      artistImportHistory {
        timestampUtc
        jobKind
        artistName
        localArtistId
        releaseFolderName
        musicBrainzArtistId
        songTitle
        success
        errorMessage
      }
    }
  }
`);

const subDlQ = graphql(`
  subscription QueuesPage_DownloadQueueUpdated {
    downloadQueueUpdated {
      id
      queueLength
      id
      items {
        id
        artistId
        releaseFolderName
        queueKey
      }
    }
  }
`);

const subSlotProgress = graphql(`
  subscription QueuesPage_SlotProgressUpdated {
    slotProgressUpdated {
      slotId
      progress {
        id
        artistId
        releaseFolderName
        status
        totalTracks
        completedTracks
        errorMessage
        artistName
        releaseTitle
        coverArtUrl
        currentTrackProgressPercent
        currentDownloadSpeedKbps
        currentProvider
        currentProviderIndex
        totalProviders
      }
    }
  }
`);

const subImportQ = graphql(`
  subscription QueuesPage_ArtistImportQueueUpdated {
    artistImportQueueUpdated {
      id
      queueLength
      items {
        id
        artistName
        songTitle
        queueKey
      }
    }
  }
`);

const subImportCur = graphql(`
  subscription QueuesPage_CurrentArtistImportUpdated {
    currentArtistImportUpdated {
      id
      artistName
      songTitle
      statusInfo {
        id
        text
      }
      totalReleases
      completedReleases
      errorMessage
    }
  }
`);

const removeDownloadMutation = graphql(`
  mutation RemoveDownloadJob($queueKey: String!) {
    removeDownloadJob(queueKey: $queueKey)
  }
`);

const removeImportMutation = graphql(`
  mutation RemoveArtistImportJob($queueKey: String!) {
    removeArtistImportJob(queueKey: $queueKey)
  }
`);

export const QueuesPage: React.FC = () => {
  const [{ data, fetching, error }] = useQuery({ query });
  useSubscription({ query: subDlQ });
  useSubscription({ query: subSlotProgress });
  useSubscription({ query: subImportQ });
  useSubscription({ query: subImportCur });

  const [, removeDownload] = useMutation(removeDownloadMutation);
  const [, removeImport] = useMutation(removeImportMutation);

  if (fetching) return <div className="p-6">Loading…</div>;
  if (error || !data) return <div className="p-6">Error</div>;

  const dl = data.downloads;
  const ai = data.artistImport;

  return (
    <PageLayout>
      {/* Header Section */}
      <PageHeader
        icon={AlertTriangle}
        title="Queues & Activity"
        subtitle="Monitor downloads, imports, and system activity"
      />

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-8 max-w-7xl mx-auto">
        {/* Left Column - Downloads */}
        <div className="space-y-8">
          {/* Download Slots */}
          <GlassCard
            title="Download Slots"
            icon={Download}
            iconBgColor="bg-green-500/20"
          >
            <div className="space-y-4">
              {dl.downloadSlots && dl.downloadSlots.length > 0 ? (
                dl.downloadSlots.map((slot) => (
                  <DownloadSlotCard key={slot.id} slot={slot} />
                ))
              ) : (
                <div className="text-gray-400 text-center py-8">
                  No download slots available
                </div>
              )}
            </div>
          </GlassCard>

          {/* Download Queue */}
          <GlassCard
            title="Download Queue"
            icon={Clock}
            iconBgColor="bg-blue-500/20"
          >
            <div className="space-y-3">
              {dl.downloadQueue.items.map((q) => (
                <QueueItem
                  key={q.queueKey}
                  item={q}
                  onRemove={() =>
                    q.queueKey && removeDownload({ queueKey: q.queueKey })
                  }
                  type="download"
                />
              ))}
              {dl.downloadQueue.items.length === 0 && (
                <div className="text-sm text-gray-400 text-center py-4">
                  Queue is empty
                </div>
              )}
            </div>
          </GlassCard>

          {/* Download History */}
          <GlassCard
            title="Download History"
            icon={CheckCircle}
            iconBgColor="bg-emerald-500/20"
          >
            <div className="space-y-3">
              {dl.downloadHistory.map((h, idx) => (
                <HistoryItem key={idx} item={h} type="download" />
              ))}
              {dl.downloadHistory.length === 0 && (
                <div className="text-sm text-gray-400 text-center py-4">
                  No recent downloads
                </div>
              )}
            </div>
          </GlassCard>
        </div>

        {/* Right Column - Artist Import */}
        <div className="space-y-8">
          {/* Current Import */}
          <GlassCard
            title="Current Import"
            icon={ListOrdered}
            iconBgColor="bg-purple-500/20"
          >
            {ai.currentArtistImport ? (
              <CurrentImportCard import={ai.currentArtistImport} />
            ) : (
              <div className="text-sm text-gray-400 text-center py-8">Idle</div>
            )}
          </GlassCard>

          {/* Import Queue */}
          <GlassCard
            title="Import Queue"
            icon={Clock}
            iconBgColor="bg-orange-500/20"
          >
            <div className="space-y-3">
              {ai.artistImportQueue.items.map((q, idx) => (
                <QueueItem
                  key={q.queueKey ?? idx}
                  item={q}
                  onRemove={() =>
                    q.queueKey && removeImport({ queueKey: q.queueKey })
                  }
                  type="import"
                />
              ))}
              {ai.artistImportQueue.items.length === 0 && (
                <div className="text-sm text-gray-400 text-center py-4">
                  Queue is empty
                </div>
              )}
            </div>
          </GlassCard>

          {/* Import History */}
          <GlassCard
            title="Import History"
            icon={CheckCircle}
            iconBgColor="bg-indigo-500/20"
          >
            <div className="space-y-3">
              {ai.artistImportHistory.map((h, idx) => (
                <HistoryItem key={idx} item={h} type="import" />
              ))}
              {ai.artistImportHistory.length === 0 && (
                <div className="text-sm text-gray-400 text-center py-4">
                  No recent imports
                </div>
              )}
            </div>
          </GlassCard>
        </div>
      </div>

      {/* Bottom Info Section */}
      <InfoSection
        icon={AlertTriangle}
        title="About Queues & Activity"
        variant="blue"
      >
        This page shows real-time information about your mmusic system. Download
        slots display active downloads with progress bars, queues show pending
        jobs waiting to be processed, and history provides a record of completed
        operations. All data updates automatically via live subscriptions.
      </InfoSection>
    </PageLayout>
  );
};

// Helper Components
const DownloadSlotCard: React.FC<{ slot: QueuesPageDownloadSlot }> = ({
  slot,
}) => (
  <div className="p-4 bg-white/5 rounded-lg border border-white/10">
    <div className="flex items-center justify-between mb-3">
      <div className="flex items-center gap-2">
        <span className="text-sm font-medium text-white">
          Slot {slot.id} - {slot.status || "Idle"}
        </span>
        <div
          className={`px-2 py-1 rounded-full text-xs ${
            slot.isActive
              ? "bg-green-500/20 text-green-400"
              : "bg-gray-500/20 text-gray-400"
          }`}
        >
          {slot.isActive ? "Active" : "Inactive"}
        </div>
      </div>
    </div>

    {slot.isWorking && slot.currentWork && slot.currentProgress ? (
      <div className="space-y-3">
        <div className="flex items-center gap-4">
          <Link
            to={`/artist/${slot.currentWork.artistId}/release/${slot.currentWork.releaseFolderName}`}
          >
            <ReleaseCoverArt
              srcUrl={
                slot.currentProgress.coverArtUrl ??
                `/library/${slot.currentWork.artistId}/releases/${slot.currentWork.releaseFolderName}/coverart`
              }
              titleForPlaceholder={
                slot.currentProgress.releaseTitle ??
                slot.currentWork.releaseFolderName
              }
              className="w-20 h-20 rounded object-cover border border-white/20"
            />
          </Link>
          <div className="flex-1 min-w-0">
            <div className="truncate text-base font-semibold text-white">
              <Link
                to={`/artist/${slot.currentWork.artistId}`}
                className="hover:underline"
              >
                {slot.currentProgress.artistName ?? slot.currentWork.artistId}
              </Link>
              {" - "}
              <Link
                to={`/artist/${slot.currentWork.artistId}/release/${slot.currentWork.releaseFolderName}`}
                className="hover:underline"
              >
                {slot.currentProgress.releaseTitle ??
                  slot.currentWork.releaseFolderName}
              </Link>
            </div>
            <div className="text-sm text-gray-300 mt-1">
              {slot.currentProgress.status}{" "}
              {slot.currentProgress.status === DownloadStatus.Downloading && (
                <>
                  — {slot.currentProgress.completedTracks}/
                  {slot.currentProgress.totalTracks}
                </>
              )}
              {slot.currentProgress.currentProvider &&
                slot.currentProgress.totalProviders && (
                  <span className="ml-2 text-gray-400">
                    via {slot.currentProgress.currentProvider} (
                    {slot.currentProgress.currentProviderIndex}/
                    {slot.currentProgress.totalProviders})
                  </span>
                )}
            </div>
            {typeof slot.currentProgress.currentDownloadSpeedKbps ===
              "number" && (
              <div className="text-xs text-gray-400 mt-1">
                Speed:{" "}
                {slot.currentProgress.currentDownloadSpeedKbps.toFixed(1)} KB/s
              </div>
            )}
          </div>
        </div>

        {slot.currentProgress && (
          <>
            <div>
              <ProgressIndicator
                progressPercent={
                  slot.currentProgress.totalTracks > 0
                    ? Math.min(
                        100,
                        Math.round(
                          (slot.currentProgress.completedTracks /
                            slot.currentProgress.totalTracks) *
                            100,
                        ),
                      )
                    : 0
                }
              />
            </div>

            {typeof slot.currentProgress.currentTrackProgressPercent ===
              "number" && (
              <div className="text-xs text-gray-400">
                Current track:{" "}
                {Math.max(1, slot.currentProgress.completedTracks)} /{" "}
                {slot.currentProgress.totalTracks}
                <div className="mt-1">
                  <ProgressIndicator
                    progressPercent={Math.max(
                      0,
                      Math.min(
                        100,
                        slot.currentProgress.currentTrackProgressPercent,
                      ),
                    )}
                  />
                </div>
              </div>
            )}

            {slot.currentProgress.errorMessage && (
              <div className="text-sm text-red-400 mt-2 p-2 bg-red-500/10 rounded border border-red-500/20">
                {slot.currentProgress.errorMessage}
              </div>
            )}
          </>
        )}
      </div>
    ) : (
      <div className="text-gray-400 text-center py-6">
        {slot.isActive ? "Waiting for work..." : "Slot inactive"}
      </div>
    )}
  </div>
);

const CurrentImportCard: React.FC<{ import: QueuesPageImport }> = ({
  import: importItem,
}) => (
  <div className="space-y-4">
    <div className="flex items-center gap-4">
      <Link to={`/artist/${importItem.artistName}`}>
        <ReleaseCoverArt
          titleForPlaceholder={importItem.artistName}
          className="w-20 h-20 rounded object-cover border border-white/20"
        />
      </Link>
      <div className="flex-1 min-w-0">
        <div className="truncate text-base font-semibold text-white">
          <Link
            to={`/artist/${importItem.artistName}`}
            className="hover:underline"
          >
            {importItem.artistName}
          </Link>
          {importItem.songTitle && (
            <>
              {" - "}
              <span className="text-gray-300">{importItem.songTitle}</span>
            </>
          )}
        </div>
        <div className="text-sm text-gray-300 mt-1">
          {importItem.statusInfo.text} — {importItem.completedReleases}/
          {importItem.totalReleases}
        </div>
      </div>
    </div>

    <div>
      <ProgressIndicator
        progressPercent={
          importItem.totalReleases > 0
            ? Math.min(
                100,
                Math.round(
                  (importItem.completedReleases / importItem.totalReleases) *
                    100,
                ),
              )
            : 0
        }
      />
    </div>

    {importItem.errorMessage && (
      <div className="text-sm text-red-400 p-2 bg-red-500/10 rounded border border-red-500/20">
        {importItem.errorMessage}
      </div>
    )}
  </div>
);

const QueueItem: React.FC<{
  item: QueuesPageQueueItem;
  onRemove: () => void;
  type: "download" | "import";
}> = ({ item, onRemove, type }) => (
  <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg border border-white/10">
    <div className="flex items-center gap-3">
      {type === "download" ? (
        <Link to={`/artist/${item.artistId}/release/${item.releaseFolderName}`}>
          <ReleaseCoverArt
            srcUrl={`/library/${item.artistId}/releases/${item.releaseFolderName}/coverart`}
            titleForPlaceholder={item.releaseFolderName || "Release"}
            className="w-8 h-8 rounded object-cover border border-white/20"
          />
        </Link>
      ) : (
        <div className="w-8 h-8 rounded bg-white/10 border border-white/20" />
      )}
      <div className="text-sm text-gray-300">
        {type === "download" ? (
          <>
            {item.artistId && (
              <Link to={`/artist/${item.artistId}`} className="hover:underline">
                {item.artistId}
              </Link>
            )}
            {item.artistId && item.releaseFolderName && (
              <>
                {" - "}
                <Link
                  to={`/artist/${item.artistId}/release/${item.releaseFolderName}`}
                  className="hover:underline"
                >
                  {item.releaseFolderName}
                </Link>
              </>
            )}
          </>
        ) : (
          <>
            {item.artistName && (
              <Link
                to={`/artist/${item.artistName}`}
                className="hover:underline"
              >
                {item.artistName}
              </Link>
            )}
            {item.releaseFolderName
              ? ` — ${item.releaseFolderName}`
              : item.songTitle
                ? ` — ${item.songTitle}`
                : ""}
          </>
        )}
      </div>
    </div>
    <Button
      variant="destructive"
      size="sm"
      onClick={onRemove}
      className="bg-red-500/20 border-red-500/30 text-red-400 hover:bg-red-500/30"
    >
      Remove
    </Button>
  </div>
);

const HistoryItem: React.FC<{
  item: QueuesPageHistoryItem;
  type: "download" | "import";
}> = ({ item, type }) => (
  <div className="flex items-center justify-between p-3 bg-white/5 rounded-lg border border-white/10">
    <div className="flex items-center gap-3">
      {type === "download" && item.releaseFolderName ? (
        <Link to={`/artist/${item.artistId}/release/${item.releaseFolderName}`}>
          <ReleaseCoverArt
            srcUrl={`/library/${item.artistId}/releases/${item.releaseFolderName}/coverart`}
            titleForPlaceholder={item.releaseTitle ?? item.releaseFolderName}
            className="w-8 h-8 rounded object-cover border border-white/20"
          />
        </Link>
      ) : type === "import" && item.releaseFolderName ? (
        <Link
          to={`/artist/${item.localArtistId ?? item.artistName}/release/${item.releaseFolderName}`}
        >
          <ReleaseCoverArt
            srcUrl={`/library/${item.localArtistId ?? item.artistName}/releases/${item.releaseFolderName}/coverart`}
            titleForPlaceholder={item.releaseFolderName}
            className="w-8 h-8 rounded object-cover border border-white/20"
          />
        </Link>
      ) : (
        <div className="w-8 h-8" />
      )}
      <div className="text-sm text-gray-300">
        <div className="text-xs text-gray-400 mb-1">
          {new Date(item.timestampUtc).toLocaleString()}
        </div>
        {type === "download" ? (
          <>
            <Link to={`/artist/${item.artistId}`} className="hover:underline">
              {item.artistName ?? item.artistId}
            </Link>
            {item.releaseFolderName ? (
              <>
                {" - "}
                <Link
                  to={`/artist/${item.artistId}/release/${item.releaseFolderName}`}
                  className="hover:underline"
                >
                  {item.releaseTitle ?? item.releaseFolderName}
                </Link>
              </>
            ) : null}
          </>
        ) : (
          <>
            <Link
              to={`/artist/${item.localArtistId ?? item.artistName}`}
              className="hover:underline"
            >
              {item.artistName}
            </Link>
            {item.releaseFolderName ? (
              <>
                {" - "}
                <Link
                  to={`/artist/${item.localArtistId ?? item.artistName}/release/${item.releaseFolderName}`}
                  className="hover:underline"
                >
                  {item.releaseFolderName}
                </Link>
              </>
            ) : null}
          </>
        )}
      </div>
    </div>
    <div className="flex flex-col items-end text-right">
      <div
        className={`flex items-center gap-1 ${
          item.success ? "text-green-400" : "text-red-400"
        }`}
      >
        {item.success ? (
          <>
            <CheckCircle className="w-4 h-4" />
            <span className="text-xs">Completed</span>
          </>
        ) : (
          <>
            <XCircle className="w-4 h-4" />
            <span className="text-xs">Failed</span>
          </>
        )}
      </div>
      {type === "download" && item.providerUsed && (
        <div className="text-xs text-gray-400 mt-1">
          via {item.providerUsed}
        </div>
      )}
    </div>
  </div>
);
