import { cacheExchange } from "@urql/exchange-graphcache";
import introspection from "./gql/introspection.json";
import { cacheKeys } from "./UrqlCacheKeys";
import { QueuesPage_QueryDocument } from "@/gql/graphql";
import { DownloadersTogglesCardDocument } from "@/features/admin/graphql/Downloaders.gql.ts";

export const optimisticCacheExchange = cacheExchange({
  schema: introspection,
  keys: cacheKeys,
  updates: {
    Mutation: {
      // Merge serverSettings from mutation result back into the cache
      updateDownloaderSettings: (result, _args, cache) => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const payload = (result as any)?.updateDownloaderSettings ?? null;
        if (!payload || payload.__typename !== "UpdateDownloaderSettingsSuccess") {
          return;
        }

        const updated = payload.serverSettings;
        if (!updated?.id) return;

        cache.writeFragment(
          {
            id: cache.keyOfEntity({ __typename: "ServerSettings", id: updated.id }),
            fragment: `fragment _ServerSettings_Downloaders on ServerSettings { id enableSabnzbdDownloader enableQBittorrentDownloader enableSoulSeekDownloader }`,
          },
          updated,
        );
      },
    },
    Subscription: {
      // Handle slot progress updates for the multi-slot download system
      slotProgressUpdated: (result, _args, cache) => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const payload = (result as any)?.slotProgressUpdated ?? null;
        if (!payload) return;

        const slotId = payload.slotId;
        const progress = payload.progress;

        // Update the slot progress in the downloads data
        cache.updateQuery({ query: QueuesPage_QueryDocument }, (data) => {
          if (!data?.downloads?.downloadSlots) return data;

          const slotIndex = data.downloads.downloadSlots.findIndex(
            (slot) => slot.id === slotId,
          );
          if (slotIndex >= 0) {
            // Update the existing slot
            data.downloads.downloadSlots[slotIndex] = {
              ...data.downloads.downloadSlots[slotIndex],
              currentProgress: progress,
              isWorking: progress !== null,
              lastActivityAt: progress
                ? new Date().toISOString()
                : data.downloads.downloadSlots[slotIndex].lastActivityAt,
            };
          }

          return data;
        });
      },
    },
  },
  optimistic: {
    // Provide an optimistic response for toggling downloader settings
    updateDownloaderSettings: (_args, cache) => {
      // Graphcache optimistic resolvers should mirror the mutation payload
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const input = (_args as any)?.input ?? {};
      const existing = cache.readQuery({ query: DownloadersTogglesCardDocument });
      const id = existing?.serverSettings?.id;
      return {
        __typename: "UpdateDownloaderSettingsSuccess",
        serverSettings: {
          __typename: "ServerSettings",
          id: id,
          enableSabnzbdDownloader: !!input.enableSabnzbdDownloader ?? null,
          enableQBittorrentDownloader: !!input.enableQBittorrentDownloader ?? null,
          enableSoulSeekDownloader: !!input.enableSoulSeekDownloader ?? null,
        },
      } as const;
    },
  },
});
