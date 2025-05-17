import { KeyGenerator } from "@urql/exchange-graphcache";
import { TypesWithNoId } from "@/gql/types-with-no-id.ts";

export const cacheKeys: Record<TypesWithNoId, KeyGenerator> = {
  NameCredit: () => null,
  ArtistImages: () => null,
  ArtistInServerLibrary: () => null,
  ArtistsInServerLibrarySearchRoot: () => null,
  Medium: () => null,
  ReleaseGroupInServerLibrary: () => null,
  ReleaseGroupsInServerLibrarySearchRoot: () => null,
  ServerLibrarySearchRoot: () => null,
};
