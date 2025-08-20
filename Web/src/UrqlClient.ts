import { Client, fetchExchange, mapExchange, subscriptionExchange } from "urql";
import { getApiUrl, getWsApiUrl } from "./ApiUrlProvider";
import { createClient as createWSClient } from "graphql-ws";
import { handleUrqlError } from "./UrqlClientErrorHandler";
import { optimisticCacheExchange } from "@/UrqlCache.ts";

const wsClient = createWSClient({
  url: getWsApiUrl(),
});

const fetchWithRedirect = async (
  url: URL | RequestInfo,
  options: RequestInit | undefined,
) => {
  const o = options || {};
  o.redirect = "follow";
  o.credentials = "include";

  const response = await fetch(url, o);
  if (response.redirected && response.url !== window.location.href) {
    window.location.href = response.url;
  }
  return response;
};

const customFetch: typeof fetch = (uri, options) => {
  if (typeof uri === "string") {
    let url = uri;
    if (options?.body && process.env.NODE_ENV !== "production") {
      const operationName = JSON.parse(options.body.toString()).operationName;

      if (operationName) {
        url += "?operationName=" + operationName;
        return fetchWithRedirect(url, options);
      }
    }
  }
  return fetchWithRedirect(uri, options);
};

const prodExchanges = [
  optimisticCacheExchange,
  mapExchange({
    onError: (_, op) => handleUrqlError(op, () => urqlClient),
  }),
  fetchExchange,
  subscriptionExchange({
    forwardSubscription(request) {
      const input = { ...request, query: request.query || "" };
      return {
        subscribe(sink) {
          const unsubscribe = wsClient.subscribe(input, sink);
          return { unsubscribe };
        },
      };
    },
  }),
];

const exchanges =
  process.env.NODE_ENV === "production" ? prodExchanges : [...prodExchanges];

export const urqlClient = new Client({
  url: getApiUrl(),
  exchanges,
  fetch: customFetch,
  fetchOptions: () => ({
    redirect: "follow",
  }),
});
