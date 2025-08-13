import { cacheExchange } from "@urql/exchange-graphcache";
import introspection from "./gql/introspection.json";
import { cacheKeys } from "./UrqlCacheKeys";
import { DownloadOverviewQueryDocument, QueuesPage_QueryDocument } from "@/gql/graphql";

export const optimisticCacheExchange = cacheExchange({
  schema: introspection,
  keys: cacheKeys,
  updates: {
    Subscription: {
      // With normalized ids in place, subscriptions auto-merge updates.
      // We still ensure the link flips to null, and also re-point the link
      // when a new release starts (different id).
      currentDownloadUpdated: (result, _args, cache) => {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const payload = (result as any)?.currentDownloadUpdated ?? null;
        if (payload === null) {
          cache.updateQuery({ query: DownloadOverviewQueryDocument }, (data) => {
            if (!data) return data;
            data.downloads.currentDownload = null;
            return data;
          });
          cache.updateQuery({ query: QueuesPage_QueryDocument }, (data) => {
            if (!data) return data;
            data.downloads.currentDownload = null;
            return data;
          });
          return;
        }

        // Ensure the link points to the latest entity (new id when moving to next release)
        cache.updateQuery({ query: DownloadOverviewQueryDocument }, (data) => {
          if (!data) return data;
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          data.downloads.currentDownload = payload as any;
          return data;
        });
        cache.updateQuery({ query: QueuesPage_QueryDocument }, (data) => {
          if (!data) return data;
          // eslint-disable-next-line @typescript-eslint/no-explicit-any
          data.downloads.currentDownload = payload as any;
          return data;
        });
      },
    },
  },
  optimistic: {},
});
