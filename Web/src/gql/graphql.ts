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

export type AddTrackToPlaylistError = {
  __typename?: 'AddTrackToPlaylistError';
  message: Scalars['String']['output'];
};

export type AddTrackToPlaylistInput = {
  artistId: Scalars['ID']['input'];
  playlistId: Scalars['ID']['input'];
  releaseFolderName: Scalars['String']['input'];
  trackNumber: Scalars['Int']['input'];
};

export type AddTrackToPlaylistResult = AddTrackToPlaylistError | AddTrackToPlaylistSuccess;

export type AddTrackToPlaylistSuccess = {
  __typename?: 'AddTrackToPlaylistSuccess';
  playlist: Playlist;
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

export type Artist = ArtistBase & {
  __typename?: 'Artist';
  albums: Array<Release>;
  connectedExternalServices: Array<ArtistConnectedExternalService>;
  eps: Array<Release>;
  id: Scalars['ID']['output'];
  images?: Maybe<ArtistImages>;
  listeners?: Maybe<Scalars['Long']['output']>;
  name: Scalars['String']['output'];
  releaseByFolderName?: Maybe<Release>;
  releases: Array<Release>;
  singles: Array<Release>;
  sortName: Scalars['String']['output'];
  topTracks: Array<ArtistTopTrack>;
};


export type ArtistReleaseByFolderNameArgs = {
  releaseFolderName: Scalars['String']['input'];
};

export type ArtistBase = {
  id: Scalars['ID']['output'];
  name: Scalars['String']['output'];
  sortName: Scalars['String']['output'];
};

export type ArtistConnectedExternalService = {
  __typename?: 'ArtistConnectedExternalService';
  artistPageUrl: Scalars['String']['output'];
  externalService: ExternalService;
  isConnected: Scalars['Boolean']['output'];
};

export type ArtistImages = {
  __typename?: 'ArtistImages';
  backgrounds: Array<Scalars['String']['output']>;
  banners: Array<Scalars['String']['output']>;
  logos: Array<Scalars['String']['output']>;
  thumbs: Array<Scalars['String']['output']>;
};

export type ArtistImportProgress = {
  __typename?: 'ArtistImportProgress';
  artistName: Scalars['String']['output'];
  completedReleases: Scalars['Int']['output'];
  errorMessage?: Maybe<Scalars['String']['output']>;
  songTitle?: Maybe<Scalars['String']['output']>;
  status: ArtistImportStatus;
  statusInfo: ArtistImportStatusInfo;
  totalReleases: Scalars['Int']['output'];
};

export type ArtistImportQueueItem = {
  __typename?: 'ArtistImportQueueItem';
  artistName: Scalars['String']['output'];
  songTitle?: Maybe<Scalars['String']['output']>;
};

export type ArtistImportQueueState = {
  __typename?: 'ArtistImportQueueState';
  items: Array<ArtistImportQueueItem>;
  queueLength: Scalars['Int']['output'];
};

export type ArtistImportSearchRoot = {
  __typename?: 'ArtistImportSearchRoot';
  artistImportQueue: ArtistImportQueueState;
  currentArtistImport?: Maybe<ArtistImportProgress>;
};

export enum ArtistImportStatus {
  Completed = 'COMPLETED',
  Failed = 'FAILED',
  Idle = 'IDLE',
  ImportingArtist = 'IMPORTING_ARTIST',
  ImportingReleases = 'IMPORTING_RELEASES',
  ResolvingArtist = 'RESOLVING_ARTIST'
}

export type ArtistImportStatusInfo = {
  __typename?: 'ArtistImportStatusInfo';
  id: ArtistImportStatus;
  text: Scalars['String']['output'];
};

export type ArtistSearchRoot = {
  __typename?: 'ArtistSearchRoot';
  all: Array<Artist>;
  byId?: Maybe<Artist>;
  externalArtistById?: Maybe<ExternalArtist>;
  searchArtists: Array<Artist>;
  searchExternalArtists: Array<ExternalArtist>;
};


export type ArtistSearchRootByIdArgs = {
  artistId: Scalars['ID']['input'];
};


export type ArtistSearchRootExternalArtistByIdArgs = {
  artistId: Scalars['ID']['input'];
  serviceType: ExternalServiceType;
};


export type ArtistSearchRootSearchArtistsArgs = {
  limit?: Scalars['Int']['input'];
  searchTerm: Scalars['String']['input'];
};


export type ArtistSearchRootSearchExternalArtistsArgs = {
  limit?: Scalars['Int']['input'];
  searchTerm: Scalars['String']['input'];
  serviceType: ExternalServiceType;
};

export type ArtistTopTrack = {
  __typename?: 'ArtistTopTrack';
  coverArtUrl?: Maybe<Scalars['String']['output']>;
  playCount?: Maybe<Scalars['Long']['output']>;
  releaseTitle?: Maybe<Scalars['String']['output']>;
  title: Scalars['String']['output'];
  track?: Maybe<Track>;
  trackLength?: Maybe<Scalars['Int']['output']>;
};

export enum BulkDownloadScope {
  Albums = 'ALBUMS',
  All = 'ALL',
  Eps = 'EPS',
  Singles = 'SINGLES'
}

export type CacheStatistics = {
  __typename?: 'CacheStatistics';
  artistCount: Scalars['Int']['output'];
  isInitialized: Scalars['Boolean']['output'];
  lastUpdated: Scalars['DateTime']['output'];
  releaseCount: Scalars['Int']['output'];
  trackCount: Scalars['Int']['output'];
};

export type CreateDirectoryError = {
  __typename?: 'CreateDirectoryError';
  message: Scalars['String']['output'];
};

export type CreateDirectoryInput = {
  path: Scalars['String']['input'];
};

export type CreateDirectoryResult = CreateDirectoryError | CreateDirectorySuccess;

export type CreateDirectorySuccess = {
  __typename?: 'CreateDirectorySuccess';
  entry: FileSystemEntry;
};

export type CreatePlaylistInput = {
  description?: InputMaybe<Scalars['String']['input']>;
  name?: InputMaybe<Scalars['String']['input']>;
};

export type CreatePlaylistResult = CreatePlaylistSuccess;

export type CreatePlaylistSuccess = {
  __typename?: 'CreatePlaylistSuccess';
  playlist: Playlist;
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
  playlistId: Scalars['ID']['input'];
};

export type DeletePlaylistNoWriteAccess = {
  __typename?: 'DeletePlaylistNoWriteAccess';
  message: Scalars['String']['output'];
};

export type DeletePlaylistResult = DeletePlaylistNoWriteAccess | DeletePlaylistSuccess;

export type DeletePlaylistSuccess = {
  __typename?: 'DeletePlaylistSuccess';
  deletedPlaylistId: Scalars['UUID']['output'];
};

export type DeleteReleaseAudioError = {
  __typename?: 'DeleteReleaseAudioError';
  message: Scalars['String']['output'];
};

export type DeleteReleaseAudioInput = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
};

export type DeleteReleaseAudioResult = DeleteReleaseAudioError | DeleteReleaseAudioSuccess;

export type DeleteReleaseAudioSuccess = {
  __typename?: 'DeleteReleaseAudioSuccess';
  release: Release;
};

export type DownloadProgress = {
  __typename?: 'DownloadProgress';
  artistId: Scalars['String']['output'];
  completedTracks: Scalars['Int']['output'];
  errorMessage?: Maybe<Scalars['String']['output']>;
  releaseFolderName: Scalars['String']['output'];
  status: DownloadStatus;
  totalTracks: Scalars['Int']['output'];
};

export type DownloadQueueItem = {
  __typename?: 'DownloadQueueItem';
  artistId: Scalars['String']['output'];
  releaseFolderName: Scalars['String']['output'];
};

export type DownloadQueueState = {
  __typename?: 'DownloadQueueState';
  items: Array<DownloadQueueItem>;
  queueLength: Scalars['Int']['output'];
};

export enum DownloadStatus {
  Completed = 'COMPLETED',
  Downloading = 'DOWNLOADING',
  Failed = 'FAILED',
  Idle = 'IDLE',
  Processing = 'PROCESSING',
  Searching = 'SEARCHING'
}

export type DownloadsSearchRoot = {
  __typename?: 'DownloadsSearchRoot';
  currentDownload?: Maybe<DownloadProgress>;
  downloadQueue: DownloadQueueState;
};

export type EnqueueArtistsFromSpotifyPlaylistInput = {
  playlistId: Scalars['String']['input'];
};

export type ExternalArtist = {
  __typename?: 'ExternalArtist';
  artistId: Scalars['String']['output'];
  artistName: Scalars['String']['output'];
  id: Scalars['ID']['output'];
  service: ExternalServiceType;
};

export type ExternalRoot = {
  __typename?: 'ExternalRoot';
  id: Scalars['ID']['output'];
  soulSeek: SoulSeekRoot;
};

export type ExternalService = {
  __typename?: 'ExternalService';
  id: Scalars['ID']['output'];
  name: Scalars['String']['output'];
};

export enum ExternalServiceType {
  AppleMusic = 'APPLE_MUSIC',
  Deezer = 'DEEZER',
  Other = 'OTHER',
  Spotify = 'SPOTIFY',
  Tidal = 'TIDAL',
  YouTubeMusic = 'YOU_TUBE_MUSIC'
}

export type ExternalServicesSearchRoot = {
  __typename?: 'ExternalServicesSearchRoot';
  all: Array<ExternalService>;
  byId?: Maybe<ExternalService>;
};


