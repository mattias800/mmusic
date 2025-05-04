/* eslint-disable */
import * as types from './graphql';
import { TypedDocumentNode as DocumentNode } from '@graphql-typed-document-node/core';

/**
 * Map of all GraphQL operations in the project.
 *
 * This map has several performance disadvantages:
 * 1. It is not tree-shakeable, so it will include all operations in the project.
 * 2. It is not minifiable, so the string of a GraphQL query will be multiple times inside the bundle.
 * 3. It does not support dead code elimination, so it will add unused operations.
 *
 * Therefore it is highly recommended to use the babel or swc plugin for production.
 * Learn more about it here: https://the-guild.dev/graphql/codegen/plugins/presets/preset-client#reducing-bundle-size
 */
type Documents = {
    "\n  query AlbumQuery($releaseId: ID!) {\n    release {\n      byId(id: $releaseId) {\n        id\n        ...AlbumPanel_Release\n      }\n    }\n  }\n": typeof types.AlbumQueryDocument,
    "\n  query ArtistQuery($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        ...ArtistPanel_Artist\n      }\n    }\n  }\n": typeof types.ArtistQueryDocument,
    "\n  query LikedSongsQuery {\n    viewer {\n      id\n      ...LikedSongsList_User\n    }\n  }\n": typeof types.LikedSongsQueryDocument,
    "\n  fragment Playlist_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": typeof types.Playlist_UserFragmentDoc,
    "\n  fragment LikedSongRow_Recoding on Recording {\n    id\n    title\n    length\n    artists {\n      id\n      name\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n": typeof types.LikedSongRow_RecodingFragmentDoc,
    "\n  fragment AlbumCard_Release on Release {\n    id\n    title\n    year\n    coverArtUri\n    artists {\n      id\n      name\n    }\n  }\n": typeof types.AlbumCard_ReleaseFragmentDoc,
    "\n  fragment AlbumHeader_Release on Release {\n    id\n    title\n    coverArtUri\n    artists {\n      id\n      name\n    }\n  }\n": typeof types.AlbumHeader_ReleaseFragmentDoc,
    "\n  fragment AlbumPanel_Release on Release {\n    id\n    ...AlbumHeader_Release\n    ...AlbumTrackList_Release\n  }\n": typeof types.AlbumPanel_ReleaseFragmentDoc,
    "\n  fragment AlbumTrackList_Release on Release {\n    id\n    recordings {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n    }\n  }\n": typeof types.AlbumTrackList_ReleaseFragmentDoc,
    "\n  fragment ArtistAlbumList_Artist on Artist {\n    id\n    mainAlbums {\n      id\n      date\n      year\n      country\n      releaseGroup {\n        id\n        primaryType\n        secondaryTypes\n      }\n      ...AlbumCard_Release\n    }\n  }\n": typeof types.ArtistAlbumList_ArtistFragmentDoc,
    "\n  fragment ArtistHeader_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistBackground\n    }\n  }\n": typeof types.ArtistHeader_ArtistFragmentDoc,
    "\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    ...ArtistHeader_Artist\n    ...PopularArtistTracks_Artist\n    ...ArtistAlbumList_Artist\n  }\n": typeof types.ArtistPanel_ArtistFragmentDoc,
    "\n  fragment PopularArtistTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      ...PopularTrackRow_LastFmTrack\n    }\n  }\n": typeof types.PopularArtistTracks_ArtistFragmentDoc,
    "\n  fragment PopularTrackRow_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    summary\n    recording {\n      id\n      title\n      length\n      mainAlbum {\n        id\n        title\n        coverArtUri\n      }\n    }\n  }\n": typeof types.PopularTrackRow_LastFmTrackFragmentDoc,
    "\n  fragment LikedSongRow_LikedSong on LikedSong {\n    id\n    recording {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n      mainAlbum {\n        id\n        title\n        coverArtUri\n        artists {\n          id\n        }\n      }\n    }\n  }\n": typeof types.LikedSongRow_LikedSongFragmentDoc,
    "\n  fragment LikedSongsList_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": typeof types.LikedSongsList_UserFragmentDoc,
    "\n  query SearchPanelQuery($text: String!) {\n    ...SearchResult_Query\n  }\n": typeof types.SearchPanelQueryDocument,
    "\n  fragment SearchResult_Query on Query {\n    artist {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultArtist_Artist\n      }\n    }\n    releaseGroup {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultReleaseGroup_ReleaseGroup\n      }\n    }\n    recording {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultRecording_Recording\n      }\n    }\n  }\n": typeof types.SearchResult_QueryFragmentDoc,
    "\n  fragment SearchResultArtist_Artist on Artist {\n    id\n    name\n    images {\n      artistThumb\n    }\n  }\n": typeof types.SearchResultArtist_ArtistFragmentDoc,
    "\n  fragment SearchResultRecording_Recording on Recording {\n    id\n    title\n    length\n    artists {\n      id\n      name\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n": typeof types.SearchResultRecording_RecordingFragmentDoc,
    "\n  fragment SearchResultReleaseGroup_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    mainRelease {\n      id\n      title\n      coverArtUri\n    }\n  }\n": typeof types.SearchResultReleaseGroup_ReleaseGroupFragmentDoc,
};
const documents: Documents = {
    "\n  query AlbumQuery($releaseId: ID!) {\n    release {\n      byId(id: $releaseId) {\n        id\n        ...AlbumPanel_Release\n      }\n    }\n  }\n": types.AlbumQueryDocument,
    "\n  query ArtistQuery($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        ...ArtistPanel_Artist\n      }\n    }\n  }\n": types.ArtistQueryDocument,
    "\n  query LikedSongsQuery {\n    viewer {\n      id\n      ...LikedSongsList_User\n    }\n  }\n": types.LikedSongsQueryDocument,
    "\n  fragment Playlist_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": types.Playlist_UserFragmentDoc,
    "\n  fragment LikedSongRow_Recoding on Recording {\n    id\n    title\n    length\n    artists {\n      id\n      name\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n": types.LikedSongRow_RecodingFragmentDoc,
    "\n  fragment AlbumCard_Release on Release {\n    id\n    title\n    year\n    coverArtUri\n    artists {\n      id\n      name\n    }\n  }\n": types.AlbumCard_ReleaseFragmentDoc,
    "\n  fragment AlbumHeader_Release on Release {\n    id\n    title\n    coverArtUri\n    artists {\n      id\n      name\n    }\n  }\n": types.AlbumHeader_ReleaseFragmentDoc,
    "\n  fragment AlbumPanel_Release on Release {\n    id\n    ...AlbumHeader_Release\n    ...AlbumTrackList_Release\n  }\n": types.AlbumPanel_ReleaseFragmentDoc,
    "\n  fragment AlbumTrackList_Release on Release {\n    id\n    recordings {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n    }\n  }\n": types.AlbumTrackList_ReleaseFragmentDoc,
    "\n  fragment ArtistAlbumList_Artist on Artist {\n    id\n    mainAlbums {\n      id\n      date\n      year\n      country\n      releaseGroup {\n        id\n        primaryType\n        secondaryTypes\n      }\n      ...AlbumCard_Release\n    }\n  }\n": types.ArtistAlbumList_ArtistFragmentDoc,
    "\n  fragment ArtistHeader_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistBackground\n    }\n  }\n": types.ArtistHeader_ArtistFragmentDoc,
    "\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    ...ArtistHeader_Artist\n    ...PopularArtistTracks_Artist\n    ...ArtistAlbumList_Artist\n  }\n": types.ArtistPanel_ArtistFragmentDoc,
    "\n  fragment PopularArtistTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      ...PopularTrackRow_LastFmTrack\n    }\n  }\n": types.PopularArtistTracks_ArtistFragmentDoc,
    "\n  fragment PopularTrackRow_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    summary\n    recording {\n      id\n      title\n      length\n      mainAlbum {\n        id\n        title\n        coverArtUri\n      }\n    }\n  }\n": types.PopularTrackRow_LastFmTrackFragmentDoc,
    "\n  fragment LikedSongRow_LikedSong on LikedSong {\n    id\n    recording {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n      mainAlbum {\n        id\n        title\n        coverArtUri\n        artists {\n          id\n        }\n      }\n    }\n  }\n": types.LikedSongRow_LikedSongFragmentDoc,
    "\n  fragment LikedSongsList_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": types.LikedSongsList_UserFragmentDoc,
    "\n  query SearchPanelQuery($text: String!) {\n    ...SearchResult_Query\n  }\n": types.SearchPanelQueryDocument,
    "\n  fragment SearchResult_Query on Query {\n    artist {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultArtist_Artist\n      }\n    }\n    releaseGroup {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultReleaseGroup_ReleaseGroup\n      }\n    }\n    recording {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultRecording_Recording\n      }\n    }\n  }\n": types.SearchResult_QueryFragmentDoc,
    "\n  fragment SearchResultArtist_Artist on Artist {\n    id\n    name\n    images {\n      artistThumb\n    }\n  }\n": types.SearchResultArtist_ArtistFragmentDoc,
    "\n  fragment SearchResultRecording_Recording on Recording {\n    id\n    title\n    length\n    artists {\n      id\n      name\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n": types.SearchResultRecording_RecordingFragmentDoc,
    "\n  fragment SearchResultReleaseGroup_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    mainRelease {\n      id\n      title\n      coverArtUri\n    }\n  }\n": types.SearchResultReleaseGroup_ReleaseGroupFragmentDoc,
};

