import { KeyGenerator } from "@urql/exchange-graphcache";
import { TypesWithNoId } from "@/gql/types-with-no-id.ts";

export const cacheKeys: Record<TypesWithNoId, KeyGenerator> = {
  AlbumImages: () => null,
  ArtistImages: () => null,
  ArtistInServerLibrary: () => null,
  ArtistSearchRoot: () => null,
  ArtistsInServerLibrarySearchRoot: () => null,
  DownloadsSearchRoot: () => null,
  LastFmStatistics: () => null,
  Medium: () => null,
  NameCredit: () => null,
  RecordingSearchRoot: () => null,
  Relation: () => null,
  ReleaseGroupInServerLibrary: () => null,
  ReleaseGroupsInServerLibrarySearchRoot: () => null,
  ReleaseSearchRoot: () => null,
  ServerLibrarySearchRoot: () => null,
};
