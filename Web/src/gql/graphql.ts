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
  /** The `Long` scalar type represents non-fractional signed whole 64-bit numeric values. Long can represent values between -(2^63) and 2^63 - 1. */
  Long: { input: any; output: any; }
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
  serverAvailability: ArtistServerAvailability;
};

export type AddArtistToServerLibraryUnknownError = {
  __typename?: 'AddArtistToServerLibraryUnknownError';
  message: Scalars['String']['output'];
};

export type AddReleaseGroupToServerLibraryInput = {
  releaseGroupId: Scalars['String']['input'];
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
  viewer: User;
};

export type AddReleaseGroupToServerLibraryUnknownError = {
  __typename?: 'AddReleaseGroupToServerLibraryUnknownError';
  message: Scalars['String']['output'];
};

export type Artist = {
  __typename?: 'Artist';
  disambiguation?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  images?: Maybe<ArtistImages>;
  listeners?: Maybe<Scalars['Long']['output']>;
  mainAlbums: Array<Release>;
  name: Scalars['String']['output'];
  releaseGroups: Array<ReleaseGroup>;
  releases: Array<Release>;
  serverAvailability: ArtistServerAvailability;
  sortName: Scalars['String']['output'];
  topTracks: Array<LastFmTrack>;
  type?: Maybe<Scalars['String']['output']>;
};

export type ArtistImages = {
  __typename?: 'ArtistImages';
  artistBackground?: Maybe<Scalars['String']['output']>;
  artistThumb?: Maybe<Scalars['String']['output']>;
  hdMusicLogo?: Maybe<Scalars['String']['output']>;
  musicBanner?: Maybe<Scalars['String']['output']>;
  musicLogo?: Maybe<Scalars['String']['output']>;
};

export type ArtistInServerLibrary = {
  __typename?: 'ArtistInServerLibrary';
  artist?: Maybe<Artist>;
};