/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 *
 *
 * @example
 * ```ts
 * const query = graphql(`query GetUser($id: ID!) { user(id: $id) { name } }`);
 * ```
 *
 * The query argument is unknown!
 * Please regenerate the types.
 */
export function graphql(source: string): unknown;

/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query AlbumQuery($releaseId: ID!) {\n    release {\n      byId(id: $releaseId) {\n        id\n        ...AlbumPanel_Release\n      }\n    }\n  }\n"): (typeof documents)["\n  query AlbumQuery($releaseId: ID!) {\n    release {\n      byId(id: $releaseId) {\n        id\n        ...AlbumPanel_Release\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ArtistQuery($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        ...ArtistPanel_Artist\n      }\n    }\n  }\n"): (typeof documents)["\n  query ArtistQuery($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        ...ArtistPanel_Artist\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query LikedSongsQuery {\n    viewer {\n      id\n      ...LikedSongsList_User\n    }\n  }\n"): (typeof documents)["\n  query LikedSongsQuery {\n    viewer {\n      id\n      ...LikedSongsList_User\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment Playlist_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n"): (typeof documents)["\n  fragment Playlist_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment LikedSongRow_Recoding on Recording {\n    id\n    title\n    length\n    artists {\n      id\n      name\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n"): (typeof documents)["\n  fragment LikedSongRow_Recoding on Recording {\n    id\n    title\n    length\n    artists {\n      id\n      name\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlbumCard_Release on Release {\n    id\n    title\n    year\n    coverArtUri\n    artists {\n      id\n      name\n    }\n  }\n"): (typeof documents)["\n  fragment AlbumCard_Release on Release {\n    id\n    title\n    year\n    coverArtUri\n    artists {\n      id\n      name\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlbumHeader_Release on Release {\n    id\n    title\n    coverArtUri\n    artists {\n      id\n      name\n    }\n  }\n"): (typeof documents)["\n  fragment AlbumHeader_Release on Release {\n    id\n    title\n    coverArtUri\n    artists {\n      id\n      name\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlbumPanel_Release on Release {\n    id\n    ...AlbumHeader_Release\n    ...AlbumTrackList_Release\n  }\n"): (typeof documents)["\n  fragment AlbumPanel_Release on Release {\n    id\n    ...AlbumHeader_Release\n    ...AlbumTrackList_Release\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlbumTrackList_Release on Release {\n    id\n    recordings {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment AlbumTrackList_Release on Release {\n    id\n    recordings {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ArtistAlbumList_Artist on Artist {\n    id\n    mainAlbums {\n      id\n      date\n      year\n      country\n      releaseGroup {\n        id\n        primaryType\n        secondaryTypes\n      }\n      ...AlbumCard_Release\n    }\n  }\n"): (typeof documents)["\n  fragment ArtistAlbumList_Artist on Artist {\n    id\n    mainAlbums {\n      id\n      date\n      year\n      country\n      releaseGroup {\n        id\n        primaryType\n        secondaryTypes\n      }\n      ...AlbumCard_Release\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ArtistHeader_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistBackground\n    }\n  }\n"): (typeof documents)["\n  fragment ArtistHeader_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistBackground\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    ...ArtistHeader_Artist\n    ...PopularArtistTracks_Artist\n    ...ArtistAlbumList_Artist\n  }\n"): (typeof documents)["\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    ...ArtistHeader_Artist\n    ...PopularArtistTracks_Artist\n    ...ArtistAlbumList_Artist\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment PopularArtistTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      ...PopularTrackRow_LastFmTrack\n    }\n  }\n"): (typeof documents)["\n  fragment PopularArtistTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      ...PopularTrackRow_LastFmTrack\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment PopularTrackRow_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    summary\n    recording {\n      id\n      title\n      length\n      mainAlbum {\n        id\n        title\n        coverArtUri\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment PopularTrackRow_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    summary\n    recording {\n      id\n      title\n      length\n      mainAlbum {\n        id\n        title\n        coverArtUri\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment LikedSongRow_LikedSong on LikedSong {\n    id\n    recording {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n      mainAlbum {\n        id\n        title\n        coverArtUri\n        artists {\n          id\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment LikedSongRow_LikedSong on LikedSong {\n    id\n    recording {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n      mainAlbum {\n        id\n        title\n        coverArtUri\n        artists {\n          id\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment LikedSongsList_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n"): (typeof documents)["\n  fragment LikedSongsList_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query SearchPanelQuery($text: String!) {\n    ...SearchResult_Query\n  }\n"): (typeof documents)["\n  query SearchPanelQuery($text: String!) {\n    ...SearchResult_Query\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment SearchResult_Query on Query {\n    artist {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultArtist_Artist\n      }\n    }\n    releaseGroup {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultReleaseGroup_ReleaseGroup\n      }\n    }\n    recording {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultRecording_Recording\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment SearchResult_Query on Query {\n    artist {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultArtist_Artist\n      }\n    }\n    releaseGroup {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultReleaseGroup_ReleaseGroup\n      }\n    }\n    recording {\n      searchByName(name: $text, limit: 5) {\n        ...SearchResultRecording_Recording\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment SearchResultArtist_Artist on Artist {\n    id\n    name\n    images {\n      artistThumb\n    }\n  }\n"): (typeof documents)["\n  fragment SearchResultArtist_Artist on Artist {\n    id\n    name\n    images {\n      artistThumb\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment SearchResultRecording_Recording on Recording {\n    id\n    title\n    length\n    artists {\n      id\n      name\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n"): (typeof documents)["\n  fragment SearchResultRecording_Recording on Recording {\n    id\n    title\n    length\n    artists {\n      id\n      name\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment SearchResultReleaseGroup_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    mainRelease {\n      id\n      title\n      coverArtUri\n    }\n  }\n"): (typeof documents)["\n  fragment SearchResultReleaseGroup_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    mainRelease {\n      id\n      title\n      coverArtUri\n    }\n  }\n"];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;