export type ExternalServicesSearchRootByIdArgs = {
  id: Scalars['ID']['input'];
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

export type ImportArtistError = {
  __typename?: 'ImportArtistError';
  message: Scalars['String']['output'];
};

export type ImportArtistInput = {
  musicBrainzArtistId: Scalars['String']['input'];
};

export type ImportArtistResult = ImportArtistError | ImportArtistSuccess;

export type ImportArtistSuccess = {
  __typename?: 'ImportArtistSuccess';
  artist: Artist;
};

export type ImportArtistsFromSpotifyPlaylistError = {
  __typename?: 'ImportArtistsFromSpotifyPlaylistError';
  message: Scalars['String']['output'];
};

export type ImportArtistsFromSpotifyPlaylistInput = {
  playlistId: Scalars['String']['input'];
};

export type ImportArtistsFromSpotifyPlaylistResult = ImportArtistsFromSpotifyPlaylistError | ImportArtistsFromSpotifyPlaylistSuccess;

export type ImportArtistsFromSpotifyPlaylistSuccess = {
  __typename?: 'ImportArtistsFromSpotifyPlaylistSuccess';
  artists: Array<Artist>;
  failedArtists: Array<Scalars['String']['output']>;
  importedArtists: Scalars['Int']['output'];
  totalArtists: Scalars['Int']['output'];
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

export type ImportSpotifyPlaylistInput = {
  playlistId: Scalars['String']['input'];
  userId: Scalars['UUID']['input'];
};

export type ImportSpotifyPlaylistResult = ImportSpotifyPlaylistError | ImportSpotifyPlaylistSuccess;

export type ImportSpotifyPlaylistSuccess = {
  __typename?: 'ImportSpotifyPlaylistSuccess';
  playlist: Playlist;
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

export type LibraryCacheTrackStatus = {
  __typename?: 'LibraryCacheTrackStatus';
  artistId: Scalars['String']['output'];
  releaseFolderName: Scalars['String']['output'];
  track?: Maybe<Track>;
  trackNumber: Scalars['Int']['output'];
};

export type LibraryReleaseDownloadStatusUpdate = {
  __typename?: 'LibraryReleaseDownloadStatusUpdate';
  release: Release;
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

export type MbArtist = ArtistBase & {
  __typename?: 'MbArtist';
  albums: Array<MbReleaseGroup>;
  country?: Maybe<Scalars['String']['output']>;
  disambiguation?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  images?: Maybe<MbArtistImages>;
  lastFmArtist?: Maybe<LastFmArtist>;
  listeners?: Maybe<Scalars['Long']['output']>;
  name: Scalars['String']['output'];
  relations: Array<MbRelation>;
  releaseGroups: Array<MbReleaseGroup>;
  releases: Array<MbRelease>;
  singles: Array<MbReleaseGroup>;
  sortName: Scalars['String']['output'];
  topTracks: Array<LastFmTrack>;
  type?: Maybe<Scalars['String']['output']>;
};

export type MbArtistImages = {
  __typename?: 'MbArtistImages';
  artistBackground?: Maybe<Scalars['String']['output']>;
  artistThumb?: Maybe<Scalars['String']['output']>;
  hdMusicLogo?: Maybe<Scalars['String']['output']>;
  musicBanner?: Maybe<Scalars['String']['output']>;
  musicLogo?: Maybe<Scalars['String']['output']>;
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

export enum MediaAvailabilityStatus {
  Available = 'AVAILABLE',
  Downloading = 'DOWNLOADING',
  Missing = 'MISSING',
  Processing = 'PROCESSING',
  QueuedForDownload = 'QUEUED_FOR_DOWNLOAD',
  Unknown = 'UNKNOWN'
}

export type MovePlaylistItemError = {
  __typename?: 'MovePlaylistItemError';
  message: Scalars['String']['output'];
};

export type MovePlaylistItemInput = {
  newIndex: Scalars['Int']['input'];
  playlistId: Scalars['ID']['input'];
  playlistItemId: Scalars['ID']['input'];
};

export type MovePlaylistItemResult = MovePlaylistItemError | MovePlaylistItemSuccess;

export type MovePlaylistItemSuccess = {
  __typename?: 'MovePlaylistItemSuccess';
  playlist: Playlist;
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
  searchByNameAndArtistName: Array<MbReleaseGroup>;
};


export type MusicBrainzReleaseGroupSearchRootByIdArgs = {
  id: Scalars['ID']['input'];
};


export type MusicBrainzReleaseGroupSearchRootSearchByNameArgs = {
  limit?: Scalars['Int']['input'];
  name: Scalars['String']['input'];
  offset?: Scalars['Int']['input'];
};


export type MusicBrainzReleaseGroupSearchRootSearchByNameAndArtistNameArgs = {
  artistName: Scalars['String']['input'];
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
  addTrackToPlaylist: AddTrackToPlaylistResult;
  createDirectory: CreateDirectoryResult;
  createPlaylist: CreatePlaylistResult;
  createUser: CreateUserResult;
  deletePlaylist: DeletePlaylistResult;
  deleteReleaseAudio: DeleteReleaseAudioResult;
  enqueueArtist: Scalars['Boolean']['output'];
  enqueueArtistsFromSpotifyPlaylist: Scalars['Boolean']['output'];
  enqueueMissingArtistsFromPlaylist: Scalars['Boolean']['output'];
  importArtist: ImportArtistResult;
  importArtistReleases: ImportReleasesResult;
  importArtistsFromSpotifyPlaylist: ImportArtistsFromSpotifyPlaylistResult;
  importSpotifyPlaylist: ImportSpotifyPlaylistResult;
  likeSong: LikeSongResult;
  movePlaylistItem: MovePlaylistItemResult;
  ping: Scalars['Boolean']['output'];
  redownloadRelease: RedownloadReleaseResult;
  refreshArtistMetaData: RefreshArtistMetaDataResult;
  refreshArtistTopTracks: RefreshArtistTopTracksResult;
  refreshRelease: RefreshReleaseResult;
  removeItemFromPlaylist: RemoveItemFromPlaylistResult;
  renamePlaylist: RenamePlaylistResult;
  scanLibraryForMissingJson: ScanLibraryForMissingJsonResult;
  scanReleaseFolderForMedia: ScanReleaseFolderForMediaResult;
  setArtistMusicBrainzMatch: SetArtistMusicBrainzMatchResult;
  setArtistSpotifyMatch: SetArtistSpotifyMatchResult;
  setPlaylistItemArtistMatch: SetPlaylistItemArtistMatchResult;
  setPlaylistItemArtistMusicBrainzMatch: SetPlaylistItemArtistMusicBrainzMatchResult;
  setReleaseGroup: SetReleaseGroupResult;
  setReleaseMatchOverride: SetReleaseMatchOverrideResult;
  signIn: SignInResult;
  signOut: SignOutResult;
  startBulkDownloadForArtist: StartBulkDownloadForArtistResult;
  startDownloadRelease: StartDownloadReleaseResult;
  unlikeSong: UnlikeSongResult;
  updateDownloadPath: UpdateDownloadPathResult;
  updateLibraryPath: UpdateLibraryPathResult;
};


export type MutationAddTrackToPlaylistArgs = {
  input: AddTrackToPlaylistInput;
};


export type MutationCreateDirectoryArgs = {
  input: CreateDirectoryInput;
};


export type MutationCreatePlaylistArgs = {
  input: CreatePlaylistInput;
};


export type MutationCreateUserArgs = {
  input: CreateUserInput;
};


export type MutationDeletePlaylistArgs = {
  input: DeletePlaylistInput;
};


export type MutationDeleteReleaseAudioArgs = {
  input: DeleteReleaseAudioInput;
};


export type MutationEnqueueArtistArgs = {
  artistName: Scalars['String']['input'];
  songTitle?: InputMaybe<Scalars['String']['input']>;
};


export type MutationEnqueueArtistsFromSpotifyPlaylistArgs = {
  input: EnqueueArtistsFromSpotifyPlaylistInput;
};


export type MutationEnqueueMissingArtistsFromPlaylistArgs = {
  playlistId: Scalars['ID']['input'];
};


export type MutationImportArtistArgs = {
  input: ImportArtistInput;
};


export type MutationImportArtistReleasesArgs = {
  artistId: Scalars['String']['input'];
};


export type MutationImportArtistsFromSpotifyPlaylistArgs = {
  input: ImportArtistsFromSpotifyPlaylistInput;
};


export type MutationImportSpotifyPlaylistArgs = {
  input: ImportSpotifyPlaylistInput;
};


export type MutationLikeSongArgs = {
  input: LikeSongInput;
};


export type MutationMovePlaylistItemArgs = {
  input: MovePlaylistItemInput;
};


export type MutationRedownloadReleaseArgs = {
  input: RedownloadReleaseInput;
};


export type MutationRefreshArtistMetaDataArgs = {
  input: RefreshArtistMetaDataInput;
};


export type MutationRefreshArtistTopTracksArgs = {
  input: RefreshArtistTopTracksInput;
};


export type MutationRefreshReleaseArgs = {
  input: RefreshReleaseInput;
};


export type MutationRemoveItemFromPlaylistArgs = {
  input: RemoveItemFromPlaylistInput;
};


export type MutationRenamePlaylistArgs = {
  input: RenamePlaylistInput;
};


export type MutationScanReleaseFolderForMediaArgs = {
  input: ScanReleaseFolderForMediaInput;
};


export type MutationSetArtistMusicBrainzMatchArgs = {
  input: SetArtistMusicBrainzMatchInput;
};


export type MutationSetArtistSpotifyMatchArgs = {
  input: SetArtistSpotifyMatchInput;
};


export type MutationSetPlaylistItemArtistMatchArgs = {
  input: SetPlaylistItemArtistMatchInput;
};


export type MutationSetPlaylistItemArtistMusicBrainzMatchArgs = {
  input: SetPlaylistItemArtistMusicBrainzMatchInput;
};


export type MutationSetReleaseGroupArgs = {
  input: SetReleaseGroupInput;
};


export type MutationSetReleaseMatchOverrideArgs = {
  input: SetReleaseMatchOverrideInput;
};


export type MutationSignInArgs = {
  input: SignInInput;
};


export type MutationSignOutArgs = {
  input: SignOutInput;
};


export type MutationStartBulkDownloadForArtistArgs = {
  input: StartBulkDownloadForArtistInput;
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

export type Ping = {
  __typename?: 'Ping';
  id: Scalars['ID']['output'];
};

export type Playlist = {
  __typename?: 'Playlist';
  coverImageUrl?: Maybe<Scalars['String']['output']>;
  createdAt: Scalars['DateTime']['output'];
  id: Scalars['ID']['output'];
  items: Array<PlaylistItem>;
  modifiedAt?: Maybe<Scalars['DateTime']['output']>;
  name?: Maybe<Scalars['String']['output']>;
};

export type PlaylistItem = {
  __typename?: 'PlaylistItem';
  addedAt: Scalars['DateTime']['output'];
  artist?: Maybe<Artist>;
  artistName?: Maybe<Scalars['String']['output']>;
  coverImageUrl?: Maybe<Scalars['String']['output']>;
  externalAlbumId?: Maybe<Scalars['String']['output']>;
  externalArtistId?: Maybe<Scalars['String']['output']>;
  externalService?: Maybe<ExternalServiceType>;
  externalTrackId?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  localCoverImageUrl?: Maybe<Scalars['String']['output']>;
  releaseTitle?: Maybe<Scalars['String']['output']>;
  releaseType?: Maybe<Scalars['String']['output']>;
  title?: Maybe<Scalars['String']['output']>;
  track?: Maybe<Track>;
  trackLengthMs?: Maybe<Scalars['Int']['output']>;
};

export type PlaylistSearchRoot = {
  __typename?: 'PlaylistSearchRoot';
  byId?: Maybe<Playlist>;
  importPlaylists: ImportPlaylistSearchRoot;
};


export type PlaylistSearchRootByIdArgs = {
  playlistId: Scalars['ID']['input'];
};

export type Query = {
  __typename?: 'Query';
  areThereAnyUsers: Scalars['Boolean']['output'];
  artist: ArtistSearchRoot;
  artistImport: ArtistImportSearchRoot;
  downloads: DownloadsSearchRoot;
  external: ExternalRoot;
  externalServices: ExternalServicesSearchRoot;
  fileSystem: FileSystemSearchRoot;
  musicBrainz: MusicBrainzSearchRoot;
  playlist: PlaylistSearchRoot;
  recommendations: RecommendationsSearchRoot;
  releasesWithScores: Array<ScoredRelease>;
  serverLibrary: ServerLibrarySearchRoot;
  serverSettings: ServerSettings;
  spotify: SpotifyArtistSearchRoot;
  user: UserSearchRoot;
  viewer?: Maybe<User>;
};


export type QueryReleasesWithScoresArgs = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
  releaseGroupId: Scalars['String']['input'];
};

export type RecommendationsSearchRoot = {
  __typename?: 'RecommendationsSearchRoot';
  topArtists: Array<LastFmArtist>;
  topTags: Array<LastFmTag>;
  topTracks: Array<LastFmTrack>;
};

export type RedownloadReleaseError = {
  __typename?: 'RedownloadReleaseError';
  message: Scalars['String']['output'];
};

export type RedownloadReleaseInput = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
};

export type RedownloadReleaseResult = RedownloadReleaseError | RedownloadReleaseSuccess;

export type RedownloadReleaseSuccess = {
  __typename?: 'RedownloadReleaseSuccess';
  release: Release;
};

export type RefreshArtistMetaDataError = {
  __typename?: 'RefreshArtistMetaDataError';
  message: Scalars['String']['output'];
};

export type RefreshArtistMetaDataInput = {
  artistId: Scalars['String']['input'];
};

export type RefreshArtistMetaDataResult = RefreshArtistMetaDataError | RefreshArtistMetaDataSuccess;

export type RefreshArtistMetaDataSuccess = {
  __typename?: 'RefreshArtistMetaDataSuccess';
  artist: Artist;
};

export type RefreshArtistTopTracksInput = {
  artistId: Scalars['String']['input'];
};

export type RefreshArtistTopTracksResult = RefreshArtistTopTracksSuccess | RefreshArtistTopTracksUnknownError;

export type RefreshArtistTopTracksSuccess = {
  __typename?: 'RefreshArtistTopTracksSuccess';
  artist: Artist;
};

export type RefreshArtistTopTracksUnknownError = {
  __typename?: 'RefreshArtistTopTracksUnknownError';
  message: Scalars['String']['output'];
};

export type RefreshReleaseError = {
  __typename?: 'RefreshReleaseError';
  message: Scalars['String']['output'];
};

export type RefreshReleaseInput = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
};

export type RefreshReleaseResult = RefreshReleaseError | RefreshReleaseSuccess;

export type RefreshReleaseSuccess = {
  __typename?: 'RefreshReleaseSuccess';
  release: Release;
};

export type Release = {
  __typename?: 'Release';
  artist: Artist;
  coverArtUrl: Scalars['String']['output'];
  downloadStatus: ReleaseDownloadStatus;
  firstReleaseDate?: Maybe<Scalars['String']['output']>;
  firstReleaseYear?: Maybe<Scalars['String']['output']>;
  folderName: Scalars['String']['output'];
  id: Scalars['ID']['output'];
  isFullyMissing: Scalars['Boolean']['output'];
  musicBrainzReleaseGroupId?: Maybe<Scalars['String']['output']>;
  musicBrainzReleaseIdOverride?: Maybe<Scalars['String']['output']>;
  musicBrainzSelectedReleaseId?: Maybe<Scalars['String']['output']>;
  title: Scalars['String']['output'];
  tracks: Array<Track>;
  type?: Maybe<ReleaseType>;
};

export enum ReleaseDownloadStatus {
  Downloading = 'DOWNLOADING',
  Idle = 'IDLE',
  NotFound = 'NOT_FOUND',
  Searching = 'SEARCHING'
}

export enum ReleaseType {
  Album = 'ALBUM',
  Ep = 'EP',
  Single = 'SINGLE'
}

export type RemoveItemFromPlaylistError = {
  __typename?: 'RemoveItemFromPlaylistError';
  message: Scalars['String']['output'];
};

export type RemoveItemFromPlaylistInput = {
  playlistId: Scalars['ID']['input'];
  playlistItemId: Scalars['ID']['input'];
};

export type RemoveItemFromPlaylistResult = RemoveItemFromPlaylistError | RemoveItemFromPlaylistSuccess;

export type RemoveItemFromPlaylistSuccess = {
  __typename?: 'RemoveItemFromPlaylistSuccess';
  playlist: Playlist;
};

export type RenamePlaylistInput = {
  newPlaylistName: Scalars['String']['input'];
  playlistId: Scalars['ID']['input'];
};

export type RenamePlaylistNoWriteAccess = {
  __typename?: 'RenamePlaylistNoWriteAccess';
  message: Scalars['String']['output'];
};

export type RenamePlaylistResult = RenamePlaylistNoWriteAccess | RenamePlaylistSuccess;

export type RenamePlaylistSuccess = {
  __typename?: 'RenamePlaylistSuccess';
  playlist: Playlist;
};

export type ScanLibraryForMissingJsonResult = ScanLibraryForMissingJsonSuccess;

export type ScanLibraryForMissingJsonSuccess = {
  __typename?: 'ScanLibraryForMissingJsonSuccess';
  artistsCreated: Scalars['Int']['output'];
  errorMessage?: Maybe<Scalars['String']['output']>;
  notes: Array<Scalars['String']['output']>;
  releasesCreated: Scalars['Int']['output'];
  success: Scalars['Boolean']['output'];
};

export type ScanReleaseFolderForMediaError = {
  __typename?: 'ScanReleaseFolderForMediaError';
  message: Scalars['String']['output'];
};

export type ScanReleaseFolderForMediaInput = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
};

export type ScanReleaseFolderForMediaResult = ScanReleaseFolderForMediaError | ScanReleaseFolderForMediaSuccess;

export type ScanReleaseFolderForMediaSuccess = {
  __typename?: 'ScanReleaseFolderForMediaSuccess';
  release: Release;
};

export type ScoredRelease = {
  __typename?: 'ScoredRelease';
  reasons: Array<Scalars['String']['output']>;
  release: MbRelease;
  score: Scalars['Int']['output'];
};

export type ServerLibrarySearchRoot = {
  __typename?: 'ServerLibrarySearchRoot';
  allArtists: Array<Artist>;
  allReleases: Array<Release>;
  allTracks: Array<Track>;
  artistById?: Maybe<Artist>;
  libraryStatistics: CacheStatistics;
  releaseForArtistByFolderName?: Maybe<Release>;
  releasesForArtist: Array<Release>;
  searchArtists: Array<Artist>;
  searchReleases: Array<Release>;
  searchTracks: Array<Track>;
  trackById?: Maybe<Track>;
  tracksForArtist: Array<Track>;
};


export type ServerLibrarySearchRootArtistByIdArgs = {
  id: Scalars['ID']['input'];
};


export type ServerLibrarySearchRootReleaseForArtistByFolderNameArgs = {
  artistId: Scalars['ID']['input'];
  releaseFolderName: Scalars['String']['input'];
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


export type ServerLibrarySearchRootTrackByIdArgs = {
  id: Scalars['ID']['input'];
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

export type SetArtistMusicBrainzMatchError = {
  __typename?: 'SetArtistMusicBrainzMatchError';
  message: Scalars['String']['output'];
};

export type SetArtistMusicBrainzMatchInput = {
  artistId: Scalars['String']['input'];
  musicBrainzArtistId: Scalars['String']['input'];
};

export type SetArtistMusicBrainzMatchResult = SetArtistMusicBrainzMatchError | SetArtistMusicBrainzMatchSuccess;

export type SetArtistMusicBrainzMatchSuccess = {
  __typename?: 'SetArtistMusicBrainzMatchSuccess';
  artist: Artist;
};

export type SetArtistSpotifyMatchError = {
  __typename?: 'SetArtistSpotifyMatchError';
  message: Scalars['String']['output'];
};

export type SetArtistSpotifyMatchInput = {
  artistId: Scalars['String']['input'];
  spotifyArtistId: Scalars['String']['input'];
};

export type SetArtistSpotifyMatchResult = SetArtistSpotifyMatchError | SetArtistSpotifyMatchSuccess;

export type SetArtistSpotifyMatchSuccess = {
  __typename?: 'SetArtistSpotifyMatchSuccess';
  artist: Artist;
};

export type SetPlaylistItemArtistMatchInput = {
  externalArtistId: Scalars['String']['input'];
  playlistId: Scalars['ID']['input'];
  playlistItemId: Scalars['ID']['input'];
};

export type SetPlaylistItemArtistMatchNotFound = {
  __typename?: 'SetPlaylistItemArtistMatchNotFound';
  message: Scalars['String']['output'];
};

export type SetPlaylistItemArtistMatchResult = SetPlaylistItemArtistMatchNotFound | SetPlaylistItemArtistMatchSuccess;

export type SetPlaylistItemArtistMatchSuccess = {
  __typename?: 'SetPlaylistItemArtistMatchSuccess';
  playlistItem: PlaylistItem;
};

export type SetPlaylistItemArtistMusicBrainzMatchError = {
  __typename?: 'SetPlaylistItemArtistMusicBrainzMatchError';
  message: Scalars['String']['output'];
};

export type SetPlaylistItemArtistMusicBrainzMatchInput = {
  musicBrainzArtistId: Scalars['String']['input'];
  playlistId: Scalars['ID']['input'];
  playlistItemId: Scalars['ID']['input'];
};

export type SetPlaylistItemArtistMusicBrainzMatchNotFound = {
  __typename?: 'SetPlaylistItemArtistMusicBrainzMatchNotFound';
  message: Scalars['String']['output'];
};

export type SetPlaylistItemArtistMusicBrainzMatchResult = SetPlaylistItemArtistMusicBrainzMatchError | SetPlaylistItemArtistMusicBrainzMatchNotFound | SetPlaylistItemArtistMusicBrainzMatchSuccess;

export type SetPlaylistItemArtistMusicBrainzMatchSuccess = {
  __typename?: 'SetPlaylistItemArtistMusicBrainzMatchSuccess';
  playlistItem: PlaylistItem;
};

export type SetReleaseGroupError = {
  __typename?: 'SetReleaseGroupError';
  message: Scalars['String']['output'];
};

export type SetReleaseGroupInput = {
  artistId: Scalars['String']['input'];
  musicBrainzReleaseGroupId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
};

export type SetReleaseGroupResult = SetReleaseGroupError | SetReleaseGroupSuccess;

export type SetReleaseGroupSuccess = {
  __typename?: 'SetReleaseGroupSuccess';
  release: Release;
};

export type SetReleaseMatchOverrideError = {
  __typename?: 'SetReleaseMatchOverrideError';
  message: Scalars['String']['output'];
};

export type SetReleaseMatchOverrideInput = {
  artistId: Scalars['String']['input'];
  musicBrainzReleaseId?: InputMaybe<Scalars['String']['input']>;
  releaseFolderName: Scalars['String']['input'];
};

export type SetReleaseMatchOverrideResult = SetReleaseMatchOverrideError | SetReleaseMatchOverrideSuccess;

export type SetReleaseMatchOverrideSuccess = {
  __typename?: 'SetReleaseMatchOverrideSuccess';
  release: Release;
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

export type SignOutInput = {
  confirm: Scalars['Boolean']['input'];
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

export type SpotifyArtist = {
  __typename?: 'SpotifyArtist';
  id: Scalars['String']['output'];
  images: Array<SpotifyImage>;
  name: Scalars['String']['output'];
};

export type SpotifyArtistSearchRoot = {
  __typename?: 'SpotifyArtistSearchRoot';
  byId?: Maybe<SpotifyArtist>;
  searchByName: Array<SpotifyArtist>;
};


export type SpotifyArtistSearchRootByIdArgs = {
  id: Scalars['ID']['input'];
};


export type SpotifyArtistSearchRootSearchByNameArgs = {
  limit?: Scalars['Int']['input'];
  name: Scalars['String']['input'];
  offset?: Scalars['Int']['input'];
};

export type SpotifyImage = {
  __typename?: 'SpotifyImage';
  url: Scalars['String']['output'];
};

export type SpotifyPlaylist = {
  __typename?: 'SpotifyPlaylist';
  coverImageUrl?: Maybe<Scalars['String']['output']>;
  description?: Maybe<Scalars['String']['output']>;
  id: Scalars['String']['output'];
  name: Scalars['String']['output'];
  totalTracks?: Maybe<Scalars['Int']['output']>;
  tracks: Array<SpotifyTrack>;
};

export type SpotifyPlaylistSearchRoot = {
  __typename?: 'SpotifyPlaylistSearchRoot';
  spotifyPlaylistById?: Maybe<SpotifyPlaylist>;
  spotifyPlaylistsForUser: Array<SpotifyPlaylist>;
};


export type SpotifyPlaylistSearchRootSpotifyPlaylistByIdArgs = {
  id: Scalars['String']['input'];
};


export type SpotifyPlaylistSearchRootSpotifyPlaylistsForUserArgs = {
  username: Scalars['String']['input'];
};

export type SpotifyTrack = {
  __typename?: 'SpotifyTrack';
  albumCoverImageUrl?: Maybe<Scalars['String']['output']>;
  artistNames: Array<Scalars['String']['output']>;
  durationMs?: Maybe<Scalars['Int']['output']>;
  id: Scalars['String']['output'];
  previewUrl?: Maybe<Scalars['String']['output']>;
  title: Scalars['String']['output'];
};

export type StartBulkDownloadForArtistError = {
  __typename?: 'StartBulkDownloadForArtistError';
  message: Scalars['String']['output'];
};

export type StartBulkDownloadForArtistInput = {
  artistId: Scalars['String']['input'];
  scope: BulkDownloadScope;
};

export type StartBulkDownloadForArtistResult = StartBulkDownloadForArtistError | StartBulkDownloadForArtistSuccess;

export type StartBulkDownloadForArtistSuccess = {
  __typename?: 'StartBulkDownloadForArtistSuccess';
  artistId: Scalars['String']['output'];
  queuedCount: Scalars['Int']['output'];
};

export type StartDownloadReleaseAccepted = {
  __typename?: 'StartDownloadReleaseAccepted';
  artistId: Scalars['String']['output'];
  releaseFolderName: Scalars['String']['output'];
};

export type StartDownloadReleaseInput = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
};

export type StartDownloadReleaseResult = StartDownloadReleaseAccepted | StartDownloadReleaseSuccess | StartDownloadReleaseUnknownError;

export type StartDownloadReleaseSuccess = {
  __typename?: 'StartDownloadReleaseSuccess';
  release: Release;
};

export type StartDownloadReleaseUnknownError = {
  __typename?: 'StartDownloadReleaseUnknownError';
  message: Scalars['String']['output'];
};

export type Subscription = {
  __typename?: 'Subscription';
  artistImportQueueUpdated: ArtistImportQueueState;
  artistImported: Artist;
  currentArtistImportUpdated: ArtistImportProgress;
  currentDownloadUpdated?: Maybe<DownloadProgress>;
  downloadQueueUpdated: DownloadQueueState;
  libraryArtistReleaseUpdated: Release;
  libraryArtistTrackUpdated: Track;
  libraryArtistUpdated: Artist;
  libraryCacheTrackUpdated: LibraryCacheTrackStatus;
  libraryCacheTracksInReleaseUpdated: LibraryCacheTrackStatus;
  libraryReleaseDownloadStatusUpdated: LibraryReleaseDownloadStatusUpdate;
  libraryReleaseMetadataUpdated: Release;
  libraryReleaseUpdated: Release;
  libraryTrackUpdated: Track;
  ping: Ping;
  playlistItemUpdated: PlaylistItem;
  soulSeekStatusUpdated: SoulSeekStatus;
};


export type SubscriptionLibraryArtistReleaseUpdatedArgs = {
  artistId: Scalars['String']['input'];
};


export type SubscriptionLibraryArtistTrackUpdatedArgs = {
  artistId: Scalars['String']['input'];
};


export type SubscriptionLibraryArtistUpdatedArgs = {
  artistId: Scalars['String']['input'];
};


export type SubscriptionLibraryCacheTrackUpdatedArgs = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
  trackNumber: Scalars['Int']['input'];
};


export type SubscriptionLibraryCacheTracksInReleaseUpdatedArgs = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
};


export type SubscriptionLibraryReleaseDownloadStatusUpdatedArgs = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
};


export type SubscriptionLibraryReleaseMetadataUpdatedArgs = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
};


export type SubscriptionLibraryReleaseUpdatedArgs = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
};


