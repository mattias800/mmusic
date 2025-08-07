/* eslint-disable */
import { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';
export type Maybe<T> = T | null;
export type InputMaybe<T> = Maybe<T>;
export type Exact<T extends { [key: string]: unknown }> = { [K in keyof T]: T[K] };
export type MakeOptional<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]?: Maybe<T[SubKey]> };
export type MakeMaybe<T, K extends keyof T> = Omit<T, K> & { [SubKey in K]: Maybe<T[SubKey]> };
export type MakeEmpty<T extends { [key: string]: unknown }, K extends keyof T> = { [_ in K]?: never };
export type Incremental<T> = T | { [P in keyof T]?: P extends ' $fragmentName' | '__typename' ? T[P] : never };
/** All built-in and custom scalars, mapped to their actual values */
export type Scalars = {
  ID: { input: string; output: string; }
  String: { input: string; output: string; }
  Boolean: { input: boolean; output: boolean; }
  Int: { input: number; output: number; }
  Float: { input: number; output: number; }
  /** The `DateTime` scalar represents an ISO-8601 compliant date time type. */
  DateTime: { input: any; output: any; }
  /** The `Long` scalar type represents non-fractional signed whole 64-bit numeric values. Long can represent values between -(2^63) and 2^63 - 1. */
  Long: { input: any; output: any; }
  UUID: { input: any; output: any; }
};

export type AddArtistToServerLibraryArtistAlreadyAdded = {
  __typename?: 'AddArtistToServerLibraryArtistAlreadyAdded';
  message: Scalars['String']['output'];
};

export type AddArtistToServerLibraryArtistDoesNotExist = {
  __typename?: 'AddArtistToServerLibraryArtistDoesNotExist';
  message: Scalars['String']['output'];
};

export type AddArtistToServerLibraryInput = {
  artistId: Scalars['ID']['input'];
};

export type AddArtistToServerLibraryResult = AddArtistToServerLibraryArtistAlreadyAdded | AddArtistToServerLibraryArtistDoesNotExist | AddArtistToServerLibrarySuccess | AddArtistToServerLibraryUnknownError;

export type AddArtistToServerLibrarySuccess = {
  __typename?: 'AddArtistToServerLibrarySuccess';
  success: Scalars['Boolean']['output'];
};

export type AddArtistToServerLibraryUnknownError = {
  __typename?: 'AddArtistToServerLibraryUnknownError';
  message: Scalars['String']['output'];
};

/** Defines when a policy shall be executed. */
export enum ApplyPolicy {
  /** After the resolver was executed. */
  AfterResolver = 'AFTER_RESOLVER',
  /** Before the resolver was executed. */
  BeforeResolver = 'BEFORE_RESOLVER',
  /** The policy is applied in the validation step before the execution. */
  Validation = 'VALIDATION'
}

export type Artist = {
  __typename?: 'Artist';
  albums: Array<Release>;
  eps: Array<Release>;
  id: Scalars['ID']['output'];
  images?: Maybe<ArtistImages>;
  listeners?: Maybe<Scalars['Long']['output']>;
  name: Scalars['String']['output'];
  releaseByFolderName?: Maybe<Release>;
  releases: Array<Release>;
  serverStatus: ArtistServerStatus;
  singles: Array<Release>;
  sortName: Scalars['String']['output'];
  topTracks: Array<ArtistTopTrack>;
};


export type ArtistReleaseByFolderNameArgs = {
  releaseFolderName: Scalars['String']['input'];
};

export type ArtistImages = {
  __typename?: 'ArtistImages';
  backgrounds: Array<Scalars['String']['output']>;
  banners: Array<Scalars['String']['output']>;
  logos: Array<Scalars['String']['output']>;
  thumbs: Array<Scalars['String']['output']>;
};

export type ArtistServerStatus = {
  __typename?: 'ArtistServerStatus';
  id: Scalars['ID']['output'];
  result: ArtistServerStatusResult;
};

export type ArtistServerStatusImportingArtist = ArtistServerStatusResultBase & {
  __typename?: 'ArtistServerStatusImportingArtist';
  artistMbId: Scalars['String']['output'];
  releasesVisible: Scalars['Boolean']['output'];
  topTracksVisible: Scalars['Boolean']['output'];
};

export type ArtistServerStatusImportingArtistReleases = ArtistServerStatusResultBase & {
  __typename?: 'ArtistServerStatusImportingArtistReleases';
  artist: Artist;
  numReleaseGroupsFinishedImporting: Scalars['Int']['output'];
  releasesVisible: Scalars['Boolean']['output'];
  topTracksVisible: Scalars['Boolean']['output'];
  totalNumReleaseGroupsBeingImported: Scalars['Int']['output'];
};

export type ArtistServerStatusNotInLibrary = ArtistServerStatusResultBase & {
  __typename?: 'ArtistServerStatusNotInLibrary';
  artistMbId: Scalars['String']['output'];
  releasesVisible: Scalars['Boolean']['output'];
  topTracksVisible: Scalars['Boolean']['output'];
};

export type ArtistServerStatusReady = ArtistServerStatusResultBase & {
  __typename?: 'ArtistServerStatusReady';
  artistMbId: Scalars['String']['output'];
  releasesVisible: Scalars['Boolean']['output'];
  topTracksVisible: Scalars['Boolean']['output'];
};

export type ArtistServerStatusResult = ArtistServerStatusImportingArtist | ArtistServerStatusImportingArtistReleases | ArtistServerStatusNotInLibrary | ArtistServerStatusReady | ArtistServerStatusUpdatingArtist | ArtistServerStatusUpdatingArtistReleases;

export type ArtistServerStatusResultBase = {
  releasesVisible: Scalars['Boolean']['output'];
  topTracksVisible: Scalars['Boolean']['output'];
};

export type ArtistServerStatusUpdatingArtist = ArtistServerStatusResultBase & {
  __typename?: 'ArtistServerStatusUpdatingArtist';
  artistMbId: Scalars['String']['output'];
  releasesVisible: Scalars['Boolean']['output'];
  topTracksVisible: Scalars['Boolean']['output'];
};

export type ArtistServerStatusUpdatingArtistReleases = ArtistServerStatusResultBase & {
  __typename?: 'ArtistServerStatusUpdatingArtistReleases';
  artistMbId: Scalars['String']['output'];
  releasesVisible: Scalars['Boolean']['output'];
  topTracksVisible: Scalars['Boolean']['output'];
};

export type ArtistTopTrack = {
  __typename?: 'ArtistTopTrack';
  releaseTitle?: Maybe<Scalars['String']['output']>;
  title: Scalars['String']['output'];
  track?: Maybe<Track>;
};

export type CacheStatistics = {
  __typename?: 'CacheStatistics';
  artistCount: Scalars['Int']['output'];
  isInitialized: Scalars['Boolean']['output'];
  lastUpdated: Scalars['DateTime']['output'];
  releaseCount: Scalars['Int']['output'];
  trackCount: Scalars['Int']['output'];
};

export type CreatePlaylistResult = CreatePlaylistSuccess;

export type CreatePlaylistSuccess = {
  __typename?: 'CreatePlaylistSuccess';
  viewer: User;
};

export type CreateUserError = {
  __typename?: 'CreateUserError';
  message: Scalars['String']['output'];
};

export type CreateUserInput = {
  password: Scalars['String']['input'];
  username: Scalars['String']['input'];
};

export type CreateUserResult = CreateUserError | CreateUserSuccess;

export type CreateUserSuccess = {
  __typename?: 'CreateUserSuccess';
  user: User;
};

export type DeletePlaylistInput = {
  playlistId: Scalars['String']['input'];
};

export type DeletePlaylistNoWriteAccess = {
  __typename?: 'DeletePlaylistNoWriteAccess';
  message: Scalars['String']['output'];
};

export type DeletePlaylistResult = DeletePlaylistNoWriteAccess | DeletePlaylistSuccess;

export type DeletePlaylistSuccess = {
  __typename?: 'DeletePlaylistSuccess';
  viewer: User;
};

export type DownloadStatus = {
  __typename?: 'DownloadStatus';
  artistName?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  numberOfTracks?: Maybe<Scalars['Int']['output']>;
  release?: Maybe<MbRelease>;
  releaseName?: Maybe<Scalars['String']['output']>;
  statusDescription: Scalars['String']['output'];
  tracksDownloaded?: Maybe<Scalars['Int']['output']>;
};

export type DownloadsSearchRoot = {
  __typename?: 'DownloadsSearchRoot';
  all: Array<DownloadStatus>;
};

export type ExternalRoot = {
  __typename?: 'ExternalRoot';
  id: Scalars['ID']['output'];
  soulSeek: SoulSeekRoot;
};

export type FileSystemEntry = {
  __typename?: 'FileSystemEntry';
  hasChildren: Scalars['Boolean']['output'];
  id: Scalars['ID']['output'];
  isAccessible: Scalars['Boolean']['output'];
  isDirectory: Scalars['Boolean']['output'];
  name: Scalars['String']['output'];
  path: Scalars['String']['output'];
};

export type FileSystemSearchRoot = {
  __typename?: 'FileSystemSearchRoot';
  browseFileSystem: Array<FileSystemEntry>;
};


export type FileSystemSearchRootBrowseFileSystemArgs = {
  path?: InputMaybe<Scalars['String']['input']>;
};

export type ImportArtistResult = {
  __typename?: 'ImportArtistResult';
  artistId?: Maybe<Scalars['String']['output']>;
  artistName: Scalars['String']['output'];
  errorMessage?: Maybe<Scalars['String']['output']>;
  musicBrainzId?: Maybe<Scalars['String']['output']>;
  photosDownloaded: PhotosDownloaded;
  spotifyId?: Maybe<Scalars['String']['output']>;
  success: Scalars['Boolean']['output'];
};

export type ImportPlaylistSearchRoot = {
  __typename?: 'ImportPlaylistSearchRoot';
  spotify: SpotifyPlaylistSearchRoot;
};

