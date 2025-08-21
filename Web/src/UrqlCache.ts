import {
  Cache,
  cacheExchange,
  ResolveInfo,
  Variables,
} from "@urql/exchange-graphcache";
import introspection from "./gql/introspection.json";
import { cacheKeys } from "./UrqlCacheKeys";
import { QueuesPage_QueryDocument } from "@/gql/graphql";
import {
  ResultOf,
  TypedDocumentNode,
  VariablesOf,
} from "@graphql-typed-document-node/core";
import { updateDownloaderSettingsMutation } from "@/features/settings/DownloadersTogglesCardMutations.tsx";

/* eslint-disable @typescript-eslint/no-explicit-any */

const createOptimistic = <
  TDocumentNode extends TypedDocumentNode<any, any>,
  TOptimisticFieldValue extends {
    __typename: string;
  } = {
    __typename: string;
  },
>(
  factory: (
    cache: Cache,
    variables: VariablesOf<TDocumentNode>,
  ) => ResultOf<TDocumentNode> | null,
  mutationFieldResolver: (
    result: ResultOf<TDocumentNode>,
  ) => TOptimisticFieldValue,
) => {
  return (
    _args: Variables,
    cache: Cache,
    info: ResolveInfo & { variables: VariablesOf<TDocumentNode> },
  ) => {
    const fullOptimisticResponse = factory(cache, info.variables);
    return fullOptimisticResponse == null
      ? null
      : mutationFieldResolver(fullOptimisticResponse);
  };
};

export const optimisticCacheExchange = cacheExchange({
  schema: introspection,
  keys: cacheKeys,
  updates: {
    Subscription: {
      // Handle slot progress updates for the multi-slot download system
      slotProgressUpdated: (result, _args, cache) => {
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
    updateDownloaderSettings: createOptimistic<
      typeof updateDownloaderSettingsMutation
    >(
      (cache, variables) => {
        // Resolve ServerSettings id without coupling to a query document
        const serverSettingsKey = cache.resolve("Query", "serverSettings") as
          | string
          | null;
        if (!serverSettingsKey) return null;
        const id = cache.resolve(serverSettingsKey, "id") as string | null;
        if (!id) return null;

        return {
          __typename: "Mutation",
          updateDownloaderSettings: {
            __typename: "UpdateDownloaderSettingsSuccess",
            serverSettings: {
              __typename: "ServerSettings",
              id,
              enableSabnzbdDownloader: variables.input.enableSabnzbdDownloader,
              enableQBittorrentDownloader:
                variables.input.enableQBittorrentDownloader,
              enableSoulSeekDownloader:
                variables.input.enableSoulSeekDownloader,
            },
          },
        } as const;
      },
      (r) => r.updateDownloaderSettings,
    ),
  },
});