export type SubscriptionLibraryTrackUpdatedArgs = {
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
  trackNumber: Scalars['Int']['input'];
};


export type SubscriptionPlaylistItemUpdatedArgs = {
  playlistId: Scalars['ID']['input'];
};

export type Track = {
  __typename?: 'Track';
  credits: Array<TrackCredit>;
  id: Scalars['ID']['output'];
  isMissing: Scalars['Boolean']['output'];
  media?: Maybe<TrackMedia>;
  mediaAvailabilityStatus: MediaAvailabilityStatus;
  playCount?: Maybe<Scalars['Long']['output']>;
  playCountForViewer?: Maybe<Scalars['Long']['output']>;
  release: Release;
  statistics?: Maybe<TrackStatistics>;
  title: Scalars['String']['output'];
  trackLength?: Maybe<Scalars['Int']['output']>;
  trackNumber: Scalars['Int']['output'];
};

export type TrackCredit = {
  __typename?: 'TrackCredit';
  artist?: Maybe<Artist>;
  artistName: Scalars['String']['output'];
  mbArtist?: Maybe<MbArtist>;
};

export type TrackMedia = {
  __typename?: 'TrackMedia';
  audioBitrateKbps?: Maybe<Scalars['Int']['output']>;
  audioFormat?: Maybe<Scalars['String']['output']>;
  audioQualityLabel: Scalars['String']['output'];
  audioUrl: Scalars['String']['output'];
  id: Scalars['ID']['output'];
  isLosslessFormat: Scalars['Boolean']['output'];
};

export type TrackStatistics = {
  __typename?: 'TrackStatistics';
  listeners: Scalars['Long']['output'];
  playCount: Scalars['Long']['output'];
};

export type UnlikeSongAlreadyNotLiked = {
  __typename?: 'UnlikeSongAlreadyNotLiked';
  message: Scalars['String']['output'];
};

export type UnlikeSongResult = UnlikeSongAlreadyNotLiked | UnlikeSongSuccess;

export type UnlikeSongSuccess = {
  __typename?: 'UnlikeSongSuccess';
  viewer: User;
};

