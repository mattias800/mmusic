import { KeyGenerator } from "@urql/exchange-graphcache";

export const cacheKeys: Record<string, KeyGenerator> = {
  NameCredit: () => null,
};