export type ImportReleasesResult = {
  __typename?: 'ImportReleasesResult';
  artistId: Scalars['String']['output'];
  errorMessage?: Maybe<Scalars['String']['output']>;
  failedReleases: Scalars['Int']['output'];
  importedReleases: Array<ImportedRelease>;
  success: Scalars['Boolean']['output'];
  successfulReleases: Scalars['Int']['output'];
  totalReleases: Scalars['Int']['output'];
};

export type ImportSpotifyPlaylistError = {
  __typename?: 'ImportSpotifyPlaylistError';
  message: Scalars['String']['output'];
};

export type ImportSpotifyPlaylistResult = ImportSpotifyPlaylistError | ImportSpotifyPlaylistSuccess;

export type ImportSpotifyPlaylistSuccess = {
  __typename?: 'ImportSpotifyPlaylistSuccess';
  success: Scalars['Boolean']['output'];
};

export type ImportedRelease = {
  __typename?: 'ImportedRelease';
  errorMessage?: Maybe<Scalars['String']['output']>;
  releaseGroupId: Scalars['String']['output'];
  success: Scalars['Boolean']['output'];
  title: Scalars['String']['output'];
};

export type LastFmAlbum = {
  __typename?: 'LastFmAlbum';
  album?: Maybe<MbReleaseGroup>;
  id: Scalars['ID']['output'];
  imageUrl?: Maybe<Scalars['String']['output']>;
  mbid: Scalars['String']['output'];
  name: Scalars['String']['output'];
  statistics: LastFmStatistics;
};

export type LastFmArtist = {
  __typename?: 'LastFmArtist';
  artist?: Maybe<Artist>;
  id: Scalars['ID']['output'];
  images?: Maybe<ArtistImages>;
  musicBrainzArtist?: Maybe<MbArtist>;
  name: Scalars['String']['output'];
  statistics: LastFmStatistics;
  summary?: Maybe<Scalars['String']['output']>;
  topTracks: Array<LastFmTrack>;
};

export type LastFmStatistics = {
  __typename?: 'LastFmStatistics';
  listeners: Scalars['Long']['output'];
  playCount: Scalars['Long']['output'];
};

export type LastFmTag = {
  __typename?: 'LastFmTag';
  name: Scalars['String']['output'];
  url: Scalars['String']['output'];
};

export type LastFmTrack = {
  __typename?: 'LastFmTrack';
  album?: Maybe<LastFmAlbum>;
  artist: LastFmArtist;
  id: Scalars['ID']['output'];
  images?: Maybe<ArtistImages>;
  mbid?: Maybe<Scalars['String']['output']>;
  musicBrainzRecording?: Maybe<MbRecording>;
  name: Scalars['String']['output'];
  playCount?: Maybe<Scalars['Long']['output']>;
  recording?: Maybe<Track>;
  statistics: LastFmStatistics;
  summary?: Maybe<Scalars['String']['output']>;
};

export type LikeSongAlreadyLiked = {
  __typename?: 'LikeSongAlreadyLiked';
  message: Scalars['String']['output'];
};

export type LikeSongInput = {
  recordingId: Scalars['String']['input'];
};

export type LikeSongResult = LikeSongAlreadyLiked | LikeSongSongDoesNotExist | LikeSongSuccess;

export type LikeSongSongDoesNotExist = {
  __typename?: 'LikeSongSongDoesNotExist';
  message: Scalars['String']['output'];
};

export type LikeSongSuccess = {
  __typename?: 'LikeSongSuccess';
  viewer: User;
};

export type LikedSong = {
  __typename?: 'LikedSong';
  id: Scalars['ID']['output'];
  recording?: Maybe<MbRecording>;
};

export type MbAlbumImages = {
  __typename?: 'MbAlbumImages';
  albumCover?: Maybe<Scalars['String']['output']>;
  cdArt?: Maybe<Scalars['String']['output']>;
};

export type MbArtist = {
  __typename?: 'MbArtist';
  albums: Array<MbReleaseGroup>;
  disambiguation?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  images?: Maybe<ArtistImages>;
  listeners?: Maybe<Scalars['Long']['output']>;
  name: Scalars['String']['output'];
  releaseGroups: Array<MbReleaseGroup>;
  releases: Array<MbRelease>;
  serverStatus: ArtistServerStatus;
  singles: Array<MbReleaseGroup>;
  sortName: Scalars['String']['output'];
  topTracks: Array<LastFmTrack>;
  type?: Maybe<Scalars['String']['output']>;
};

export type MbGenre = {
  __typename?: 'MbGenre';
  id: Scalars['ID']['output'];
  name: Scalars['String']['output'];
};

export type MbLabel = {
  __typename?: 'MbLabel';
  disambiguation?: Maybe<Scalars['String']['output']>;
  id: Scalars['String']['output'];
  name: Scalars['String']['output'];
};

export type MbLabelInfo = {
  __typename?: 'MbLabelInfo';
  catalogNumber?: Maybe<Scalars['String']['output']>;
  label: MbLabel;
};

export type MbMedium = {
  __typename?: 'MbMedium';
  trackCount: Scalars['Int']['output'];
  tracks: Array<MbTrack>;
};

export type MbNameCredit = {
  __typename?: 'MbNameCredit';
  artist: MbArtist;
  joinPhrase?: Maybe<Scalars['String']['output']>;
  name: Scalars['String']['output'];
};

export type MbRecording = {
  __typename?: 'MbRecording';
  artists: Array<MbArtist>;
  id: Scalars['ID']['output'];
  length?: Maybe<Scalars['Int']['output']>;
  mainAlbum?: Maybe<MbRelease>;
  nameCredits: Array<MbNameCredit>;
  relations: Array<MbRelation>;
  releases: Array<MbRelease>;
  statistics?: Maybe<LastFmStatistics>;
  streamingServiceInfo: MbRecordingStreamingServiceInfo;
  title: Scalars['String']['output'];
};

export type MbRecordingStreamingServiceInfo = {
  __typename?: 'MbRecordingStreamingServiceInfo';
  id: Scalars['ID']['output'];
  youtubeMusicUrl?: Maybe<Scalars['String']['output']>;
  youtubeSearchVideoId?: Maybe<Scalars['String']['output']>;
  youtubeVideoId?: Maybe<Scalars['String']['output']>;
};

export type MbRelation = {
  __typename?: 'MbRelation';
  attributes?: Maybe<Array<Scalars['String']['output']>>;
  begin?: Maybe<Scalars['String']['output']>;
  direction?: Maybe<Scalars['String']['output']>;
  end?: Maybe<Scalars['String']['output']>;
  targetType?: Maybe<Scalars['String']['output']>;
  type?: Maybe<Scalars['String']['output']>;
  typeId?: Maybe<Scalars['String']['output']>;
  url?: Maybe<Url>;
};

export type MbRelease = {
  __typename?: 'MbRelease';
  artists: Array<MbArtist>;
  barcode?: Maybe<Scalars['String']['output']>;
  country?: Maybe<Scalars['String']['output']>;
  coverArtUri: Scalars['String']['output'];
  credits: Array<MbNameCredit>;
  date?: Maybe<Scalars['String']['output']>;
  genres: Array<MbGenre>;
  id: Scalars['ID']['output'];
  labels: Array<MbLabelInfo>;
  media: Array<MbMedium>;
  quality?: Maybe<Scalars['String']['output']>;
  recordings: Array<MbRecording>;
  releaseGroup?: Maybe<MbReleaseGroup>;
  status?: Maybe<Scalars['String']['output']>;
  title: Scalars['String']['output'];
  year?: Maybe<Scalars['String']['output']>;
};

export type MbReleaseGroup = {
  __typename?: 'MbReleaseGroup';
  coverArtUri?: Maybe<Scalars['String']['output']>;
  credits: Array<MbNameCredit>;
  firstReleaseDate?: Maybe<Scalars['String']['output']>;
  firstReleaseYear?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  images?: Maybe<MbAlbumImages>;
  mainRelease?: Maybe<MbRelease>;
  primaryType?: Maybe<Scalars['String']['output']>;
  relations: Array<MbRelation>;
  secondaryTypes: Array<Scalars['String']['output']>;
  tags: Array<Scalars['String']['output']>;
  title: Scalars['String']['output'];
};

export type MbTrack = {
  __typename?: 'MbTrack';
  id: Scalars['ID']['output'];
  mbRecording: MbRecording;
  position: Scalars['Int']['output'];
};

export type MusicBrainzArtistSearchRoot = {
  __typename?: 'MusicBrainzArtistSearchRoot';
  byId?: Maybe<MbArtist>;
  searchByName: Array<MbArtist>;
};


export type MusicBrainzArtistSearchRootByIdArgs = {
  id: Scalars['ID']['input'];
};


export type MusicBrainzArtistSearchRootSearchByNameArgs = {
  limit?: Scalars['Int']['input'];
  name: Scalars['String']['input'];
  offset?: Scalars['Int']['input'];
};

export type MusicBrainzRecordingSearchRoot = {
  __typename?: 'MusicBrainzRecordingSearchRoot';
  byId?: Maybe<MbRecording>;
  searchByName: Array<MbRecording>;
};


export type MusicBrainzRecordingSearchRootByIdArgs = {
  id: Scalars['ID']['input'];
};


export type MusicBrainzRecordingSearchRootSearchByNameArgs = {
  limit?: Scalars['Int']['input'];
  name: Scalars['String']['input'];
  offset?: Scalars['Int']['input'];
};

export type MusicBrainzReleaseGroupSearchRoot = {
  __typename?: 'MusicBrainzReleaseGroupSearchRoot';
  byId?: Maybe<MbReleaseGroup>;
  searchByName: Array<MbReleaseGroup>;
};


export type MusicBrainzReleaseGroupSearchRootByIdArgs = {
  id: Scalars['ID']['input'];
};


export type MusicBrainzReleaseGroupSearchRootSearchByNameArgs = {
  limit?: Scalars['Int']['input'];
  name: Scalars['String']['input'];
  offset?: Scalars['Int']['input'];
};

