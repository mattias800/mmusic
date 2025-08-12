import { cacheExchange } from "@urql/exchange-graphcache";
import introspection from "./gql/introspection.json";
import { cacheKeys } from "./UrqlCacheKeys";
import { DownloadOverviewQueryDocument, QueuesPage_QueryDocument } from "@/gql/graphql";

export const optimisticCacheExchange = cacheExchange({
  schema: introspection,
  keys: cacheKeys,
  updates: {
    Subscription: {
      downloadQueueUpdated: (result, _args, cache) => {
        const queue = (result as any)?.downloadQueueUpdated;
        if (!queue) return;

        cache.updateQuery({ query: DownloadOverviewQueryDocument }, (data) => {
          if (!data) return data;
          data.downloads.downloadQueue = {
            ...(data.downloads.downloadQueue as any),
            ...queue,
          };
          return data;
        });

        cache.updateQuery({ query: QueuesPage_QueryDocument }, (data) => {
          if (!data) return data;
          data.downloads.downloadQueue = {
            ...(data.downloads.downloadQueue as any),
            ...queue,
          };
          return data;
        });
      },
      currentDownloadUpdated: (result, _args, cache) => {
        const progress = (result as any)?.currentDownloadUpdated ?? null;

        cache.updateQuery({ query: DownloadOverviewQueryDocument }, (data) => {
          if (!data) return data;
          if (progress === null) {
            data.downloads.currentDownload = null;
          } else {
            data.downloads.currentDownload = {
              ...(data.downloads.currentDownload as any),
              ...progress,
            };
          }
          return data;
        });

        cache.updateQuery({ query: QueuesPage_QueryDocument }, (data) => {
          if (!data) return data;
          if (progress === null) {
            data.downloads.currentDownload = null;
          } else {
            data.downloads.currentDownload = {
              ...(data.downloads.currentDownload as any),
              ...progress,
            };
          }
          return data;
        });
      },
      artistImportQueueUpdated: (result, _args, cache) => {
        const queue = (result as any)?.artistImportQueueUpdated;
        if (!queue) return;
        cache.updateQuery({ query: QueuesPage_QueryDocument }, (data) => {
          if (!data) return data;
          data.artistImport.artistImportQueue = {
            ...(data.artistImport.artistImportQueue as any),
            ...queue,
          };
          return data;
        });
      },
      currentArtistImportUpdated: (result, _args, cache) => {
        const current = (result as any)?.currentArtistImportUpdated ?? null;
        cache.updateQuery({ query: QueuesPage_QueryDocument }, (data) => {
          if (!data) return data;
          if (current === null) {
            data.artistImport.currentArtistImport = null as any;
          } else {
            data.artistImport.currentArtistImport = {
              ...(data.artistImport.currentArtistImport as any),
              ...current,
            };
          }
          return data;
        });
      },
    },
  },
  optimistic: {},
});
