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

  // Calculate working slots and total download speed
  const workingSlots =
    slots?.filter((slot) => slot.isWorking && slot.currentProgress) || [];
  const totalSlots = slots?.length || 0;
  const workingCount = workingSlots.length;

  // Calculate total download speed from all working slots
  let totalSpeedKbps = 0;
  workingSlots.forEach((slot) => {
    if (slot.currentProgress?.currentDownloadSpeedKbps) {
      totalSpeedKbps += slot.currentProgress.currentDownloadSpeedKbps;
    }
  });

  return (
    <div className="space-y-3 text-sm">
      {/* Slots Summary */}
      <div>
        <div className="font-medium text-zinc-200">
          {workingCount > 0
            ? `${workingCount}/${totalSlots} slots working`
            : `${totalSlots} slots idle`}
        </div>
      </div>

      {/* Total Download Speed */}
      {totalSpeedKbps > 0 && (
        <div>
          <div className="font-medium text-zinc-200">Total Speed</div>
          <div className="text-zinc-400">{totalSpeedKbps.toFixed(1)} KB/s</div>
        </div>
      )}

      {/* Queue Status */}
      <div>
        <div className="font-medium text-zinc-200">
          {queue?.queueLength
            ? `${queue.queueLength} in queue`
            : "Queue is empty"}
        </div>
      </div>

      {/* Quick Status Summary */}
      {workingCount > 0 && (
        <div className="text-xs text-zinc-400">
          {workingSlots.map((slot) => {
            const progress = slot.currentProgress!;
            return (
              <div key={slot.id} className="truncate">
                <Link
                  to={`/artist/${progress.artistId}`}
                  className="hover:underline"
                >
                  {progress.artistId}
                </Link>
                {" - "}
                <Link
                  to={`/artist/${progress.artistId}/release/${progress.releaseFolderName}`}
                  className="hover:underline"
                >
                  {progress.releaseFolderName}
                </Link>
                : {statusText(progress.status)}
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
};
