import { KeyGenerator } from "@urql/exchange-graphcache";
import { TypesWithNoId } from "@/gql/types-with-no-id.ts";

export const cacheKeys: Record<TypesWithNoId, KeyGenerator> = {
  AlbumImages: () => null,
  ArtistImages: () => null,
  ArtistInServerLibrary: () => null,
  ArtistSearchRoot: () => null,
  ArtistsInServerLibrarySearchRoot: () => null,
  DownloadsSearchRoot: () => null,
  ImportPlaylistSearchRoot: () => null,
  LastFmStatistics: () => null,
  Medium: () => null,
  NameCredit: () => null,
  PageInfo: () => null,
  PlaylistSearchRoot: () => null,
  RecommendationsSearchRoot: () => null,
  RecordingSearchRoot: () => null,
  Relation: () => null,
  ReleaseGroupInServerLibrary: () => null,
  ReleaseGroupsInServerLibrarySearchRoot: () => null,
  ReleaseSearchRoot: () => null,
  ServerLibrarySearchRoot: () => null,
  SpotifyPlaylistSearchRoot: () => null,
  UserProjection: () => null,
  UserSearchRoot: () => null,
  UsersConnection: () => null,
  UsersEdge: () => null,
};
