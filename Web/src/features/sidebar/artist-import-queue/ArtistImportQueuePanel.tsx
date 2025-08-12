import { graphql } from "@/gql";
import { useQuery, useSubscription } from "urql";
import React from "react";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { ArtistImportStatus } from "@/gql/graphql.ts";

const query = graphql(`
  query ArtistImportQueuePanel_Query {
    artistImport {
      artistImportQueue {
        id
        queueLength
        items {
          id
          artistName
          songTitle
        }
      }
      currentArtistImport {
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
  }
`);

const queueSub = graphql(`
  subscription ArtistImportQueueUpdatedSub {
    artistImportQueueUpdated {
      id
      queueLength
      items {
        id
        artistName
        songTitle
      }
    }
  }
`);

const currentSub = graphql(`
  subscription CurrentArtistImportUpdatedSub {
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

export const ArtistImportQueuePanel: React.FC = () => {
  const [{ data, error, fetching }, reexecute] = useQuery({ query });

  useSubscription({ query: queueSub }, () => {
    // refetch queue snapshot on update
    reexecute({ requestPolicy: "network-only" });
    return null;
  });

  useSubscription({ query: currentSub }, () => {
    reexecute({ requestPolicy: "network-only" });
    return null;
  });

  if (fetching) {
    return <Spinner />;
  }

  if (error) {
    return <div>Error: {error.message}</div>;
  }

  if (!data) {
    return <div>No data</div>;
  }

  const queue = data.artistImport.artistImportQueue;
  const current = data.artistImport.currentArtistImport;

  return (
    <div className="space-y-3 text-sm">
      <div>
        <div className="font-medium text-zinc-200">Current</div>
        {current && current.statusInfo.id !== ArtistImportStatus.Idle ? (
          <div className="text-zinc-300">
            <div>{current.artistName || "Idle"}</div>
            {current.statusInfo && (
              <div className="text-xs text-zinc-400">
                {current.statusInfo.text} {current.completedReleases}/
                {current.totalReleases}
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
          {queue?.queueLength ? `${queue.queueLength} in queue` : "Queue is empty"}
        </div>
      </div>
    </div>
  );
};
