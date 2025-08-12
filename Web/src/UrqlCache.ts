import { cacheExchange } from "@urql/exchange-graphcache";
import introspection from "./gql/introspection.json";
import { cacheKeys } from "./UrqlCacheKeys";
import { DownloadOverviewQueryDocument, QueuesPage_QueryDocument } from "@/gql/graphql";

export const optimisticCacheExchange = cacheExchange({
  schema: introspection,
  keys: cacheKeys,
  updates: {
    Subscription: {
      // With normalized ids in place, subscriptions auto-merge.
      // Only special-case null transitions for currentDownload.
      currentDownloadUpdated: (result, _args, cache) => {
        if ((result as any)?.currentDownloadUpdated !== null) return;
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
      },
    },
  },
  optimistic: {},
});
