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

export type AddReleaseGroupToServerLibraryInput = {
  releaseGroupId: Scalars['ID']['input'];
};

export type AddReleaseGroupToServerLibraryReleaseGroupAlreadyAdded = {
  __typename?: 'AddReleaseGroupToServerLibraryReleaseGroupAlreadyAdded';
  message: Scalars['String']['output'];
};

export type AddReleaseGroupToServerLibraryReleaseGroupDoesNotExist = {
  __typename?: 'AddReleaseGroupToServerLibraryReleaseGroupDoesNotExist';
  message: Scalars['String']['output'];
};

export type AddReleaseGroupToServerLibraryResult = AddReleaseGroupToServerLibraryReleaseGroupAlreadyAdded | AddReleaseGroupToServerLibraryReleaseGroupDoesNotExist | AddReleaseGroupToServerLibrarySuccess | AddReleaseGroupToServerLibraryUnknownError;

export type AddReleaseGroupToServerLibrarySuccess = {
  __typename?: 'AddReleaseGroupToServerLibrarySuccess';
  success: Scalars['Boolean']['output'];
};

export type AddReleaseGroupToServerLibraryUnknownError = {
  __typename?: 'AddReleaseGroupToServerLibraryUnknownError';
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

export type Artist = ArtistBase & {
  __typename?: 'Artist';
  albums: Array<ReleaseGroup>;
  eps: Array<ReleaseGroup>;
  gender?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  images?: Maybe<ArtistImages>;
  listeners?: Maybe<Scalars['Long']['output']>;
  musicBrainzArtist?: Maybe<MbArtist>;
  name: Scalars['String']['output'];
  releaseGroups: Array<ReleaseGroup>;
  releases: Array<Release>;
  serverStatus: ArtistServerStatus;
  singles: Array<ReleaseGroup>;
  sortName: Scalars['String']['output'];
  topTracks: Array<LastFmTrack>;
};

export type ArtistBase = {
  id: Scalars['ID']['output'];
  images?: Maybe<ArtistImages>;
  listeners?: Maybe<Scalars['Long']['output']>;
  name: Scalars['String']['output'];
  serverStatus: ArtistServerStatus;
  sortName: Scalars['String']['output'];
};

export type ArtistImages = {
  __typename?: 'ArtistImages';
  artistBackground?: Maybe<Scalars['String']['output']>;
  artistThumb?: Maybe<Scalars['String']['output']>;
  hdMusicLogo?: Maybe<Scalars['String']['output']>;
  musicBanner?: Maybe<Scalars['String']['output']>;
  musicLogo?: Maybe<Scalars['String']['output']>;
};

export type ArtistSearchRoot = {
  __typename?: 'ArtistSearchRoot';
  all: Array<Artist>;
  byId?: Maybe<Artist>;
  searchByName: Array<Artist>;
};


export type ArtistSearchRootByIdArgs = {
  id: Scalars['ID']['input'];
};


export type ArtistSearchRootSearchByNameArgs = {
  limit?: Scalars['Int']['input'];
  name: Scalars['String']['input'];
  offset?: Scalars['Int']['input'];
};

export type ArtistServerStatus = {
  __typename?: 'ArtistServerStatus';
  id: Scalars['ID']['output'];
  result: ArtistServerStatusResult;
};

export type ArtistServerStatusImportingArtist = ArtistServerStatusResultBase & {
  __typename?: 'ArtistServerStatusImportingArtist';
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
  releasesVisible: Scalars['Boolean']['output'];
  topTracksVisible: Scalars['Boolean']['output'];
};

export type ArtistServerStatusReady = ArtistServerStatusResultBase & {
  __typename?: 'ArtistServerStatusReady';
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
  releasesVisible: Scalars['Boolean']['output'];
  topTracksVisible: Scalars['Boolean']['output'];
};

export type ArtistServerStatusUpdatingArtistReleases = ArtistServerStatusResultBase & {
  __typename?: 'ArtistServerStatusUpdatingArtistReleases';
  releasesVisible: Scalars['Boolean']['output'];
  topTracksVisible: Scalars['Boolean']['output'];
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
  isAccessible: Scalars['Boolean']['output'];
  isDirectory: Scalars['Boolean']['output'];
  name: Scalars['String']['output'];
  path: Scalars['String']['output'];
};

export type IReleaseBase = {
  country?: Maybe<Scalars['String']['output']>;
  coverArtUri: Scalars['String']['output'];
  date?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  title: Scalars['String']['output'];
  year?: Maybe<Scalars['String']['output']>;
};

export type ImportPlaylistSearchRoot = {
  __typename?: 'ImportPlaylistSearchRoot';
  spotify: SpotifyPlaylistSearchRoot;
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

export type Label = {
  __typename?: 'Label';
  disambiguation: Scalars['String']['output'];
  id: Scalars['String']['output'];
  name: Scalars['String']['output'];
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
  recording?: Maybe<Recording>;
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

export type MbArtist = ArtistBase & {
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

export type MbRelease = IReleaseBase & {
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

export type MbReleaseGroup = ReleaseGroupBase & {
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
  addReleaseGroupToServerLibrary: AddReleaseGroupToServerLibraryResult;
  createDirectory: FileSystemEntry;
  createPlaylist: CreatePlaylistResult;
  createUser: CreateUserResult;
  deletePlaylist: DeletePlaylistResult;
  importSpotifyPlaylistById: ImportSpotifyPlaylistResult;
  likeSong: LikeSongResult;
  ping: Scalars['Boolean']['output'];
  renamePlaylist: RenamePlaylistResult;
  signIn: SignInResult;
  signOut: SignOutResult;
  startDownloadRelease: StartDownloadReleaseResult;
  unlikeSong: UnlikedSongPayload;
  updateLibraryPath: UpdateLibraryPathResult;
};


export type MutationAddArtistToServerLibraryArgs = {
  input: AddArtistToServerLibraryInput;
};


export type MutationAddReleaseGroupToServerLibraryArgs = {
  input: AddReleaseGroupToServerLibraryInput;
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


export type MutationUpdateLibraryPathArgs = {
  input: UpdateLibraryPathInput;
};

export type NameCredit = {
  __typename?: 'NameCredit';
  artist: Artist;
  joinPhrase: Scalars['String']['output'];
  name: Scalars['String']['output'];
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
  modifiedAt?: Maybe<Scalars['DateTime']['output']>;
  name?: Maybe<Scalars['String']['output']>;
  recordings: Array<Recording>;
};

export type PlaylistSearchRoot = {
  __typename?: 'PlaylistSearchRoot';
  importPlaylists: ImportPlaylistSearchRoot;
};

export type Query = {
  __typename?: 'Query';
  areThereAnyUsers: Scalars['Boolean']['output'];
  artist: ArtistSearchRoot;
  browseFileSystem: Array<FileSystemEntry>;
  download: DownloadsSearchRoot;
  external: ExternalRoot;
  musicBrainz: MusicBrainzSearchRoot;
  playlist: PlaylistSearchRoot;
  recommendations: RecommendationsSearchRoot;
  recording: RecordingSearchRoot;
  release: ReleaseSearchRoot;
  releaseGroup: ReleaseGroupSearchRoot;
  serverSettings: ServerSettings;
  user: UserSearchRoot;
  viewer?: Maybe<User>;
};


export type QueryBrowseFileSystemArgs = {
  path?: InputMaybe<Scalars['String']['input']>;
};

export type RecommendationsSearchRoot = {
  __typename?: 'RecommendationsSearchRoot';
  topArtists: Array<LastFmArtist>;
  topTags: Array<LastFmTag>;
  topTracks: Array<LastFmTrack>;
};

export type Recording = {
  __typename?: 'Recording';
  id: Scalars['ID']['output'];
  length?: Maybe<Scalars['Int']['output']>;
  mainAlbum?: Maybe<ReleaseGroup>;
  nameCredits: Array<NameCredit>;
  statistics?: Maybe<LastFmStatistics>;
  streamingServiceInfo: RecordingStreamingServiceInfo;
  title: Scalars['String']['output'];
};

export type RecordingSearchRoot = {
  __typename?: 'RecordingSearchRoot';
  byId?: Maybe<Recording>;
  searchByName: Array<Recording>;
};


export type RecordingSearchRootByIdArgs = {
  id: Scalars['ID']['input'];
};


export type RecordingSearchRootSearchByNameArgs = {
  limit?: Scalars['Int']['input'];
  name: Scalars['String']['input'];
  offset?: Scalars['Int']['input'];
};

export type RecordingStreamingServiceInfo = {
  __typename?: 'RecordingStreamingServiceInfo';
  id: Scalars['ID']['output'];
  youtubeMusicUrl?: Maybe<Scalars['String']['output']>;
  youtubeSearchVideoId?: Maybe<Scalars['String']['output']>;
  youtubeVideoId?: Maybe<Scalars['String']['output']>;
};

export type Release = IReleaseBase & {
  __typename?: 'Release';
  country?: Maybe<Scalars['String']['output']>;
  coverArtUri: Scalars['String']['output'];
  credits: Array<NameCredit>;
  date?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  labels: Array<Label>;
  recordings: Array<Recording>;
  releaseGroup?: Maybe<ReleaseGroup>;
  status?: Maybe<Scalars['String']['output']>;
  title: Scalars['String']['output'];
  year?: Maybe<Scalars['String']['output']>;
};

export type ReleaseGroup = ReleaseGroupBase & {
  __typename?: 'ReleaseGroup';
  coverArtUri?: Maybe<Scalars['String']['output']>;
  credits: Array<NameCredit>;
  firstReleaseDate?: Maybe<Scalars['String']['output']>;
  firstReleaseYear?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  mainRelease?: Maybe<Release>;
  musicBrainzReleaseGroup?: Maybe<MbReleaseGroup>;
  primaryType?: Maybe<Scalars['String']['output']>;
  secondaryTypes: Array<Scalars['String']['output']>;
  title: Scalars['String']['output'];
};

export type ReleaseGroupBase = {
  firstReleaseDate?: Maybe<Scalars['String']['output']>;
  firstReleaseYear?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  primaryType?: Maybe<Scalars['String']['output']>;
  secondaryTypes: Array<Scalars['String']['output']>;
  title: Scalars['String']['output'];
};

export type ReleaseGroupSearchRoot = {
  __typename?: 'ReleaseGroupSearchRoot';
  all: Array<ReleaseGroup>;
  byId?: Maybe<ReleaseGroup>;
  searchByName: Array<ReleaseGroup>;
};


export type ReleaseGroupSearchRootByIdArgs = {
  id: Scalars['ID']['input'];
};


export type ReleaseGroupSearchRootSearchByNameArgs = {
  limit?: Scalars['Int']['input'];
  name: Scalars['String']['input'];
  offset?: Scalars['Int']['input'];
};

export type ReleaseSearchRoot = {
  __typename?: 'ReleaseSearchRoot';
  byId?: Maybe<Release>;
  searchByName: Array<Release>;
};


export type ReleaseSearchRootByIdArgs = {
  id: Scalars['ID']['input'];
};


export type ReleaseSearchRootSearchByNameArgs = {
  limit?: Scalars['Int']['input'];
  name: Scalars['String']['input'];
  offset?: Scalars['Int']['input'];
};

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

export type ServerSettings = {
  __typename?: 'ServerSettings';
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

export type UnlikedSongInput = {
  recordingId: Scalars['String']['input'];
};

export type UnlikedSongPayload = {
  __typename?: 'UnlikedSongPayload';
  success: Scalars['Boolean']['output'];
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


export type AlbumListQueryQuery = { __typename?: 'Query', releaseGroup: { __typename?: 'ReleaseGroupSearchRoot', all: Array<(
      { __typename?: 'ReleaseGroup', id: string }
      & { ' $fragmentRefs'?: { 'AlbumList_ReleaseGroupFragment': AlbumList_ReleaseGroupFragment } }
    )> } };

export type AlbumQueryQueryVariables = Exact<{
  releaseGroupId: Scalars['ID']['input'];
}>;


export type AlbumQueryQuery = { __typename?: 'Query', releaseGroup: { __typename?: 'ReleaseGroupSearchRoot', byId?: (
      { __typename?: 'ReleaseGroup', id: string, title: string, credits: Array<{ __typename?: 'NameCredit', name: string }> }
      & { ' $fragmentRefs'?: { 'AlbumPanel_ReleaseGroupFragment': AlbumPanel_ReleaseGroupFragment } }
    ) | null } };

export type ArtistListQueryQueryVariables = Exact<{ [key: string]: never; }>;


export type ArtistListQueryQuery = { __typename?: 'Query', artist: { __typename?: 'ArtistSearchRoot', all: Array<(
      { __typename?: 'Artist', id: string }
      & { ' $fragmentRefs'?: { 'ArtistList_ArtistFragment': ArtistList_ArtistFragment } }
    )> } };

export type ArtistQueryQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistQueryQuery = { __typename?: 'Query', artist: { __typename?: 'ArtistSearchRoot', byId?: (
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
    & { ' $fragmentRefs'?: { 'ServerSettingsFormContainer_ServerSettingsFragment': ServerSettingsFormContainer_ServerSettingsFragment } }
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

export type LikedSongRow_RecodingFragment = { __typename?: 'Recording', id: string, title: string, length?: number | null, nameCredits: Array<{ __typename?: 'NameCredit', artist: { __typename?: 'Artist', id: string, name: string } }>, mainAlbum?: { __typename?: 'ReleaseGroup', id: string, title: string, coverArtUri?: string | null } | null } & { ' $fragmentName'?: 'LikedSongRow_RecodingFragment' };

export type AddArtistToServerLibraryMutationVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type AddArtistToServerLibraryMutation = { __typename?: 'Mutation', addArtistToServerLibrary: { __typename: 'AddArtistToServerLibraryArtistAlreadyAdded' } | { __typename: 'AddArtistToServerLibraryArtistDoesNotExist' } | { __typename: 'AddArtistToServerLibrarySuccess' } | { __typename: 'AddArtistToServerLibraryUnknownError' } };

type AlbumCard_ReleaseGroup_MbReleaseGroup_Fragment = { __typename?: 'MbReleaseGroup', coverArtUri?: string | null, id: string, title: string, firstReleaseYear?: string | null, credits: Array<{ __typename?: 'MbNameCredit', name: string, artist: { __typename?: 'MbArtist', id: string, name: string } }> } & { ' $fragmentName'?: 'AlbumCard_ReleaseGroup_MbReleaseGroup_Fragment' };

type AlbumCard_ReleaseGroup_ReleaseGroup_Fragment = { __typename?: 'ReleaseGroup', coverArtUri?: string | null, id: string, title: string, firstReleaseYear?: string | null, credits: Array<{ __typename?: 'NameCredit', name: string, artist: { __typename?: 'Artist', id: string, name: string } }> } & { ' $fragmentName'?: 'AlbumCard_ReleaseGroup_ReleaseGroup_Fragment' };

export type AlbumCard_ReleaseGroupFragment = AlbumCard_ReleaseGroup_MbReleaseGroup_Fragment | AlbumCard_ReleaseGroup_ReleaseGroup_Fragment;

export type AlbumHeader_ReleaseGroupFragment = { __typename?: 'ReleaseGroup', id: string, title: string, primaryType?: string | null, coverArtUri?: string | null, firstReleaseYear?: string | null, credits: Array<{ __typename?: 'NameCredit', artist: { __typename?: 'Artist', id: string, name: string } }>, mainRelease?: { __typename?: 'Release', id: string, recordings: Array<{ __typename?: 'Recording', id: string, length?: number | null }> } | null } & { ' $fragmentName'?: 'AlbumHeader_ReleaseGroupFragment' };

export type AlbumPanel_ReleaseGroupFragment = (
  { __typename?: 'ReleaseGroup', id: string, firstReleaseYear?: string | null, mainRelease?: { __typename?: 'Release', labels: Array<{ __typename?: 'Label', id: string, name: string }> } | null }
  & { ' $fragmentRefs'?: { 'AlbumHeader_ReleaseGroupFragment': AlbumHeader_ReleaseGroupFragment;'AlbumTrackList_ReleaseGroupFragment': AlbumTrackList_ReleaseGroupFragment } }
) & { ' $fragmentName'?: 'AlbumPanel_ReleaseGroupFragment' };

export type AlbumTrackList_ReleaseGroupFragment = { __typename?: 'ReleaseGroup', id: string, title: string, mainRelease?: { __typename?: 'Release', id: string, title: string, recordings: Array<(
      { __typename?: 'Recording', id: string, title: string, length?: number | null, statistics?: { __typename?: 'LastFmStatistics', listeners: any } | null, nameCredits: Array<{ __typename?: 'NameCredit', name: string, artist: { __typename?: 'Artist', id: string, name: string } }> }
      & { ' $fragmentRefs'?: { 'RecordingPlayButton_RecordingFragment': RecordingPlayButton_RecordingFragment } }
    )> } | null } & { ' $fragmentName'?: 'AlbumTrackList_ReleaseGroupFragment' };

export type AlbumList_ReleaseGroupFragment = (
  { __typename?: 'ReleaseGroup', id: string }
  & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseGroup_ReleaseGroup_Fragment': AlbumCard_ReleaseGroup_ReleaseGroup_Fragment } }
) & { ' $fragmentName'?: 'AlbumList_ReleaseGroupFragment' };

type ArtistCard_Artist_Artist_Fragment = { __typename?: 'Artist', id: string, name: string, listeners?: any | null, images?: { __typename?: 'ArtistImages', artistThumb?: string | null } | null } & { ' $fragmentName'?: 'ArtistCard_Artist_Artist_Fragment' };

type ArtistCard_Artist_MbArtist_Fragment = { __typename?: 'MbArtist', id: string, name: string, listeners?: any | null, images?: { __typename?: 'ArtistImages', artistThumb?: string | null } | null } & { ' $fragmentName'?: 'ArtistCard_Artist_MbArtist_Fragment' };

export type ArtistCard_ArtistFragment = ArtistCard_Artist_Artist_Fragment | ArtistCard_Artist_MbArtist_Fragment;

export type TopArtistCard_ArtistFragment = { __typename?: 'LastFmArtist', id: string, name: string, musicBrainzArtist?: { __typename?: 'MbArtist', id: string, name: string, images?: { __typename?: 'ArtistImages', artistThumb?: string | null } | null } | null, statistics: { __typename?: 'LastFmStatistics', listeners: any } } & { ' $fragmentName'?: 'TopArtistCard_ArtistFragment' };

export type ArtistList_ArtistFragment = (
  { __typename?: 'Artist', id: string }
  & { ' $fragmentRefs'?: { 'ArtistCard_Artist_Artist_Fragment': ArtistCard_Artist_Artist_Fragment } }
) & { ' $fragmentName'?: 'ArtistList_ArtistFragment' };

export type ArtistNotInLibraryTopTracks_ArtistFragment = { __typename?: 'Artist', id: string, topTracks: Array<{ __typename?: 'LastFmTrack', id: string, name: string, statistics: { __typename?: 'LastFmStatistics', listeners: any } }> } & { ' $fragmentName'?: 'ArtistNotInLibraryTopTracks_ArtistFragment' };

export type RefreshArtistMutationVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type RefreshArtistMutation = { __typename?: 'Mutation', addArtistToServerLibrary: { __typename: 'AddArtistToServerLibraryArtistAlreadyAdded' } | { __typename: 'AddArtistToServerLibraryArtistDoesNotExist' } | { __typename: 'AddArtistToServerLibrarySuccess' } | { __typename: 'AddArtistToServerLibraryUnknownError' } };

export type ArtistAlbumListQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistAlbumListQuery = { __typename?: 'Query', artist: { __typename?: 'ArtistSearchRoot', byId?: { __typename?: 'Artist', id: string, albums: Array<(
        { __typename?: 'ReleaseGroup', id: string, firstReleaseDate?: string | null }
        & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseGroup_ReleaseGroup_Fragment': AlbumCard_ReleaseGroup_ReleaseGroup_Fragment } }
      )> } | null } };

export type ArtistEpListQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistEpListQuery = { __typename?: 'Query', artist: { __typename?: 'ArtistSearchRoot', byId?: { __typename?: 'Artist', id: string, eps: Array<(
        { __typename?: 'ReleaseGroup', id: string, firstReleaseDate?: string | null }
        & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseGroup_ReleaseGroup_Fragment': AlbumCard_ReleaseGroup_ReleaseGroup_Fragment } }
      )> } | null } };

export type ArtistPanel_ArtistFragment = (
  { __typename?: 'Artist', id: string, name: string, listeners?: any | null, albums: Array<(
    { __typename?: 'ReleaseGroup', id: string, firstReleaseDate?: string | null }
    & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseGroup_ReleaseGroup_Fragment': AlbumCard_ReleaseGroup_ReleaseGroup_Fragment } }
  )>, eps: Array<(
    { __typename?: 'ReleaseGroup', id: string, firstReleaseDate?: string | null }
    & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseGroup_ReleaseGroup_Fragment': AlbumCard_ReleaseGroup_ReleaseGroup_Fragment } }
  )>, singles: Array<(
    { __typename?: 'ReleaseGroup', id: string, firstReleaseDate?: string | null }
    & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseGroup_ReleaseGroup_Fragment': AlbumCard_ReleaseGroup_ReleaseGroup_Fragment } }
  )>, images?: { __typename?: 'ArtistImages', artistBackground?: string | null } | null, serverStatus: { __typename?: 'ArtistServerStatus', id: string, result: { __typename: 'ArtistServerStatusImportingArtist', topTracksVisible: boolean, releasesVisible: boolean } | { __typename: 'ArtistServerStatusImportingArtistReleases', topTracksVisible: boolean, releasesVisible: boolean, numReleaseGroupsFinishedImporting: number, totalNumReleaseGroupsBeingImported: number } | { __typename: 'ArtistServerStatusNotInLibrary', topTracksVisible: boolean, releasesVisible: boolean } | { __typename: 'ArtistServerStatusReady', topTracksVisible: boolean, releasesVisible: boolean } | { __typename: 'ArtistServerStatusUpdatingArtist', topTracksVisible: boolean, releasesVisible: boolean } | { __typename: 'ArtistServerStatusUpdatingArtistReleases', topTracksVisible: boolean, releasesVisible: boolean } } }
  & { ' $fragmentRefs'?: { 'ArtistNotInLibraryTopTracks_ArtistFragment': ArtistNotInLibraryTopTracks_ArtistFragment } }
) & { ' $fragmentName'?: 'ArtistPanel_ArtistFragment' };

export type ArtistSingleListQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistSingleListQuery = { __typename?: 'Query', artist: { __typename?: 'ArtistSearchRoot', byId?: { __typename?: 'Artist', id: string, singles: Array<(
        { __typename?: 'ReleaseGroup', id: string, firstReleaseDate?: string | null }
        & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseGroup_ReleaseGroup_Fragment': AlbumCard_ReleaseGroup_ReleaseGroup_Fragment } }
      )> } | null } };

export type TopArtistTrackItem_LastFmTrackFragment = { __typename?: 'LastFmTrack', id: string, name: string, playCount?: any | null, summary?: string | null, recording?: (
    { __typename?: 'Recording', id: string, title: string, length?: number | null, mainAlbum?: { __typename?: 'ReleaseGroup', id: string, title: string, coverArtUri?: string | null } | null }
    & { ' $fragmentRefs'?: { 'RecordingPlayButton_RecordingFragment': RecordingPlayButton_RecordingFragment } }
  ) | null } & { ' $fragmentName'?: 'TopArtistTrackItem_LastFmTrackFragment' };

export type TopArtistTracksQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type TopArtistTracksQuery = { __typename?: 'Query', artist: { __typename?: 'ArtistSearchRoot', byId?: { __typename?: 'Artist', id: string, topTracks: Array<(
        { __typename?: 'LastFmTrack', id: string }
        & { ' $fragmentRefs'?: { 'TopArtistTrackItem_LastFmTrackFragment': TopArtistTrackItem_LastFmTrackFragment } }
      )> } | null } };

export type ArtistServerStatusQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistServerStatusQuery = { __typename?: 'Query', artist: { __typename?: 'ArtistSearchRoot', byId?: { __typename?: 'Artist', id: string, serverStatus: { __typename?: 'ArtistServerStatus', id: string, result: { __typename: 'ArtistServerStatusImportingArtist', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusImportingArtistReleases', releasesVisible: boolean, topTracksVisible: boolean, totalNumReleaseGroupsBeingImported: number, numReleaseGroupsFinishedImporting: number } | { __typename: 'ArtistServerStatusNotInLibrary', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusReady', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusUpdatingArtist', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusUpdatingArtistReleases', releasesVisible: boolean, topTracksVisible: boolean } } } | null } };

export type ArtistServerStatusSubSubscriptionVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistServerStatusSubSubscription = { __typename?: 'Subscription', artistServerStatusUpdated: { __typename?: 'ArtistServerStatus', id: string, result: { __typename: 'ArtistServerStatusImportingArtist', releasesVisible: boolean, topTracksVisible: boolean } | { __typename: 'ArtistServerStatusImportingArtistReleases', releasesVisible: boolean, topTracksVisible: boolean, totalNumReleaseGroupsBeingImported: number, numReleaseGroupsFinishedImporting: number, artist: { __typename?: 'Artist', id: string, albums: Array<(
          { __typename?: 'ReleaseGroup', id: string, firstReleaseDate?: string | null }
          & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseGroup_ReleaseGroup_Fragment': AlbumCard_ReleaseGroup_ReleaseGroup_Fragment } }
        )>, eps: Array<(
          { __typename?: 'ReleaseGroup', id: string, firstReleaseDate?: string | null }
          & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseGroup_ReleaseGroup_Fragment': AlbumCard_ReleaseGroup_ReleaseGroup_Fragment } }
        )>, singles: Array<(
          { __typename?: 'ReleaseGroup', id: string, firstReleaseDate?: string | null }
          & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseGroup_ReleaseGroup_Fragment': AlbumCard_ReleaseGroup_ReleaseGroup_Fragment } }
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

export type RecordingPlayButton_RecordingFragment = { __typename?: 'Recording', id: string, streamingServiceInfo: { __typename?: 'RecordingStreamingServiceInfo', id: string, youtubeVideoId?: string | null } } & { ' $fragmentName'?: 'RecordingPlayButton_RecordingFragment' };

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

export type TopTrackCard_LastFmTrackFragment = { __typename?: 'LastFmTrack', id: string, playCount?: any | null, name: string, artist: { __typename?: 'LastFmArtist', id: string, musicBrainzArtist?: { __typename?: 'MbArtist', id: string, name: string, images?: { __typename?: 'ArtistImages', artistThumb?: string | null } | null } | null }, album?: { __typename?: 'LastFmAlbum', id: string, imageUrl?: string | null } | null, images?: { __typename?: 'ArtistImages', artistThumb?: string | null } | null } & { ' $fragmentName'?: 'TopTrackCard_LastFmTrackFragment' };

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
        & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseGroup_MbReleaseGroup_Fragment': AlbumCard_ReleaseGroup_MbReleaseGroup_Fragment } }
      )> } } };

export type ArtistSearchResultsSearchQueryVariables = Exact<{
  searchText: Scalars['String']['input'];
  limit: Scalars['Int']['input'];
}>;


export type ArtistSearchResultsSearchQuery = { __typename?: 'Query', musicBrainz: { __typename?: 'MusicBrainzSearchRoot', artist: { __typename?: 'MusicBrainzArtistSearchRoot', searchByName: Array<(
        { __typename?: 'MbArtist', id: string, name: string, images?: { __typename?: 'ArtistImages', artistThumb?: string | null } | null }
        & { ' $fragmentRefs'?: { 'ArtistCard_Artist_MbArtist_Fragment': ArtistCard_Artist_MbArtist_Fragment } }
      )> } } };

export type RecordingSearchResultsSearchQueryVariables = Exact<{
  text: Scalars['String']['input'];
  limit: Scalars['Int']['input'];
}>;


export type RecordingSearchResultsSearchQuery = { __typename?: 'Query', musicBrainz: { __typename?: 'MusicBrainzSearchRoot', recording: { __typename?: 'MusicBrainzRecordingSearchRoot', searchByName: Array<{ __typename?: 'MbRecording', id: string, title: string, length?: number | null, nameCredits: Array<{ __typename?: 'MbNameCredit', artist: { __typename?: 'MbArtist', id: string, name: string } }>, mainAlbum?: { __typename?: 'MbRelease', id: string, title: string, coverArtUri: string } | null }> } } };

export type SearchResultArtistSearchQueryVariables = Exact<{
  text: Scalars['String']['input'];
}>;


export type SearchResultArtistSearchQuery = { __typename?: 'Query', artist: { __typename?: 'ArtistSearchRoot', searchByName: Array<{ __typename?: 'Artist', id: string, name: string, images?: { __typename?: 'ArtistImages', artistThumb?: string | null } | null }> } };

export type SearchResultSongSearchQueryVariables = Exact<{
  text: Scalars['String']['input'];
}>;


export type SearchResultSongSearchQuery = { __typename?: 'Query', recording: { __typename?: 'RecordingSearchRoot', searchByName: Array<{ __typename?: 'Recording', id: string, title: string, length?: number | null, nameCredits: Array<{ __typename?: 'NameCredit', artist: { __typename?: 'Artist', id: string, name: string } }>, mainAlbum?: { __typename?: 'ReleaseGroup', id: string, title: string, coverArtUri?: string | null } | null }> } };

export type SearchResultAlbumSearchQueryVariables = Exact<{
  text: Scalars['String']['input'];
}>;


export type SearchResultAlbumSearchQuery = { __typename?: 'Query', releaseGroup: { __typename?: 'ReleaseGroupSearchRoot', searchByName: Array<{ __typename?: 'ReleaseGroup', id: string, title: string, mainRelease?: { __typename?: 'Release', id: string, title: string, coverArtUri: string } | null }> } };

export type ServerSettingsFormContainer_ServerSettingsFragment = { __typename?: 'ServerSettings', id: string, libraryPath: string } & { ' $fragmentName'?: 'ServerSettingsFormContainer_ServerSettingsFragment' };

export type UpdateServerSettingsMutationVariables = Exact<{
  newLibraryPath: Scalars['String']['input'];
}>;


export type UpdateServerSettingsMutation = { __typename?: 'Mutation', updateLibraryPath: { __typename?: 'UpdateLibraryPathSuccess', serverSettings: { __typename?: 'ServerSettings', id: string, libraryPath: string } } };

export type DirectoryTreeItem_FileSystemEntryFragment = { __typename?: 'FileSystemEntry', name: string, path: string, isDirectory: boolean, hasChildren: boolean, isAccessible: boolean } & { ' $fragmentName'?: 'DirectoryTreeItem_FileSystemEntryFragment' };

export type DirectoryTreeItemQueryVariables = Exact<{
  path?: InputMaybe<Scalars['String']['input']>;
}>;


export type DirectoryTreeItemQuery = { __typename?: 'Query', browseFileSystem: Array<(
    { __typename?: 'FileSystemEntry' }
    & { ' $fragmentRefs'?: { 'DirectoryTreeItem_FileSystemEntryFragment': DirectoryTreeItem_FileSystemEntryFragment } }
  )> };

export type BrowseFileSystemQueryVariables = Exact<{
  path?: InputMaybe<Scalars['String']['input']>;
}>;


export type BrowseFileSystemQuery = { __typename?: 'Query', browseFileSystem: Array<(
    { __typename?: 'FileSystemEntry', path: string }
    & { ' $fragmentRefs'?: { 'DirectoryTreeItem_FileSystemEntryFragment': DirectoryTreeItem_FileSystemEntryFragment } }
  )> };

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
export const LikedSongRow_RecodingFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_Recoding"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Recording"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"nameCredits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]} as unknown as DocumentNode<LikedSongRow_RecodingFragment, unknown>;
export const AlbumHeader_ReleaseGroupFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"primaryType"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"length"}}]}}]}}]}}]} as unknown as DocumentNode<AlbumHeader_ReleaseGroupFragment, unknown>;
export const RecordingPlayButton_RecordingFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RecordingPlayButton_Recording"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Recording"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"streamingServiceInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"youtubeVideoId"}}]}}]}}]} as unknown as DocumentNode<RecordingPlayButton_RecordingFragment, unknown>;
export const AlbumTrackList_ReleaseGroupFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"RecordingPlayButton_Recording"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}},{"kind":"Field","name":{"kind":"Name","value":"nameCredits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RecordingPlayButton_Recording"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Recording"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"streamingServiceInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"youtubeVideoId"}}]}}]}}]} as unknown as DocumentNode<AlbumTrackList_ReleaseGroupFragment, unknown>;
export const AlbumPanel_ReleaseGroupFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_ReleaseGroup"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_ReleaseGroup"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"labels"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RecordingPlayButton_Recording"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Recording"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"streamingServiceInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"youtubeVideoId"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"primaryType"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"length"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"RecordingPlayButton_Recording"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}},{"kind":"Field","name":{"kind":"Name","value":"nameCredits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<AlbumPanel_ReleaseGroupFragment, unknown>;
export const AlbumCard_ReleaseGroupFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroupBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]} as unknown as DocumentNode<AlbumCard_ReleaseGroupFragment, unknown>;
export const AlbumList_ReleaseGroupFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumList_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroupBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]} as unknown as DocumentNode<AlbumList_ReleaseGroupFragment, unknown>;
export const TopArtistCard_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]} as unknown as DocumentNode<TopArtistCard_ArtistFragment, unknown>;
export const ArtistCard_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]} as unknown as DocumentNode<ArtistCard_ArtistFragment, unknown>;
export const ArtistList_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistList_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistCard_Artist"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]} as unknown as DocumentNode<ArtistList_ArtistFragment, unknown>;
export const ArtistNotInLibraryTopTracks_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNotInLibraryTopTracks_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistNotInLibraryTopTracks_ArtistFragment, unknown>;
export const ArtistPanel_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistPanel_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistBackground"}}]}},{"kind":"Field","name":{"kind":"Name","value":"serverStatus"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"result"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusResultBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"topTracksVisible"}},{"kind":"Field","name":{"kind":"Name","value":"releasesVisible"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusImportingArtistReleases"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"numReleaseGroupsFinishedImporting"}},{"kind":"Field","name":{"kind":"Name","value":"totalNumReleaseGroupsBeingImported"}}]}}]}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistNotInLibraryTopTracks_Artist"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroupBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNotInLibraryTopTracks_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]}}]} as unknown as DocumentNode<ArtistPanel_ArtistFragment, unknown>;
export const TopArtistTrackItem_LastFmTrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistTrackItem_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"summary"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"RecordingPlayButton_Recording"}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RecordingPlayButton_Recording"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Recording"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"streamingServiceInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"youtubeVideoId"}}]}}]}}]} as unknown as DocumentNode<TopArtistTrackItem_LastFmTrackFragment, unknown>;
export const DownloadStatus_DownloadStatusFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"numberOfTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracksDownloaded"}},{"kind":"Field","name":{"kind":"Name","value":"statusDescription"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]} as unknown as DocumentNode<DownloadStatus_DownloadStatusFragment, unknown>;
export const DownloadOverview_DownloadStatusFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadOverview_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"numberOfTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracksDownloaded"}},{"kind":"Field","name":{"kind":"Name","value":"statusDescription"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]} as unknown as DocumentNode<DownloadOverview_DownloadStatusFragment, unknown>;
export const LikedSongsList_UserFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongsList_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongRow_LikedSong"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]} as unknown as DocumentNode<LikedSongsList_UserFragment, unknown>;
export const TopTrackCard_LastFmTrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopTrackCard_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"album"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"imageUrl"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]} as unknown as DocumentNode<TopTrackCard_LastFmTrackFragment, unknown>;
export const ServerSettingsFormContainer_ServerSettingsFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ServerSettingsFormContainer_ServerSettings"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServerSettings"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"libraryPath"}}]}}]} as unknown as DocumentNode<ServerSettingsFormContainer_ServerSettingsFragment, unknown>;
export const DirectoryTreeItem_FileSystemEntryFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"FileSystemEntry"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"Field","name":{"kind":"Name","value":"hasChildren"}},{"kind":"Field","name":{"kind":"Name","value":"isAccessible"}}]}}]} as unknown as DocumentNode<DirectoryTreeItem_FileSystemEntryFragment, unknown>;
export const SoulSeekNetworkStatus_SoulSeekStatusFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatus_SoulSeekStatusFragment, unknown>;
export const SpotifyPlaylistsList_SpotifyPlaylistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SpotifyPlaylistsList_SpotifyPlaylist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SpotifyPlaylist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}}]}}]} as unknown as DocumentNode<SpotifyPlaylistsList_SpotifyPlaylistFragment, unknown>;
export const UserProfilePanel_UserFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserProfilePanel_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"updatedAt"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<UserProfilePanel_UserFragment, unknown>;
export const BootstrapDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"Bootstrap"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"areThereAnyUsers"}},{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<BootstrapQuery, BootstrapQueryVariables>;
export const AlbumListQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"AlbumListQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"all"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumList_ReleaseGroup"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroupBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumList_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}}]} as unknown as DocumentNode<AlbumListQueryQuery, AlbumListQueryQueryVariables>;
export const AlbumQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"AlbumQuery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"releaseGroupId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"releaseGroupId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumPanel_ReleaseGroup"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"primaryType"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"length"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RecordingPlayButton_Recording"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Recording"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"streamingServiceInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"youtubeVideoId"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"RecordingPlayButton_Recording"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}},{"kind":"Field","name":{"kind":"Name","value":"nameCredits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_ReleaseGroup"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_ReleaseGroup"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"labels"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]} as unknown as DocumentNode<AlbumQueryQuery, AlbumQueryQueryVariables>;
export const ArtistListQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistListQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"all"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistList_Artist"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistList_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistCard_Artist"}}]}}]} as unknown as DocumentNode<ArtistListQueryQuery, ArtistListQueryQueryVariables>;
export const ArtistQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistQuery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistPanel_Artist"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroupBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistNotInLibraryTopTracks_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistPanel_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistBackground"}}]}},{"kind":"Field","name":{"kind":"Name","value":"serverStatus"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"result"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusResultBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"topTracksVisible"}},{"kind":"Field","name":{"kind":"Name","value":"releasesVisible"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusImportingArtistReleases"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"numReleaseGroupsFinishedImporting"}},{"kind":"Field","name":{"kind":"Name","value":"totalNumReleaseGroupsBeingImported"}}]}}]}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistNotInLibraryTopTracks_Artist"}}]}}]} as unknown as DocumentNode<ArtistQueryQuery, ArtistQueryQueryVariables>;
export const LikedSongsQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"LikedSongsQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongsList_User"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongsList_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongRow_LikedSong"}}]}}]}}]} as unknown as DocumentNode<LikedSongsQueryQuery, LikedSongsQueryQueryVariables>;
export const SettingsPageDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SettingsPage"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverSettings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"ServerSettingsFormContainer_ServerSettings"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ServerSettingsFormContainer_ServerSettings"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ServerSettings"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"libraryPath"}}]}}]} as unknown as DocumentNode<SettingsPageQuery, SettingsPageQueryVariables>;
export const ProfilePageDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ProfilePage"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"UserProfilePanel_User"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"UserProfilePanel_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}},{"kind":"Field","name":{"kind":"Name","value":"createdAt"}},{"kind":"Field","name":{"kind":"Name","value":"updatedAt"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]} as unknown as DocumentNode<ProfilePageQuery, ProfilePageQueryVariables>;
export const AddArtistToServerLibraryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"AddArtistToServerLibrary"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"addArtistToServerLibrary"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}}]}}]}}]} as unknown as DocumentNode<AddArtistToServerLibraryMutation, AddArtistToServerLibraryMutationVariables>;
export const RefreshArtistDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"RefreshArtist"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"addArtistToServerLibrary"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}}]}}]}}]} as unknown as DocumentNode<RefreshArtistMutation, RefreshArtistMutationVariables>;
export const ArtistAlbumListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistAlbumList"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroupBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]} as unknown as DocumentNode<ArtistAlbumListQuery, ArtistAlbumListQueryVariables>;
export const ArtistEpListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistEpList"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroupBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]} as unknown as DocumentNode<ArtistEpListQuery, ArtistEpListQueryVariables>;
export const ArtistSingleListDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistSingleList"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroupBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]} as unknown as DocumentNode<ArtistSingleListQuery, ArtistSingleListQueryVariables>;
export const TopArtistTracksDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"TopArtistTracks"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TopArtistTrackItem_LastFmTrack"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"RecordingPlayButton_Recording"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Recording"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"streamingServiceInfo"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"youtubeVideoId"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistTrackItem_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"summary"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"RecordingPlayButton_Recording"}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]}}]} as unknown as DocumentNode<TopArtistTracksQuery, TopArtistTracksQueryVariables>;
export const ArtistServerStatusDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistServerStatus"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"serverStatus"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"result"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusResultBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releasesVisible"}},{"kind":"Field","name":{"kind":"Name","value":"topTracksVisible"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusImportingArtistReleases"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"totalNumReleaseGroupsBeingImported"}},{"kind":"Field","name":{"kind":"Name","value":"numReleaseGroupsFinishedImporting"}}]}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<ArtistServerStatusQuery, ArtistServerStatusQueryVariables>;
export const ArtistServerStatusSubDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"ArtistServerStatusSub"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistServerStatusUpdated"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"result"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusResultBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releasesVisible"}},{"kind":"Field","name":{"kind":"Name","value":"topTracksVisible"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistServerStatusImportingArtistReleases"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"totalNumReleaseGroupsBeingImported"}},{"kind":"Field","name":{"kind":"Name","value":"numReleaseGroupsFinishedImporting"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"albums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}},{"kind":"Field","name":{"kind":"Name","value":"eps"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}},{"kind":"Field","name":{"kind":"Name","value":"singles"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseDate"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroupBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]} as unknown as DocumentNode<ArtistServerStatusSubSubscription, ArtistServerStatusSubSubscriptionVariables>;
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
export const TopArtistRecommendationsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"TopArtistRecommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"topArtists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TopArtistCard_Artist"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmArtist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"statistics"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"listeners"}}]}}]}}]} as unknown as DocumentNode<TopArtistRecommendationsQuery, TopArtistRecommendationsQueryVariables>;
export const TopTrackRecommendationsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"TopTrackRecommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recommendations"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"TopTrackCard_LastFmTrack"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"TopTrackCard_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"musicBrainzArtist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"album"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"imageUrl"}}]}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]} as unknown as DocumentNode<TopTrackRecommendationsQuery, TopTrackRecommendationsQueryVariables>;
export const AlbumSearchResultsSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"AlbumSearchResultsSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroupBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"firstReleaseYear"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"MbReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"credits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]} as unknown as DocumentNode<AlbumSearchResultsSearchQuery, AlbumSearchResultsSearchQueryVariables>;
export const ArtistSearchResultsSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistSearchResultsSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"searchText"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"searchText"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistCard_Artist"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistCard_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ArtistBase"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]} as unknown as DocumentNode<ArtistSearchResultsSearchQuery, ArtistSearchResultsSearchQueryVariables>;
export const RecordingSearchResultsSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"RecordingSearchResultsSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"limit"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"Int"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"musicBrainz"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"Variable","name":{"kind":"Name","value":"limit"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"nameCredits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]}}]}}]}}]} as unknown as DocumentNode<RecordingSearchResultsSearchQuery, RecordingSearchResultsSearchQueryVariables>;
export const SearchResultArtistSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchResultArtistSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]}}]}}]} as unknown as DocumentNode<SearchResultArtistSearchQuery, SearchResultArtistSearchQueryVariables>;
export const SearchResultSongSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchResultSongSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"nameCredits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]}}]}}]} as unknown as DocumentNode<SearchResultSongSearchQuery, SearchResultSongSearchQueryVariables>;
export const SearchResultAlbumSearchDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchResultAlbumSearch"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]}}]}}]} as unknown as DocumentNode<SearchResultAlbumSearchQuery, SearchResultAlbumSearchQueryVariables>;
export const UpdateServerSettingsDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"UpdateServerSettings"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"newLibraryPath"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"updateLibraryPath"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"newLibraryPath"},"value":{"kind":"Variable","name":{"kind":"Name","value":"newLibraryPath"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"UpdateLibraryPathSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverSettings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"libraryPath"}}]}}]}}]}}]}}]} as unknown as DocumentNode<UpdateServerSettingsMutation, UpdateServerSettingsMutationVariables>;
export const DirectoryTreeItemDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"DirectoryTreeItem"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"path"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"browseFileSystem"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"path"},"value":{"kind":"Variable","name":{"kind":"Name","value":"path"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"FileSystemEntry"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"Field","name":{"kind":"Name","value":"hasChildren"}},{"kind":"Field","name":{"kind":"Name","value":"isAccessible"}}]}}]} as unknown as DocumentNode<DirectoryTreeItemQuery, DirectoryTreeItemQueryVariables>;
export const BrowseFileSystemDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"BrowseFileSystem"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"path"}},"type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"browseFileSystem"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"path"},"value":{"kind":"Variable","name":{"kind":"Name","value":"path"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DirectoryTreeItem_FileSystemEntry"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"FileSystemEntry"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"path"}},{"kind":"Field","name":{"kind":"Name","value":"isDirectory"}},{"kind":"Field","name":{"kind":"Name","value":"hasChildren"}},{"kind":"Field","name":{"kind":"Name","value":"isAccessible"}}]}}]} as unknown as DocumentNode<BrowseFileSystemQuery, BrowseFileSystemQueryVariables>;
export const SidebarDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"Sidebar"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]}}]} as unknown as DocumentNode<SidebarQuery, SidebarQueryVariables>;
export const SoulSeekNetworkStatusSubscriptionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"SoulSeekNetworkStatusSubscription"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"soulSeekStatusUpdated"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatusSubscriptionSubscription, SoulSeekNetworkStatusSubscriptionSubscriptionVariables>;
export const SoulSeekNetworkStatusQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SoulSeekNetworkStatusQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"external"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"soulSeek"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatusQueryQuery, SoulSeekNetworkStatusQueryQueryVariables>;
export const ImportSpotifyPlaylistByIdDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"ImportSpotifyPlaylistById"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}},{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"userId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"UUID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"importSpotifyPlaylistById"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"playlistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"playlistId"}}},{"kind":"Argument","name":{"kind":"Name","value":"userId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"userId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ImportSpotifyPlaylistSuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"success"}}]}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ImportSpotifyPlaylistError"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"message"}}]}}]}}]}}]} as unknown as DocumentNode<ImportSpotifyPlaylistByIdMutation, ImportSpotifyPlaylistByIdMutationVariables>;
export const UserPlaylistsLoader_QueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"UserPlaylistsLoader_Query"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"spotifyUsername"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"playlist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"importPlaylists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"spotify"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"spotifyPlaylistsForUser"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"username"},"value":{"kind":"Variable","name":{"kind":"Name","value":"spotifyUsername"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"SpotifyPlaylistsList_SpotifyPlaylist"}}]}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SpotifyPlaylistsList_SpotifyPlaylist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SpotifyPlaylist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"description"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"coverImageUrl"}}]}}]} as unknown as DocumentNode<UserPlaylistsLoader_QueryQuery, UserPlaylistsLoader_QueryQueryVariables>;
export const UserProfileWidgetDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"UserProfileWidget"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"username"}}]}}]}}]} as unknown as DocumentNode<UserProfileWidgetQuery, UserProfileWidgetQueryVariables>;