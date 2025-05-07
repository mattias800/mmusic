import { cacheExchange } from "@urql/exchange-graphcache";
import introspection from "./gql/introspection.json";
import { cacheKeys } from "./UrqlCacheKeys";

export const optimisticCacheExchange = cacheExchange({
  schema: introspection,
  keys: cacheKeys,
  optimistic: {},
});