export type UnlikedSongInput = {
  recordingId: Scalars['String']['input'];
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

export type MbArtistQueryQueryVariables = Exact<{
  mbArtistId: Scalars['ID']['input'];
}>;


export type MbArtistQueryQuery = { __typename?: 'Query', musicBrainz: { __typename?: 'MusicBrainzSearchRoot', artist: { __typename?: 'MusicBrainzArtistSearchRoot', byId?: (
        { __typename?: 'MbArtist', id: string, name: string }
        & { ' $fragmentRefs'?: { 'ArtistNotInLibraryPanel_MbArtistFragment': ArtistNotInLibraryPanel_MbArtistFragment } }
      ) | null } } };

export type PlaylistQueryQueryVariables = Exact<{
  playlistId: Scalars['ID']['input'];
}>;


export type PlaylistQueryQuery = { __typename?: 'Query', playlist: { __typename?: 'PlaylistSearchRoot', byId?: (
      { __typename?: 'Playlist', id: string, name?: string | null }
      & { ' $fragmentRefs'?: { 'PlaylistPanel_PlaylistFragment': PlaylistPanel_PlaylistFragment } }
    ) | null } };

export type SettingsPageQueryVariables = Exact<{ [key: string]: never; }>;


export type SettingsPageQuery = { __typename?: 'Query', serverSettings: (
    { __typename?: 'ServerSettings' }
    & { ' $fragmentRefs'?: { 'LibraryPathForm_ServerSettingsFragment': LibraryPathForm_ServerSettingsFragment;'DownloadPathForm_ServerSettingsFragment': DownloadPathForm_ServerSettingsFragment } }
  ) };

export type SpotifyPlaylistDetailsQueryVariables = Exact<{
  playlistId: Scalars['String']['input'];
}>;


export type SpotifyPlaylistDetailsQuery = { __typename?: 'Query', playlist: { __typename?: 'PlaylistSearchRoot', importPlaylists: { __typename?: 'ImportPlaylistSearchRoot', spotify: { __typename?: 'SpotifyPlaylistSearchRoot', byId?: (
          { __typename?: 'SpotifyPlaylist', id: string }
          & { ' $fragmentRefs'?: { 'SpotifyPlaylistPanel_SpotifyPlaylistFragment': SpotifyPlaylistPanel_SpotifyPlaylistFragment } }
        ) | null } } } };

export type ProfilePageQueryVariables = Exact<{ [key: string]: never; }>;


export type ProfilePageQuery = { __typename?: 'Query', viewer?: (
    { __typename?: 'User', id: string, username: string }
    & { ' $fragmentRefs'?: { 'UserProfilePanel_UserFragment': UserProfilePanel_UserFragment } }
  ) | null };

export type ImportArtistMutationVariables = Exact<{
  musicBrainzArtistId: Scalars['String']['input'];
}>;


export type ImportArtistMutation = { __typename?: 'Mutation', importArtist: { __typename: 'ImportArtistError', message: string } | { __typename: 'ImportArtistSuccess', artist: { __typename?: 'Artist', id: string, name: string } } };

export type AlbumCard_ReleaseFragment = { __typename?: 'Release', id: string, title: string, firstReleaseYear?: string | null, coverArtUrl: string, folderName: string, isFullyMissing: boolean, artist: { __typename?: 'Artist', id: string, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null } } & { ' $fragmentName'?: 'AlbumCard_ReleaseFragment' };

export type AlbumHeader_ReleaseFragment = { __typename?: 'Release', id: string, title: string, type?: ReleaseType | null, coverArtUrl: string, firstReleaseYear?: string | null, artist: { __typename?: 'Artist', id: string, name: string, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null }, tracks: Array<{ __typename?: 'Track', id: string, trackLength?: number | null }> } & { ' $fragmentName'?: 'AlbumHeader_ReleaseFragment' };

export type AlbumPanel_ReleaseFragment = (
  { __typename?: 'Release', id: string, folderName: string, isFullyMissing: boolean, firstReleaseYear?: string | null, artist: { __typename?: 'Artist', id: string } }
  & { ' $fragmentRefs'?: { 'PlayAlbumButton_ReleaseFragment': PlayAlbumButton_ReleaseFragment;'ReleaseDownloadButton_ReleaseFragment': ReleaseDownloadButton_ReleaseFragment;'AlbumHeader_ReleaseFragment': AlbumHeader_ReleaseFragment;'AlbumTrackList_ReleaseFragment': AlbumTrackList_ReleaseFragment } }
) & { ' $fragmentName'?: 'AlbumPanel_ReleaseFragment' };

export type AlbumPanelUpdatesSubscriptionVariables = Exact<{
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
}>;


export type AlbumPanelUpdatesSubscription = { __typename?: 'Subscription', libraryCacheTracksInReleaseUpdated: { __typename?: 'LibraryCacheTrackStatus', track?: { __typename?: 'Track', id: string, isMissing: boolean, mediaAvailabilityStatus: MediaAvailabilityStatus } | null } };

export type AlbumPanelReleaseMetadataUpdatedSubscriptionVariables = Exact<{
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
}>;


export type AlbumPanelReleaseMetadataUpdatedSubscription = { __typename?: 'Subscription', libraryReleaseMetadataUpdated: (
    { __typename?: 'Release', id: string }
    & { ' $fragmentRefs'?: { 'AlbumPanel_ReleaseFragment': AlbumPanel_ReleaseFragment } }
  ) };

export type RefreshReleaseMutationVariables = Exact<{
  input: RefreshReleaseInput;
}>;


export type RefreshReleaseMutation = { __typename?: 'Mutation', refreshRelease: { __typename?: 'RefreshReleaseError', message: string } | { __typename?: 'RefreshReleaseSuccess', release: (
      { __typename?: 'Release', id: string }
      & { ' $fragmentRefs'?: { 'AlbumPanel_ReleaseFragment': AlbumPanel_ReleaseFragment } }
    ) } };

export type DeleteReleaseAudioMutationVariables = Exact<{
  input: DeleteReleaseAudioInput;
}>;


export type DeleteReleaseAudioMutation = { __typename?: 'Mutation', deleteReleaseAudio: { __typename?: 'DeleteReleaseAudioError', message: string } | { __typename?: 'DeleteReleaseAudioSuccess', release: (
      { __typename?: 'Release', id: string }
      & { ' $fragmentRefs'?: { 'AlbumPanel_ReleaseFragment': AlbumPanel_ReleaseFragment } }
    ) } };

export type ScanReleaseFolderForMediaMutationVariables = Exact<{
  input: ScanReleaseFolderForMediaInput;
}>;


export type ScanReleaseFolderForMediaMutation = { __typename?: 'Mutation', scanReleaseFolderForMedia: { __typename?: 'ScanReleaseFolderForMediaError', message: string } | { __typename?: 'ScanReleaseFolderForMediaSuccess', release: (
      { __typename?: 'Release', id: string }
      & { ' $fragmentRefs'?: { 'AlbumPanel_ReleaseFragment': AlbumPanel_ReleaseFragment } }
    ) } };

export type AlbumTrackList_ReleaseFragment = { __typename?: 'Release', id: string, title: string, folderName: string, artist: { __typename?: 'Artist', id: string }, tracks: Array<(
    { __typename?: 'Track', id: string, title: string, trackLength?: number | null, isMissing: boolean, media?: { __typename?: 'TrackMedia', id: string, audioUrl: string, audioQualityLabel: string } | null, credits: Array<{ __typename?: 'TrackCredit', artistName: string, artist?: { __typename?: 'Artist', id: string } | null, mbArtist?: { __typename?: 'MbArtist', id: string } | null }>, statistics?: { __typename?: 'TrackStatistics', listeners: any, playCount: any } | null }
    & { ' $fragmentRefs'?: { 'TrackCreditLinks_TrackFragment': TrackCreditLinks_TrackFragment;'MusicPlayerTrackFactory_TrackFragment': MusicPlayerTrackFactory_TrackFragment;'AlbumTrackTag_TrackFragment': AlbumTrackTag_TrackFragment } }
  )> } & { ' $fragmentName'?: 'AlbumTrackList_ReleaseFragment' };

export type AlbumTrackTag_TrackFragment = { __typename?: 'Track', id: string, isMissing: boolean, mediaAvailabilityStatus: MediaAvailabilityStatus } & { ' $fragmentName'?: 'AlbumTrackTag_TrackFragment' };

export type MbReleaseGroupCard_MbReleaseGroupFragment = { __typename?: 'MbReleaseGroup', id: string, title: string, firstReleaseYear?: string | null, coverArtUri?: string | null } & { ' $fragmentName'?: 'MbReleaseGroupCard_MbReleaseGroupFragment' };

export type PlayAlbumButton_ReleaseFragment = { __typename?: 'Release', id: string, folderName: string, coverArtUrl: string, artist: { __typename?: 'Artist', id: string, name: string }, tracks: Array<{ __typename?: 'Track', id: string, title: string, isMissing: boolean, trackLength?: number | null, media?: { __typename?: 'TrackMedia', id: string, audioQualityLabel: string } | null }> } & { ' $fragmentName'?: 'PlayAlbumButton_ReleaseFragment' };

export type TrackCreditLinks_TrackFragment = { __typename?: 'Track', id: string, credits: Array<{ __typename?: 'TrackCredit', artistName: string, artist?: { __typename?: 'Artist', id: string, name: string } | null, mbArtist?: { __typename?: 'MbArtist', id: string, name: string } | null }> } & { ' $fragmentName'?: 'TrackCreditLinks_TrackFragment' };

export type AlbumList_ReleaseFragment = (
  { __typename?: 'Release', id: string }
  & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
) & { ' $fragmentName'?: 'AlbumList_ReleaseFragment' };

export type FixMatch_ReleasesWithScoresQueryVariables = Exact<{
  releaseGroupId: Scalars['String']['input'];
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
}>;


export type FixMatch_ReleasesWithScoresQuery = { __typename?: 'Query', releasesWithScores: Array<{ __typename?: 'ScoredRelease', score: number, reasons: Array<string>, release: { __typename?: 'MbRelease', id: string, title: string, coverArtUri: string, year?: string | null, country?: string | null, status?: string | null, media: Array<{ __typename?: 'MbMedium', tracks: Array<{ __typename?: 'MbTrack', id: string }> }> } }> };

export type FixMatch_SetReleaseMatchOverrideMutationVariables = Exact<{
  input: SetReleaseMatchOverrideInput;
}>;


export type FixMatch_SetReleaseMatchOverrideMutation = { __typename?: 'Mutation', setReleaseMatchOverride: { __typename: 'SetReleaseMatchOverrideError', message: string } | { __typename: 'SetReleaseMatchOverrideSuccess', release: { __typename?: 'Release', id: string } } };

export type FixMatch_SetReleaseGroupMutationVariables = Exact<{
  input: SetReleaseGroupInput;
}>;


export type FixMatch_SetReleaseGroupMutation = { __typename?: 'Mutation', setReleaseGroup: { __typename: 'SetReleaseGroupError', message: string } | { __typename: 'SetReleaseGroupSuccess', release: { __typename?: 'Release', id: string } } };

export type FixMatch_ReleaseConnectionsQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
  releaseFolderName: Scalars['String']['input'];
}>;


export type FixMatch_ReleaseConnectionsQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', releaseForArtistByFolderName?: { __typename?: 'Release', id: string, musicBrainzReleaseGroupId?: string | null, title: string, artist: { __typename?: 'Artist', id: string, name: string } } | null } };

export type FixMatch_SearchReleaseGroupsQueryVariables = Exact<{
  name: Scalars['String']['input'];
  artistName: Scalars['String']['input'];
  limit: Scalars['Int']['input'];
  offset: Scalars['Int']['input'];
}>;


export type FixMatch_SearchReleaseGroupsQuery = { __typename?: 'Query', musicBrainz: { __typename?: 'MusicBrainzSearchRoot', releaseGroup: { __typename?: 'MusicBrainzReleaseGroupSearchRoot', searchByNameAndArtistName: Array<{ __typename?: 'MbReleaseGroup', id: string, title: string, primaryType?: string | null, secondaryTypes: Array<string>, firstReleaseDate?: string | null }> } } };

export type ArtistCard_ArtistFragment = { __typename?: 'Artist', id: string, name: string, listeners?: any | null, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null } & { ' $fragmentName'?: 'ArtistCard_ArtistFragment' };

export type MbArtistCard_MbArtistFragment = { __typename?: 'MbArtist', id: string, name: string, listeners?: any | null, images?: { __typename?: 'MbArtistImages', artistThumb?: string | null } | null } & { ' $fragmentName'?: 'MbArtistCard_MbArtistFragment' };

export type TopArtistCard_ArtistFragment = { __typename?: 'LastFmArtist', id: string, name: string, musicBrainzArtist?: { __typename?: 'MbArtist', id: string, name: string, images?: { __typename?: 'MbArtistImages', artistThumb?: string | null } | null } | null, statistics: { __typename?: 'LastFmStatistics', listeners: any } } & { ' $fragmentName'?: 'TopArtistCard_ArtistFragment' };

export type ArtistList_ArtistFragment = (
  { __typename?: 'Artist', id: string }
  & { ' $fragmentRefs'?: { 'ArtistCard_ArtistFragment': ArtistCard_ArtistFragment } }
) & { ' $fragmentName'?: 'ArtistList_ArtistFragment' };

export type ArtistNotInLibraryPanel_MbArtistFragment = { __typename?: 'MbArtist', id: string, name: string, listeners?: any | null, images?: { __typename?: 'MbArtistImages', artistBackground?: string | null } | null, lastFmArtist?: (
    { __typename?: 'LastFmArtist', id: string }
    & { ' $fragmentRefs'?: { 'ArtistNotInLibraryTopTracks_LastFmArtistFragment': ArtistNotInLibraryTopTracks_LastFmArtistFragment } }
  ) | null } & { ' $fragmentName'?: 'ArtistNotInLibraryPanel_MbArtistFragment' };

export type ArtistNotInLibraryTopTracks_LastFmArtistFragment = { __typename?: 'LastFmArtist', id: string, topTracks: Array<{ __typename?: 'LastFmTrack', id: string, name: string, statistics: { __typename?: 'LastFmStatistics', listeners: any } }> } & { ' $fragmentName'?: 'ArtistNotInLibraryTopTracks_LastFmArtistFragment' };

export type ArtistAlbumListQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistAlbumListQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: { __typename?: 'Artist', id: string, albums: Array<(
        { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
        & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
      )> } | null } };

export type ArtistDownloadAllReleasesButton_ArtistFragment = { __typename?: 'Artist', id: string } & { ' $fragmentName'?: 'ArtistDownloadAllReleasesButton_ArtistFragment' };

export type ArtistDownloadAll_StartBulkMutationVariables = Exact<{
  input: StartBulkDownloadForArtistInput;
}>;


export type ArtistDownloadAll_StartBulkMutation = { __typename?: 'Mutation', startBulkDownloadForArtist: { __typename: 'StartBulkDownloadForArtistError', message: string } | { __typename: 'StartBulkDownloadForArtistSuccess', queuedCount: number } };

export type ArtistEpListQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistEpListQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: { __typename?: 'Artist', id: string, eps: Array<(
        { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
        & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
      )> } | null } };

export type ArtistImportStatusInfo_ArtistFragment = { __typename?: 'Artist', id: string, name: string } & { ' $fragmentName'?: 'ArtistImportStatusInfo_ArtistFragment' };

export type ArtistPanel_CurrentImportQueryVariables = Exact<{ [key: string]: never; }>;


export type ArtistPanel_CurrentImportQuery = { __typename?: 'Query', artistImport: { __typename?: 'ArtistImportSearchRoot', currentArtistImport?: { __typename?: 'ArtistImportProgress', status: ArtistImportStatus, completedReleases: number, totalReleases: number, errorMessage?: string | null, artistName: string } | null } };

export type ArtistImportStatusInfo_SubSubscriptionVariables = Exact<{ [key: string]: never; }>;


export type ArtistImportStatusInfo_SubSubscription = { __typename?: 'Subscription', currentArtistImportUpdated: { __typename?: 'ArtistImportProgress', status: ArtistImportStatus, completedReleases: number, totalReleases: number, errorMessage?: string | null, artistName: string } };

export type ArtistNumReleasesAvailableIndicator_ArtistFragment = { __typename?: 'Artist', id: string, albums: Array<{ __typename?: 'Release', id: string, isFullyMissing: boolean }>, eps: Array<{ __typename?: 'Release', id: string, isFullyMissing: boolean }>, singles: Array<{ __typename?: 'Release', id: string, isFullyMissing: boolean }> } & { ' $fragmentName'?: 'ArtistNumReleasesAvailableIndicator_ArtistFragment' };

export type ArtistPanel_ArtistFragment = (
  { __typename?: 'Artist', id: string, name: string, listeners?: any | null, connectedExternalServices: Array<{ __typename?: 'ArtistConnectedExternalService', isConnected: boolean, externalService: { __typename?: 'ExternalService', id: string, name: string } }>, albums: Array<(
    { __typename?: 'Release', id: string, firstReleaseDate?: string | null, isFullyMissing: boolean }
    & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
  )>, eps: Array<(
    { __typename?: 'Release', id: string, firstReleaseDate?: string | null, isFullyMissing: boolean }
    & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
  )>, singles: Array<(
    { __typename?: 'Release', id: string, firstReleaseDate?: string | null, isFullyMissing: boolean }
    & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
  )>, images?: { __typename?: 'ArtistImages', backgrounds: Array<string> } | null }
  & { ' $fragmentRefs'?: { 'ArtistImportStatusInfo_ArtistFragment': ArtistImportStatusInfo_ArtistFragment;'ArtistNumReleasesAvailableIndicator_ArtistFragment': ArtistNumReleasesAvailableIndicator_ArtistFragment;'ArtistServiceConnections_ArtistFragment': ArtistServiceConnections_ArtistFragment;'ArtistDownloadAllReleasesButton_ArtistFragment': ArtistDownloadAllReleasesButton_ArtistFragment } }
) & { ' $fragmentName'?: 'ArtistPanel_ArtistFragment' };

export type PanelRefreshTopTracksMutationVariables = Exact<{
  input: RefreshArtistTopTracksInput;
}>;


