import { graphql } from "@/gql";
import * as React from "react";
import { useQuery, useSubscription } from "urql";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { DownloadStatus } from "@/gql/graphql.ts";

export interface DownloadOverviewFetcherProps {}

const downloadOverviewFetcherQuery = graphql(`
  query DownloadOverviewQuery {
    downloads {
      downloadQueue {
        queueLength
        items {
          artistId
          releaseFolderName
        }
      }
      currentDownload {
        artistId
        releaseFolderName
        status
        totalTracks
        completedTracks
        errorMessage
      }
    }
  }
`);

const downloadQueueUpdatedSub = graphql(`
  subscription DownloadQueueUpdatedSub {
    downloadQueueUpdated {
      queueLength
      items {
        artistId
        releaseFolderName
      }
    }
  }
`);

const currentDownloadUpdatedSub = graphql(`
  subscription CurrentDownloadUpdatedSub {
    currentDownloadUpdated {
      artistId
      releaseFolderName
      status
      totalTracks
      completedTracks
      errorMessage
    }
  }
`);

export const DownloadOverviewFetcher: React.FC<
  DownloadOverviewFetcherProps
> = () => {
  const [{ data, error, fetching }, reexecute] = useQuery({
    query: downloadOverviewFetcherQuery,
  });

  useSubscription({ query: downloadQueueUpdatedSub }, () => {
    reexecute({ requestPolicy: "network-only" });
    return null;
  });

  useSubscription({ query: currentDownloadUpdatedSub }, () => {
    reexecute({ requestPolicy: "network-only" });
    return null;
  });

  if (fetching) return <Spinner />;
  if (error || !data) return null;

  const queue = data.downloads.downloadQueue;
  const current = data.downloads.currentDownload;

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

  return (
    <div className="space-y-3 text-sm">
      <div>
        <div className="font-medium text-zinc-200">Current</div>
        {current && current.status !== DownloadStatus.Idle ? (
          <div className="text-zinc-300">
            <div>
              {current.artistId}/{current.releaseFolderName}
            </div>
            {current.status && (
              <div className="text-xs text-zinc-400">
                {statusText(current.status)} {current.completedTracks}/
                {current.totalTracks}
              </div>
            )}
            {current.errorMessage && (
              <div className="text-xs text-red-400">{current.errorMessage}</div>
            )}
          </div>
        ) : (
          <div className="text-zinc-400">Idle</div>
        )}
      </div>

      <div>
        <div className="font-medium text-zinc-200">
          Queue ({queue?.queueLength ?? 0})
        </div>
        <ul className="text-zinc-300 list-disc pl-4 space-y-1">
          {queue?.items?.map((i, idx) => (
            <li key={idx}>
              {i.artistId}/{i.releaseFolderName}
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};
