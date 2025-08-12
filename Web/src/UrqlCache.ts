import { cacheExchange } from "@urql/exchange-graphcache";
import introspection from "./gql/introspection.json";
import { cacheKeys } from "./UrqlCacheKeys";
import { DownloadOverviewQueryDocument } from "@/gql/graphql";

export const optimisticCacheExchange = cacheExchange({
  schema: introspection,
  keys: cacheKeys,
  updates: {
    Subscription: {
      downloadQueueUpdated: (result, _args, cache) => {
        cache.updateQuery({ query: DownloadOverviewQueryDocument }, (data) => {
          if (!data) return data;
          const queue = (result as any)?.downloadQueueUpdated;
          if (queue) {
            data.downloads.downloadQueue = queue;
          }
          return data;
        });
      },
      currentDownloadUpdated: (result, _args, cache) => {
        cache.updateQuery({ query: DownloadOverviewQueryDocument }, (data) => {
          if (!data) return data;
          const progress = (result as any)?.currentDownloadUpdated ?? null;
          data.downloads.currentDownload = progress;
          return data;
        });
      },
    },
  },
  optimistic: {},
});