export type PanelRefreshTopTracksMutation = { __typename?: 'Mutation', refreshArtistTopTracks: { __typename: 'RefreshArtistTopTracksSuccess', artist: { __typename?: 'Artist', id: string, topTracks: Array<(
        { __typename?: 'ArtistTopTrack' }
        & { ' $fragmentRefs'?: { 'TopArtistTrackItem_ArtistTopTrackFragment': TopArtistTrackItem_ArtistTopTrackFragment } }
      )> } } | { __typename: 'RefreshArtistTopTracksUnknownError' } };

export type PanelRefreshArtistMetaDataMutationVariables = Exact<{
  artistId: Scalars['String']['input'];
}>;


export type PanelRefreshArtistMetaDataMutation = { __typename?: 'Mutation', refreshArtistMetaData: { __typename: 'RefreshArtistMetaDataError', message: string } | { __typename: 'RefreshArtistMetaDataSuccess', artist: (
      { __typename?: 'Artist', id: string }
      & { ' $fragmentRefs'?: { 'ArtistPanel_ArtistFragment': ArtistPanel_ArtistFragment } }
    ) } };

export type ArtistServiceConnections_ArtistFragment = { __typename?: 'Artist', id: string, connectedExternalServices: Array<{ __typename?: 'ArtistConnectedExternalService', isConnected: boolean, artistPageUrl: string, externalService: { __typename?: 'ExternalService', id: string, name: string } }> } & { ' $fragmentName'?: 'ArtistServiceConnections_ArtistFragment' };

export type ArtistSingleListQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistSingleListQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: { __typename?: 'Artist', id: string, singles: Array<(
        { __typename?: 'Release', id: string, firstReleaseDate?: string | null }
        & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
      )> } | null } };

export type TopArtistTrackItem_ArtistTopTrackFragment = { __typename?: 'ArtistTopTrack', title: string, releaseTitle?: string | null, playCount?: any | null, coverArtUrl?: string | null, track?: (
    { __typename?: 'Track', id: string, trackLength?: number | null, trackNumber: number, isMissing: boolean, release: { __typename?: 'Release', id: string, folderName: string, artist: { __typename?: 'Artist', id: string, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null } } }
    & { ' $fragmentRefs'?: { 'MusicPlayerTrackFactory_TrackFragment': MusicPlayerTrackFactory_TrackFragment;'AlbumTrackTag_TrackFragment': AlbumTrackTag_TrackFragment } }
  ) | null } & { ' $fragmentName'?: 'TopArtistTrackItem_ArtistTopTrackFragment' };

export type TopArtistTracksQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type TopArtistTracksQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: { __typename?: 'Artist', id: string, topTracks: Array<(
        { __typename?: 'ArtistTopTrack' }
        & { ' $fragmentRefs'?: { 'TopArtistTrackItem_ArtistTopTrackFragment': TopArtistTrackItem_ArtistTopTrackFragment } }
      )> } | null } };

export type ArtistTopTracksForQueueQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistTopTracksForQueueQuery = { __typename?: 'Query', serverLibrary: { __typename?: 'ServerLibrarySearchRoot', artistById?: { __typename?: 'Artist', id: string, topTracks: Array<{ __typename?: 'ArtistTopTrack', title: string, coverArtUrl?: string | null, track?: { __typename?: 'Track', trackNumber: number, trackLength?: number | null, release: { __typename?: 'Release', folderName: string, artist: { __typename?: 'Artist', id: string, name: string } } } | null }> } | null } };

export type FixArtistMatch_SearchMbArtistsQueryVariables = Exact<{
  name: Scalars['String']['input'];
  limit: Scalars['Int']['input'];
  offset: Scalars['Int']['input'];
}>;


export type FixArtistMatch_SearchMbArtistsQuery = { __typename?: 'Query', musicBrainz: { __typename?: 'MusicBrainzSearchRoot', artist: { __typename?: 'MusicBrainzArtistSearchRoot', searchByName: Array<{ __typename?: 'MbArtist', id: string, name: string, country?: string | null, type?: string | null }> } } };

export type FixArtistMatch_SetMbMutationVariables = Exact<{
  input: SetArtistMusicBrainzMatchInput;
}>;


export type FixArtistMatch_SetMbMutation = { __typename?: 'Mutation', setArtistMusicBrainzMatch: { __typename: 'SetArtistMusicBrainzMatchError', message: string } | { __typename: 'SetArtistMusicBrainzMatchSuccess', artist: { __typename?: 'Artist', id: string } } };

export type FixArtistMatch_SetSpotifyMutationVariables = Exact<{
  input: SetArtistSpotifyMatchInput;
}>;


export type FixArtistMatch_SetSpotifyMutation = { __typename?: 'Mutation', setArtistSpotifyMatch: { __typename: 'SetArtistSpotifyMatchError', message: string } | { __typename: 'SetArtistSpotifyMatchSuccess', artist: { __typename?: 'Artist', id: string } } };

export type FixArtistMatch_SearchSpotifyQueryVariables = Exact<{
  name: Scalars['String']['input'];
  limit: Scalars['Int']['input'];
  offset: Scalars['Int']['input'];
}>;


export type FixArtistMatch_SearchSpotifyQuery = { __typename?: 'Query', spotify: { __typename?: 'SpotifyArtistSearchRoot', searchByName: Array<{ __typename?: 'SpotifyArtist', id: string, name: string, images: Array<{ __typename?: 'SpotifyImage', url: string }> }> } };

export type SignInMutationVariables = Exact<{
  username: Scalars['String']['input'];
  password: Scalars['String']['input'];
}>;


export type SignInMutation = { __typename?: 'Mutation', signIn: { __typename: 'SignInError', message: string } | { __typename: 'SignInSuccess', user: { __typename?: 'User', id: string, username: string } } };

export type SignOutMutationVariables = Exact<{ [key: string]: never; }>;


export type SignOutMutation = { __typename?: 'Mutation', signOut: { __typename: 'SignOutError', message: string } | { __typename: 'SignOutSuccess', success: boolean } };

export type DownloadOverviewQueryQueryVariables = Exact<{ [key: string]: never; }>;


export type DownloadOverviewQueryQuery = { __typename?: 'Query', downloads: { __typename?: 'DownloadsSearchRoot', downloadQueue: { __typename?: 'DownloadQueueState', queueLength: number, items: Array<{ __typename?: 'DownloadQueueItem', artistId: string, releaseFolderName: string }> }, currentDownload?: { __typename?: 'DownloadProgress', artistId: string, releaseFolderName: string, status: DownloadStatus, totalTracks: number, completedTracks: number, errorMessage?: string | null } | null } };

export type DownloadQueueUpdatedSubSubscriptionVariables = Exact<{ [key: string]: never; }>;


export type DownloadQueueUpdatedSubSubscription = { __typename?: 'Subscription', downloadQueueUpdated: { __typename?: 'DownloadQueueState', queueLength: number, items: Array<{ __typename?: 'DownloadQueueItem', artistId: string, releaseFolderName: string }> } };

export type CurrentDownloadUpdatedSubSubscriptionVariables = Exact<{ [key: string]: never; }>;


export type CurrentDownloadUpdatedSubSubscription = { __typename?: 'Subscription', currentDownloadUpdated?: { __typename?: 'DownloadProgress', artistId: string, releaseFolderName: string, status: DownloadStatus, totalTracks: number, completedTracks: number, errorMessage?: string | null } | null };

export type ReleaseDownloadButton_ReleaseFragment = { __typename?: 'Release', id: string, isFullyMissing: boolean, folderName: string, downloadStatus: ReleaseDownloadStatus, artist: { __typename?: 'Artist', id: string } } & { ' $fragmentName'?: 'ReleaseDownloadButton_ReleaseFragment' };

export type AlbumPanel_StartDownloadReleaseMutationVariables = Exact<{
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
}>;


export type AlbumPanel_StartDownloadReleaseMutation = { __typename?: 'Mutation', startDownloadRelease: { __typename: 'StartDownloadReleaseAccepted', artistId: string, releaseFolderName: string } | { __typename: 'StartDownloadReleaseSuccess', release: { __typename?: 'Release', id: string } } | { __typename: 'StartDownloadReleaseUnknownError' } };

export type ReleaseDownloadButtonSubscriptionVariables = Exact<{
  artistId: Scalars['String']['input'];
  releaseFolderName: Scalars['String']['input'];
}>;


export type ReleaseDownloadButtonSubscription = { __typename?: 'Subscription', libraryReleaseDownloadStatusUpdated: { __typename?: 'LibraryReleaseDownloadStatusUpdate', release: { __typename?: 'Release', id: string, downloadStatus: ReleaseDownloadStatus, isFullyMissing: boolean } } };

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

export type MusicPlayerTrackFactory_TrackFragment = { __typename?: 'Track', id: string, title: string, trackLength?: number | null, trackNumber: number, media?: { __typename?: 'TrackMedia', id: string, audioQualityLabel: string } | null, release: { __typename?: 'Release', id: string, folderName: string, coverArtUrl: string, artist: { __typename?: 'Artist', id: string, name: string } } } & { ' $fragmentName'?: 'MusicPlayerTrackFactory_TrackFragment' };

export type CreatePlaylistMutationVariables = Exact<{ [key: string]: never; }>;


export type CreatePlaylistMutation = { __typename?: 'Mutation', createPlaylist: { __typename: 'CreatePlaylistSuccess', playlist: { __typename?: 'Playlist', id: string, name?: string | null, createdAt: any } } };

export type MissingArtistsInPlaylistBox_PlaylistFragment = { __typename?: 'Playlist', id: string, items: Array<{ __typename?: 'PlaylistItem', id: string, artist?: { __typename?: 'Artist', id: string } | null }> } & { ' $fragmentName'?: 'MissingArtistsInPlaylistBox_PlaylistFragment' };

export type EnqueueMissingArtistsFromPlaylistMutationVariables = Exact<{
  playlistId: Scalars['ID']['input'];
}>;


export type EnqueueMissingArtistsFromPlaylistMutation = { __typename?: 'Mutation', enqueueMissingArtistsFromPlaylist: boolean };

export type PlaylistHeader_PlaylistFragment = { __typename?: 'Playlist', id: string, name?: string | null, items: Array<{ __typename?: 'PlaylistItem', id: string, coverImageUrl?: string | null }> } & { ' $fragmentName'?: 'PlaylistHeader_PlaylistFragment' };

export type PlaylistPanel_PlaylistFragment = (
  { __typename?: 'Playlist', id: string, name?: string | null, items: Array<(
    { __typename?: 'PlaylistItem', id: string }
    & { ' $fragmentRefs'?: { 'PlaylistTrackItem_PlaylistItemFragment': PlaylistTrackItem_PlaylistItemFragment } }
  )> }
  & { ' $fragmentRefs'?: { 'PlaylistHeader_PlaylistFragment': PlaylistHeader_PlaylistFragment;'MissingArtistsInPlaylistBox_PlaylistFragment': MissingArtistsInPlaylistBox_PlaylistFragment } }
) & { ' $fragmentName'?: 'PlaylistPanel_PlaylistFragment' };

export type PlaylistTrackItem_PlaylistItemFragment = { __typename?: 'PlaylistItem', id: string, title?: string | null, artistName?: string | null, coverImageUrl?: string | null, trackLengthMs?: number | null, artist?: { __typename?: 'Artist', id: string, name: string } | null, track?: (
    { __typename?: 'Track', trackLength?: number | null, trackNumber: number, title: string, media?: { __typename?: 'TrackMedia', id: string, audioQualityLabel: string } | null, release: { __typename?: 'Release', id: string, folderName: string, coverArtUrl: string, artist: { __typename?: 'Artist', id: string, name: string, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null } } }
    & { ' $fragmentRefs'?: { 'TrackCreditLinks_TrackFragment': TrackCreditLinks_TrackFragment;'AlbumTrackTag_TrackFragment': AlbumTrackTag_TrackFragment } }
  ) | null } & { ' $fragmentName'?: 'PlaylistTrackItem_PlaylistItemFragment' };

export type RemoveItemFromPlaylistMutationVariables = Exact<{
  playlistId: Scalars['ID']['input'];
  playlistItemId: Scalars['ID']['input'];
}>;


export type RemoveItemFromPlaylistMutation = { __typename?: 'Mutation', removeItemFromPlaylist: { __typename: 'RemoveItemFromPlaylistError', message: string } | { __typename: 'RemoveItemFromPlaylistSuccess', playlist: { __typename?: 'Playlist', id: string } } };

export type MovePlaylistItemMutationVariables = Exact<{
  newIndex: Scalars['Int']['input'];
  playlistId: Scalars['ID']['input'];
  playlistItemId: Scalars['ID']['input'];
}>;


export type MovePlaylistItemMutation = { __typename?: 'Mutation', movePlaylistItem: { __typename: 'MovePlaylistItemError', message: string } | { __typename: 'MovePlaylistItemSuccess', playlist: { __typename?: 'Playlist', id: string } } };

export type FixArtist_MbArtistSearchQueryVariables = Exact<{
  artistName: Scalars['String']['input'];
  trackName: Scalars['String']['input'];
}>;


export type FixArtist_MbArtistSearchQuery = { __typename?: 'Query', musicBrainz: { __typename?: 'MusicBrainzSearchRoot', artist: { __typename?: 'MusicBrainzArtistSearchRoot', searchByName: Array<{ __typename?: 'MbArtist', id: string, name: string, disambiguation?: string | null, images?: { __typename?: 'MbArtistImages', artistThumb?: string | null } | null }> }, recording: { __typename?: 'MusicBrainzRecordingSearchRoot', searchByName: Array<{ __typename?: 'MbRecording', id: string, title: string, artists: Array<{ __typename?: 'MbArtist', id: string, name: string }> }> } } };

export type SetPlaylistItemArtistMusicBrainzMatchMutationVariables = Exact<{
  input: SetPlaylistItemArtistMusicBrainzMatchInput;
}>;


export type SetPlaylistItemArtistMusicBrainzMatchMutation = { __typename?: 'Mutation', setPlaylistItemArtistMusicBrainzMatch: { __typename: 'SetPlaylistItemArtistMusicBrainzMatchError', message: string } | { __typename: 'SetPlaylistItemArtistMusicBrainzMatchNotFound', message: string } | { __typename: 'SetPlaylistItemArtistMusicBrainzMatchSuccess', playlistItem: { __typename?: 'PlaylistItem', id: string } } };

export type PlaylistListQueryVariables = Exact<{ [key: string]: never; }>;


export type PlaylistListQuery = { __typename?: 'Query', viewer?: { __typename?: 'User', id: string, playlists: Array<{ __typename?: 'Playlist', id: string, name?: string | null, createdAt: any }> } | null };

export type RenamePlaylistMutationVariables = Exact<{
  playlistId: Scalars['ID']['input'];
  newPlaylistName: Scalars['String']['input'];
}>;


