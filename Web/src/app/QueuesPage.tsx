import React from "react";
import { graphql } from "@/gql";
import { useMutation, useQuery, useSubscription } from "urql";
import { Button } from "@/components/ui/button";
import { Link } from "react-router";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import { ProgressIndicator } from "@/components/progress/ProgressIndicator";
import { DownloadStatus } from "@/gql/graphql.ts";

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
    <div className="p-6 grid grid-cols-1 lg:grid-cols-2 gap-8">
      <section>
        <h2 className="text-xl font-semibold mb-2">Download Slots ({dl.downloadSlots?.length || 0})</h2>
        <div className="space-y-4">
          {dl.downloadSlots && dl.downloadSlots.length > 0 ? (
            dl.downloadSlots.map((slot) => (
              <div key={slot.id} className="rounded border border-zinc-700 p-4">
                <div className="text-zinc-300">
                  <div className="flex items-center justify-between mb-2">
                    <div className="text-sm font-medium">
                      Slot {slot.id} - {slot.status || "Idle"}
                    </div>
                    <div className="text-xs text-zinc-500">
                      {slot.isActive ? "Active" : "Inactive"}
                    </div>
                  </div>
                  
                  {slot.isWorking && slot.currentWork && slot.currentProgress ? (
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
                          className="w-24 h-24 rounded object-cover border border-zinc-700"
                        />
                      </Link>
                      <div className="flex-1 min-w-0">
                        <div className="truncate text-lg font-semibold">
                          <Link
                            to={`/artist/${slot.currentWork.artistId}`}
                            className="hover:underline"
                          >
                            {slot.currentProgress.artistName ??
                              slot.currentWork.artistId}
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
                        <div className="text-sm text-zinc-400 mt-1">
                          {slot.currentProgress.status}{" "}
                          {slot.currentProgress.status === DownloadStatus.Downloading && (
                            <>
                              — {slot.currentProgress.completedTracks}/
                              {slot.currentProgress.totalTracks}
                            </>
                          )}
                          {slot.currentProgress.currentProvider && slot.currentProgress.totalProviders && (
                            <span className="ml-2">
                              via {slot.currentProgress.currentProvider} ({slot.currentProgress.currentProviderIndex}/{slot.currentProgress.totalProviders})
                            </span>
                          )}
                        </div>
                        {typeof slot.currentProgress.currentDownloadSpeedKbps === "number" && (
                          <div className="text-xs text-zinc-400 mt-1">
                            Speed: {slot.currentProgress.currentDownloadSpeedKbps.toFixed(1)} KB/s
                          </div>
                        )}
                      </div>
                    </div>
                  ) : (
                    <div className="text-zinc-400 text-center py-8">
                      {slot.isActive ? "Waiting for work..." : "Slot inactive"}
                    </div>
                  )}

                  {slot.currentProgress && (
                    <>
                      <div className="mt-3">
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

                      {typeof slot.currentProgress.currentTrackProgressPercent === "number" && (
                        <div className="mt-3 text-xs text-zinc-400">
                          Current track: {Math.max(1, slot.currentProgress.completedTracks)} / {slot.currentProgress.totalTracks}
                          <div className="mt-1">
                            <ProgressIndicator
                              progressPercent={Math.max(
                                0,
                                Math.min(100, slot.currentProgress.currentTrackProgressPercent),
                              )}
                            />
                          </div>
                        </div>
                      )}

                      {slot.currentProgress.errorMessage && (
                        <div className="text-sm text-red-400 mt-2">
                          {slot.currentProgress.errorMessage}
                        </div>
                      )}
                    </>
                  )}
                </div>
              </div>
            ))
          ) : (
            <div className="text-zinc-400 text-center py-8">No download slots available</div>
          )}
        </div>

        <h2 className="text-xl font-semibold mt-6 mb-2">
          Download queue ({dl.downloadQueue.queueLength})
        </h2>
        <div className="rounded border border-zinc-700 p-3 space-y-2">
          {dl.downloadQueue.items.map((q) => (
            <div
              key={q.queueKey}
              className="flex items-center justify-between text-sm text-zinc-300"
            >
              <div className="flex items-center gap-3">
                <Link
                  to={`/artist/${q.artistId}/release/${q.releaseFolderName}`}
                >
                  <ReleaseCoverArt
                    srcUrl={`/library/${q.artistId}/releases/${q.releaseFolderName}/coverart`}
                    titleForPlaceholder={q.releaseFolderName}
                    className="w-8 h-8 rounded object-cover border border-zinc-700"
                  />
                </Link>
                <div>
                  <Link
                    to={`/artist/${q.artistId}`}
                    className="hover:underline"
                  >
                    {q.artistId}
                  </Link>
                  {" - "}
                  <Link
                    to={`/artist/${q.artistId}/release/${q.releaseFolderName}`}
                    className="hover:underline"
                  >
                    {q.releaseFolderName}
                  </Link>
                </div>
              </div>
              <Button
                variant="destructive"
                size="sm"
                className="text-black"
                onClick={() =>
                  q.queueKey && removeDownload({ queueKey: q.queueKey })
                }
              >
                Remove
              </Button>
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
              <div className="flex items-center gap-3">
                {h.releaseFolderName ? (
                  <Link
                    to={`/artist/${h.artistId}/release/${h.releaseFolderName}`}
                  >
                    <ReleaseCoverArt
                      srcUrl={`/library/${h.artistId}/releases/${h.releaseFolderName}/coverart`}
                      titleForPlaceholder={
                        h.releaseTitle ?? h.releaseFolderName
                      }
                      className="w-8 h-8 rounded object-cover border border-zinc-700"
                    />
                  </Link>
                ) : (
                  <div className="w-8 h-8" />
                )}
                <div className="flex flex-col">
                  <div className="text-xs text-zinc-400">
                    {new Date(h.timestampUtc).toLocaleString()}
                  </div>
                  <div className="text-zinc-400">
                    <Link
                      to={`/artist/${h.artistId}`}
                      className="hover:underline"
                    >
                      {h.artistName ?? h.artistId}
                    </Link>
                    {h.releaseFolderName ? (
                      <>
                        {" - "}
                        <Link
                          to={`/artist/${h.artistId}/release/${h.releaseFolderName}`}
                          className="hover:underline"
                        >
                          {h.releaseTitle ?? h.releaseFolderName}
                        </Link>
                      </>
                    ) : null}
                  </div>
                </div>
              </div>
              <div className="flex flex-col items-end text-right">
                <div className={h.success ? "text-green-400" : "text-red-400"}>
                  {h.success ? "Completed" : "Failed"}
                </div>
                {h.providerUsed && (
                  <div className="text-xs text-zinc-500 mt-1">
                    via {h.providerUsed}
                  </div>
                )}
              </div>
            </div>
          ))}
          {dl.downloadHistory.length === 0 && (
            <div className="text-zinc-400">No recent downloads</div>
          )}
        </div>
      </section>

      <section>
        <h2 className="text-xl font-semibold mb-2">Current import</h2>
        <div className="rounded border border-zinc-700 p-4">
          {ai.currentArtistImport ? (
            <div className="text-zinc-300">
              <div className="flex items-center gap-4">
                <Link to={`/artist/${ai.currentArtistImport.artistName}`}>
                  <ReleaseCoverArt
                    titleForPlaceholder={ai.currentArtistImport.artistName}
                    className="w-24 h-24 rounded object-cover border border-zinc-700"
                  />
                </Link>
                <div className="flex-1 min-w-0">
                  <div className="truncate text-lg font-semibold">
                    <Link
                      to={`/artist/${ai.currentArtistImport.artistName}`}
                      className="hover:underline"
                    >
                      {ai.currentArtistImport.artistName}
                    </Link>
                  </div>
                  <div className="text-sm text-zinc-400 mt-1">
                    {ai.currentArtistImport.statusInfo.text} —
                    {" "}
                    {ai.currentArtistImport.completedReleases}/
                    {ai.currentArtistImport.totalReleases}
                  </div>
                </div>
              </div>

              <div className="mt-3">
                <ProgressIndicator
                  progressPercent={
                    ai.currentArtistImport.totalReleases > 0
                      ? Math.min(
                          100,
                          Math.round(
                            (ai.currentArtistImport.completedReleases /
                              ai.currentArtistImport.totalReleases) *
                              100,
                          ),
                        )
                      : 0
                  }
                />
              </div>

              {ai.currentArtistImport.errorMessage && (
                <div className="text-sm text-red-400 mt-2">
                  {ai.currentArtistImport.errorMessage}
                </div>
              )}
            </div>
          ) : (
            <div className="text-sm text-zinc-400">Idle</div>
          )}
        </div>

        <h2 className="text-xl font-semibold mt-6 mb-2">
          Import queue ({ai.artistImportQueue.queueLength})
        </h2>
        <div className="rounded border border-zinc-700 p-3 space-y-2">
          {ai.artistImportQueue.items.map((q, idx) => (
            <div
              key={q.queueKey ?? idx}
              className="flex items-center justify-between text-sm text-zinc-300"
            >
              <div className="flex items-center gap-3">
                <div className="w-8 h-8 rounded bg-zinc-800 border border-zinc-700" />
                <div>
                  <Link
                    to={`/artist/${q.artistName}`}
                    className="hover:underline"
                  >
                    {q.artistName}
                  </Link>
                  {q.releaseFolderName
                    ? ` — ${q.releaseFolderName}`
                    : q.songTitle
                      ? ` — ${q.songTitle}`
                      : ""}
                </div>
              </div>
              {q.queueKey && (
                <Button
                  variant="destructive"
                  size="sm"
                  className="text-black"
                  onClick={() => removeImport({ queueKey: q.queueKey! })}
                >
                  Remove
                </Button>
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
              <div className="flex items-center gap-3">
                {h.releaseFolderName ? (
                  <Link
                    to={`/artist/${h.localArtistId ?? h.artistName}/release/${h.releaseFolderName}`}
                  >
                    <ReleaseCoverArt
                      srcUrl={`/library/${h.localArtistId ?? h.artistName}/releases/${h.releaseFolderName}/coverart`}
                      titleForPlaceholder={h.releaseFolderName}
                      className="w-8 h-8 rounded object-cover border border-zinc-700"
                    />
                  </Link>
                ) : (
                  <div className="w-8 h-8" />
                )}
                <div className="text-zinc-400">
                  {new Date(h.timestampUtc).toLocaleString()} —{" "}
                  <Link
                    to={`/artist/${h.localArtistId ?? h.artistName}`}
                    className="hover:underline"
                  >
                    {h.artistName}
                  </Link>
                  {h.releaseFolderName ? (
                    <>
                      {" - "}
                      <Link
                        to={`/artist/${h.localArtistId ?? h.artistName}/release/${h.releaseFolderName}`}
                        className="hover:underline"
                      >
                        {h.releaseFolderName}
                      </Link>
                    </>
                  ) : null}
                </div>
              </div>
              <div className={h.success ? "text-green-400" : "text-red-400"}>
                {h.success ? "Completed" : "Failed"}
              </div>
            </div>
          ))}
          {ai.artistImportHistory.length === 0 && (
            <div className="text-zinc-400">No recent imports</div>
          )}
        </div>
      </section>
    </div>
  );
};