export type MusicBrainzReleaseSearchRoot = {
  __typename?: 'MusicBrainzReleaseSearchRoot';
  byId?: Maybe<MbRelease>;
  searchByName: Array<MbRelease>;
};


export type MusicBrainzReleaseSearchRootByIdArgs = {
  id: Scalars['ID']['input'];
};


export type MusicBrainzReleaseSearchRootSearchByNameArgs = {
  limit?: Scalars['Int']['input'];
  name: Scalars['String']['input'];
  offset?: Scalars['Int']['input'];
};

export type MusicBrainzSearchRoot = {
  __typename?: 'MusicBrainzSearchRoot';
  artist: MusicBrainzArtistSearchRoot;
  recording: MusicBrainzRecordingSearchRoot;
  release: MusicBrainzReleaseSearchRoot;
  releaseGroup: MusicBrainzReleaseGroupSearchRoot;
};

export type Mutation = {
  __typename?: 'Mutation';
  addArtistToServerLibrary: AddArtistToServerLibraryResult;
  createDirectory: FileSystemEntry;
  createPlaylist: CreatePlaylistResult;
  createUser: CreateUserResult;
  deletePlaylist: DeletePlaylistResult;
  importArtist: ImportArtistResult;
  importArtistReleases: ImportReleasesResult;
  importSpotifyPlaylistById: ImportSpotifyPlaylistResult;
  likeSong: LikeSongResult;
  ping: Scalars['Boolean']['output'];
  renamePlaylist: RenamePlaylistResult;
  scanLibraryForMissingJson: ScanLibraryResult;
  signIn: SignInResult;
  signOut: SignOutResult;
  startDownloadRelease: StartDownloadReleaseResult;
  unlikeSong: UnlikedSongPayload;
  updateDownloadPath: UpdateDownloadPathResult;
  updateLibraryPath: UpdateLibraryPathResult;
};


export type MutationAddArtistToServerLibraryArgs = {
  input: AddArtistToServerLibraryInput;
};


export type MutationCreateDirectoryArgs = {
  path: Scalars['String']['input'];
};


export type MutationCreateUserArgs = {
  input: CreateUserInput;
};


export type MutationDeletePlaylistArgs = {
  input: DeletePlaylistInput;
};


export type MutationImportArtistArgs = {
  artistName: Scalars['String']['input'];
};


export type MutationImportArtistReleasesArgs = {
  artistId: Scalars['String']['input'];
};


export type MutationImportSpotifyPlaylistByIdArgs = {
  playlistId: Scalars['String']['input'];
  userId: Scalars['UUID']['input'];
};


export type MutationLikeSongArgs = {
  input: LikeSongInput;
};


export type MutationRenamePlaylistArgs = {
  input: RenamePlaylistInput;
};


export type MutationSignInArgs = {
  input: SignInInput;
};


export type MutationStartDownloadReleaseArgs = {
  input: StartDownloadReleaseInput;
};


export type MutationUnlikeSongArgs = {
  input: UnlikedSongInput;
};


export type MutationUpdateDownloadPathArgs = {
  input: UpdateDownloadPathInput;
};


export type MutationUpdateLibraryPathArgs = {
  input: UpdateLibraryPathInput;
};

/** Information about pagination in a connection. */
export type PageInfo = {
  __typename?: 'PageInfo';
  /** When paginating forwards, the cursor to continue. */
  endCursor?: Maybe<Scalars['String']['output']>;
  /** Indicates whether more edges exist following the set defined by the clients arguments. */
  hasNextPage: Scalars['Boolean']['output'];
  /** Indicates whether more edges exist prior the set defined by the clients arguments. */
  hasPreviousPage: Scalars['Boolean']['output'];
  /** When paginating backwards, the cursor to continue. */
  startCursor?: Maybe<Scalars['String']['output']>;
};

export type PhotosDownloaded = {
  __typename?: 'PhotosDownloaded';
  backgrounds: Scalars['Int']['output'];
  banners: Scalars['Int']['output'];
  logos: Scalars['Int']['output'];
  thumbs: Scalars['Int']['output'];
};

export type Ping = {
  __typename?: 'Ping';
  id: Scalars['ID']['output'];
};

export type Playlist = {
  __typename?: 'Playlist';
  coverImageUrl?: Maybe<Scalars['String']['output']>;
  createdAt: Scalars['DateTime']['output'];
  id: Scalars['ID']['output'];
  modifiedAt?: Maybe<Scalars['DateTime']['output']>;
  name?: Maybe<Scalars['String']['output']>;
  tracks: Array<Track>;
};

export type PlaylistSearchRoot = {
  __typename?: 'PlaylistSearchRoot';
  importPlaylists: ImportPlaylistSearchRoot;
};

export type Query = {
  __typename?: 'Query';
  areThereAnyUsers: Scalars['Boolean']['output'];
  download: DownloadsSearchRoot;
  external: ExternalRoot;
  fileSystem: FileSystemSearchRoot;
  musicBrainz: MusicBrainzSearchRoot;
  playlist: PlaylistSearchRoot;
  recommendations: RecommendationsSearchRoot;
  serverLibrary: ServerLibrarySearchRoot;
  serverSettings: ServerSettings;
  user: UserSearchRoot;
  viewer?: Maybe<User>;
};

export type RecommendationsSearchRoot = {
  __typename?: 'RecommendationsSearchRoot';
  topArtists: Array<LastFmArtist>;
  topTags: Array<LastFmTag>;
  topTracks: Array<LastFmTrack>;
};

export type Release = {
  __typename?: 'Release';
  artist: Artist;
  coverArtUrl: Scalars['String']['output'];
  firstReleaseDate?: Maybe<Scalars['String']['output']>;
  firstReleaseYear?: Maybe<Scalars['String']['output']>;
  folderName: Scalars['String']['output'];
  id: Scalars['ID']['output'];
  title: Scalars['String']['output'];
  tracks: Array<Track>;
  type?: Maybe<ReleaseType>;
};

export enum ReleaseType {
  Album = 'ALBUM',
  Ep = 'EP',
  Single = 'SINGLE'
}

export type RenamePlaylistInput = {
  newPlaylistName: Scalars['String']['input'];
  playlistId: Scalars['String']['input'];
};

export type RenamePlaylistNoWriteAccess = {
  __typename?: 'RenamePlaylistNoWriteAccess';
  message: Scalars['String']['output'];
};

export type RenamePlaylistResult = RenamePlaylistNoWriteAccess | RenamePlaylistSuccess;

export type RenamePlaylistSuccess = {
  __typename?: 'RenamePlaylistSuccess';
  viewer: User;
};

export type ScanLibraryResult = {
  __typename?: 'ScanLibraryResult';
  artistsCreated: Scalars['Int']['output'];
  errorMessage?: Maybe<Scalars['String']['output']>;
  notes: Array<Scalars['String']['output']>;
  releasesCreated: Scalars['Int']['output'];
  success: Scalars['Boolean']['output'];
};

export type ServerLibrarySearchRoot = {
  __typename?: 'ServerLibrarySearchRoot';
  allArtists: Array<Artist>;
  allReleases: Array<Release>;
  allTracks: Array<Track>;
  artistById?: Maybe<Artist>;
  libraryStatistics: CacheStatistics;
  releasesForArtist: Array<Release>;
  searchArtists: Array<Artist>;
  searchReleases: Array<Release>;
  searchTracks: Array<Track>;
  tracksForArtist: Array<Track>;
};


export type ServerLibrarySearchRootArtistByIdArgs = {
  id: Scalars['ID']['input'];
};


export type ServerLibrarySearchRootReleasesForArtistArgs = {
  artistId: Scalars['ID']['input'];
};


export type ServerLibrarySearchRootSearchArtistsArgs = {
  limit?: Scalars['Int']['input'];
  searchTerm: Scalars['String']['input'];
};


export type ServerLibrarySearchRootSearchReleasesArgs = {
  limit?: Scalars['Int']['input'];
  searchTerm: Scalars['String']['input'];
};


export type ServerLibrarySearchRootSearchTracksArgs = {
  limit?: Scalars['Int']['input'];
  searchTerm: Scalars['String']['input'];
};


export type ServerLibrarySearchRootTracksForArtistArgs = {
  artistId: Scalars['ID']['input'];
};

export type ServerSettings = {
  __typename?: 'ServerSettings';
  downloadPath: Scalars['String']['output'];
  id: Scalars['ID']['output'];
  libraryPath: Scalars['String']['output'];
};

export type SignInError = {
  __typename?: 'SignInError';
  message: Scalars['String']['output'];
};

export type SignInInput = {
  password: Scalars['String']['input'];
  username: Scalars['String']['input'];
};

export type SignInResult = SignInError | SignInSuccess;

export type SignInSuccess = {
  __typename?: 'SignInSuccess';
  user: User;
};

export type SignOutError = {
  __typename?: 'SignOutError';
  message: Scalars['String']['output'];
};

export type SignOutResult = SignOutError | SignOutSuccess;

export type SignOutSuccess = {
  __typename?: 'SignOutSuccess';
  success: Scalars['Boolean']['output'];
};

export type SoulSeekRoot = {
  __typename?: 'SoulSeekRoot';
  id: Scalars['ID']['output'];
  status: SoulSeekStatus;
};

export type SoulSeekStatus = {
  __typename?: 'SoulSeekStatus';
  id: Scalars['ID']['output'];
  status: SoulSeekStatusType;
};

export enum SoulSeekStatusType {
  Connecting = 'CONNECTING',
  Offline = 'OFFLINE',
  Online = 'ONLINE'
}

export type SpotifyPlaylist = {
  __typename?: 'SpotifyPlaylist';
  coverImageUrl?: Maybe<Scalars['String']['output']>;
  description?: Maybe<Scalars['String']['output']>;
  id: Scalars['String']['output'];
  name: Scalars['String']['output'];
};

export type SpotifyPlaylistSearchRoot = {
  __typename?: 'SpotifyPlaylistSearchRoot';
  spotifyPlaylistsForUser: Array<SpotifyPlaylist>;
};


