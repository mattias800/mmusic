import { graphql } from "@/gql";
import { useQuery, useSubscription } from "urql";

const query = graphql(`
  query ArtistImportQueuePanel_Query {
    artistImportQueue {
      queueLength
      items { artistName songTitle }
    }
    currentArtistImport {
      artistName
      songTitle
      status
      totalReleases
      completedReleases
      errorMessage
    }
  }
`);

const queueSub = graphql(`
  subscription ArtistImportQueueUpdatedSub {
    artistImportQueueUpdated {
      queueLength
      items { artistName songTitle }
    }
  }
`);

const currentSub = graphql(`
  subscription CurrentArtistImportUpdatedSub {
    currentArtistImportUpdated {
      artistName
      songTitle
      status
      totalReleases
      completedReleases
      errorMessage
    }
  }
`);

export const ArtistImportQueuePanel: React.FC = () => {
  const [res, reexecute] = useQuery({ query });
  useSubscription({ query: queueSub }, () => {
    // refetch queue snapshot on update
    reexecute({ requestPolicy: "network-only" });
    return null;
  });
  useSubscription({ query: currentSub }, () => {
    reexecute({ requestPolicy: "network-only" });
    return null;
  });

  const queue = res.data?.artistImportQueue;
  const current = res.data?.currentArtistImport;

  return (
    <div className="space-y-3 text-sm">
      <div>
        <div className="font-medium text-zinc-200">Current</div>
        {current ? (
          <div className="text-zinc-300">
            <div>{current.artistName || "Idle"}</div>
            {current.status && (
              <div className="text-xs text-zinc-400">
                {current.status} {current.completedReleases}/{current.totalReleases}
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
        <div className="font-medium text-zinc-200">Queue ({queue?.queueLength ?? 0})</div>
        <ul className="text-zinc-300 list-disc pl-4 space-y-1">
          {queue?.items?.map((i, idx) => (
            <li key={idx}>
              {i.artistName}
              {i.songTitle ? ` – ${i.songTitle}` : ""}
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
};