export type RenamePlaylistMutation = { __typename?: 'Mutation', renamePlaylist: { __typename: 'RenamePlaylistNoWriteAccess' } | { __typename: 'RenamePlaylistSuccess', playlist: { __typename?: 'Playlist', id: string, name?: string | null, createdAt: any } } };

export type DeletePlaylistMutationVariables = Exact<{
  playlistId: Scalars['ID']['input'];
}>;


export type DeletePlaylistMutation = { __typename?: 'Mutation', deletePlaylist: { __typename: 'DeletePlaylistNoWriteAccess' } | { __typename: 'DeletePlaylistSuccess', deletedPlaylistId: any } };

export type TopArtistRecommendationsQueryVariables = Exact<{ [key: string]: never; }>;


export type TopArtistRecommendationsQuery = { __typename?: 'Query', recommendations: { __typename?: 'RecommendationsSearchRoot', topArtists: Array<(
      { __typename?: 'LastFmArtist', id: string }
      & { ' $fragmentRefs'?: { 'TopArtistCard_ArtistFragment': TopArtistCard_ArtistFragment } }
    )> } };

export type TopTrackCard_LastFmTrackFragment = { __typename?: 'LastFmTrack', id: string, playCount?: any | null, name: string, artist: { __typename?: 'LastFmArtist', id: string, musicBrainzArtist?: { __typename?: 'MbArtist', id: string, name: string, images?: { __typename?: 'MbArtistImages', artistThumb?: string | null } | null } | null }, album?: { __typename?: 'LastFmAlbum', id: string, imageUrl?: string | null } | null, images?: { __typename?: 'ArtistImages', thumbs: Array<string> } | null } & { ' $fragmentName'?: 'TopTrackCard_LastFmTrackFragment' };

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
        { __typename?: 'MbArtist', id: string, name: string, images?: { __typename?: 'MbArtistImages', artistThumb?: string | null } | null }
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

export type ArtistImportQueuePanel_QueryQueryVariables = Exact<{ [key: string]: never; }>;


export type ArtistImportQueuePanel_QueryQuery = { __typename?: 'Query', artistImport: { __typename?: 'ArtistImportSearchRoot', artistImportQueue: { __typename?: 'ArtistImportQueueState', queueLength: number, items: Array<{ __typename?: 'ArtistImportQueueItem', artistName: string, songTitle?: string | null }> }, currentArtistImport?: { __typename?: 'ArtistImportProgress', artistName: string, songTitle?: string | null, totalReleases: number, completedReleases: number, errorMessage?: string | null, statusInfo: { __typename?: 'ArtistImportStatusInfo', id: ArtistImportStatus, text: string } } | null } };

export type ArtistImportQueueUpdatedSubSubscriptionVariables = Exact<{ [key: string]: never; }>;


export type ArtistImportQueueUpdatedSubSubscription = { __typename?: 'Subscription', artistImportQueueUpdated: { __typename?: 'ArtistImportQueueState', queueLength: number, items: Array<{ __typename?: 'ArtistImportQueueItem', artistName: string, songTitle?: string | null }> } };

export type CurrentArtistImportUpdatedSubSubscriptionVariables = Exact<{ [key: string]: never; }>;


export type CurrentArtistImportUpdatedSubSubscription = { __typename?: 'Subscription', currentArtistImportUpdated: { __typename?: 'ArtistImportProgress', artistName: string, songTitle?: string | null, totalReleases: number, completedReleases: number, errorMessage?: string | null, statusInfo: { __typename?: 'ArtistImportStatusInfo', id: ArtistImportStatus, text: string } } };

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

export type ImportSpotifyPlaylistMutationVariables = Exact<{
  playlistId: Scalars['String']['input'];
  userId: Scalars['UUID']['input'];
}>;


export type ImportSpotifyPlaylistMutation = { __typename?: 'Mutation', importSpotifyPlaylist: { __typename: 'ImportSpotifyPlaylistError', message: string } | { __typename: 'ImportSpotifyPlaylistSuccess', playlist: { __typename?: 'Playlist', id: string, name?: string | null } } };

export type SpotifyPlaylistsList_SpotifyPlaylistFragment = { __typename?: 'SpotifyPlaylist', id: string, description?: string | null, name: string, coverImageUrl?: string | null } & { ' $fragmentName'?: 'SpotifyPlaylistsList_SpotifyPlaylistFragment' };

export type UserPlaylistsLoader_QueryQueryVariables = Exact<{
  spotifyUsername: Scalars['String']['input'];
}>;


export type UserPlaylistsLoader_QueryQuery = { __typename?: 'Query', playlist: { __typename?: 'PlaylistSearchRoot', importPlaylists: { __typename?: 'ImportPlaylistSearchRoot', spotify: { __typename?: 'SpotifyPlaylistSearchRoot', spotifyPlaylistsForUser: Array<(
          { __typename?: 'SpotifyPlaylist', id: string }
          & { ' $fragmentRefs'?: { 'SpotifyPlaylistsList_SpotifyPlaylistFragment': SpotifyPlaylistsList_SpotifyPlaylistFragment } }
        )> } } } };

export type SpotifyPlaylistPanel_SpotifyPlaylistFragment = { __typename?: 'SpotifyPlaylist', id: string, name: string, description?: string | null, coverImageUrl?: string | null, totalTracks?: number | null, tracks: Array<{ __typename?: 'SpotifyTrack', id: string, title: string, durationMs?: number | null, artistNames: Array<string>, albumCoverImageUrl?: string | null, previewUrl?: string | null }> } & { ' $fragmentName'?: 'SpotifyPlaylistPanel_SpotifyPlaylistFragment' };

export type ViewerIdForSpotifyImportQueryVariables = Exact<{ [key: string]: never; }>;


export type ViewerIdForSpotifyImportQuery = { __typename?: 'Query', viewer?: { __typename?: 'User', id: string } | null };

export type ImportArtistsFromSpotifyPlaylistMutationVariables = Exact<{
  playlistId: Scalars['String']['input'];
}>;


export type ImportArtistsFromSpotifyPlaylistMutation = { __typename?: 'Mutation', importArtistsFromSpotifyPlaylist: { __typename: 'ImportArtistsFromSpotifyPlaylistError', message: string } | { __typename: 'ImportArtistsFromSpotifyPlaylistSuccess', totalArtists: number, importedArtists: number, failedArtists: Array<string>, artists: Array<{ __typename?: 'Artist', id: string, name: string }> } };

export type UserProfilePanel_UserFragment = { __typename?: 'User', id: string, username: string, createdAt: any, updatedAt: any, likedSongs: Array<{ __typename?: 'LikedSong', id: string }> } & { ' $fragmentName'?: 'UserProfilePanel_UserFragment' };

export type UserProfileWidgetQueryVariables = Exact<{ [key: string]: never; }>;


export type UserProfileWidgetQuery = { __typename?: 'Query', viewer?: { __typename?: 'User', id: string, username: string } | null };