export type SpotifyPlaylistSearchRootSpotifyPlaylistsForUserArgs = {
  username: Scalars['String']['input'];
};

export type StartDownloadReleaseInput = {
  releaseId: Scalars['String']['input'];
};

export type StartDownloadReleaseResult = StartDownloadReleaseSuccess;

export type StartDownloadReleaseSuccess = {
  __typename?: 'StartDownloadReleaseSuccess';
  success: Scalars['Boolean']['output'];
};

export type Subscription = {
  __typename?: 'Subscription';
  artistServerStatusUpdated: ArtistServerStatus;
  downloadStarted: DownloadStatus;
  downloadStatusUpdated: DownloadStatus;
  ping: Ping;
  soulSeekStatusUpdated: SoulSeekStatus;
};


export type SubscriptionArtistServerStatusUpdatedArgs = {
  artistId: Scalars['ID']['input'];
};

export type Track = {
  __typename?: 'Track';
  audioUrl: Scalars['String']['output'];
  id: Scalars['ID']['output'];
  release: Release;
  statistics?: Maybe<LastFmStatistics>;
  title: Scalars['String']['output'];
  trackLength?: Maybe<Scalars['Int']['output']>;
  trackNumber: Scalars['Int']['output'];
};

export type UnlikedSongInput = {
  recordingId: Scalars['String']['input'];
};

export type UnlikedSongPayload = {
  __typename?: 'UnlikedSongPayload';
  success: Scalars['Boolean']['output'];
};

export type UpdateDownloadPathInput = {
  newDownloadPath: Scalars['String']['input'];
};

export type UpdateDownloadPathResult = UpdateDownloadPathSuccess;

export type UpdateDownloadPathSuccess = {
  __typename?: 'UpdateDownloadPathSuccess';
  serverSettings: ServerSettings;
};

export type UpdateLibraryPathInput = {
  newLibraryPath: Scalars['String']['input'];
};

export type UpdateLibraryPathResult = UpdateLibraryPathSuccess;

export type UpdateLibraryPathSuccess = {
  __typename?: 'UpdateLibraryPathSuccess';
  serverSettings: ServerSettings;
};

export type Url = {
  __typename?: 'Url';
  id?: Maybe<Scalars['String']['output']>;
  resource?: Maybe<Scalars['String']['output']>;
};

export type User = {
  __typename?: 'User';
  createdAt: Scalars['DateTime']['output'];
  id: Scalars['ID']['output'];
  likedSongs: Array<LikedSong>;
  playlists: Array<Playlist>;
  updatedAt: Scalars['DateTime']['output'];
  username: Scalars['String']['output'];
};

export type UserSearchRoot = {
  __typename?: 'UserSearchRoot';
  users?: Maybe<UsersConnection>;
};


export type UserSearchRootUsersArgs = {
  after?: InputMaybe<Scalars['String']['input']>;
  before?: InputMaybe<Scalars['String']['input']>;
  first?: InputMaybe<Scalars['Int']['input']>;
  last?: InputMaybe<Scalars['Int']['input']>;
};

/** A connection to a list of items. */
export type UsersConnection = {
  __typename?: 'UsersConnection';
  /** A list of edges. */
  edges?: Maybe<Array<UsersEdge>>;
  /** A flattened list of the nodes. */
  nodes?: Maybe<Array<User>>;
  /** Information to aid in pagination. */
  pageInfo: PageInfo;
};

/** An edge in a connection. */
export type UsersEdge = {
  __typename?: 'UsersEdge';
  /** A cursor for use in pagination. */
  cursor: Scalars['String']['output'];
  /** The item at the end of the edge. */
  node: User;
};

export type BootstrapQueryVariables = Exact<{ [key: string]: never; }>;


export type BootstrapQuery = { __typename?: 'Query', areThereAnyUsers: boolean, viewer?: { __typename?: 'User', id: string } | null };

export type AlbumListQueryQueryVariables = Exact<{ [key: string]: never; }>;


export type AlbumListQueryQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', allReleases: Array<(
      { __typename?: 'Release', id: string }
      & { ' $fragmentRefs'?: { 'AlbumList_ReleaseFragment': AlbumList_ReleaseFragment } }
    )> } };

export type AlbumQueryQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
  releaseFolderName: Scalars['String']['input'];
}>;


export type AlbumQueryQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: { __typename?: 'Artist', id: string, name: string, releaseByFolderName?: (
        { __typename?: 'Release', id: string, title: string }
        & { ' $fragmentRefs'?: { 'AlbumPanel_ReleaseFragment': AlbumPanel_ReleaseFragment } }
      ) | null } | null } };

export type ArtistListQueryQueryVariables = Exact<{ [key: string]: never; }>;


export type ArtistListQueryQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', allArtists: Array<(
      { __typename?: 'Artist', id: string }
      & { ' $fragmentRefs'?: { 'ArtistList_ArtistFragment': ArtistList_ArtistFragment } }
    )> } };

export type ArtistQueryQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistQueryQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: (
      { __typename?: 'Artist', id: string, name: string }
      & { ' $fragmentRefs'?: { 'ArtistPanel_ArtistFragment': ArtistPanel_ArtistFragment } }
    ) | null } };

export type LikedSongsQueryQueryVariables = Exact<{ [key: string]: never; }>;


export type LikedSongsQueryQuery = { __typename?: 'Query', viewer?: (
    { __typename?: 'User', id: string }
    & { ' $fragmentRefs'?: { 'LikedSongsList_UserFragment': LikedSongsList_UserFragment } }
  ) | null };

export type SettingsPageQueryVariables = Exact<{ [key: string]: never; }>;


export type SettingsPageQuery = { __typename?: 'Query', serverSettings: (
    { __typename?: 'ServerSettings' }
    & { ' $fragmentRefs'?: { 'LibraryPathForm_ServerSettingsFragment': LibraryPathForm_ServerSettingsFragment;'DownloadPathForm_ServerSettingsFragment': DownloadPathForm_ServerSettingsFragment } }
  ) };

export type ProfilePageQueryVariables = Exact<{ [key: string]: never; }>;


export type ProfilePageQuery = { __typename?: 'Query', viewer?: (
    { __typename?: 'User', id: string, username: string }
    & { ' $fragmentRefs'?: { 'UserProfilePanel_UserFragment': UserProfilePanel_UserFragment } }
  ) | null };

export type Playlist_UserFragment = { __typename?: 'User', id: string, likedSongs: Array<(
    { __typename?: 'LikedSong', id: string }
    & { ' $fragmentRefs'?: { 'LikedSongRow_LikedSongFragment': LikedSongRow_LikedSongFragment } }
  )> } & { ' $fragmentName'?: 'Playlist_UserFragment' };

export type LikedSongRow_TrackFragment = { __typename?: 'Track', id: string, title: string, trackLength?: number | null } & { ' $fragmentName'?: 'LikedSongRow_TrackFragment' };

export type AddArtistToServerLibraryMutationVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type AddArtistToServerLibraryMutation = { __typename?: 'Mutation', addArtistToServerLibrary: { __typename: 'AddArtistToServerLibraryArtistAlreadyAdded' } | { __typename: 'AddArtistToServerLibraryArtistDoesNotExist' } | { __typename: 'AddArtistToServerLibrarySuccess' } | { __typename: 'AddArtistToServerLibraryUnknownError' } };

export type AlbumCard_ReleaseFragment = { __typename?: 'Release', id: string, title: string, firstReleaseYear?: string | null, coverArtUrl: string, folderName: string, artist: { __typename?: 'Artist', id: string } } & { ' $fragmentName'?: 'AlbumCard_ReleaseFragment' };

export type AlbumHeader_ReleaseFragment = { __typename?: 'Release', id: string, title: string, type?: ReleaseType | null, coverArtUrl: string, firstReleaseYear?: string | null, artist: { __typename?: 'Artist', id: string, name: string }, tracks: Array<{ __typename?: 'Track', id: string, trackLength?: number | null }> } & { ' $fragmentName'?: 'AlbumHeader_ReleaseFragment' };

export type AlbumPanel_ReleaseFragment = (
  { __typename?: 'Release', id: string, firstReleaseYear?: string | null }
  & { ' $fragmentRefs'?: { 'AlbumHeader_ReleaseFragment': AlbumHeader_ReleaseFragment;'AlbumTrackList_ReleaseFragment': AlbumTrackList_ReleaseFragment } }
) & { ' $fragmentName'?: 'AlbumPanel_ReleaseFragment' };

export type AlbumTrackList_ReleaseFragment = { __typename?: 'Release', id: string, title: string, folderName: string, artist: { __typename?: 'Artist', id: string }, tracks: Array<(
    { __typename?: 'Track', id: string, title: string, trackLength?: number | null, statistics?: { __typename?: 'LastFmStatistics', listeners: any, playCount: any } | null }
    & { ' $fragmentRefs'?: { 'RecordingPlayButton_TrackFragment': RecordingPlayButton_TrackFragment } }
  )> } & { ' $fragmentName'?: 'AlbumTrackList_ReleaseFragment' };

export type MbReleaseGroupCard_MbReleaseGroupFragment = { __typename?: 'MbReleaseGroup', id: string, title: string, firstReleaseYear?: string | null, coverArtUri?: string | null } & { ' $fragmentName'?: 'MbReleaseGroupCard_MbReleaseGroupFragment' };

export type AlbumList_ReleaseFragment = (
  { __typename?: 'Release', id: string }
  & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
) & { ' $fragmentName'?: 'AlbumList_ReleaseFragment' };

export type ArtistCard_ArtistFragment = { __typename?: 'Artist', id: string, name: string, listeners?: any | null, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null } & { ' $fragmentName'?: 'ArtistCard_ArtistFragment' };

export type MbArtistCard_MbArtistFragment = { __typename?: 'MbArtist', id: string, name: string, listeners?: any | null, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null } & { ' $fragmentName'?: 'MbArtistCard_MbArtistFragment' };

