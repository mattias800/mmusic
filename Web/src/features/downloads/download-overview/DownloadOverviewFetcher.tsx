import { graphql } from "@/gql";
import * as React from "react";
import { useQuery, useSubscription } from "urql";
import { Link } from "react-router";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { DownloadStatus } from "@/gql/graphql.ts";

export interface DownloadOverviewFetcherProps {}

const downloadOverviewFetcherQuery = graphql(`
  query DownloadOverviewQuery {
    downloads {
      downloadQueue {
        id
        queueLength
        items {
          id
          artistId
          releaseFolderName
        }
      }
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
          currentDownloadSpeedKbps
          currentProvider
          currentProviderIndex
          totalProviders
        }
        status
      }
    }
  }
`);

const downloadQueueUpdatedSub = graphql(`
  subscription DownloadQueueUpdatedSub {
    downloadQueueUpdated {
      queueLength
      items {
        id
        artistId
        releaseFolderName
      }
    }
  }
`);

const slotProgressUpdatedSub = graphql(`
  subscription SlotProgressUpdatedSub {
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
        currentDownloadSpeedKbps
        currentProvider
        currentProviderIndex
        totalProviders
      }
    }
  }
`);

const slotStatusUpdatedSub = graphql(`
  subscription SlotStatusUpdatedSub {
    slotStatusUpdated {
      slotId
      isActive
      currentWork {
        id
        artistId
        releaseFolderName
      }
    }
  }
`);

export const DownloadOverviewFetcher: React.FC<
  DownloadOverviewFetcherProps
> = () => {
  const [{ data, error, fetching }] = useQuery({
    query: downloadOverviewFetcherQuery,
  });

  useSubscription({ query: downloadQueueUpdatedSub });

  useSubscription({ query: slotProgressUpdatedSub });

  useSubscription({ query: slotStatusUpdatedSub });

  if (fetching) return <Spinner />;
  if (error || !data) return null;

  const queue = data.downloads.downloadQueue;
  const slots = data.downloads.downloadSlots;

  const statusText = (s?: DownloadStatus | null) => {
    switch (s) {
      case "SEARCHING":
        return "Searching…";
      case "DOWNLOADING":
        return "Downloading…";
      case "PROCESSING":
        return "Processing…";
      case "COMPLETED":
        return "Completed";
      case "FAILED":
        return "Failed";
      case "IDLE":
      default:
        return "Idle";
    }
  };

  // Find active working slots
  const workingSlots = slots?.filter(slot => slot.isWorking && slot.currentProgress) || [];

  return (
    <div className="space-y-3 text-sm">
      <div>
        <div className="font-medium text-zinc-200">Active Downloads ({workingSlots.length})</div>
        {workingSlots.length > 0 ? (
          workingSlots.map((slot) => {
            const progress = slot.currentProgress!;
            return (
              <div key={slot.id} className="text-zinc-300 mb-2">
                <div>
                  Slot {slot.id}:{" "}
                  <Link
                    to={`/artist/${progress.artistId}`}
                    className="text-blue-400 hover:text-blue-300 hover:underline"
                  >
                    {progress.artistId}
                  </Link>
                  {" - "}
                  <Link
                    to={`/artist/${progress.artistId}/release/${progress.releaseFolderName}`}
                    className="text-blue-400 hover:text-blue-300 hover:underline"
                  >
                    {progress.releaseFolderName}
                  </Link>
                </div>
                {progress.status && (
                  <div className="text-xs text-zinc-400">
                    {statusText(progress.status)} {progress.completedTracks}/
                    {progress.totalTracks}
                    {progress.currentProvider && progress.totalProviders && (
                      <span className="ml-2">
                        via {progress.currentProvider} ({progress.currentProviderIndex}/{progress.totalProviders})
                      </span>
                    )}
                  </div>
                )}
                {typeof progress.currentDownloadSpeedKbps === "number" && (
                  <div className="text-xs text-zinc-400">
                    Speed: {progress.currentDownloadSpeedKbps.toFixed(1)} KB/s
                  </div>
                )}
                {progress.errorMessage && (
                  <div className="text-xs text-red-400">{progress.errorMessage}</div>
                )}
              </div>
            );
          })
        ) : (
          <div className="text-zinc-400">No active downloads</div>
        )}
      </div>

      <div>
        <div className="font-medium text-zinc-200">
          {queue?.queueLength
            ? `${queue.queueLength} in queue`
            : "Queue is empty"}
        </div>
      </div>
    </div>
  );
};