export const PlayAlbumButton_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlayAlbumButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}}]}}]}}]} as unknown as DocumentNode<PlayAlbumButton_ReleaseFragment, unknown>;
export const ReleaseDownloadButton_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"downloadStatus"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<ReleaseDownloadButton_ReleaseFragment, unknown>;
export const AlbumHeader_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}}]}}]}}]} as unknown as DocumentNode<AlbumHeader_ReleaseFragment, unknown>;
export const TrackCreditLinks_TrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TrackCreditLinks_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]} as unknown as DocumentNode<TrackCreditLinks_TrackFragment, unknown>;
export const MusicPlayerTrackFactory_TrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}}]}}]} as unknown as DocumentNode<MusicPlayerTrackFactory_TrackFragment, unknown>;
export const AlbumTrackTag_TrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}}]} as unknown as DocumentNode<AlbumTrackTag_TrackFragment, unknown>;
export const AlbumTrackList_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioUrl"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TrackCreditLinks_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TrackCreditLinks_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}}]} as unknown as DocumentNode<AlbumTrackList_ReleaseFragment, unknown>;
export const AlbumPanel_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlayAlbumButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_Release"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TrackCreditLinks_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlayAlbumButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"downloadStatus"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioUrl"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TrackCreditLinks_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}}]}}]}}]}}]} as unknown as DocumentNode<AlbumPanel_ReleaseFragment, unknown>;
export const MbReleaseGroupCard_MbReleaseGroupFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MbReleaseGroupCard_MbReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]} as unknown as DocumentNode<MbReleaseGroupCard_MbReleaseGroupFragment, unknown>;
export const AlbumCard_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]} as unknown as DocumentNode<AlbumCard_ReleaseFragment, unknown>;
export const AlbumList_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]} as unknown as DocumentNode<AlbumList_ReleaseFragment, unknown>;
export const MbArtistCard_MbArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MbArtistCard_MbArtist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]} as unknown as DocumentNode<MbArtistCard_MbArtistFragment, unknown>;
export const TopArtistCard_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]} as unknown as DocumentNode<TopArtistCard_ArtistFragment, unknown>;
export const ArtistCard_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]} as unknown as DocumentNode<ArtistCard_ArtistFragment, unknown>;
export const ArtistList_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistList_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistCard_Artist"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]} as unknown as DocumentNode<ArtistList_ArtistFragment, unknown>;
export const ArtistNotInLibraryTopTracks_LastFmArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNotInLibraryTopTracks_LastFmArtist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistNotInLibraryTopTracks_LastFmArtistFragment, unknown>;
export const ArtistNotInLibraryPanel_MbArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNotInLibraryPanel_MbArtist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistBackground"}}]}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"lastFmArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistNotInLibraryTopTracks_LastFmArtist"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNotInLibraryTopTracks_LastFmArtist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistNotInLibraryPanel_MbArtistFragment, unknown>;
export const ArtistImportStatusInfo_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistImportStatusInfo_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]} as unknown as DocumentNode<ArtistImportStatusInfo_ArtistFragment, unknown>;
export const ArtistNumReleasesAvailableIndicator_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNumReleasesAvailableIndicator_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}}]}}]} as unknown as DocumentNode<ArtistNumReleasesAvailableIndicator_ArtistFragment, unknown>;
export const ArtistServiceConnections_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistServiceConnections_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"connectedExternalServices"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"isConnected"}},{"kind":"Field","name":{"kind":"Name","value":"artistPageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"externalService"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistServiceConnections_ArtistFragment, unknown>;
export const ArtistDownloadAllReleasesButton_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistDownloadAllReleasesButton_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]} as unknown as DocumentNode<ArtistDownloadAllReleasesButton_ArtistFragment, unknown>;
export const ArtistPanel_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistPanel_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistImportStatusInfo_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistNumReleasesAvailableIndicator_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistServiceConnections_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistDownloadAllReleasesButton_Artist"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"connectedExternalServices"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"isConnected"}},{"kind":"Field","name":{"kind":"Name","value":"externalService"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"backgrounds"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistImportStatusInfo_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNumReleasesAvailableIndicator_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistServiceConnections_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"connectedExternalServices"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"isConnected"}},{"kind":"Field","name":{"kind":"Name","value":"artistPageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"externalService"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistDownloadAllReleasesButton_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistPanel_ArtistFragment, unknown>;
export const TopArtistTrackItem_ArtistTopTrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistTrackItem_ArtistTopTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistTopTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"releaseTitle"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"track"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}}]} as unknown as DocumentNode<TopArtistTrackItem_ArtistTopTrackFragment, unknown>;
export const LikedSongRow_LikedSongFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]} as unknown as DocumentNode<LikedSongRow_LikedSongFragment, unknown>;
export const LikedSongsList_UserFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongsList_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongRow_LikedSong"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]} as unknown as DocumentNode<LikedSongsList_UserFragment, unknown>;
export const PlaylistHeader_PlaylistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlaylistHeader_Playlist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Playlist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}}]}}]}}]} as unknown as DocumentNode<PlaylistHeader_PlaylistFragment, unknown>;
export const MissingArtistsInPlaylistBox_PlaylistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MissingArtistsInPlaylistBox_Playlist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Playlist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]} as unknown as DocumentNode<MissingArtistsInPlaylistBox_PlaylistFragment, unknown>;
export const PlaylistTrackItem_PlaylistItemFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlaylistTrackItem_PlaylistItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PlaylistItem"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"trackLengthMs"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"track"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"TrackCreditLinks_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TrackCreditLinks_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}}]} as unknown as DocumentNode<PlaylistTrackItem_PlaylistItemFragment, unknown>;
export const PlaylistPanel_PlaylistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlaylistPanel_Playlist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Playlist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlaylistHeader_Playlist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MissingArtistsInPlaylistBox_Playlist"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlaylistTrackItem_PlaylistItem"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TrackCreditLinks_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlaylistHeader_Playlist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Playlist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MissingArtistsInPlaylistBox_Playlist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Playlist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlaylistTrackItem_PlaylistItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PlaylistItem"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"trackLengthMs"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"track"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"TrackCreditLinks_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<PlaylistPanel_PlaylistFragment, unknown>;
export const TopTrackCard_LastFmTrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopTrackCard_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"album"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"imageUrl"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]} as unknown as DocumentNode<TopTrackCard_LastFmTrackFragment, unknown>;
export const DownloadPathForm_ServerSettingsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadPathForm_ServerSettings"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServerSettings"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"downloadPath"}}]}}]} as unknown as DocumentNode<DownloadPathForm_ServerSettingsFragment, unknown>;
export const LibraryPathForm_ServerSettingsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LibraryPathForm_ServerSettings"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServerSettings"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"libraryPath"}}]}}]} as unknown as DocumentNode<LibraryPathForm_ServerSettingsFragment, unknown>;
export const DirectoryTreeItem_FileSystemEntryFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"FileSystemEntry"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"Field","name":{"kind":"Name","value":"hasChildren"}},{"kind":"Field","name":{"kind":"Name","value":"isAccessible"}}]}}]} as unknown as DocumentNode<DirectoryTreeItem_FileSystemEntryFragment, unknown>;
export const SoulSeekNetworkStatus_SoulSeekStatusFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatus_SoulSeekStatusFragment, unknown>;
export const SpotifyPlaylistsList_SpotifyPlaylistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SpotifyPlaylistsList_SpotifyPlaylist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SpotifyPlaylist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}}]}}]} as unknown as DocumentNode<SpotifyPlaylistsList_SpotifyPlaylistFragment, unknown>;
export const SpotifyPlaylistPanel_SpotifyPlaylistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SpotifyPlaylistPanel_SpotifyPlaylist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SpotifyPlaylist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"totalTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"durationMs"}},{"kind":"Field","name":{"kind":"Name","value":"artistNames"}},{"kind":"Field","name":{"kind":"Name","value":"albumCoverImageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"previewUrl"}}]}}]}}]} as unknown as DocumentNode<SpotifyPlaylistPanel_SpotifyPlaylistFragment, unknown>;
export const UserProfilePanel_UserFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserProfilePanel_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"updatedAt"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<UserProfilePanel_UserFragment, unknown>;
export const BootstrapDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"Bootstrap"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"areThereAnyUsers"}},{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<BootstrapQuery, BootstrapQueryVariables>;
export const AlbumListQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"AlbumListQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"allReleases"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumList_Release"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]} as unknown as DocumentNode<AlbumListQueryQuery, AlbumListQueryQueryVariables>;
export const AlbumQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"AlbumQuery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"releaseByFolderName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"releaseFolderName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumPanel_Release"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlayAlbumButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"downloadStatus"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TrackCreditLinks_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioUrl"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TrackCreditLinks_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlayAlbumButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_Release"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<AlbumQueryQuery, AlbumQueryQueryVariables>;
export const ArtistListQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistListQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"allArtists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistList_Artist"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistList_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistCard_Artist"}}]}}]} as unknown as DocumentNode<ArtistListQueryQuery, ArtistListQueryQueryVariables>;
export const ArtistQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistQuery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistPanel_Artist"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistImportStatusInfo_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNumReleasesAvailableIndicator_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistServiceConnections_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"connectedExternalServices"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"isConnected"}},{"kind":"Field","name":{"kind":"Name","value":"artistPageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"externalService"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistDownloadAllReleasesButton_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistPanel_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistImportStatusInfo_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistNumReleasesAvailableIndicator_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistServiceConnections_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistDownloadAllReleasesButton_Artist"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"connectedExternalServices"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"isConnected"}},{"kind":"Field","name":{"kind":"Name","value":"externalService"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"backgrounds"}}]}}]}}]} as unknown as DocumentNode<ArtistQueryQuery, ArtistQueryQueryVariables>;
export const LikedSongsQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"LikedSongsQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongsList_User"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongsList_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongRow_LikedSong"}}]}}]}}]} as unknown as DocumentNode<LikedSongsQueryQuery, LikedSongsQueryQueryVariables>;
export const MbArtistQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"MbArtistQuery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"mbArtistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"mbArtistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistNotInLibraryPanel_MbArtist"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNotInLibraryTopTracks_LastFmArtist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNotInLibraryPanel_MbArtist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistBackground"}}]}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"lastFmArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistNotInLibraryTopTracks_LastFmArtist"}}]}}]}}]} as unknown as DocumentNode<MbArtistQueryQuery, MbArtistQueryQueryVariables>;
export const PlaylistQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"PlaylistQuery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"playlist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlaylistPanel_Playlist"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlaylistHeader_Playlist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Playlist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MissingArtistsInPlaylistBox_Playlist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Playlist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TrackCreditLinks_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlaylistTrackItem_PlaylistItem"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"PlaylistItem"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"trackLengthMs"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"track"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"TrackCreditLinks_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlaylistPanel_Playlist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Playlist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlaylistHeader_Playlist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MissingArtistsInPlaylistBox_Playlist"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlaylistTrackItem_PlaylistItem"}}]}}]}}]} as unknown as DocumentNode<PlaylistQueryQuery, PlaylistQueryQueryVariables>;
export const SettingsPageDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SettingsPage"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverSettings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"LibraryPathForm_ServerSettings"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadPathForm_ServerSettings"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LibraryPathForm_ServerSettings"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServerSettings"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"libraryPath"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadPathForm_ServerSettings"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServerSettings"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"downloadPath"}}]}}]} as unknown as DocumentNode<SettingsPageQuery, SettingsPageQueryVariables>;
export const SpotifyPlaylistDetailsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SpotifyPlaylistDetails"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"playlist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"importPlaylists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"spotify"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","alias":{"kind":"Name","value":"byId"},"name":{"kind":"Name","value":"spotifyPlaylistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"SpotifyPlaylistPanel_SpotifyPlaylist"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SpotifyPlaylistPanel_SpotifyPlaylist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SpotifyPlaylist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"totalTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"durationMs"}},{"kind":"Field","name":{"kind":"Name","value":"artistNames"}},{"kind":"Field","name":{"kind":"Name","value":"albumCoverImageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"previewUrl"}}]}}]}}]} as unknown as DocumentNode<SpotifyPlaylistDetailsQuery, SpotifyPlaylistDetailsQueryVariables>;
export const ProfilePageDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ProfilePage"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"UserProfilePanel_User"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserProfilePanel_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"updatedAt"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<ProfilePageQuery, ProfilePageQueryVariables>;
export const ImportArtistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ImportArtist"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"musicBrainzArtistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"importArtist"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"musicBrainzArtistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"musicBrainzArtistId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ImportArtistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ImportArtistError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<ImportArtistMutation, ImportArtistMutationVariables>;
export const AlbumPanelUpdatesDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"AlbumPanelUpdates"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"libraryCacheTracksInReleaseUpdated"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}},{"kind":"Argument","name":{"kind":"Name","value":"releaseFolderName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"track"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}}]}}]}}]} as unknown as DocumentNode<AlbumPanelUpdatesSubscription, AlbumPanelUpdatesSubscriptionVariables>;
export const AlbumPanelReleaseMetadataUpdatedDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"AlbumPanelReleaseMetadataUpdated"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"libraryReleaseMetadataUpdated"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}},{"kind":"Argument","name":{"kind":"Name","value":"releaseFolderName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumPanel_Release"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlayAlbumButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"downloadStatus"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TrackCreditLinks_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioUrl"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TrackCreditLinks_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlayAlbumButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_Release"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<AlbumPanelReleaseMetadataUpdatedSubscription, AlbumPanelReleaseMetadataUpdatedSubscriptionVariables>;
export const RefreshReleaseDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"RefreshRelease"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"RefreshReleaseInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"refreshRelease"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"RefreshReleaseSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumPanel_Release"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"RefreshReleaseError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlayAlbumButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"downloadStatus"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TrackCreditLinks_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioUrl"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TrackCreditLinks_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlayAlbumButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_Release"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<RefreshReleaseMutation, RefreshReleaseMutationVariables>;
export const DeleteReleaseAudioDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeleteReleaseAudio"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"DeleteReleaseAudioInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deleteReleaseAudio"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DeleteReleaseAudioSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumPanel_Release"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DeleteReleaseAudioError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlayAlbumButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"downloadStatus"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TrackCreditLinks_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioUrl"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TrackCreditLinks_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlayAlbumButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_Release"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<DeleteReleaseAudioMutation, DeleteReleaseAudioMutationVariables>;
export const ScanReleaseFolderForMediaDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ScanReleaseFolderForMedia"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ScanReleaseFolderForMediaInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"scanReleaseFolderForMedia"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ScanReleaseFolderForMediaSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumPanel_Release"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ScanReleaseFolderForMediaError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PlayAlbumButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"downloadStatus"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"type"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TrackCreditLinks_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioUrl"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TrackCreditLinks_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mbArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PlayAlbumButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ReleaseDownloadButton_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_Release"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<ScanReleaseFolderForMediaMutation, ScanReleaseFolderForMediaMutationVariables>;
export const FixMatch_ReleasesWithScoresDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"FixMatch_ReleasesWithScores"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"releaseGroupId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releasesWithScores"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"releaseGroupId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"releaseGroupId"}}},{"kind":"Argument","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}},{"kind":"Argument","name":{"kind":"Name","value":"releaseFolderName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"score"}},{"kind":"Field","name":{"kind":"Name","value":"reasons"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"country"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"tracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<FixMatch_ReleasesWithScoresQuery, FixMatch_ReleasesWithScoresQueryVariables>;
export const FixMatch_SetReleaseMatchOverrideDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"FixMatch_SetReleaseMatchOverride"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"SetReleaseMatchOverrideInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"setReleaseMatchOverride"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SetReleaseMatchOverrideSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SetReleaseMatchOverrideError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<FixMatch_SetReleaseMatchOverrideMutation, FixMatch_SetReleaseMatchOverrideMutationVariables>;
export const FixMatch_SetReleaseGroupDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"FixMatch_SetReleaseGroup"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"SetReleaseGroupInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"setReleaseGroup"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SetReleaseGroupSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SetReleaseGroupError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<FixMatch_SetReleaseGroupMutation, FixMatch_SetReleaseGroupMutationVariables>;
export const FixMatch_ReleaseConnectionsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"FixMatch_ReleaseConnections"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releaseForArtistByFolderName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}},{"kind":"Argument","name":{"kind":"Name","value":"releaseFolderName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzReleaseGroupId"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]} as unknown as DocumentNode<FixMatch_ReleaseConnectionsQuery, FixMatch_ReleaseConnectionsQueryVariables>;
export const FixMatch_SearchReleaseGroupsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"FixMatch_SearchReleaseGroups"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"name"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"offset"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByNameAndArtistName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"name"}}},{"kind":"Argument","name":{"kind":"Name","value":"artistName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistName"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}},{"kind":"Argument","name":{"kind":"Name","value":"offset"},"value":{"kind":"Variable","name":{"kind":"Name","value":"offset"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"primaryType"}},{"kind":"Field","name":{"kind":"Name","value":"secondaryTypes"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}}]}}]}}]}}]}}]} as unknown as DocumentNode<FixMatch_SearchReleaseGroupsQuery, FixMatch_SearchReleaseGroupsQueryVariables>;
export const ArtistAlbumListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistAlbumList"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistAlbumListQuery, ArtistAlbumListQueryVariables>;
export const ArtistDownloadAll_StartBulkDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ArtistDownloadAll_StartBulk"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"StartBulkDownloadForArtistInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"startBulkDownloadForArtist"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"StartBulkDownloadForArtistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"queuedCount"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"StartBulkDownloadForArtistError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistDownloadAll_StartBulkMutation, ArtistDownloadAll_StartBulkMutationVariables>;
export const ArtistEpListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistEpList"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistEpListQuery, ArtistEpListQueryVariables>;
export const ArtistPanel_CurrentImportDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistPanel_CurrentImport"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistImport"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"currentArtistImport"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"completedReleases"}},{"kind":"Field","name":{"kind":"Name","value":"totalReleases"}},{"kind":"Field","name":{"kind":"Name","value":"errorMessage"}},{"kind":"Field","name":{"kind":"Name","value":"artistName"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistPanel_CurrentImportQuery, ArtistPanel_CurrentImportQueryVariables>;
export const ArtistImportStatusInfo_SubDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"ArtistImportStatusInfo_Sub"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"currentArtistImportUpdated"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"completedReleases"}},{"kind":"Field","name":{"kind":"Name","value":"totalReleases"}},{"kind":"Field","name":{"kind":"Name","value":"errorMessage"}},{"kind":"Field","name":{"kind":"Name","value":"artistName"}}]}}]}}]} as unknown as DocumentNode<ArtistImportStatusInfo_SubSubscription, ArtistImportStatusInfo_SubSubscriptionVariables>;
export const PanelRefreshTopTracksDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"PanelRefreshTopTracks"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"RefreshArtistTopTracksInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"refreshArtistTopTracks"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"RefreshArtistTopTracksSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"TopArtistTrackItem_ArtistTopTrack"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistTrackItem_ArtistTopTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistTopTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"releaseTitle"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"track"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<PanelRefreshTopTracksMutation, PanelRefreshTopTracksMutationVariables>;
export const PanelRefreshArtistMetaDataDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"PanelRefreshArtistMetaData"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"refreshArtistMetaData"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"RefreshArtistMetaDataSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistPanel_Artist"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"RefreshArtistMetaDataError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistImportStatusInfo_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNumReleasesAvailableIndicator_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistServiceConnections_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"connectedExternalServices"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"isConnected"}},{"kind":"Field","name":{"kind":"Name","value":"artistPageUrl"}},{"kind":"Field","name":{"kind":"Name","value":"externalService"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistDownloadAllReleasesButton_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistPanel_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistImportStatusInfo_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistNumReleasesAvailableIndicator_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistServiceConnections_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistDownloadAllReleasesButton_Artist"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"connectedExternalServices"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"isConnected"}},{"kind":"Field","name":{"kind":"Name","value":"externalService"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"backgrounds"}}]}}]}}]} as unknown as DocumentNode<PanelRefreshArtistMetaDataMutation, PanelRefreshArtistMetaDataMutationVariables>;
export const ArtistSingleListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistSingleList"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistSingleListQuery, ArtistSingleListQueryVariables>;
export const TopArtistTracksDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"TopArtistTracks"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"TopArtistTrackItem_ArtistTopTrack"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"media"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"audioQualityLabel"}}]}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackTag_Track"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Track"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"mediaAvailabilityStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistTrackItem_ArtistTopTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistTopTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"releaseTitle"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"track"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MusicPlayerTrackFactory_Track"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackTag_Track"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"isMissing"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<TopArtistTracksQuery, TopArtistTracksQueryVariables>;
export const ArtistTopTracksForQueueDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistTopTracksForQueue"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"track"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"trackNumber"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<ArtistTopTracksForQueueQuery, ArtistTopTracksForQueueQueryVariables>;
export const FixArtistMatch_SearchMbArtistsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"FixArtistMatch_SearchMbArtists"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"name"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"offset"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"name"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}},{"kind":"Argument","name":{"kind":"Name","value":"offset"},"value":{"kind":"Variable","name":{"kind":"Name","value":"offset"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"country"}},{"kind":"Field","name":{"kind":"Name","value":"type"}}]}}]}}]}}]}}]} as unknown as DocumentNode<FixArtistMatch_SearchMbArtistsQuery, FixArtistMatch_SearchMbArtistsQueryVariables>;
export const FixArtistMatch_SetMbDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"FixArtistMatch_SetMb"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"SetArtistMusicBrainzMatchInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"setArtistMusicBrainzMatch"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SetArtistMusicBrainzMatchSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SetArtistMusicBrainzMatchError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<FixArtistMatch_SetMbMutation, FixArtistMatch_SetMbMutationVariables>;
export const FixArtistMatch_SetSpotifyDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"FixArtistMatch_SetSpotify"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"SetArtistSpotifyMatchInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"setArtistSpotifyMatch"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SetArtistSpotifyMatchSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SetArtistSpotifyMatchError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<FixArtistMatch_SetSpotifyMutation, FixArtistMatch_SetSpotifyMutationVariables>;
export const FixArtistMatch_SearchSpotifyDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"FixArtistMatch_SearchSpotify"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"name"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"offset"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"spotify"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"name"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}},{"kind":"Argument","name":{"kind":"Name","value":"offset"},"value":{"kind":"Variable","name":{"kind":"Name","value":"offset"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"url"}}]}}]}}]}}]}}]} as unknown as DocumentNode<FixArtistMatch_SearchSpotifyQuery, FixArtistMatch_SearchSpotifyQueryVariables>;
export const SignInDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SignIn"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"username"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"password"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"signIn"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"username"},"value":{"kind":"Variable","name":{"kind":"Name","value":"username"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"password"},"value":{"kind":"Variable","name":{"kind":"Name","value":"password"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SignInSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"user"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SignInError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<SignInMutation, SignInMutationVariables>;
export const SignOutDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SignOut"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"signOut"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"confirm"},"value":{"kind":"BooleanValue","value":true}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SignOutSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"success"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SignOutError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<SignOutMutation, SignOutMutationVariables>;
export const DownloadOverviewQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"DownloadOverviewQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"downloads"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"downloadQueue"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"queueLength"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistId"}},{"kind":"Field","name":{"kind":"Name","value":"releaseFolderName"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"currentDownload"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistId"}},{"kind":"Field","name":{"kind":"Name","value":"releaseFolderName"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"totalTracks"}},{"kind":"Field","name":{"kind":"Name","value":"completedTracks"}},{"kind":"Field","name":{"kind":"Name","value":"errorMessage"}}]}}]}}]}}]} as unknown as DocumentNode<DownloadOverviewQueryQuery, DownloadOverviewQueryQueryVariables>;
export const DownloadQueueUpdatedSubDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"DownloadQueueUpdatedSub"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"downloadQueueUpdated"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"queueLength"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistId"}},{"kind":"Field","name":{"kind":"Name","value":"releaseFolderName"}}]}}]}}]}}]} as unknown as DocumentNode<DownloadQueueUpdatedSubSubscription, DownloadQueueUpdatedSubSubscriptionVariables>;
export const CurrentDownloadUpdatedSubDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"CurrentDownloadUpdatedSub"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"currentDownloadUpdated"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistId"}},{"kind":"Field","name":{"kind":"Name","value":"releaseFolderName"}},{"kind":"Field","name":{"kind":"Name","value":"status"}},{"kind":"Field","name":{"kind":"Name","value":"totalTracks"}},{"kind":"Field","name":{"kind":"Name","value":"completedTracks"}},{"kind":"Field","name":{"kind":"Name","value":"errorMessage"}}]}}]}}]} as unknown as DocumentNode<CurrentDownloadUpdatedSubSubscription, CurrentDownloadUpdatedSubSubscriptionVariables>;
export const AlbumPanel_StartDownloadReleaseDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"AlbumPanel_StartDownloadRelease"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"startDownloadRelease"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"releaseFolderName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"StartDownloadReleaseSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"StartDownloadReleaseAccepted"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistId"}},{"kind":"Field","name":{"kind":"Name","value":"releaseFolderName"}}]}}]}}]}}]} as unknown as DocumentNode<AlbumPanel_StartDownloadReleaseMutation, AlbumPanel_StartDownloadReleaseMutationVariables>;
export const ReleaseDownloadButtonDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"ReleaseDownloadButton"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"libraryReleaseDownloadStatusUpdated"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}},{"kind":"Argument","name":{"kind":"Name","value":"releaseFolderName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"releaseFolderName"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"downloadStatus"}},{"kind":"Field","name":{"kind":"Name","value":"isFullyMissing"}}]}}]}}]}}]} as unknown as DocumentNode<ReleaseDownloadButtonSubscription, ReleaseDownloadButtonSubscriptionVariables>;
export const CreateUserDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreateUser"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"username"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"password"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"username"},"value":{"kind":"Variable","name":{"kind":"Name","value":"username"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"password"},"value":{"kind":"Variable","name":{"kind":"Name","value":"password"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"CreateUserSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"user"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"CreateUserError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<CreateUserMutation, CreateUserMutationVariables>;
export const CreatePlaylistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"CreatePlaylist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"createPlaylist"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"CreatePlaylistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"playlist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]}}]}}]} as unknown as DocumentNode<CreatePlaylistMutation, CreatePlaylistMutationVariables>;
export const EnqueueMissingArtistsFromPlaylistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"EnqueueMissingArtistsFromPlaylist"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"enqueueMissingArtistsFromPlaylist"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}}]}]}}]} as unknown as DocumentNode<EnqueueMissingArtistsFromPlaylistMutation, EnqueueMissingArtistsFromPlaylistMutationVariables>;
export const RemoveItemFromPlaylistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"RemoveItemFromPlaylist"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistItemId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"removeItemFromPlaylist"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"playlistItemId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistItemId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"RemoveItemFromPlaylistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"playlist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"RemoveItemFromPlaylistError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<RemoveItemFromPlaylistMutation, RemoveItemFromPlaylistMutationVariables>;
export const MovePlaylistItemDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"MovePlaylistItem"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"newIndex"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistItemId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"movePlaylistItem"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"newIndex"},"value":{"kind":"Variable","name":{"kind":"Name","value":"newIndex"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"playlistItemId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistItemId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MovePlaylistItemSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"playlist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MovePlaylistItemError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<MovePlaylistItemMutation, MovePlaylistItemMutationVariables>;
export const FixArtist_MbArtistSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"FixArtist_MbArtistSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"trackName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistName"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"15"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"disambiguation"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"trackName"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<FixArtist_MbArtistSearchQuery, FixArtist_MbArtistSearchQueryVariables>;
export const SetPlaylistItemArtistMusicBrainzMatchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"SetPlaylistItemArtistMusicBrainzMatch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"input"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"SetPlaylistItemArtistMusicBrainzMatchInput"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"setPlaylistItemArtistMusicBrainzMatch"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"Variable","name":{"kind":"Name","value":"input"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SetPlaylistItemArtistMusicBrainzMatchSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"playlistItem"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SetPlaylistItemArtistMusicBrainzMatchNotFound"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SetPlaylistItemArtistMusicBrainzMatchError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<SetPlaylistItemArtistMusicBrainzMatchMutation, SetPlaylistItemArtistMusicBrainzMatchMutationVariables>;
export const PlaylistListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"PlaylistList"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playlists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]}}]} as unknown as DocumentNode<PlaylistListQuery, PlaylistListQueryVariables>;
export const RenamePlaylistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"RenamePlaylist"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"newPlaylistName"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"renamePlaylist"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"newPlaylistName"},"value":{"kind":"Variable","name":{"kind":"Name","value":"newPlaylistName"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"RenamePlaylistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"playlist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}}]}}]}}]}}]}}]} as unknown as DocumentNode<RenamePlaylistMutation, RenamePlaylistMutationVariables>;
export const DeletePlaylistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"DeletePlaylist"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deletePlaylist"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DeletePlaylistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"deletedPlaylistId"}}]}}]}}]}}]} as unknown as DocumentNode<DeletePlaylistMutation, DeletePlaylistMutationVariables>;
export const TopArtistRecommendationsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"TopArtistRecommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"topArtists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TopArtistCard_Artist"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]} as unknown as DocumentNode<TopArtistRecommendationsQuery, TopArtistRecommendationsQueryVariables>;
export const TopTrackRecommendationsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"TopTrackRecommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TopTrackCard_LastFmTrack"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopTrackCard_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"album"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"imageUrl"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]} as unknown as DocumentNode<TopTrackRecommendationsQuery, TopTrackRecommendationsQueryVariables>;
export const AlbumSearchResultsSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"AlbumSearchResultsSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MbReleaseGroupCard_MbReleaseGroup"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MbReleaseGroupCard_MbReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]} as unknown as DocumentNode<AlbumSearchResultsSearchQuery, AlbumSearchResultsSearchQueryVariables>;
export const ArtistSearchResultsSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistSearchResultsSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"searchText"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"searchText"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"MbArtistCard_MbArtist"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"MbArtistCard_MbArtist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]} as unknown as DocumentNode<ArtistSearchResultsSearchQuery, ArtistSearchResultsSearchQueryVariables>;
export const RecordingSearchResultsSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"RecordingSearchResultsSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"nameCredits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<RecordingSearchResultsSearchQuery, RecordingSearchResultsSearchQueryVariables>;
export const SearchResultArtistSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchResultArtistSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchArtists"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"searchTerm"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"thumbs"}}]}}]}}]}}]}}]} as unknown as DocumentNode<SearchResultArtistSearchQuery, SearchResultArtistSearchQueryVariables>;
export const SearchResultReleaseSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchResultReleaseSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchReleases"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"searchTerm"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}}]}}]}}]}}]} as unknown as DocumentNode<SearchResultReleaseSearchQuery, SearchResultReleaseSearchQueryVariables>;
export const SearchResultTrackSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchResultTrackSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverLibrary"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchTracks"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"searchTerm"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"trackLength"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUrl"}},{"kind":"Field","name":{"kind":"Name","value":"folderName"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<SearchResultTrackSearchQuery, SearchResultTrackSearchQueryVariables>;
export const UpdateDownloadPathDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateDownloadPath"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"newDownloadPath"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateDownloadPath"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"newDownloadPath"},"value":{"kind":"Variable","name":{"kind":"Name","value":"newDownloadPath"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateDownloadPathSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverSettings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"downloadPath"}}]}}]}}]}}]}}]} as unknown as DocumentNode<UpdateDownloadPathMutation, UpdateDownloadPathMutationVariables>;
export const UpdateLibraryPathDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateLibraryPath"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"newLibraryPath"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateLibraryPath"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"newLibraryPath"},"value":{"kind":"Variable","name":{"kind":"Name","value":"newLibraryPath"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateLibraryPathSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverSettings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"libraryPath"}}]}}]}}]}}]}}]} as unknown as DocumentNode<UpdateLibraryPathMutation, UpdateLibraryPathMutationVariables>;
export const DirectoryTreeItemDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"DirectoryTreeItem"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"path"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"fileSystem"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"browseFileSystem"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"path"},"value":{"kind":"Variable","name":{"kind":"Name","value":"path"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"FileSystemEntry"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"Field","name":{"kind":"Name","value":"hasChildren"}},{"kind":"Field","name":{"kind":"Name","value":"isAccessible"}}]}}]} as unknown as DocumentNode<DirectoryTreeItemQuery, DirectoryTreeItemQueryVariables>;
export const BrowseFileSystemDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"BrowseFileSystem"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"path"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"fileSystem"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"browseFileSystem"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"path"},"value":{"kind":"Variable","name":{"kind":"Name","value":"path"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"FileSystemEntry"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"Field","name":{"kind":"Name","value":"hasChildren"}},{"kind":"Field","name":{"kind":"Name","value":"isAccessible"}}]}}]} as unknown as DocumentNode<BrowseFileSystemQuery, BrowseFileSystemQueryVariables>;
export const SidebarDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"Sidebar"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]}}]} as unknown as DocumentNode<SidebarQuery, SidebarQueryVariables>;
export const ArtistImportQueuePanel_QueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistImportQueuePanel_Query"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistImport"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistImportQueue"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"queueLength"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"songTitle"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"currentArtistImport"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"songTitle"}},{"kind":"Field","name":{"kind":"Name","value":"statusInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"text"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalReleases"}},{"kind":"Field","name":{"kind":"Name","value":"completedReleases"}},{"kind":"Field","name":{"kind":"Name","value":"errorMessage"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistImportQueuePanel_QueryQuery, ArtistImportQueuePanel_QueryQueryVariables>;
export const ArtistImportQueueUpdatedSubDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"ArtistImportQueueUpdatedSub"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistImportQueueUpdated"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"queueLength"}},{"kind":"Field","name":{"kind":"Name","value":"items"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"songTitle"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistImportQueueUpdatedSubSubscription, ArtistImportQueueUpdatedSubSubscriptionVariables>;
export const CurrentArtistImportUpdatedSubDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"CurrentArtistImportUpdatedSub"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"currentArtistImportUpdated"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistName"}},{"kind":"Field","name":{"kind":"Name","value":"songTitle"}},{"kind":"Field","name":{"kind":"Name","value":"statusInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"text"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalReleases"}},{"kind":"Field","name":{"kind":"Name","value":"completedReleases"}},{"kind":"Field","name":{"kind":"Name","value":"errorMessage"}}]}}]}}]} as unknown as DocumentNode<CurrentArtistImportUpdatedSubSubscription, CurrentArtistImportUpdatedSubSubscriptionVariables>;
export const SoulSeekNetworkStatusSubscriptionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"SoulSeekNetworkStatusSubscription"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"soulSeekStatusUpdated"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatusSubscriptionSubscription, SoulSeekNetworkStatusSubscriptionSubscriptionVariables>;
export const SoulSeekNetworkStatusQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SoulSeekNetworkStatusQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"external"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"soulSeek"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatusQueryQuery, SoulSeekNetworkStatusQueryQueryVariables>;
export const ImportSpotifyPlaylistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ImportSpotifyPlaylist"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"importSpotifyPlaylist"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}},{"kind":"ObjectField","name":{"kind":"Name","value":"userId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ImportSpotifyPlaylistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"playlist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ImportSpotifyPlaylistError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<ImportSpotifyPlaylistMutation, ImportSpotifyPlaylistMutationVariables>;
export const UserPlaylistsLoader_QueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"UserPlaylistsLoader_Query"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"spotifyUsername"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"playlist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"importPlaylists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"spotify"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"spotifyPlaylistsForUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"username"},"value":{"kind":"Variable","name":{"kind":"Name","value":"spotifyUsername"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"SpotifyPlaylistsList_SpotifyPlaylist"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SpotifyPlaylistsList_SpotifyPlaylist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SpotifyPlaylist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}}]}}]} as unknown as DocumentNode<UserPlaylistsLoader_QueryQuery, UserPlaylistsLoader_QueryQueryVariables>;
export const ViewerIdForSpotifyImportDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ViewerIdForSpotifyImport"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<ViewerIdForSpotifyImportQuery, ViewerIdForSpotifyImportQueryVariables>;
export const ImportArtistsFromSpotifyPlaylistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ImportArtistsFromSpotifyPlaylist"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"importArtistsFromSpotifyPlaylist"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ImportArtistsFromSpotifyPlaylistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"totalArtists"}},{"kind":"Field","name":{"kind":"Name","value":"importedArtists"}},{"kind":"Field","name":{"kind":"Name","value":"failedArtists"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ImportArtistsFromSpotifyPlaylistError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<ImportArtistsFromSpotifyPlaylistMutation, ImportArtistsFromSpotifyPlaylistMutationVariables>;
export const UserProfileWidgetDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"UserProfileWidget"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]}}]} as unknown as DocumentNode<UserProfileWidgetQuery, UserProfileWidgetQueryVariables>;