export type TopArtistCard_ArtistFragment = { __typename?: 'LastFmArtist', id: string, name: string, musicBrainzArtist?: { __typename?: 'MbArtist', id: string, name: string, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null } | null, statistics: { __typename?: 'LastFmStatistics', listeners: any } } & { ' $fragmentName'?: 'TopArtistCard_ArtistFragment' };

export type ArtistList_ArtistFragment = (
  { __typename?: 'Artist', id: string }
  & { ' $fragmentRefs'?: { 'ArtistCard_ArtistFragment': ArtistCard_ArtistFragment } }
) & { ' $fragmentName'?: 'ArtistList_ArtistFragment' };

export type ArtistNotInLibraryTopTracks_ArtistFragment = { __typename?: 'LastFmArtist', id: string, topTracks: Array<{ __typename?: 'LastFmTrack', id: string, name: string, statistics: { __typename?: 'LastFmStatistics', listeners: any } }> } & { ' $fragmentName'?: 'ArtistNotInLibraryTopTracks_ArtistFragment' };

export type RefreshArtistMutationVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type RefreshArtistMutation = { __typename?: 'Mutation', addArtistToServerLibrary: { __typename: 'AddArtistToServerLibraryArtistAlreadyAdded' } | { __typename: 'AddArtistToServerLibraryArtistDoesNotExist' } | { __typename: 'AddArtistToServerLibrarySuccess' } | { __typename: 'AddArtistToServerLibraryUnknownError' } };

export type ArtistAlbumListQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistAlbumListQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: { __typename?: 'Artist', id: string, albums: Array<(
        { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
        & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
      )> } | null } };

export type ArtistEpListQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistEpListQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: { __typename?: 'Artist', id: string, eps: Array<(
        { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
        & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
      )> } | null } };

export type ArtistPanel_ArtistFragment = { __typename?: 'Artist', id: string, name: string, listeners?: any | null, albums: Array<(
    { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
    & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
  )>, eps: Array<(
    { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
    & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
  )>, singles: Array<(
    { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
    & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
  )>, images?: { __typename?: 'ArtistImages', backgrounds: Array<string> } | null, serverStatus: { __typename?: 'ArtistServerStatus', id: string, result: { __typename: 'ArtistServerStatusImportingArtist', topTracksVisible: boolean, releasesVisible: boolean } | { __typename: 'ArtistServerStatusImportingArtistReleases', topTracksVisible: boolean, releasesVisible: boolean, numReleaseGroupsFinishedImporting: number, totalNumReleaseGroupsBeingImported: number } | { __typename: 'ArtistServerStatusNotInLibrary', topTracksVisible: boolean, releasesVisible: boolean } | { __typename: 'ArtistServerStatusReady', topTracksVisible: boolean, releasesVisible: boolean } | { __typename: 'ArtistServerStatusUpdatingArtist', topTracksVisible: boolean, releasesVisible: boolean } | { __typename: 'ArtistServerStatusUpdatingArtistReleases', topTracksVisible: boolean, releasesVisible: boolean } } } & { ' $fragmentName'?: 'ArtistPanel_ArtistFragment' };

export type ArtistSingleListQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistSingleListQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: { __typename?: 'Artist', id: string, singles: Array<(
        { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
        & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
      )> } | null } };

export type TopArtistTrackItem_ArtistTopTrackFragment = { __typename?: 'ArtistTopTrack', title: string, releaseTitle?: string | null, track?: { __typename?: 'Track', id: string, trackLength?: number | null, trackNumber: number, release: { __typename?: 'Release', id: string, folderName: string, artist: { __typename?: 'Artist', id: string } } } | null } & { ' $fragmentName'?: 'TopArtistTrackItem_ArtistTopTrackFragment' };

export type TopArtistTracksQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type TopArtistTracksQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: { __typename?: 'Artist', id: string, topTracks: Array<(
        { __typename?: 'ArtistTopTrack' }
        & { ' $fragmentRefs'?: { 'TopArtistTrackItem_ArtistTopTrackFragment': TopArtistTrackItem_ArtistTopTrackFragment } }
      )> } | null } };

export type ArtistServerStatusQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistServerStatusQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: { __typename?: 'Artist', id: string, serverStatus: { __typename?: 'ArtistServerStatus', id: string, result: { __typename: 'ArtistServerStatusImportingArtist', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusImportingArtistReleases', releasesVisible: boolean, topTracksVisible: boolean, totalNumReleaseGroupsBeingImported: number, numReleaseGroupsFinishedImporting: number } | { __typename: 'ArtistServerStatusNotInLibrary', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusReady', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusUpdatingArtist', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusUpdatingArtistReleases', releasesVisible: boolean, topTracksVisible: boolean } } } | null } };

export type ArtistServerStatusSubSubscriptionVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistServerStatusSubSubscription = { __typename?: 'Subscription', artistServerStatusUpdated: { __typename?: 'ArtistServerStatus', id: string, result: { __typename: 'ArtistServerStatusImportingArtist', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusImportingArtistReleases', releasesVisible: boolean, topTracksVisible: boolean, totalNumReleaseGroupsBeingImported: number, numReleaseGroupsFinishedImporting: number, artist: { __typename?: 'Artist', id: string, albums: Array<(
          { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
          & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
        )>, eps: Array<(
          { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
          & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
        )>, singles: Array<(
          { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
          & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
        )> } } | { __typename: 'ArtistServerStatusNotInLibrary', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusReady', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusUpdatingArtist', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusUpdatingArtistReleases', releasesVisible: boolean, topTracksVisible: boolean } } };

export type SignInMutationVariables = Exact<{
  username: Scalars['String']['input'];
  password: Scalars['String']['input'];
}>;


export type SignInMutation = { __typename?: 'Mutation', signIn: { __typename: 'SignInError', message: string } | { __typename: 'SignInSuccess', user: { __typename?: 'User', id: string, username: string } } };

export type SignOutMutationVariables = Exact<{ [key: string]: never; }>;


export type SignOutMutation = { __typename?: 'Mutation', signOut: { __typename: 'SignOutError', message: string } | { __typename: 'SignOutSuccess', success: boolean } };

export type DownloadOverview_DownloadStatusFragment = (
  { __typename?: 'DownloadStatus', id: string }
  & { ' $fragmentRefs'?: { 'DownloadStatus_DownloadStatusFragment': DownloadStatus_DownloadStatusFragment } }
) & { ' $fragmentName'?: 'DownloadOverview_DownloadStatusFragment' };

export type DownloadOverviewQueryQueryVariables = Exact<{ [key: string]: never; }>;


export type DownloadOverviewQueryQuery = { __typename?: 'Query', download: { __typename?: 'DownloadsSearchRoot', all: Array<(
      { __typename?: 'DownloadStatus', id: string }
      & { ' $fragmentRefs'?: { 'DownloadOverview_DownloadStatusFragment': DownloadOverview_DownloadStatusFragment } }
    )> } };

export type DownloadOverviewSubscriptionSubscriptionVariables = Exact<{ [key: string]: never; }>;


export type DownloadOverviewSubscriptionSubscription = { __typename?: 'Subscription', downloadStatusUpdated: (
    { __typename?: 'DownloadStatus', id: string }
    & { ' $fragmentRefs'?: { 'DownloadOverview_DownloadStatusFragment': DownloadOverview_DownloadStatusFragment } }
  ) };

export type DownloadStatus_DownloadStatusFragment = { __typename?: 'DownloadStatus', id: string, numberOfTracks?: number | null, tracksDownloaded?: number | null, statusDescription: string, release?: { __typename?: 'MbRelease', id: string, title: string, year?: string | null, artists: Array<{ __typename?: 'MbArtist', id: string, name: string }>, recordings: Array<{ __typename?: 'MbRecording', id: string }> } | null } & { ' $fragmentName'?: 'DownloadStatus_DownloadStatusFragment' };

export type CreateUserMutationVariables = Exact<{
  username: Scalars['String']['input'];
  password: Scalars['String']['input'];
}>;


export type CreateUserMutation = { __typename?: 'Mutation', createUser: { __typename: 'CreateUserError', message: string } | { __typename: 'CreateUserSuccess', user: { __typename?: 'User', id: string, username: string } } };

export type LikedSongRow_LikedSongFragment = { __typename?: 'LikedSong', id: string, recording?: { __typename?: 'MbRecording', id: string, title: string, length?: number | null, artists: Array<{ __typename?: 'MbArtist', id: string, name: string }>, mainAlbum?: { __typename?: 'MbRelease', id: string, title: string, coverArtUri: string, artists: Array<{ __typename?: 'MbArtist', id: string }> } | null } | null } & { ' $fragmentName'?: 'LikedSongRow_LikedSongFragment' };

export type LikedSongsList_UserFragment = { __typename?: 'User', id: string, likedSongs: Array<(
    { __typename?: 'LikedSong', id: string }
    & { ' $fragmentRefs'?: { 'LikedSongRow_LikedSongFragment': LikedSongRow_LikedSongFragment } }
  )> } & { ' $fragmentName'?: 'LikedSongsList_UserFragment' };

export type RecordingPlayButton_TrackFragment = { __typename?: 'Track', id: string, title: string, release: { __typename?: 'Release', folderName: string, artist: { __typename?: 'Artist', id: string } } } & { ' $fragmentName'?: 'RecordingPlayButton_TrackFragment' };

export type YoutubeVideoSearchQueryVariables = Exact<{
  recordingId: Scalars['ID']['input'];
}>;


export type YoutubeVideoSearchQuery = { __typename?: 'Query', musicBrainz: { __typename?: 'MusicBrainzSearchRoot', recording: { __typename?: 'MusicBrainzRecordingSearchRoot', byId?: { __typename?: 'MbRecording', id: string, streamingServiceInfo: { __typename?: 'MbRecordingStreamingServiceInfo', id: string, youtubeSearchVideoId?: string | null } } | null } } };

export type CreatePlaylistMutationVariables = Exact<{ [key: string]: never; }>;


export type CreatePlaylistMutation = { __typename?: 'Mutation', createPlaylist: { __typename: 'CreatePlaylistSuccess', viewer: { __typename?: 'User', id: string, playlists: Array<{ __typename?: 'Playlist', id: string, name?: string | null, createdAt: any }> } } };

export type PlaylistListQueryVariables = Exact<{ [key: string]: never; }>;


export type PlaylistListQuery = { __typename?: 'Query', viewer?: { __typename?: 'User', id: string, playlists: Array<{ __typename?: 'Playlist', id: string, name?: string | null, createdAt: any }> } | null };

export type RenamePlaylistMutationVariables = Exact<{
  playlistId: Scalars['String']['input'];
  newPlaylistName: Scalars['String']['input'];
}>;


export type RenamePlaylistMutation = { __typename?: 'Mutation', renamePlaylist: { __typename: 'RenamePlaylistNoWriteAccess' } | { __typename: 'RenamePlaylistSuccess', viewer: { __typename?: 'User', id: string, playlists: Array<{ __typename?: 'Playlist', id: string, name?: string | null, createdAt: any }> } } };

export type DeletePlaylistMutationVariables = Exact<{
  playlistId: Scalars['String']['input'];
}>;


export type DeletePlaylistMutation = { __typename?: 'Mutation', deletePlaylist: { __typename: 'DeletePlaylistNoWriteAccess' } | { __typename: 'DeletePlaylistSuccess', viewer: { __typename?: 'User', id: string, playlists: Array<{ __typename?: 'Playlist', id: string, name?: string | null, createdAt: any }> } } };

export type TopArtistRecommendationsQueryVariables = Exact<{ [key: string]: never; }>;


export type TopArtistRecommendationsQuery = { __typename?: 'Query', recommendations: { __typename?: 'RecommendationsSearchRoot', topArtists: Array<(
      { __typename?: 'LastFmArtist', id: string }
      & { ' $fragmentRefs'?: { 'TopArtistCard_ArtistFragment': TopArtistCard_ArtistFragment } }
    )> } };

export type TopTrackCard_LastFmTrackFragment = { __typename?: 'LastFmTrack', id: string, playCount?: any | null, name: string, artist: { __typename?: 'LastFmArtist', id: string, musicBrainzArtist?: { __typename?: 'MbArtist', id: string, name: string, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null } | null }, album?: { __typename?: 'LastFmAlbum', id: string, imageUrl?: string | null } | null, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null } & { ' $fragmentName'?: 'TopTrackCard_LastFmTrackFragment' };

export type TopTrackRecommendationsQueryVariables = Exact<{ [key: string]: never; }>;


export type TopTrackRecommendationsQuery = { __typename?: 'Query', recommendations: { __typename?: 'RecommendationsSearchRoot', topTracks: Array<(
      { __typename?: 'LastFmTrack', id: string }
      & { ' $fragmentRefs'?: { 'TopTrackCard_LastFmTrackFragment': TopTrackCard_LastFmTrackFragment } }
    )> } };

export type AlbumSearchResultsSearchQueryVariables = Exact<{
  text: Scalars['String']['input'];
  limit: Scalars['Int']['input'];
}>;


export type AlbumSearchResultsSearchQuery = { __typename?: 'Query', musicBrainz: { __typename?: 'MusicBrainzSearchRoot', releaseGroup: { __typename?: 'MusicBrainzReleaseGroupSearchRoot', searchByName: Array<(
        { __typename?: 'MbReleaseGroup', id: string, title: string }
        & { ' $fragmentRefs'?: { 'MbReleaseGroupCard_MbReleaseGroupFragment': MbReleaseGroupCard_MbReleaseGroupFragment } }
      )> } } };

export type ArtistSearchResultsSearchQueryVariables = Exact<{
  searchText: Scalars['String']['input'];
  limit: Scalars['Int']['input'];
}>;


export type ArtistSearchResultsSearchQuery = { __typename?: 'Query', musicBrainz: { __typename?: 'MusicBrainzSearchRoot', artist: { __typename?: 'MusicBrainzArtistSearchRoot', searchByName: Array<(
        { __typename?: 'MbArtist', id: string, name: string, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null }
        & { ' $fragmentRefs'?: { 'MbArtistCard_MbArtistFragment': MbArtistCard_MbArtistFragment } }
      )> } } };

export type RecordingSearchResultsSearchQueryVariables = Exact<{
  text: Scalars['String']['input'];
  limit: Scalars['Int']['input'];
}>;


export type RecordingSearchResultsSearchQuery = { __typename?: 'Query', musicBrainz: { __typename?: 'MusicBrainzSearchRoot', recording: { __typename?: 'MusicBrainzRecordingSearchRoot', searchByName: Array<{ __typename?: 'MbRecording', id: string, title: string, length?: number | null, nameCredits: Array<{ __typename?: 'MbNameCredit', artist: { __typename?: 'MbArtist', id: string, name: string } }>, mainAlbum?: { __typename?: 'MbRelease', id: string, title: string, coverArtUri: string } | null }> } } };

export type SearchResultArtistSearchQueryVariables = Exact<{
  text: Scalars['String']['input'];
}>;


export type SearchResultArtistSearchQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', searchArtists: Array<{ __typename?: 'Artist', id: string, name: string, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null }> } };

export type SearchResultReleaseSearchQueryVariables = Exact<{
  text: Scalars['String']['input'];
}>;


export type SearchResultReleaseSearchQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', searchReleases: Array<{ __typename?: 'Release', id: string, title: string, coverArtUrl: string }> } };

export type SearchResultTrackSearchQueryVariables = Exact<{
  text: Scalars['String']['input'];
}>;


export type SearchResultTrackSearchQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', searchTracks: Array<{ __typename?: 'Track', id: string, title: string, trackLength?: number | null, release: { __typename?: 'Release', id: string, title: string, coverArtUrl: string, folderName: string, artist: { __typename?: 'Artist', id: string, name: string } } }> } };

export type DownloadPathForm_ServerSettingsFragment = { __typename?: 'ServerSettings', id: string, downloadPath: string } & { ' $fragmentName'?: 'DownloadPathForm_ServerSettingsFragment' };

export type UpdateDownloadPathMutationVariables = Exact<{
  newDownloadPath: Scalars['String']['input'];
}>;


export type UpdateDownloadPathMutation = { __typename?: 'Mutation', updateDownloadPath: { __typename?: 'UpdateDownloadPathSuccess', serverSettings: { __typename?: 'ServerSettings', id: string, downloadPath: string } } };

export type LibraryPathForm_ServerSettingsFragment = { __typename?: 'ServerSettings', id: string, libraryPath: string } & { ' $fragmentName'?: 'LibraryPathForm_ServerSettingsFragment' };

export type UpdateLibraryPathMutationVariables = Exact<{
  newLibraryPath: Scalars['String']['input'];
}>;


export type UpdateLibraryPathMutation = { __typename?: 'Mutation', updateLibraryPath: { __typename?: 'UpdateLibraryPathSuccess', serverSettings: { __typename?: 'ServerSettings', id: string, libraryPath: string } } };

export type DirectoryTreeItem_FileSystemEntryFragment = { __typename?: 'FileSystemEntry', id: string, name: string, path: string, isDirectory: boolean, hasChildren: boolean, isAccessible: boolean } & { ' $fragmentName'?: 'DirectoryTreeItem_FileSystemEntryFragment' };

export type DirectoryTreeItemQueryVariables = Exact<{
  path?: InputMaybe<Scalars['String']['input']>;
}>;


export type DirectoryTreeItemQuery = { __typename?: 'Query', fileSystem: { __typename?: 'FileSystemSearchRoot', browseFileSystem: Array<(
      { __typename?: 'FileSystemEntry', id: string, isDirectory: boolean, path: string }
      & { ' $fragmentRefs'?: { 'DirectoryTreeItem_FileSystemEntryFragment': DirectoryTreeItem_FileSystemEntryFragment } }
    )> } };

export type BrowseFileSystemQueryVariables = Exact<{
  path?: InputMaybe<Scalars['String']['input']>;
}>;


export type BrowseFileSystemQuery = { __typename?: 'Query', fileSystem: { __typename?: 'FileSystemSearchRoot', browseFileSystem: Array<(
      { __typename?: 'FileSystemEntry', path: string, isDirectory: boolean }
      & { ' $fragmentRefs'?: { 'DirectoryTreeItem_FileSystemEntryFragment': DirectoryTreeItem_FileSystemEntryFragment } }
    )> } };

export type SidebarQueryVariables = Exact<{ [key: string]: never; }>;


export type SidebarQuery = { __typename?: 'Query', viewer?: { __typename?: 'User', id: string, username: string } | null };

export type SoulSeekNetworkStatus_SoulSeekStatusFragment = { __typename?: 'SoulSeekStatus', id: string, status: SoulSeekStatusType } & { ' $fragmentName'?: 'SoulSeekNetworkStatus_SoulSeekStatusFragment' };

export type SoulSeekNetworkStatusSubscriptionSubscriptionVariables = Exact<{ [key: string]: never; }>;


export type SoulSeekNetworkStatusSubscriptionSubscription = { __typename?: 'Subscription', soulSeekStatusUpdated: (
    { __typename?: 'SoulSeekStatus' }
    & { ' $fragmentRefs'?: { 'SoulSeekNetworkStatus_SoulSeekStatusFragment': SoulSeekNetworkStatus_SoulSeekStatusFragment } }
  ) };

export type SoulSeekNetworkStatusQueryQueryVariables = Exact<{ [key: string]: never; }>;


export type SoulSeekNetworkStatusQueryQuery = { __typename?: 'Query', external: { __typename?: 'ExternalRoot', id: string, soulSeek: { __typename?: 'SoulSeekRoot', id: string, status: (
        { __typename?: 'SoulSeekStatus' }
        & { ' $fragmentRefs'?: { 'SoulSeekNetworkStatus_SoulSeekStatusFragment': SoulSeekNetworkStatus_SoulSeekStatusFragment } }
      ) } } };

export type ImportSpotifyPlaylistByIdMutationVariables = Exact<{
  playlistId: Scalars['String']['input'];
  userId: Scalars['UUID']['input'];
}>;


export type ImportSpotifyPlaylistByIdMutation = { __typename?: 'Mutation', importSpotifyPlaylistById: { __typename: 'ImportSpotifyPlaylistError', message: string } | { __typename: 'ImportSpotifyPlaylistSuccess', success: boolean } };

export type SpotifyPlaylistsList_SpotifyPlaylistFragment = { __typename?: 'SpotifyPlaylist', id: string, description?: string | null, name: string, coverImageUrl?: string | null } & { ' $fragmentName'?: 'SpotifyPlaylistsList_SpotifyPlaylistFragment' };

export type UserPlaylistsLoader_QueryQueryVariables = Exact<{
  spotifyUsername: Scalars['String']['input'];
}>;


export type UserPlaylistsLoader_QueryQuery = { __typename?: 'Query', playlist: { __typename?: 'PlaylistSearchRoot', importPlaylists: { __typename?: 'ImportPlaylistSearchRoot', spotify: { __typename?: 'SpotifyPlaylistSearchRoot', spotifyPlaylistsForUser: Array<(
          { __typename?: 'SpotifyPlaylist', id: string }
          & { ' $fragmentRefs'?: { 'SpotifyPlaylistsList_SpotifyPlaylistFragment': SpotifyPlaylistsList_SpotifyPlaylistFragment } }
        )> } } } };

export type UserProfilePanel_UserFragment = { __typename?: 'User', id: string, username: string, createdAt: any, updatedAt: any, likedSongs: Array<{ __typename?: 'LikedSong', id: string }> } & { ' $fragmentName'?: 'UserProfilePanel_UserFragment' };

export type UserProfileWidgetQueryVariables = Exact<{ [key: string]: never; }>;


export type UserProfileWidgetQuery = { __typename?: 'Query', viewer?: { __typename?: 'User', id: string, username: string } | null };

export const LikedSongRow_LikedSongFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]} as unknown as DocumentNode<LikedSongRow_LikedSongFragment, unknown>;
export const Playlist_UserFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"Playlist_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongRow_LikedSong"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]} as unknown as DocumentNode<Playlist_UserFragment, unknown>;
export const LikedSongRow_TrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}}]}}]} as unknown as DocumentNode<LikedSongRow_TrackFragment, unknown>;
export const AlbumHeader_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}}]}}]}}]} as unknown as DocumentNode<AlbumHeader_ReleaseFragment, unknown>;
export const RecordingPlayButton_TrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RecordingPlayButton_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}}]}},{"kind":"Field","name":{"kind":"Name","value":"title"}}]}}]} as unknown as DocumentNode<RecordingPlayButton_TrackFragment, unknown>;
export const AlbumTrackList_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"RecordingPlayButton_Track"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RecordingPlayButton_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}}]}},{"kind":"Field","name":{"kind":"Name","value":"title"}}]}}]} as unknown as DocumentNode<AlbumTrackList_ReleaseFragment, unknown>;
export const AlbumPanel_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_Release"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RecordingPlayButton_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}}]}},{"kind":"Field","name":{"kind":"Name","value":"title"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"RecordingPlayButton_Track"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}}]}}]}}]}}]} as unknown as DocumentNode<AlbumPanel_ReleaseFragment, unknown>;
export const MbReleaseGroupCard_MbReleaseGroupFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MbReleaseGroupCard_MbReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]} as unknown as DocumentNode<MbReleaseGroupCard_MbReleaseGroupFragment, unknown>;
export const AlbumCard_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<AlbumCard_ReleaseFragment, unknown>;
export const AlbumList_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<AlbumList_ReleaseFragment, unknown>;
export const MbArtistCard_MbArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MbArtistCard_MbArtist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]} as unknown as DocumentNode<MbArtistCard_MbArtistFragment, unknown>;
export const TopArtistCard_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]} as unknown as DocumentNode<TopArtistCard_ArtistFragment, unknown>;
export const ArtistCard_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]} as unknown as DocumentNode<ArtistCard_ArtistFragment, unknown>;
export const ArtistList_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistList_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistCard_Artist"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]} as unknown as DocumentNode<ArtistList_ArtistFragment, unknown>;
export const ArtistNotInLibraryTopTracks_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNotInLibraryTopTracks_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistNotInLibraryTopTracks_ArtistFragment, unknown>;
export const ArtistPanel_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistPanel_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"backgrounds"}}]}},{"kind":"Field","name":{"kind":"Name","value":"serverStatus"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"result"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusResultBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"topTracksVisible"}},{"kind":"Field","name":{"kind":"Name","value":"releasesVisible"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusImportingArtistReleases"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"numReleaseGroupsFinishedImporting"}},{"kind":"Field","name":{"kind":"Name","value":"totalNumReleaseGroupsBeingImported"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<ArtistPanel_ArtistFragment, unknown>;
export const TopArtistTrackItem_ArtistTopTrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistTrackItem_ArtistTopTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistTopTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"releaseTitle"}},{"kind":"Field","name":{"kind":"Name","value":"track"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]} as unknown as DocumentNode<TopArtistTrackItem_ArtistTopTrackFragment, unknown>;
export const DownloadStatus_DownloadStatusFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"numberOfTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracksDownloaded"}},{"kind":"Field","name":{"kind":"Name","value":"statusDescription"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]} as unknown as DocumentNode<DownloadStatus_DownloadStatusFragment, unknown>;
export const DownloadOverview_DownloadStatusFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadOverview_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"numberOfTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracksDownloaded"}},{"kind":"Field","name":{"kind":"Name","value":"statusDescription"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]} as unknown as DocumentNode<DownloadOverview_DownloadStatusFragment, unknown>;
export const LikedSongsList_UserFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongsList_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongRow_LikedSong"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]} as unknown as DocumentNode<LikedSongsList_UserFragment, unknown>;
export const TopTrackCard_LastFmTrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopTrackCard_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"album"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"imageUrl"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]} as unknown as DocumentNode<TopTrackCard_LastFmTrackFragment, unknown>;
export const DownloadPathForm_ServerSettingsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadPathForm_ServerSettings"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServerSettings"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"downloadPath"}}]}}]} as unknown as DocumentNode<DownloadPathForm_ServerSettingsFragment, unknown>;
export const LibraryPathForm_ServerSettingsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LibraryPathForm_ServerSettings"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServerSettings"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"libraryPath"}}]}}]} as unknown as DocumentNode<LibraryPathForm_ServerSettingsFragment, unknown>;
export const DirectoryTreeItem_FileSystemEntryFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"FileSystemEntry"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"Field","name":{"kind":"Name","value":"hasChildren"}},{"kind":"Field","name":{"kind":"Name","value":"isAccessible"}}]}}]} as unknown as DocumentNode<DirectoryTreeItem_FileSystemEntryFragment, unknown>;
export const SoulSeekNetworkStatus_SoulSeekStatusFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatus_SoulSeekStatusFragment, unknown>;
export const SpotifyPlaylistsList_SpotifyPlaylistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SpotifyPlaylistsList_SpotifyPlaylist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SpotifyPlaylist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}}]}}]} as unknown as DocumentNode<SpotifyPlaylistsList_SpotifyPlaylistFragment, unknown>;
export const UserProfilePanel_UserFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserProfilePanel_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"updatedAt"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<UserProfilePanel_UserFragment, unknown>;
export const BootstrapDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"Bootstrap"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"areThereAnyUsers"}},{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<BootstrapQuery, BootstrapQueryVariables>;
export const AlbumListQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"AlbumListQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"allReleases"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumList_Release"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]} as unknown as DocumentNode<AlbumListQueryQuery, AlbumListQueryQueryVariables>;
export const AlbumQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"AlbumQuery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"releaseByFolderName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"releaseFolderName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumPanel_Release"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RecordingPlayButton_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}}]}},{"kind":"Field","name":{"kind":"Name","value":"title"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"RecordingPlayButton_Track"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_Release"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}}]}}]} as unknown as DocumentNode<AlbumQueryQuery, AlbumQueryQueryVariables>;
export const ArtistListQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistListQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"allArtists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistList_Artist"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistList_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistCard_Artist"}}]}}]} as unknown as DocumentNode<ArtistListQueryQuery, ArtistListQueryQueryVariables>;
export const ArtistQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistQuery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistPanel_Artist"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistPanel_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"backgrounds"}}]}},{"kind":"Field","name":{"kind":"Name","value":"serverStatus"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"result"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusResultBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"topTracksVisible"}},{"kind":"Field","name":{"kind":"Name","value":"releasesVisible"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusImportingArtistReleases"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"numReleaseGroupsFinishedImporting"}},{"kind":"Field","name":{"kind":"Name","value":"totalNumReleaseGroupsBeingImported"}}]}}]}}]}}]}}]} as unknown as DocumentNode<ArtistQueryQuery, ArtistQueryQueryVariables>;
export const LikedSongsQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"LikedSongsQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongsList_User"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongsList_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongRow_LikedSong"}}]}}]}}]} as unknown as DocumentNode<LikedSongsQueryQuery, LikedSongsQueryQueryVariables>;
export const SettingsPageDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SettingsPage"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverSettings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"LibraryPathForm_ServerSettings"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadPathForm_ServerSettings"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LibraryPathForm_ServerSettings"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServerSettings"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"libraryPath"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadPathForm_ServerSettings"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServerSettings"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"downloadPath"}}]}}]} as unknown as DocumentNode<SettingsPageQuery, SettingsPageQueryVariables>;
export const ProfilePageDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ProfilePage"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"UserProfilePanel_User"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserProfilePanel_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"updatedAt"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<ProfilePageQuery, ProfilePageQueryVariables>;
export const AddArtistToServerLibraryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"AddArtistToServerLibrary"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"addArtistToServerLibrary"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}}]}}]}}]} as unknown as DocumentNode<AddArtistToServerLibraryMutation, AddArtistToServerLibraryMutationVariables>;
export const RefreshArtistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"RefreshArtist"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"addArtistToServerLibrary"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}}]}}]}}]} as unknown as DocumentNode<RefreshArtistMutation, RefreshArtistMutationVariables>;
export const ArtistAlbumListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistAlbumList"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<ArtistAlbumListQuery, ArtistAlbumListQueryVariables>;
export const ArtistEpListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistEpList"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<ArtistEpListQuery, ArtistEpListQueryVariables>;
export const ArtistSingleListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistSingleList"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<ArtistSingleListQuery, ArtistSingleListQueryVariables>;
export const TopArtistTracksDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"TopArtistTracks"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"TopArtistTrackItem_ArtistTopTrack"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistTrackItem_ArtistTopTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistTopTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"releaseTitle"}},{"kind":"Field","name":{"kind":"Name","value":"track"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]} as unknown as DocumentNode<TopArtistTracksQuery, TopArtistTracksQueryVariables>;
export const ArtistServerStatusDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistServerStatus"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"serverStatus"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"result"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusResultBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releasesVisible"}},{"kind":"Field","name":{"kind":"Name","value":"topTracksVisible"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusImportingArtistReleases"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"totalNumReleaseGroupsBeingImported"}},{"kind":"Field","name":{"kind":"Name","value":"numReleaseGroupsFinishedImporting"}}]}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<ArtistServerStatusQuery, ArtistServerStatusQueryVariables>;
export const ArtistServerStatusSubDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"ArtistServerStatusSub"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistServerStatusUpdated"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"result"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusResultBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releasesVisible"}},{"kind":"Field","name":{"kind":"Name","value":"topTracksVisible"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusImportingArtistReleases"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"totalNumReleaseGroupsBeingImported"}},{"kind":"Field","name":{"kind":"Name","value":"numReleaseGroupsFinishedImporting"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<ArtistServerStatusSubSubscription, ArtistServerStatusSubSubscriptionVariables>;
export const SignInDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SignIn"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"username"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"password"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"signIn"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"username"},"value":{"kind":"Variable","name":{"kind":"Name","value":"username"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"password"},"value":{"kind":"Variable","name":{"kind":"Name","value":"password"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SignInSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"user"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SignInError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<SignInMutation, SignInMutationVariables>;
export const SignOutDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SignOut"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"signOut"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SignOutSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"success"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SignOutError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<SignOutMutation, SignOutMutationVariables>;
export const DownloadOverviewQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"DownloadOverviewQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"download"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"all"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadOverview_DownloadStatus"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"numberOfTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracksDownloaded"}},{"kind":"Field","name":{"kind":"Name","value":"statusDescription"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadOverview_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"}}]}}]} as unknown as DocumentNode<DownloadOverviewQueryQuery, DownloadOverviewQueryQueryVariables>;
export const DownloadOverviewSubscriptionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"DownloadOverviewSubscription"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"downloadStatusUpdated"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadOverview_DownloadStatus"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"numberOfTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracksDownloaded"}},{"kind":"Field","name":{"kind":"Name","value":"statusDescription"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadOverview_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"}}]}}]} as unknown as DocumentNode<DownloadOverviewSubscriptionSubscription, DownloadOverviewSubscriptionSubscriptionVariables>;
export const CreateUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"username"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"password"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"username"},"value":{"kind":"Variable","name":{"kind":"Name","value":"username"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"password"},"value":{"kind":"Variable","name":{"kind":"Name","value":"password"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"CreateUserSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"user"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"CreateUserError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<CreateUserMutation, CreateUserMutationVariables>;
export const YoutubeVideoSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"YoutubeVideoSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"recordingId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"recordingId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"streamingServiceInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"youtubeSearchVideoId"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<YoutubeVideoSearchQuery, YoutubeVideoSearchQueryVariables>;
export const CreatePlaylistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreatePlaylist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createPlaylist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"CreatePlaylistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playlists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<CreatePlaylistMutation, CreatePlaylistMutationVariables>;
export const PlaylistListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"PlaylistList"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playlists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]}}]} as unknown as DocumentNode<PlaylistListQuery, PlaylistListQueryVariables>;
export const RenamePlaylistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"RenamePlaylist"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"newPlaylistName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"renamePlaylist"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"newPlaylistName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"newPlaylistName"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"RenamePlaylistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playlists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<RenamePlaylistMutation, RenamePlaylistMutationVariables>;
export const DeletePlaylistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeletePlaylist"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deletePlaylist"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DeletePlaylistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playlists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<DeletePlaylistMutation, DeletePlaylistMutationVariables>;
export const TopArtistRecommendationsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"TopArtistRecommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"topArtists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TopArtistCard_Artist"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]} as unknown as DocumentNode<TopArtistRecommendationsQuery, TopArtistRecommendationsQueryVariables>;
export const TopTrackRecommendationsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"TopTrackRecommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TopTrackCard_LastFmTrack"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopTrackCard_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"album"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"imageUrl"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]} as unknown as DocumentNode<TopTrackRecommendationsQuery, TopTrackRecommendationsQueryVariables>;
export const AlbumSearchResultsSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"AlbumSearchResultsSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MbReleaseGroupCard_MbReleaseGroup"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MbReleaseGroupCard_MbReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]} as unknown as DocumentNode<AlbumSearchResultsSearchQuery, AlbumSearchResultsSearchQueryVariables>;
export const ArtistSearchResultsSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistSearchResultsSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"searchText"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"searchText"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MbArtistCard_MbArtist"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MbArtistCard_MbArtist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]} as unknown as DocumentNode<ArtistSearchResultsSearchQuery, ArtistSearchResultsSearchQueryVariables>;
export const RecordingSearchResultsSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"RecordingSearchResultsSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"nameCredits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<RecordingSearchResultsSearchQuery, RecordingSearchResultsSearchQueryVariables>;
export const SearchResultArtistSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchResultArtistSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchArtists"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"searchTerm"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]}}]} as unknown as DocumentNode<SearchResultArtistSearchQuery, SearchResultArtistSearchQueryVariables>;
export const SearchResultReleaseSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchResultReleaseSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchReleases"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"searchTerm"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}}]}}]}}]}}]} as unknown as DocumentNode<SearchResultReleaseSearchQuery, SearchResultReleaseSearchQueryVariables>;
export const SearchResultTrackSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchResultTrackSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchTracks"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"searchTerm"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<SearchResultTrackSearchQuery, SearchResultTrackSearchQueryVariables>;
export const UpdateDownloadPathDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateDownloadPath"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"newDownloadPath"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateDownloadPath"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"newDownloadPath"},"value":{"kind":"Variable","name":{"kind":"Name","value":"newDownloadPath"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateDownloadPathSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverSettings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"downloadPath"}}]}}]}}]}}]}}]} as unknown as DocumentNode<UpdateDownloadPathMutation, UpdateDownloadPathMutationVariables>;
export const UpdateLibraryPathDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateLibraryPath"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"newLibraryPath"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateLibraryPath"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"newLibraryPath"},"value":{"kind":"Variable","name":{"kind":"Name","value":"newLibraryPath"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateLibraryPathSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverSettings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"libraryPath"}}]}}]}}]}}]}}]} as unknown as DocumentNode<UpdateLibraryPathMutation, UpdateLibraryPathMutationVariables>;
export const DirectoryTreeItemDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"DirectoryTreeItem"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"path"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"fileSystem"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"browseFileSystem"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"path"},"value":{"kind":"Variable","name":{"kind":"Name","value":"path"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"FileSystemEntry"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"Field","name":{"kind":"Name","value":"hasChildren"}},{"kind":"Field","name":{"kind":"Name","value":"isAccessible"}}]}}]} as unknown as DocumentNode<DirectoryTreeItemQuery, DirectoryTreeItemQueryVariables>;
export const BrowseFileSystemDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"BrowseFileSystem"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"path"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"fileSystem"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"browseFileSystem"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"path"},"value":{"kind":"Variable","name":{"kind":"Name","value":"path"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"FileSystemEntry"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"Field","name":{"kind":"Name","value":"hasChildren"}},{"kind":"Field","name":{"kind":"Name","value":"isAccessible"}}]}}]} as unknown as DocumentNode<BrowseFileSystemQuery, BrowseFileSystemQueryVariables>;
export const SidebarDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"Sidebar"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]}}]} as unknown as DocumentNode<SidebarQuery, SidebarQueryVariables>;
export const SoulSeekNetworkStatusSubscriptionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"SoulSeekNetworkStatusSubscription"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"soulSeekStatusUpdated"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatusSubscriptionSubscription, SoulSeekNetworkStatusSubscriptionSubscriptionVariables>;
export const SoulSeekNetworkStatusQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SoulSeekNetworkStatusQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"external"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"soulSeek"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatusQueryQuery, SoulSeekNetworkStatusQueryQueryVariables>;
export const ImportSpotifyPlaylistByIdDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ImportSpotifyPlaylistById"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"importSpotifyPlaylistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}},{"kind":"Argument","name":{"kind":"Name","value":"userId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ImportSpotifyPlaylistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"success"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ImportSpotifyPlaylistError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<ImportSpotifyPlaylistByIdMutation, ImportSpotifyPlaylistByIdMutationVariables>;
export const UserPlaylistsLoader_QueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"UserPlaylistsLoader_Query"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"spotifyUsername"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"playlist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"importPlaylists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"spotify"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"spotifyPlaylistsForUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"username"},"value":{"kind":"Variable","name":{"kind":"Name","value":"spotifyUsername"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"SpotifyPlaylistsList_SpotifyPlaylist"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SpotifyPlaylistsList_SpotifyPlaylist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SpotifyPlaylist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}}]}}]} as unknown as DocumentNode<UserPlaylistsLoader_QueryQuery, UserPlaylistsLoader_QueryQueryVariables>;
export const UserProfileWidgetDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"UserProfileWidget"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]}}]} as unknown as DocumentNode<UserProfileWidgetQuery, UserProfileWidgetQueryVariables>;