export type ArtistSearchRoot = {
  __typename?: 'ArtistSearchRoot';
  byId?: Maybe<Artist>;
  id: Scalars['ID']['output'];
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

export type ArtistServerAvailability = {
  __typename?: 'ArtistServerAvailability';
  artist?: Maybe<Artist>;
  id: Scalars['ID']['output'];
  isInServerLibrary: Scalars['Boolean']['output'];
};

export type ArtistsInServerLibrarySearchRoot = {
  __typename?: 'ArtistsInServerLibrarySearchRoot';
  all: Array<ArtistInServerLibrary>;
};

export type DownloadStatus = {
  __typename?: 'DownloadStatus';
  artistName?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  numberOfTracks?: Maybe<Scalars['Int']['output']>;
  release?: Maybe<Release>;
  releaseName?: Maybe<Scalars['String']['output']>;
  statusDescription: Scalars['String']['output'];
  tracksDownloaded?: Maybe<Scalars['Int']['output']>;
};

export type DownloadsSearchRoot = {
  __typename?: 'DownloadsSearchRoot';
  all: Array<DownloadStatus>;
  id: Scalars['ID']['output'];
};

export type ExternalRoot = {
  __typename?: 'ExternalRoot';
  id: Scalars['ID']['output'];
  soulSeek: SoulSeekRoot;
};

export type Genre = {
  __typename?: 'Genre';
  id: Scalars['ID']['output'];
  name: Scalars['String']['output'];
};

export type LastFmTrack = {
  __typename?: 'LastFmTrack';
  id: Scalars['ID']['output'];
  playCount?: Maybe<Scalars['Long']['output']>;
  recording?: Maybe<Recording>;
  summary?: Maybe<Scalars['String']['output']>;
  userPlayCount?: Maybe<Scalars['Int']['output']>;
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
  recording?: Maybe<Recording>;
};

export type Medium = {
  __typename?: 'Medium';
  trackCount: Scalars['Int']['output'];
  tracks: Array<Track>;
};

export type Mutation = {
  __typename?: 'Mutation';
  addArtistToServerLibrary: AddArtistToServerLibraryResult;
  addReleaseGroupToServerLibrary: AddReleaseGroupToServerLibraryResult;
  likeSong: LikeSongResult;
  ping: Scalars['Boolean']['output'];
  startDownloadRelease: StartDownloadReleaseResult;
  unlikeSong: UnlikedSongPayload;
};


export type MutationAddArtistToServerLibraryArgs = {
  input: AddArtistToServerLibraryInput;
};


export type MutationAddReleaseGroupToServerLibraryArgs = {
  input: AddReleaseGroupToServerLibraryInput;
};


export type MutationLikeSongArgs = {
  input: LikeSongInput;
};


export type MutationStartDownloadReleaseArgs = {
  input: StartDownloadReleaseInput;
};


export type MutationUnlikeSongArgs = {
  input: UnlikedSongInput;
};

export type NameCredit = {
  __typename?: 'NameCredit';
  artist: Artist;
  joinPhrase?: Maybe<Scalars['String']['output']>;
  name: Scalars['String']['output'];
};

export type Ping = {
  __typename?: 'Ping';
  id: Scalars['ID']['output'];
};

export type Query = {
  __typename?: 'Query';
  artist: ArtistSearchRoot;
  download: DownloadsSearchRoot;
  external: ExternalRoot;
  recording: RecordingSearchRoot;
  release: ReleaseSearchRoot;
  releaseGroup: ReleaseGroupSearchRoot;
  serverLibrary: ServerLibrarySearchRoot;
  viewer: User;
};

export type Recording = {
  __typename?: 'Recording';
  artists: Array<Artist>;
  id: Scalars['ID']['output'];
  length?: Maybe<Scalars['Int']['output']>;
  mainAlbum?: Maybe<Release>;
  nameCredits: Array<NameCredit>;
  releases: Array<Release>;
  title: Scalars['String']['output'];
};

export type RecordingSearchRoot = {
  __typename?: 'RecordingSearchRoot';
  byId?: Maybe<Recording>;
  id: Scalars['ID']['output'];
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

export type Release = {
  __typename?: 'Release';
  artists: Array<Artist>;
  barcode?: Maybe<Scalars['String']['output']>;
  country?: Maybe<Scalars['String']['output']>;
  coverArtUri: Scalars['String']['output'];
  credits: Array<NameCredit>;
  date?: Maybe<Scalars['String']['output']>;
  genres: Array<Genre>;
  id: Scalars['ID']['output'];
  media: Array<Medium>;
  quality?: Maybe<Scalars['String']['output']>;
  recordings: Array<Recording>;
  releaseGroup?: Maybe<ReleaseGroup>;
  status?: Maybe<Scalars['String']['output']>;
  title: Scalars['String']['output'];
  year?: Maybe<Scalars['String']['output']>;
};

export type ReleaseGroup = {
  __typename?: 'ReleaseGroup';
  firstReleaseDate?: Maybe<Scalars['String']['output']>;
  id: Scalars['ID']['output'];
  mainRelease?: Maybe<Release>;
  primaryType?: Maybe<Scalars['String']['output']>;
  secondaryTypes: Array<Scalars['String']['output']>;
  title: Scalars['String']['output'];
};

export type ReleaseGroupInServerLibrary = {
  __typename?: 'ReleaseGroupInServerLibrary';
  releaseGroup?: Maybe<ReleaseGroup>;
};

export type ReleaseGroupSearchRoot = {
  __typename?: 'ReleaseGroupSearchRoot';
  byId?: Maybe<ReleaseGroup>;
  id: Scalars['ID']['output'];
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

export type ReleaseGroupsInServerLibrarySearchRoot = {
  __typename?: 'ReleaseGroupsInServerLibrarySearchRoot';
  all: Array<ReleaseGroupInServerLibrary>;
};

export type ReleaseSearchRoot = {
  __typename?: 'ReleaseSearchRoot';
  byId?: Maybe<Release>;
  id: Scalars['ID']['output'];
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

export type ServerLibrarySearchRoot = {
  __typename?: 'ServerLibrarySearchRoot';
  artistsInServerLibrary: ArtistsInServerLibrarySearchRoot;
  releaseGroupsInServerLibrary: ReleaseGroupsInServerLibrarySearchRoot;
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
  downloadStarted: DownloadStatus;
  downloadStatusUpdated: DownloadStatus;
  ping: Ping;
  soulSeekStatusUpdated: SoulSeekStatus;
};

export type Track = {
  __typename?: 'Track';
  id: Scalars['ID']['output'];
  position: Scalars['Int']['output'];
  recording: Recording;
};

export type UnlikedSongInput = {
  recordingId: Scalars['String']['input'];
};

export type UnlikedSongPayload = {
  __typename?: 'UnlikedSongPayload';
  success: Scalars['Boolean']['output'];
};

export type User = {
  __typename?: 'User';
  id: Scalars['Int']['output'];
  likedSongs: Array<LikedSong>;
};

export type AlbumQueryQueryVariables = Exact<{
  releaseId: Scalars['ID']['input'];
}>;


export type AlbumQueryQuery = { __typename?: 'Query', release: { __typename?: 'ReleaseSearchRoot', id: string, byId?: (
      { __typename?: 'Release', id: string }
      & { ' $fragmentRefs'?: { 'AlbumPanel_ReleaseFragment': AlbumPanel_ReleaseFragment } }
    ) | null } };

export type ArtistQueryQueryVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type ArtistQueryQuery = { __typename?: 'Query', artist: { __typename?: 'ArtistSearchRoot', byId?: (
      { __typename?: 'Artist', id: string }
      & { ' $fragmentRefs'?: { 'ArtistPanel_ArtistFragment': ArtistPanel_ArtistFragment } }
    ) | null } };

export type LikedSongsQueryQueryVariables = Exact<{ [key: string]: never; }>;


export type LikedSongsQueryQuery = { __typename?: 'Query', viewer: (
    { __typename?: 'User', id: number }
    & { ' $fragmentRefs'?: { 'LikedSongsList_UserFragment': LikedSongsList_UserFragment } }
  ) };

export type Playlist_UserFragment = { __typename?: 'User', id: number, likedSongs: Array<(
    { __typename?: 'LikedSong', id: string }
    & { ' $fragmentRefs'?: { 'LikedSongRow_LikedSongFragment': LikedSongRow_LikedSongFragment } }
  )> } & { ' $fragmentName'?: 'Playlist_UserFragment' };

export type LikedSongRow_RecodingFragment = { __typename?: 'Recording', id: string, title: string, length?: number | null, artists: Array<{ __typename?: 'Artist', id: string, name: string }>, mainAlbum?: { __typename?: 'Release', id: string, title: string, coverArtUri: string } | null } & { ' $fragmentName'?: 'LikedSongRow_RecodingFragment' };

export type AddArtistToServerLibraryMutationVariables = Exact<{
  artistId: Scalars['ID']['input'];
}>;


export type AddArtistToServerLibraryMutation = { __typename?: 'Mutation', addArtistToServerLibrary: { __typename: 'AddArtistToServerLibraryArtistAlreadyAdded' } | { __typename: 'AddArtistToServerLibraryArtistDoesNotExist' } | { __typename: 'AddArtistToServerLibrarySuccess', serverAvailability: { __typename?: 'ArtistServerAvailability', id: string, isInServerLibrary: boolean } } | { __typename: 'AddArtistToServerLibraryUnknownError' } };

export type ArtistInLibraryButton_ArtistFragment = { __typename?: 'Artist', id: string, serverAvailability: { __typename?: 'ArtistServerAvailability', id: string, isInServerLibrary: boolean } } & { ' $fragmentName'?: 'ArtistInLibraryButton_ArtistFragment' };

export type AlbumCard_ReleaseFragment = { __typename?: 'Release', id: string, title: string, year?: string | null, coverArtUri: string, artists: Array<{ __typename?: 'Artist', id: string, name: string }> } & { ' $fragmentName'?: 'AlbumCard_ReleaseFragment' };

export type AlbumHeader_ReleaseFragment = { __typename?: 'Release', id: string, title: string, coverArtUri: string, artists: Array<{ __typename?: 'Artist', id: string, name: string }> } & { ' $fragmentName'?: 'AlbumHeader_ReleaseFragment' };

export type AlbumPanel_ReleaseFragment = (
  { __typename?: 'Release', id: string }
  & { ' $fragmentRefs'?: { 'AlbumHeader_ReleaseFragment': AlbumHeader_ReleaseFragment;'AlbumTrackList_ReleaseFragment': AlbumTrackList_ReleaseFragment } }
) & { ' $fragmentName'?: 'AlbumPanel_ReleaseFragment' };

export type AlbumTrackList_ReleaseFragment = { __typename?: 'Release', id: string, recordings: Array<{ __typename?: 'Recording', id: string, title: string, length?: number | null, nameCredits: Array<{ __typename?: 'NameCredit', name: string, artist: { __typename?: 'Artist', id: string, name: string } }> }> } & { ' $fragmentName'?: 'AlbumTrackList_ReleaseFragment' };

export type ArtistAlbumList_ArtistFragment = { __typename?: 'Artist', id: string, mainAlbums: Array<(
    { __typename?: 'Release', id: string, date?: string | null, year?: string | null, country?: string | null, releaseGroup?: { __typename?: 'ReleaseGroup', id: string, primaryType?: string | null, secondaryTypes: Array<string> } | null }
    & { ' $fragmentRefs'?: { 'AlbumCard_ReleaseFragment': AlbumCard_ReleaseFragment } }
  )> } & { ' $fragmentName'?: 'ArtistAlbumList_ArtistFragment' };

export type ArtistHeader_ArtistFragment = { __typename?: 'Artist', id: string, name: string, listeners?: any | null, images?: { __typename?: 'ArtistImages', artistBackground?: string | null } | null } & { ' $fragmentName'?: 'ArtistHeader_ArtistFragment' };

export type ArtistPanel_ArtistFragment = (
  { __typename?: 'Artist', id: string, name: string }
  & { ' $fragmentRefs'?: { 'ArtistHeader_ArtistFragment': ArtistHeader_ArtistFragment;'PopularArtistTracks_ArtistFragment': PopularArtistTracks_ArtistFragment;'ArtistAlbumList_ArtistFragment': ArtistAlbumList_ArtistFragment;'ArtistInLibraryButton_ArtistFragment': ArtistInLibraryButton_ArtistFragment } }
) & { ' $fragmentName'?: 'ArtistPanel_ArtistFragment' };

export type PopularArtistTracks_ArtistFragment = { __typename?: 'Artist', id: string, topTracks: Array<(
    { __typename?: 'LastFmTrack', id: string }
    & { ' $fragmentRefs'?: { 'PopularTrackRow_LastFmTrackFragment': PopularTrackRow_LastFmTrackFragment } }
  )> } & { ' $fragmentName'?: 'PopularArtistTracks_ArtistFragment' };

export type PopularTrackRow_LastFmTrackFragment = { __typename?: 'LastFmTrack', id: string, playCount?: any | null, summary?: string | null, recording?: { __typename?: 'Recording', id: string, title: string, length?: number | null, mainAlbum?: { __typename?: 'Release', id: string, title: string, coverArtUri: string } | null } | null } & { ' $fragmentName'?: 'PopularTrackRow_LastFmTrackFragment' };

export type DownloadOverview_DownloadStatusFragment = (
  { __typename?: 'DownloadStatus', id: string }
  & { ' $fragmentRefs'?: { 'DownloadStatus_DownloadStatusFragment': DownloadStatus_DownloadStatusFragment } }
) & { ' $fragmentName'?: 'DownloadOverview_DownloadStatusFragment' };

export type DownloadOverviewQueryQueryVariables = Exact<{ [key: string]: never; }>;


export type DownloadOverviewQueryQuery = { __typename?: 'Query', download: { __typename?: 'DownloadsSearchRoot', id: string, all: Array<(
      { __typename?: 'DownloadStatus', id: string }
      & { ' $fragmentRefs'?: { 'DownloadOverview_DownloadStatusFragment': DownloadOverview_DownloadStatusFragment } }
    )> } };

export type DownloadOverviewSubscriptionSubscriptionVariables = Exact<{ [key: string]: never; }>;


export type DownloadOverviewSubscriptionSubscription = { __typename?: 'Subscription', downloadStatusUpdated: (
    { __typename?: 'DownloadStatus', id: string }
    & { ' $fragmentRefs'?: { 'DownloadOverview_DownloadStatusFragment': DownloadOverview_DownloadStatusFragment } }
  ) };

export type DownloadStatus_DownloadStatusFragment = { __typename?: 'DownloadStatus', id: string, numberOfTracks?: number | null, tracksDownloaded?: number | null, statusDescription: string, release?: { __typename?: 'Release', id: string, title: string, year?: string | null, artists: Array<{ __typename?: 'Artist', id: string, name: string }>, recordings: Array<{ __typename?: 'Recording', id: string }> } | null } & { ' $fragmentName'?: 'DownloadStatus_DownloadStatusFragment' };

export type LikedSongRow_LikedSongFragment = { __typename?: 'LikedSong', id: string, recording?: { __typename?: 'Recording', id: string, title: string, length?: number | null, artists: Array<{ __typename?: 'Artist', id: string, name: string }>, mainAlbum?: { __typename?: 'Release', id: string, title: string, coverArtUri: string, artists: Array<{ __typename?: 'Artist', id: string }> } | null } | null } & { ' $fragmentName'?: 'LikedSongRow_LikedSongFragment' };

export type LikedSongsList_UserFragment = { __typename?: 'User', id: number, likedSongs: Array<(
    { __typename?: 'LikedSong', id: string }
    & { ' $fragmentRefs'?: { 'LikedSongRow_LikedSongFragment': LikedSongRow_LikedSongFragment } }
  )> } & { ' $fragmentName'?: 'LikedSongsList_UserFragment' };

export type SearchPanelQueryQueryVariables = Exact<{
  text: Scalars['String']['input'];
}>;


export type SearchPanelQueryQuery = (
  { __typename?: 'Query' }
  & { ' $fragmentRefs'?: { 'SearchResult_QueryFragment': SearchResult_QueryFragment } }
);

export type SearchResult_QueryFragment = { __typename?: 'Query', artist: { __typename?: 'ArtistSearchRoot', searchByName: Array<(
      { __typename?: 'Artist' }
      & { ' $fragmentRefs'?: { 'SearchResultArtist_ArtistFragment': SearchResultArtist_ArtistFragment } }
    )> }, releaseGroup: { __typename?: 'ReleaseGroupSearchRoot', searchByName: Array<(
      { __typename?: 'ReleaseGroup' }
      & { ' $fragmentRefs'?: { 'SearchResultReleaseGroup_ReleaseGroupFragment': SearchResultReleaseGroup_ReleaseGroupFragment } }
    )> }, recording: { __typename?: 'RecordingSearchRoot', searchByName: Array<(
      { __typename?: 'Recording' }
      & { ' $fragmentRefs'?: { 'SearchResultRecording_RecordingFragment': SearchResultRecording_RecordingFragment } }
    )> } } & { ' $fragmentName'?: 'SearchResult_QueryFragment' };

export type SearchResultArtist_ArtistFragment = { __typename?: 'Artist', id: string, name: string, images?: { __typename?: 'ArtistImages', artistThumb?: string | null } | null } & { ' $fragmentName'?: 'SearchResultArtist_ArtistFragment' };

export type SearchResultRecording_RecordingFragment = { __typename?: 'Recording', id: string, title: string, length?: number | null, artists: Array<{ __typename?: 'Artist', id: string, name: string }>, mainAlbum?: { __typename?: 'Release', id: string, title: string, coverArtUri: string } | null } & { ' $fragmentName'?: 'SearchResultRecording_RecordingFragment' };

export type SearchResultReleaseGroup_ReleaseGroupFragment = { __typename?: 'ReleaseGroup', id: string, title: string, mainRelease?: { __typename?: 'Release', id: string, title: string, coverArtUri: string } | null } & { ' $fragmentName'?: 'SearchResultReleaseGroup_ReleaseGroupFragment' };

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

export const LikedSongRow_LikedSongFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]} as unknown as DocumentNode<LikedSongRow_LikedSongFragment, unknown>;
export const Playlist_UserFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"Playlist_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongRow_LikedSong"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]} as unknown as DocumentNode<Playlist_UserFragment, unknown>;
export const LikedSongRow_RecodingFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_Recoding"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Recording"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]} as unknown as DocumentNode<LikedSongRow_RecodingFragment, unknown>;
export const AlbumHeader_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]} as unknown as DocumentNode<AlbumHeader_ReleaseFragment, unknown>;
export const AlbumTrackList_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"nameCredits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]} as unknown as DocumentNode<AlbumTrackList_ReleaseFragment, unknown>;
export const AlbumPanel_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_Release"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"nameCredits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}}]} as unknown as DocumentNode<AlbumPanel_ReleaseFragment, unknown>;
export const ArtistHeader_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistHeader_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistBackground"}}]}}]}}]} as unknown as DocumentNode<ArtistHeader_ArtistFragment, unknown>;
export const PopularTrackRow_LastFmTrackFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PopularTrackRow_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"summary"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]}}]} as unknown as DocumentNode<PopularTrackRow_LastFmTrackFragment, unknown>;
export const PopularArtistTracks_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PopularArtistTracks_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PopularTrackRow_LastFmTrack"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PopularTrackRow_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"summary"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]}}]} as unknown as DocumentNode<PopularArtistTracks_ArtistFragment, unknown>;
export const AlbumCard_ReleaseFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]} as unknown as DocumentNode<AlbumCard_ReleaseFragment, unknown>;
export const ArtistAlbumList_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistAlbumList_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"date"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"country"}},{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"primaryType"}},{"kind":"Field","name":{"kind":"Name","value":"secondaryTypes"}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]} as unknown as DocumentNode<ArtistAlbumList_ArtistFragment, unknown>;
export const ArtistInLibraryButton_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistInLibraryButton_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"serverAvailability"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isInServerLibrary"}}]}}]}}]} as unknown as DocumentNode<ArtistInLibraryButton_ArtistFragment, unknown>;
export const ArtistPanel_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistPanel_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistHeader_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PopularArtistTracks_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistAlbumList_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistInLibraryButton_Artist"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PopularTrackRow_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"summary"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistHeader_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistBackground"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PopularArtistTracks_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PopularTrackRow_LastFmTrack"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistAlbumList_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"date"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"country"}},{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"primaryType"}},{"kind":"Field","name":{"kind":"Name","value":"secondaryTypes"}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistInLibraryButton_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"serverAvailability"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isInServerLibrary"}}]}}]}}]} as unknown as DocumentNode<ArtistPanel_ArtistFragment, unknown>;
export const DownloadStatus_DownloadStatusFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"numberOfTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracksDownloaded"}},{"kind":"Field","name":{"kind":"Name","value":"statusDescription"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]} as unknown as DocumentNode<DownloadStatus_DownloadStatusFragment, unknown>;
export const DownloadOverview_DownloadStatusFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadOverview_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"numberOfTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracksDownloaded"}},{"kind":"Field","name":{"kind":"Name","value":"statusDescription"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]} as unknown as DocumentNode<DownloadOverview_DownloadStatusFragment, unknown>;
export const LikedSongsList_UserFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongsList_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongRow_LikedSong"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}}]} as unknown as DocumentNode<LikedSongsList_UserFragment, unknown>;
export const SearchResultArtist_ArtistFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SearchResultArtist_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}}]} as unknown as DocumentNode<SearchResultArtist_ArtistFragment, unknown>;
export const SearchResultReleaseGroup_ReleaseGroupFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SearchResultReleaseGroup_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]} as unknown as DocumentNode<SearchResultReleaseGroup_ReleaseGroupFragment, unknown>;
export const SearchResultRecording_RecordingFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SearchResultRecording_Recording"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Recording"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]} as unknown as DocumentNode<SearchResultRecording_RecordingFragment, unknown>;
export const SearchResult_QueryFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SearchResult_Query"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Query"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SearchResultArtist_Artist"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SearchResultReleaseGroup_ReleaseGroup"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SearchResultRecording_Recording"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SearchResultArtist_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SearchResultReleaseGroup_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SearchResultRecording_Recording"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Recording"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]} as unknown as DocumentNode<SearchResult_QueryFragment, unknown>;
export const SoulSeekNetworkStatus_SoulSeekStatusFragmentDoc = {"kind":"Document","definitions":[{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatus_SoulSeekStatusFragment, unknown>;
export const AlbumQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"AlbumQuery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"releaseId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"releaseId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumPanel_Release"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumHeader_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumTrackList_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"nameCredits"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumPanel_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumHeader_Release"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumTrackList_Release"}}]}}]} as unknown as DocumentNode<AlbumQueryQuery, AlbumQueryQueryVariables>;
export const ArtistQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"ArtistQuery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"byId"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"id"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistPanel_Artist"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistHeader_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"listeners"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistBackground"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PopularTrackRow_LastFmTrack"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LastFmTrack"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"playCount"}},{"kind":"Field","name":{"kind":"Name","value":"summary"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"PopularArtistTracks_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"topTracks"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PopularTrackRow_LastFmTrack"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"AlbumCard_Release"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Release"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistAlbumList_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbums"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"date"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"country"}},{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"primaryType"}},{"kind":"Field","name":{"kind":"Name","value":"secondaryTypes"}}]}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"AlbumCard_Release"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistInLibraryButton_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"serverAvailability"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isInServerLibrary"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"ArtistPanel_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistHeader_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"PopularArtistTracks_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistAlbumList_Artist"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"ArtistInLibraryButton_Artist"}}]}}]} as unknown as DocumentNode<ArtistQueryQuery, ArtistQueryQueryVariables>;
export const LikedSongsQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"LikedSongsQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"viewer"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongsList_User"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongRow_LikedSong"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"LikedSong"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"LikedSongsList_User"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"User"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"likedSongs"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"LikedSongRow_LikedSong"}}]}}]}}]} as unknown as DocumentNode<LikedSongsQueryQuery, LikedSongsQueryQueryVariables>;
export const AddArtistToServerLibraryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"mutation","name":{"kind":"Name","value":"AddArtistToServerLibrary"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"ID"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"addArtistToServerLibrary"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"input"},"value":{"kind":"ObjectValue","fields":[{"kind":"ObjectField","name":{"kind":"Name","value":"artistId"},"value":{"kind":"Variable","name":{"kind":"Name","value":"artistId"}}}]}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"__typename"}},{"kind":"InlineFragment","typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"AddArtistToServerLibrarySuccess"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"serverAvailability"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"isInServerLibrary"}}]}}]}}]}}]}}]} as unknown as DocumentNode<AddArtistToServerLibraryMutation, AddArtistToServerLibraryMutationVariables>;
export const DownloadOverviewQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"DownloadOverviewQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"download"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"all"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadOverview_DownloadStatus"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"numberOfTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracksDownloaded"}},{"kind":"Field","name":{"kind":"Name","value":"statusDescription"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadOverview_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"}}]}}]} as unknown as DocumentNode<DownloadOverviewQueryQuery, DownloadOverviewQueryQueryVariables>;
export const DownloadOverviewSubscriptionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"DownloadOverviewSubscription"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"downloadStatusUpdated"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadOverview_DownloadStatus"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"numberOfTracks"}},{"kind":"Field","name":{"kind":"Name","value":"tracksDownloaded"}},{"kind":"Field","name":{"kind":"Name","value":"statusDescription"}},{"kind":"Field","name":{"kind":"Name","value":"release"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"year"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"recordings"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"DownloadOverview_DownloadStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"DownloadStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"FragmentSpread","name":{"kind":"Name","value":"DownloadStatus_DownloadStatus"}}]}}]} as unknown as DocumentNode<DownloadOverviewSubscriptionSubscription, DownloadOverviewSubscriptionSubscriptionVariables>;
export const SearchPanelQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SearchPanelQuery"},"variableDefinitions":[{"kind":"VariableDefinition","variable":{"kind":"Variable","name":{"kind":"Name","value":"text"}},"type":{"kind":"NonNullType","type":{"kind":"NamedType","name":{"kind":"Name","value":"String"}}}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SearchResult_Query"}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SearchResultArtist_Artist"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Artist"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}},{"kind":"Field","name":{"kind":"Name","value":"images"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artistThumb"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SearchResultReleaseGroup_ReleaseGroup"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"ReleaseGroup"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"mainRelease"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SearchResultRecording_Recording"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Recording"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"length"}},{"kind":"Field","name":{"kind":"Name","value":"artists"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"name"}}]}},{"kind":"Field","name":{"kind":"Name","value":"mainAlbum"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"title"}},{"kind":"Field","name":{"kind":"Name","value":"coverArtUri"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SearchResult_Query"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"Query"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"artist"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SearchResultArtist_Artist"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"releaseGroup"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SearchResultReleaseGroup_ReleaseGroup"}}]}}]}},{"kind":"Field","name":{"kind":"Name","value":"recording"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"searchByName"},"arguments":[{"kind":"Argument","name":{"kind":"Name","value":"name"},"value":{"kind":"Variable","name":{"kind":"Name","value":"text"}}},{"kind":"Argument","name":{"kind":"Name","value":"limit"},"value":{"kind":"IntValue","value":"5"}}],"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SearchResultRecording_Recording"}}]}}]}}]}}]} as unknown as DocumentNode<SearchPanelQueryQuery, SearchPanelQueryQueryVariables>;
export const SoulSeekNetworkStatusSubscriptionDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"subscription","name":{"kind":"Name","value":"SoulSeekNetworkStatusSubscription"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"soulSeekStatusUpdated"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatusSubscriptionSubscription, SoulSeekNetworkStatusSubscriptionSubscriptionVariables>;
export const SoulSeekNetworkStatusQueryDocument = {"kind":"Document","definitions":[{"kind":"OperationDefinition","operation":"query","name":{"kind":"Name","value":"SoulSeekNetworkStatusQuery"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"external"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"soulSeek"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"FragmentSpread","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"}}]}}]}}]}}]}},{"kind":"FragmentDefinition","name":{"kind":"Name","value":"SoulSeekNetworkStatus_SoulSeekStatus"},"typeCondition":{"kind":"NamedType","name":{"kind":"Name","value":"SoulSeekStatus"}},"selectionSet":{"kind":"SelectionSet","selections":[{"kind":"Field","name":{"kind":"Name","value":"id"}},{"kind":"Field","name":{"kind":"Name","value":"status"}}]}}]} as unknown as DocumentNode<SoulSeekNetworkStatusQueryQuery, SoulSeekNetworkStatusQueryQueryVariables>;