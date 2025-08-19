import { cacheExchange } from "@urql/exchange-graphcache";
import introspection from "./gql/introspection.json";
import { cacheKeys } from "./UrqlCacheKeys";
import { QueuesPage_QueryDocument } from "@/gql/graphql";

export const optimisticCacheExchange = cacheExchange({
  schema: introspection,
  keys: cacheKeys,
  updates: {
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
  optimistic: {},
});
