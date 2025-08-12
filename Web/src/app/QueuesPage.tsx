import React from "react";
import { graphql } from "@/gql";
import { useMutation, useQuery, useSubscription } from "urql";
import { Button } from "@/components/ui/button";

const query = graphql(`
  query QueuesPage_Query {
    downloads {
      currentDownload {
        artistId
        releaseFolderName
        status
        totalTracks
        completedTracks
        errorMessage
      }
      downloadQueue {
        queueLength
        items {
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
      }
    }
    artistImport {
      currentArtistImport {
        artistName
        statusInfo { id text }
        totalReleases
        completedReleases
        errorMessage
      }
      artistImportQueue {
        queueLength
        items { artistName songTitle queueKey }
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
    downloadQueueUpdated { queueLength }
  }
`);

const subDlCur = graphql(`
  subscription QueuesPage_CurrentDownloadUpdated {
    currentDownloadUpdated { status }
  }
`);

const subImportQ = graphql(`
  subscription QueuesPage_ArtistImportQueueUpdated {
    artistImportQueueUpdated { queueLength }
  }
`);

const subImportCur = graphql(`
  subscription QueuesPage_CurrentArtistImportUpdated {
    currentArtistImportUpdated { status }
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
  const [{ data, fetching, error }, reexec] = useQuery({ query });
  useSubscription({ query: subDlQ }, () => { reexec({ requestPolicy: "network-only" }); return null; });
  useSubscription({ query: subDlCur }, () => { reexec({ requestPolicy: "network-only" }); return null; });
  useSubscription({ query: subImportQ }, () => { reexec({ requestPolicy: "network-only" }); return null; });
  useSubscription({ query: subImportCur }, () => { reexec({ requestPolicy: "network-only" }); return null; });

  const [, removeDownload] = useMutation(removeDownloadMutation);
  const [, removeImport] = useMutation(removeImportMutation);

  if (fetching) return <div className="p-6">Loading…</div>;
  if (error || !data) return <div className="p-6">Error</div>;

  const dl = data.downloads;
  const ai = data.artistImport;

  return (
    <div className="p-6 grid grid-cols-1 lg:grid-cols-2 gap-8">
      <section>
        <h2 className="text-xl font-semibold mb-2">Current download</h2>
        <div className="rounded border border-zinc-700 p-3">
          {dl.currentDownload ? (
            <div className="text-sm text-zinc-300">
              <div>
                {dl.currentDownload.artistId}/{dl.currentDownload.releaseFolderName}
              </div>
              <div className="text-xs text-zinc-400">
                {dl.currentDownload.status} {dl.currentDownload.completedTracks}/{dl.currentDownload.totalTracks}
              </div>
              {dl.currentDownload.errorMessage && (
                <div className="text-xs text-red-400">{dl.currentDownload.errorMessage}</div>
              )}
            </div>
          ) : (
            <div className="text-sm text-zinc-400">Idle</div>
          )}
        </div>

        <h2 className="text-xl font-semibold mt-6 mb-2">Download queue ({dl.downloadQueue.queueLength})</h2>
        <div className="rounded border border-zinc-700 p-3 space-y-2">
          {dl.downloadQueue.items.map((q) => (
            <div key={q.queueKey} className="flex items-center justify-between text-sm text-zinc-300">
              <div>{q.artistId}/{q.releaseFolderName}</div>
              <Button variant="destructive" size="sm" onClick={() => q.queueKey && removeDownload({ queueKey: q.queueKey })}>Remove</Button>
            </div>
          ))}
          {dl.downloadQueue.items.length === 0 && (
            <div className="text-sm text-zinc-400">Queue is empty</div>
          )}
        </div>

        <h2 className="text-xl font-semibold mt-6 mb-2">Download history</h2>
        <div className="rounded border border-zinc-700 p-3 space-y-1 text-sm">
          {dl.downloadHistory.map((h, idx) => (
            <div key={idx} className="flex items-center justify-between">
              <div className="text-zinc-400">
                {new Date(h.timestampUtc).toLocaleString()} — {h.artistName ?? h.artistId}/{h.releaseTitle ?? h.releaseFolderName}
              </div>
              <div className={h.success ? "text-green-400" : "text-red-400"}>
                {h.success ? "Completed" : "Failed"}
              </div>
            </div>
          ))}
          {dl.downloadHistory.length === 0 && <div className="text-zinc-400">No recent downloads</div>}
        </div>
      </section>

      <section>
        <h2 className="text-xl font-semibold mb-2">Current import</h2>
        <div className="rounded border border-zinc-700 p-3">
          {ai.currentArtistImport ? (
            <div className="text-sm text-zinc-300">
              <div>{ai.currentArtistImport.artistName}</div>
              <div className="text-xs text-zinc-400">
                {ai.currentArtistImport.statusInfo.text} {ai.currentArtistImport.completedReleases}/{ai.currentArtistImport.totalReleases}
              </div>
              {ai.currentArtistImport.errorMessage && (
                <div className="text-xs text-red-400">{ai.currentArtistImport.errorMessage}</div>
              )}
            </div>
          ) : (
            <div className="text-sm text-zinc-400">Idle</div>
          )}
        </div>

        <h2 className="text-xl font-semibold mt-6 mb-2">Import queue ({ai.artistImportQueue.queueLength})</h2>
        <div className="rounded border border-zinc-700 p-3 space-y-2">
          {ai.artistImportQueue.items.map((q, idx) => (
            <div key={q.queueKey ?? idx} className="flex items-center justify-between text-sm text-zinc-300">
              <div>{q.artistName}{q.songTitle ? ` — ${q.songTitle}` : ""}</div>
              {q.queueKey && (
                <Button variant="destructive" size="sm" onClick={() => removeImport({ queueKey: q.queueKey! })}>Remove</Button>
              )}
            </div>
          ))}
          {ai.artistImportQueue.items.length === 0 && (
            <div className="text-sm text-zinc-400">Queue is empty</div>
          )}
        </div>

        <h2 className="text-xl font-semibold mt-6 mb-2">Import history</h2>
        <div className="rounded border border-zinc-700 p-3 space-y-1 text-sm">
          {ai.artistImportHistory.map((h, idx) => (
            <div key={idx} className="flex items-center justify-between">
              <div className="text-zinc-400">
                {new Date(h.timestampUtc).toLocaleString()} — {h.artistName}
                {h.releaseFolderName ? `/${h.releaseFolderName}` : ""}
              </div>
              <div className={h.success ? "text-green-400" : "text-red-400"}>
                {h.success ? "Completed" : "Failed"}
              </div>
            </div>
          ))}
          {ai.artistImportHistory.length === 0 && <div className="text-zinc-400">No recent imports</div>}
        </div>
      </section>
    </div>
  );
};


