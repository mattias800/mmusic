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
    "\n  query Bootstrap {\n    areThereAnyUsers\n    viewer {\n      id\n      username\n    }\n  }\n": typeof types.BootstrapDocument,
    "\n  query AlbumListQuery {\n    releaseGroup {\n      all {\n        id\n        ...AlbumList_ReleaseGroup\n      }\n    }\n  }\n": typeof types.AlbumListQueryDocument,
    "\n  query AlbumQuery($releaseGroupId: ID!) {\n    releaseGroup {\n      byId(id: $releaseGroupId) {\n        id\n        ...AlbumPanel_ReleaseGroup\n      }\n    }\n  }\n": typeof types.AlbumQueryDocument,
    "\n  query ArtistListQuery {\n    artist {\n      all {\n        id\n        ...ArtistList_Artist\n      }\n    }\n  }\n": typeof types.ArtistListQueryDocument,
    "\n  query ArtistQuery($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        ...ArtistPanel_Artist\n      }\n    }\n  }\n": typeof types.ArtistQueryDocument,
    "\n  query LikedSongsQuery {\n    viewer {\n      id\n      ...LikedSongsList_User\n    }\n  }\n": typeof types.LikedSongsQueryDocument,
    "\n  mutation CreateUser($username: String!, $password: String!) {\n    createUser(input: { username: $username, password: $password }) {\n      __typename\n      ... on CreateUserSuccess {\n        # Assuming a similar success payload\n        user {\n          id\n          username\n        }\n      }\n      ... on CreateUserError {\n        # Assuming a similar error payload\n        message\n      }\n    }\n  }\n": typeof types.CreateUserDocument,
    "\n  mutation SignIn($username: String!, $password: String!) {\n    signIn(input: { username: $username, password: $password }) {\n      __typename\n      ... on SignInSuccess {\n        user {\n          id\n          username\n        }\n      }\n      ... on SignInError {\n        message\n      }\n    }\n  }\n": typeof types.SignInDocument,
    "\n  query ProfilePage {\n    viewer {\n      id\n      ...UserProfilePanel_User\n    }\n  }\n": typeof types.ProfilePageDocument,
    "\n  fragment Playlist_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": typeof types.Playlist_UserFragmentDoc,
    "\n  fragment LikedSongRow_Recoding on Recording {\n    id\n    title\n    length\n    nameCredits {\n      artist {\n        id\n        name\n      }\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n": typeof types.LikedSongRow_RecodingFragmentDoc,
    "\n  mutation AddArtistToServerLibrary($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n      ... on AddArtistToServerLibrarySuccess {\n        serverAvailability {\n          id\n          isInServerLibrary\n        }\n      }\n    }\n  }\n": typeof types.AddArtistToServerLibraryDocument,
    "\n  fragment ArtistInLibraryButton_Artist on Artist {\n    id\n  }\n": typeof types.ArtistInLibraryButton_ArtistFragmentDoc,
    "\n  fragment AlbumCard_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    firstReleaseYear\n    coverArtUri\n    credits {\n      name\n      artist {\n        id\n        name\n      }\n    }\n  }\n": typeof types.AlbumCard_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumHeader_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    primaryType\n    mainRelease {\n      id\n      title\n      coverArtUri\n      year\n\n      credits {\n        artist {\n          id\n          name\n        }\n      }\n      recordings {\n        id\n        length\n      }\n    }\n  }\n": typeof types.AlbumHeader_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumPanel_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumHeader_ReleaseGroup\n    ...AlbumTrackList_ReleaseGroup\n    firstReleaseYear\n    mainRelease {\n      labels {\n        id\n        name\n      }\n    }\n  }\n": typeof types.AlbumPanel_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumTrackList_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    mainRelease {\n      id\n      title\n      recordings {\n        id\n        ...RecordingPlayButton_Recording\n        title\n        length\n        statistics {\n          listeners\n        }\n        nameCredits {\n          name\n          artist {\n            id\n            name\n          }\n        }\n      }\n    }\n  }\n": typeof types.AlbumTrackList_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumList_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumCard_ReleaseGroup\n  }\n": typeof types.AlbumList_ReleaseGroupFragmentDoc,
    "\n  fragment ArtistCard_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistThumb\n    }\n  }\n": typeof types.ArtistCard_ArtistFragmentDoc,
    "\n  fragment ArtistList_Artist on Artist {\n    id\n    ...ArtistCard_Artist\n  }\n": typeof types.ArtistList_ArtistFragmentDoc,
    "\n  query ArtistAlbumList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        albums {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": typeof types.ArtistAlbumListDocument,
    "\n  query ArtistEpList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        eps {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": typeof types.ArtistEpListDocument,
    "\n  fragment ArtistHeader_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistBackground\n    }\n  }\n": typeof types.ArtistHeader_ArtistFragmentDoc,
    "\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    ...ArtistHeader_Artist\n    ...TopArtistTracks_Artist\n    ...ArtistInLibraryButton_Artist\n  }\n": typeof types.ArtistPanel_ArtistFragmentDoc,
    "\n  query ArtistSingleList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        singles {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": typeof types.ArtistSingleListDocument,
    "\n  fragment TopArtistTrackItem_LastFmTrack on LastFmTrack {\n    id\n    name\n    playCount\n    summary\n    recording {\n      id\n      title\n      length\n      ...RecordingPlayButton_Recording\n      mainAlbum {\n        id\n        title\n        coverArtUri\n      }\n    }\n  }\n": typeof types.TopArtistTrackItem_LastFmTrackFragmentDoc,
    "\n  fragment TopArtistTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      ...TopArtistTrackItem_LastFmTrack\n    }\n  }\n": typeof types.TopArtistTracks_ArtistFragmentDoc,
    "\n  mutation SignOut {\n    signOut {\n      __typename\n\n      ... on SignOutSuccess {\n        success\n      }\n      ... on SignOutError {\n        message\n      }\n    }\n  }\n": typeof types.SignOutDocument,
    "\n  fragment DownloadOverview_DownloadStatus on DownloadStatus {\n    id\n    ...DownloadStatus_DownloadStatus\n  }\n": typeof types.DownloadOverview_DownloadStatusFragmentDoc,
    "\n  query DownloadOverviewQuery {\n    download {\n      all {\n        id\n        ...DownloadOverview_DownloadStatus\n      }\n    }\n  }\n": typeof types.DownloadOverviewQueryDocument,
    "\n  subscription DownloadOverviewSubscription {\n    downloadStatusUpdated {\n      id\n      ...DownloadOverview_DownloadStatus\n    }\n  }\n": typeof types.DownloadOverviewSubscriptionDocument,
    "\n  fragment DownloadStatus_DownloadStatus on DownloadStatus {\n    id\n    numberOfTracks\n    tracksDownloaded\n    statusDescription\n    release {\n      id\n      title\n      year\n      artists {\n        id\n        name\n      }\n      recordings {\n        id\n      }\n    }\n  }\n": typeof types.DownloadStatus_DownloadStatusFragmentDoc,
    "\n  fragment LikedSongRow_LikedSong on LikedSong {\n    id\n    recording {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n      mainAlbum {\n        id\n        title\n        coverArtUri\n        artists {\n          id\n        }\n      }\n    }\n  }\n": typeof types.LikedSongRow_LikedSongFragmentDoc,
    "\n  fragment LikedSongsList_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": typeof types.LikedSongsList_UserFragmentDoc,
    "\n  fragment RecordingPlayButton_Recording on Recording {\n    id\n    streamingServiceInfo {\n      id\n      youtubeVideoId\n    }\n  }\n": typeof types.RecordingPlayButton_RecordingFragmentDoc,
    "\n  query YoutubeVideoSearch($recordingId: ID!) {\n    musicBrainz {\n      recording {\n        byId(id: $recordingId) {\n          id\n          streamingServiceInfo {\n            id\n            youtubeSearchVideoId\n          }\n        }\n      }\n    }\n  }\n": typeof types.YoutubeVideoSearchDocument,
    "\n  query TopArtistRecommendations {\n    recommendations {\n      topArtists {\n        id\n        artist {\n          id\n          ...ArtistCard_Artist\n        }\n      }\n      topTracks {\n        id\n        name\n        summary\n        playCount\n      }\n      topTags {\n        name\n        url\n      }\n    }\n  }\n": typeof types.TopArtistRecommendationsDocument,
    "\n  fragment TopTrackCard_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    name\n    musicBrainzRecording {\n      id\n      mainAlbum {\n        id\n        coverArtUri\n      }\n    }\n    artist {\n      id\n      name\n      musicBrainzArtist {\n        id\n        images {\n          artistThumb\n        }\n      }\n    }\n  }\n": typeof types.TopTrackCard_LastFmTrackFragmentDoc,
    "\n  query TopTrackRecommendations {\n    recommendations {\n      topTracks {\n        id\n        ...TopTrackCard_LastFmTrack\n      }\n    }\n  }\n": typeof types.TopTrackRecommendationsDocument,
    "\n  query SearchResultArtistSearch($text: String!) {\n    artist {\n      searchByName(name: $text, limit: 5) {\n        id\n        name\n        images {\n          artistThumb\n        }\n      }\n    }\n  }\n": typeof types.SearchResultArtistSearchDocument,
    "\n  query SearchResultSongSearch($text: String!) {\n    recording {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        length\n        nameCredits {\n          artist {\n            id\n            name\n          }\n        }\n        mainAlbum {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n": typeof types.SearchResultSongSearchDocument,
    "\n  query SearchResultAlbumSearch($text: String!) {\n    releaseGroup {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        mainRelease {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n": typeof types.SearchResultAlbumSearchDocument,
    "\n  query Sidebar {\n    viewer {\n      id\n      username\n    }\n  }\n": typeof types.SidebarDocument,
    "\n  fragment SoulSeekNetworkStatus_SoulSeekStatus on SoulSeekStatus {\n    id\n    status\n  }\n": typeof types.SoulSeekNetworkStatus_SoulSeekStatusFragmentDoc,
    "\n  subscription SoulSeekNetworkStatusSubscription {\n    soulSeekStatusUpdated {\n      ...SoulSeekNetworkStatus_SoulSeekStatus\n    }\n  }\n": typeof types.SoulSeekNetworkStatusSubscriptionDocument,
    "\n  query SoulSeekNetworkStatusQuery {\n    external {\n      id\n      soulSeek {\n        id\n        status {\n          ...SoulSeekNetworkStatus_SoulSeekStatus\n        }\n      }\n    }\n  }\n": typeof types.SoulSeekNetworkStatusQueryDocument,
    "\n  query UserPlaylistsLoader_Query($spotifyUsername: String!) {\n    playlist {\n      importPlaylists {\n        spotify {\n          spotifyPlaylistsForUser(username: $spotifyUsername) {\n            id\n            ...UserPlaylistsList_SpotifyPlaylist\n          }\n        }\n      }\n    }\n  }\n": typeof types.UserPlaylistsLoader_QueryDocument,
    "\n  mutation ImportSpotifyPlaylistById($playlistId: String!, $userId: UUID!) {\n    importSpotifyPlaylistById(playlistId: $playlistId, userId: $userId) {\n      __typename\n      ... on ImportSpotifyPlaylistSuccess {\n        success\n      }\n      ... on ImportSpotifyPlaylistError {\n        message\n      }\n    }\n  }\n": typeof types.ImportSpotifyPlaylistByIdDocument,
    "\n  fragment UserPlaylistsList_SpotifyPlaylist on SpotifyPlaylist {\n    id\n    description\n    name\n    coverImageUrl\n  }\n": typeof types.UserPlaylistsList_SpotifyPlaylistFragmentDoc,
    "\n  fragment UserProfilePanel_User on User {\n    id\n    username\n    createdAt\n    updatedAt\n    likedSongs {\n      id\n    }\n  }\n": typeof types.UserProfilePanel_UserFragmentDoc,
};
const documents: Documents = {
    "\n  query Bootstrap {\n    areThereAnyUsers\n    viewer {\n      id\n      username\n    }\n  }\n": types.BootstrapDocument,
    "\n  query AlbumListQuery {\n    releaseGroup {\n      all {\n        id\n        ...AlbumList_ReleaseGroup\n      }\n    }\n  }\n": types.AlbumListQueryDocument,
    "\n  query AlbumQuery($releaseGroupId: ID!) {\n    releaseGroup {\n      byId(id: $releaseGroupId) {\n        id\n        ...AlbumPanel_ReleaseGroup\n      }\n    }\n  }\n": types.AlbumQueryDocument,
    "\n  query ArtistListQuery {\n    artist {\n      all {\n        id\n        ...ArtistList_Artist\n      }\n    }\n  }\n": types.ArtistListQueryDocument,
    "\n  query ArtistQuery($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        ...ArtistPanel_Artist\n      }\n    }\n  }\n": types.ArtistQueryDocument,
    "\n  query LikedSongsQuery {\n    viewer {\n      id\n      ...LikedSongsList_User\n    }\n  }\n": types.LikedSongsQueryDocument,
    "\n  mutation CreateUser($username: String!, $password: String!) {\n    createUser(input: { username: $username, password: $password }) {\n      __typename\n      ... on CreateUserSuccess {\n        # Assuming a similar success payload\n        user {\n          id\n          username\n        }\n      }\n      ... on CreateUserError {\n        # Assuming a similar error payload\n        message\n      }\n    }\n  }\n": types.CreateUserDocument,
    "\n  mutation SignIn($username: String!, $password: String!) {\n    signIn(input: { username: $username, password: $password }) {\n      __typename\n      ... on SignInSuccess {\n        user {\n          id\n          username\n        }\n      }\n      ... on SignInError {\n        message\n      }\n    }\n  }\n": types.SignInDocument,
    "\n  query ProfilePage {\n    viewer {\n      id\n      ...UserProfilePanel_User\n    }\n  }\n": types.ProfilePageDocument,
    "\n  fragment Playlist_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": types.Playlist_UserFragmentDoc,
    "\n  fragment LikedSongRow_Recoding on Recording {\n    id\n    title\n    length\n    nameCredits {\n      artist {\n        id\n        name\n      }\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n": types.LikedSongRow_RecodingFragmentDoc,
    "\n  mutation AddArtistToServerLibrary($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n      ... on AddArtistToServerLibrarySuccess {\n        serverAvailability {\n          id\n          isInServerLibrary\n        }\n      }\n    }\n  }\n": types.AddArtistToServerLibraryDocument,
    "\n  fragment ArtistInLibraryButton_Artist on Artist {\n    id\n  }\n": types.ArtistInLibraryButton_ArtistFragmentDoc,
    "\n  fragment AlbumCard_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    firstReleaseYear\n    coverArtUri\n    credits {\n      name\n      artist {\n        id\n        name\n      }\n    }\n  }\n": types.AlbumCard_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumHeader_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    primaryType\n    mainRelease {\n      id\n      title\n      coverArtUri\n      year\n\n      credits {\n        artist {\n          id\n          name\n        }\n      }\n      recordings {\n        id\n        length\n      }\n    }\n  }\n": types.AlbumHeader_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumPanel_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumHeader_ReleaseGroup\n    ...AlbumTrackList_ReleaseGroup\n    firstReleaseYear\n    mainRelease {\n      labels {\n        id\n        name\n      }\n    }\n  }\n": types.AlbumPanel_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumTrackList_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    mainRelease {\n      id\n      title\n      recordings {\n        id\n        ...RecordingPlayButton_Recording\n        title\n        length\n        statistics {\n          listeners\n        }\n        nameCredits {\n          name\n          artist {\n            id\n            name\n          }\n        }\n      }\n    }\n  }\n": types.AlbumTrackList_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumList_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumCard_ReleaseGroup\n  }\n": types.AlbumList_ReleaseGroupFragmentDoc,
    "\n  fragment ArtistCard_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistThumb\n    }\n  }\n": types.ArtistCard_ArtistFragmentDoc,
    "\n  fragment ArtistList_Artist on Artist {\n    id\n    ...ArtistCard_Artist\n  }\n": types.ArtistList_ArtistFragmentDoc,
    "\n  query ArtistAlbumList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        albums {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": types.ArtistAlbumListDocument,
    "\n  query ArtistEpList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        eps {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": types.ArtistEpListDocument,
    "\n  fragment ArtistHeader_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistBackground\n    }\n  }\n": types.ArtistHeader_ArtistFragmentDoc,
    "\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    ...ArtistHeader_Artist\n    ...TopArtistTracks_Artist\n    ...ArtistInLibraryButton_Artist\n  }\n": types.ArtistPanel_ArtistFragmentDoc,
    "\n  query ArtistSingleList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        singles {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": types.ArtistSingleListDocument,
    "\n  fragment TopArtistTrackItem_LastFmTrack on LastFmTrack {\n    id\n    name\n    playCount\n    summary\n    recording {\n      id\n      title\n      length\n      ...RecordingPlayButton_Recording\n      mainAlbum {\n        id\n        title\n        coverArtUri\n      }\n    }\n  }\n": types.TopArtistTrackItem_LastFmTrackFragmentDoc,
    "\n  fragment TopArtistTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      ...TopArtistTrackItem_LastFmTrack\n    }\n  }\n": types.TopArtistTracks_ArtistFragmentDoc,
    "\n  mutation SignOut {\n    signOut {\n      __typename\n\n      ... on SignOutSuccess {\n        success\n      }\n      ... on SignOutError {\n        message\n      }\n    }\n  }\n": types.SignOutDocument,
    "\n  fragment DownloadOverview_DownloadStatus on DownloadStatus {\n    id\n    ...DownloadStatus_DownloadStatus\n  }\n": types.DownloadOverview_DownloadStatusFragmentDoc,
    "\n  query DownloadOverviewQuery {\n    download {\n      all {\n        id\n        ...DownloadOverview_DownloadStatus\n      }\n    }\n  }\n": types.DownloadOverviewQueryDocument,
    "\n  subscription DownloadOverviewSubscription {\n    downloadStatusUpdated {\n      id\n      ...DownloadOverview_DownloadStatus\n    }\n  }\n": types.DownloadOverviewSubscriptionDocument,
    "\n  fragment DownloadStatus_DownloadStatus on DownloadStatus {\n    id\n    numberOfTracks\n    tracksDownloaded\n    statusDescription\n    release {\n      id\n      title\n      year\n      artists {\n        id\n        name\n      }\n      recordings {\n        id\n      }\n    }\n  }\n": types.DownloadStatus_DownloadStatusFragmentDoc,
    "\n  fragment LikedSongRow_LikedSong on LikedSong {\n    id\n    recording {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n      mainAlbum {\n        id\n        title\n        coverArtUri\n        artists {\n          id\n        }\n      }\n    }\n  }\n": types.LikedSongRow_LikedSongFragmentDoc,
    "\n  fragment LikedSongsList_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": types.LikedSongsList_UserFragmentDoc,
    "\n  fragment RecordingPlayButton_Recording on Recording {\n    id\n    streamingServiceInfo {\n      id\n      youtubeVideoId\n    }\n  }\n": types.RecordingPlayButton_RecordingFragmentDoc,
    "\n  query YoutubeVideoSearch($recordingId: ID!) {\n    musicBrainz {\n      recording {\n        byId(id: $recordingId) {\n          id\n          streamingServiceInfo {\n            id\n            youtubeSearchVideoId\n          }\n        }\n      }\n    }\n  }\n": types.YoutubeVideoSearchDocument,
    "\n  query TopArtistRecommendations {\n    recommendations {\n      topArtists {\n        id\n        artist {\n          id\n          ...ArtistCard_Artist\n        }\n      }\n      topTracks {\n        id\n        name\n        summary\n        playCount\n      }\n      topTags {\n        name\n        url\n      }\n    }\n  }\n": types.TopArtistRecommendationsDocument,
    "\n  fragment TopTrackCard_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    name\n    musicBrainzRecording {\n      id\n      mainAlbum {\n        id\n        coverArtUri\n      }\n    }\n    artist {\n      id\n      name\n      musicBrainzArtist {\n        id\n        images {\n          artistThumb\n        }\n      }\n    }\n  }\n": types.TopTrackCard_LastFmTrackFragmentDoc,
    "\n  query TopTrackRecommendations {\n    recommendations {\n      topTracks {\n        id\n        ...TopTrackCard_LastFmTrack\n      }\n    }\n  }\n": types.TopTrackRecommendationsDocument,
    "\n  query SearchResultArtistSearch($text: String!) {\n    artist {\n      searchByName(name: $text, limit: 5) {\n        id\n        name\n        images {\n          artistThumb\n        }\n      }\n    }\n  }\n": types.SearchResultArtistSearchDocument,
    "\n  query SearchResultSongSearch($text: String!) {\n    recording {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        length\n        nameCredits {\n          artist {\n            id\n            name\n          }\n        }\n        mainAlbum {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n": types.SearchResultSongSearchDocument,
    "\n  query SearchResultAlbumSearch($text: String!) {\n    releaseGroup {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        mainRelease {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n": types.SearchResultAlbumSearchDocument,
    "\n  query Sidebar {\n    viewer {\n      id\n      username\n    }\n  }\n": types.SidebarDocument,
    "\n  fragment SoulSeekNetworkStatus_SoulSeekStatus on SoulSeekStatus {\n    id\n    status\n  }\n": types.SoulSeekNetworkStatus_SoulSeekStatusFragmentDoc,
    "\n  subscription SoulSeekNetworkStatusSubscription {\n    soulSeekStatusUpdated {\n      ...SoulSeekNetworkStatus_SoulSeekStatus\n    }\n  }\n": types.SoulSeekNetworkStatusSubscriptionDocument,
    "\n  query SoulSeekNetworkStatusQuery {\n    external {\n      id\n      soulSeek {\n        id\n        status {\n          ...SoulSeekNetworkStatus_SoulSeekStatus\n        }\n      }\n    }\n  }\n": types.SoulSeekNetworkStatusQueryDocument,
    "\n  query UserPlaylistsLoader_Query($spotifyUsername: String!) {\n    playlist {\n      importPlaylists {\n        spotify {\n          spotifyPlaylistsForUser(username: $spotifyUsername) {\n            id\n            ...UserPlaylistsList_SpotifyPlaylist\n          }\n        }\n      }\n    }\n  }\n": types.UserPlaylistsLoader_QueryDocument,
    "\n  mutation ImportSpotifyPlaylistById($playlistId: String!, $userId: UUID!) {\n    importSpotifyPlaylistById(playlistId: $playlistId, userId: $userId) {\n      __typename\n      ... on ImportSpotifyPlaylistSuccess {\n        success\n      }\n      ... on ImportSpotifyPlaylistError {\n        message\n      }\n    }\n  }\n": types.ImportSpotifyPlaylistByIdDocument,
    "\n  fragment UserPlaylistsList_SpotifyPlaylist on SpotifyPlaylist {\n    id\n    description\n    name\n    coverImageUrl\n  }\n": types.UserPlaylistsList_SpotifyPlaylistFragmentDoc,
    "\n  fragment UserProfilePanel_User on User {\n    id\n    username\n    createdAt\n    updatedAt\n    likedSongs {\n      id\n    }\n  }\n": types.UserProfilePanel_UserFragmentDoc,
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
export function graphql(source: "\n  query Bootstrap {\n    areThereAnyUsers\n    viewer {\n      id\n      username\n    }\n  }\n"): (typeof documents)["\n  query Bootstrap {\n    areThereAnyUsers\n    viewer {\n      id\n      username\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query AlbumListQuery {\n    releaseGroup {\n      all {\n        id\n        ...AlbumList_ReleaseGroup\n      }\n    }\n  }\n"): (typeof documents)["\n  query AlbumListQuery {\n    releaseGroup {\n      all {\n        id\n        ...AlbumList_ReleaseGroup\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query AlbumQuery($releaseGroupId: ID!) {\n    releaseGroup {\n      byId(id: $releaseGroupId) {\n        id\n        ...AlbumPanel_ReleaseGroup\n      }\n    }\n  }\n"): (typeof documents)["\n  query AlbumQuery($releaseGroupId: ID!) {\n    releaseGroup {\n      byId(id: $releaseGroupId) {\n        id\n        ...AlbumPanel_ReleaseGroup\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ArtistListQuery {\n    artist {\n      all {\n        id\n        ...ArtistList_Artist\n      }\n    }\n  }\n"): (typeof documents)["\n  query ArtistListQuery {\n    artist {\n      all {\n        id\n        ...ArtistList_Artist\n      }\n    }\n  }\n"];
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
export function graphql(source: "\n  mutation CreateUser($username: String!, $password: String!) {\n    createUser(input: { username: $username, password: $password }) {\n      __typename\n      ... on CreateUserSuccess {\n        # Assuming a similar success payload\n        user {\n          id\n          username\n        }\n      }\n      ... on CreateUserError {\n        # Assuming a similar error payload\n        message\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation CreateUser($username: String!, $password: String!) {\n    createUser(input: { username: $username, password: $password }) {\n      __typename\n      ... on CreateUserSuccess {\n        # Assuming a similar success payload\n        user {\n          id\n          username\n        }\n      }\n      ... on CreateUserError {\n        # Assuming a similar error payload\n        message\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation SignIn($username: String!, $password: String!) {\n    signIn(input: { username: $username, password: $password }) {\n      __typename\n      ... on SignInSuccess {\n        user {\n          id\n          username\n        }\n      }\n      ... on SignInError {\n        message\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation SignIn($username: String!, $password: String!) {\n    signIn(input: { username: $username, password: $password }) {\n      __typename\n      ... on SignInSuccess {\n        user {\n          id\n          username\n        }\n      }\n      ... on SignInError {\n        message\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ProfilePage {\n    viewer {\n      id\n      ...UserProfilePanel_User\n    }\n  }\n"): (typeof documents)["\n  query ProfilePage {\n    viewer {\n      id\n      ...UserProfilePanel_User\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment Playlist_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n"): (typeof documents)["\n  fragment Playlist_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment LikedSongRow_Recoding on Recording {\n    id\n    title\n    length\n    nameCredits {\n      artist {\n        id\n        name\n      }\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n"): (typeof documents)["\n  fragment LikedSongRow_Recoding on Recording {\n    id\n    title\n    length\n    nameCredits {\n      artist {\n        id\n        name\n      }\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation AddArtistToServerLibrary($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n      ... on AddArtistToServerLibrarySuccess {\n        serverAvailability {\n          id\n          isInServerLibrary\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation AddArtistToServerLibrary($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n      ... on AddArtistToServerLibrarySuccess {\n        serverAvailability {\n          id\n          isInServerLibrary\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ArtistInLibraryButton_Artist on Artist {\n    id\n  }\n"): (typeof documents)["\n  fragment ArtistInLibraryButton_Artist on Artist {\n    id\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlbumCard_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    firstReleaseYear\n    coverArtUri\n    credits {\n      name\n      artist {\n        id\n        name\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment AlbumCard_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    firstReleaseYear\n    coverArtUri\n    credits {\n      name\n      artist {\n        id\n        name\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlbumHeader_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    primaryType\n    mainRelease {\n      id\n      title\n      coverArtUri\n      year\n\n      credits {\n        artist {\n          id\n          name\n        }\n      }\n      recordings {\n        id\n        length\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment AlbumHeader_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    primaryType\n    mainRelease {\n      id\n      title\n      coverArtUri\n      year\n\n      credits {\n        artist {\n          id\n          name\n        }\n      }\n      recordings {\n        id\n        length\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlbumPanel_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumHeader_ReleaseGroup\n    ...AlbumTrackList_ReleaseGroup\n    firstReleaseYear\n    mainRelease {\n      labels {\n        id\n        name\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment AlbumPanel_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumHeader_ReleaseGroup\n    ...AlbumTrackList_ReleaseGroup\n    firstReleaseYear\n    mainRelease {\n      labels {\n        id\n        name\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlbumTrackList_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    mainRelease {\n      id\n      title\n      recordings {\n        id\n        ...RecordingPlayButton_Recording\n        title\n        length\n        statistics {\n          listeners\n        }\n        nameCredits {\n          name\n          artist {\n            id\n            name\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment AlbumTrackList_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    mainRelease {\n      id\n      title\n      recordings {\n        id\n        ...RecordingPlayButton_Recording\n        title\n        length\n        statistics {\n          listeners\n        }\n        nameCredits {\n          name\n          artist {\n            id\n            name\n          }\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlbumList_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumCard_ReleaseGroup\n  }\n"): (typeof documents)["\n  fragment AlbumList_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumCard_ReleaseGroup\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ArtistCard_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistThumb\n    }\n  }\n"): (typeof documents)["\n  fragment ArtistCard_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistThumb\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ArtistList_Artist on Artist {\n    id\n    ...ArtistCard_Artist\n  }\n"): (typeof documents)["\n  fragment ArtistList_Artist on Artist {\n    id\n    ...ArtistCard_Artist\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ArtistAlbumList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        albums {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query ArtistAlbumList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        albums {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ArtistEpList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        eps {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query ArtistEpList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        eps {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ArtistHeader_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistBackground\n    }\n  }\n"): (typeof documents)["\n  fragment ArtistHeader_Artist on Artist {\n    id\n    name\n    listeners\n    images {\n      artistBackground\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    ...ArtistHeader_Artist\n    ...TopArtistTracks_Artist\n    ...ArtistInLibraryButton_Artist\n  }\n"): (typeof documents)["\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    ...ArtistHeader_Artist\n    ...TopArtistTracks_Artist\n    ...ArtistInLibraryButton_Artist\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ArtistSingleList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        singles {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query ArtistSingleList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        singles {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment TopArtistTrackItem_LastFmTrack on LastFmTrack {\n    id\n    name\n    playCount\n    summary\n    recording {\n      id\n      title\n      length\n      ...RecordingPlayButton_Recording\n      mainAlbum {\n        id\n        title\n        coverArtUri\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment TopArtistTrackItem_LastFmTrack on LastFmTrack {\n    id\n    name\n    playCount\n    summary\n    recording {\n      id\n      title\n      length\n      ...RecordingPlayButton_Recording\n      mainAlbum {\n        id\n        title\n        coverArtUri\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment TopArtistTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      ...TopArtistTrackItem_LastFmTrack\n    }\n  }\n"): (typeof documents)["\n  fragment TopArtistTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      ...TopArtistTrackItem_LastFmTrack\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation SignOut {\n    signOut {\n      __typename\n\n      ... on SignOutSuccess {\n        success\n      }\n      ... on SignOutError {\n        message\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation SignOut {\n    signOut {\n      __typename\n\n      ... on SignOutSuccess {\n        success\n      }\n      ... on SignOutError {\n        message\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment DownloadOverview_DownloadStatus on DownloadStatus {\n    id\n    ...DownloadStatus_DownloadStatus\n  }\n"): (typeof documents)["\n  fragment DownloadOverview_DownloadStatus on DownloadStatus {\n    id\n    ...DownloadStatus_DownloadStatus\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query DownloadOverviewQuery {\n    download {\n      all {\n        id\n        ...DownloadOverview_DownloadStatus\n      }\n    }\n  }\n"): (typeof documents)["\n  query DownloadOverviewQuery {\n    download {\n      all {\n        id\n        ...DownloadOverview_DownloadStatus\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  subscription DownloadOverviewSubscription {\n    downloadStatusUpdated {\n      id\n      ...DownloadOverview_DownloadStatus\n    }\n  }\n"): (typeof documents)["\n  subscription DownloadOverviewSubscription {\n    downloadStatusUpdated {\n      id\n      ...DownloadOverview_DownloadStatus\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment DownloadStatus_DownloadStatus on DownloadStatus {\n    id\n    numberOfTracks\n    tracksDownloaded\n    statusDescription\n    release {\n      id\n      title\n      year\n      artists {\n        id\n        name\n      }\n      recordings {\n        id\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment DownloadStatus_DownloadStatus on DownloadStatus {\n    id\n    numberOfTracks\n    tracksDownloaded\n    statusDescription\n    release {\n      id\n      title\n      year\n      artists {\n        id\n        name\n      }\n      recordings {\n        id\n      }\n    }\n  }\n"];
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
export function graphql(source: "\n  fragment RecordingPlayButton_Recording on Recording {\n    id\n    streamingServiceInfo {\n      id\n      youtubeVideoId\n    }\n  }\n"): (typeof documents)["\n  fragment RecordingPlayButton_Recording on Recording {\n    id\n    streamingServiceInfo {\n      id\n      youtubeVideoId\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query YoutubeVideoSearch($recordingId: ID!) {\n    musicBrainz {\n      recording {\n        byId(id: $recordingId) {\n          id\n          streamingServiceInfo {\n            id\n            youtubeSearchVideoId\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query YoutubeVideoSearch($recordingId: ID!) {\n    musicBrainz {\n      recording {\n        byId(id: $recordingId) {\n          id\n          streamingServiceInfo {\n            id\n            youtubeSearchVideoId\n          }\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query TopArtistRecommendations {\n    recommendations {\n      topArtists {\n        id\n        artist {\n          id\n          ...ArtistCard_Artist\n        }\n      }\n      topTracks {\n        id\n        name\n        summary\n        playCount\n      }\n      topTags {\n        name\n        url\n      }\n    }\n  }\n"): (typeof documents)["\n  query TopArtistRecommendations {\n    recommendations {\n      topArtists {\n        id\n        artist {\n          id\n          ...ArtistCard_Artist\n        }\n      }\n      topTracks {\n        id\n        name\n        summary\n        playCount\n      }\n      topTags {\n        name\n        url\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment TopTrackCard_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    name\n    musicBrainzRecording {\n      id\n      mainAlbum {\n        id\n        coverArtUri\n      }\n    }\n    artist {\n      id\n      name\n      musicBrainzArtist {\n        id\n        images {\n          artistThumb\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment TopTrackCard_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    name\n    musicBrainzRecording {\n      id\n      mainAlbum {\n        id\n        coverArtUri\n      }\n    }\n    artist {\n      id\n      name\n      musicBrainzArtist {\n        id\n        images {\n          artistThumb\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query TopTrackRecommendations {\n    recommendations {\n      topTracks {\n        id\n        ...TopTrackCard_LastFmTrack\n      }\n    }\n  }\n"): (typeof documents)["\n  query TopTrackRecommendations {\n    recommendations {\n      topTracks {\n        id\n        ...TopTrackCard_LastFmTrack\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query SearchResultArtistSearch($text: String!) {\n    artist {\n      searchByName(name: $text, limit: 5) {\n        id\n        name\n        images {\n          artistThumb\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query SearchResultArtistSearch($text: String!) {\n    artist {\n      searchByName(name: $text, limit: 5) {\n        id\n        name\n        images {\n          artistThumb\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query SearchResultSongSearch($text: String!) {\n    recording {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        length\n        nameCredits {\n          artist {\n            id\n            name\n          }\n        }\n        mainAlbum {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query SearchResultSongSearch($text: String!) {\n    recording {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        length\n        nameCredits {\n          artist {\n            id\n            name\n          }\n        }\n        mainAlbum {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query SearchResultAlbumSearch($text: String!) {\n    releaseGroup {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        mainRelease {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query SearchResultAlbumSearch($text: String!) {\n    releaseGroup {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        mainRelease {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query Sidebar {\n    viewer {\n      id\n      username\n    }\n  }\n"): (typeof documents)["\n  query Sidebar {\n    viewer {\n      id\n      username\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment SoulSeekNetworkStatus_SoulSeekStatus on SoulSeekStatus {\n    id\n    status\n  }\n"): (typeof documents)["\n  fragment SoulSeekNetworkStatus_SoulSeekStatus on SoulSeekStatus {\n    id\n    status\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  subscription SoulSeekNetworkStatusSubscription {\n    soulSeekStatusUpdated {\n      ...SoulSeekNetworkStatus_SoulSeekStatus\n    }\n  }\n"): (typeof documents)["\n  subscription SoulSeekNetworkStatusSubscription {\n    soulSeekStatusUpdated {\n      ...SoulSeekNetworkStatus_SoulSeekStatus\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query SoulSeekNetworkStatusQuery {\n    external {\n      id\n      soulSeek {\n        id\n        status {\n          ...SoulSeekNetworkStatus_SoulSeekStatus\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query SoulSeekNetworkStatusQuery {\n    external {\n      id\n      soulSeek {\n        id\n        status {\n          ...SoulSeekNetworkStatus_SoulSeekStatus\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query UserPlaylistsLoader_Query($spotifyUsername: String!) {\n    playlist {\n      importPlaylists {\n        spotify {\n          spotifyPlaylistsForUser(username: $spotifyUsername) {\n            id\n            ...UserPlaylistsList_SpotifyPlaylist\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query UserPlaylistsLoader_Query($spotifyUsername: String!) {\n    playlist {\n      importPlaylists {\n        spotify {\n          spotifyPlaylistsForUser(username: $spotifyUsername) {\n            id\n            ...UserPlaylistsList_SpotifyPlaylist\n          }\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation ImportSpotifyPlaylistById($playlistId: String!, $userId: UUID!) {\n    importSpotifyPlaylistById(playlistId: $playlistId, userId: $userId) {\n      __typename\n      ... on ImportSpotifyPlaylistSuccess {\n        success\n      }\n      ... on ImportSpotifyPlaylistError {\n        message\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation ImportSpotifyPlaylistById($playlistId: String!, $userId: UUID!) {\n    importSpotifyPlaylistById(playlistId: $playlistId, userId: $userId) {\n      __typename\n      ... on ImportSpotifyPlaylistSuccess {\n        success\n      }\n      ... on ImportSpotifyPlaylistError {\n        message\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment UserPlaylistsList_SpotifyPlaylist on SpotifyPlaylist {\n    id\n    description\n    name\n    coverImageUrl\n  }\n"): (typeof documents)["\n  fragment UserPlaylistsList_SpotifyPlaylist on SpotifyPlaylist {\n    id\n    description\n    name\n    coverImageUrl\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment UserProfilePanel_User on User {\n    id\n    username\n    createdAt\n    updatedAt\n    likedSongs {\n      id\n    }\n  }\n"): (typeof documents)["\n  fragment UserProfilePanel_User on User {\n    id\n    username\n    createdAt\n    updatedAt\n    likedSongs {\n      id\n    }\n  }\n"];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;