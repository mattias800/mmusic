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
    "\n  query Bootstrap {\n    areThereAnyUsers\n    viewer {\n      id\n    }\n  }\n": typeof types.BootstrapDocument,
    "\n  query AlbumListQuery {\n    releaseGroup {\n      all {\n        id\n        ...AlbumList_ReleaseGroup\n      }\n    }\n  }\n": typeof types.AlbumListQueryDocument,
    "\n  query AlbumQuery($releaseGroupId: ID!) {\n    releaseGroup {\n      byId(id: $releaseGroupId) {\n        id\n        title\n        credits {\n          name\n        }\n        ...AlbumPanel_ReleaseGroup\n      }\n    }\n  }\n": typeof types.AlbumQueryDocument,
    "\n  query ArtistListQuery {\n    artist {\n      all {\n        id\n        ...ArtistList_Artist\n      }\n    }\n  }\n": typeof types.ArtistListQueryDocument,
    "\n  query ArtistQuery($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        name\n        ...ArtistPanel_Artist\n      }\n    }\n  }\n": typeof types.ArtistQueryDocument,
    "\n  query LikedSongsQuery {\n    viewer {\n      id\n      ...LikedSongsList_User\n    }\n  }\n": typeof types.LikedSongsQueryDocument,
    "\n  query SettingsPage {\n    serverSettings {\n      ...ServerSettingsFormContainer_ServerSettings\n    }\n  }\n": typeof types.SettingsPageDocument,
    "\n  query ProfilePage {\n    viewer {\n      id\n      username\n      ...UserProfilePanel_User\n    }\n  }\n": typeof types.ProfilePageDocument,
    "\n  fragment Playlist_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": typeof types.Playlist_UserFragmentDoc,
    "\n  fragment LikedSongRow_Recoding on Recording {\n    id\n    title\n    length\n    nameCredits {\n      artist {\n        id\n        name\n      }\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n": typeof types.LikedSongRow_RecodingFragmentDoc,
    "\n  mutation AddArtistToServerLibrary($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n    }\n  }\n": typeof types.AddArtistToServerLibraryDocument,
    "\n  fragment AlbumCard_ReleaseGroup on ReleaseGroupBase {\n    id\n    title\n    firstReleaseYear\n    ... on MbReleaseGroup {\n      coverArtUri\n      credits {\n        name\n        artist {\n          id\n          name\n        }\n      }\n    }\n    ... on ReleaseGroup {\n      coverArtUri\n      credits {\n        name\n        artist {\n          id\n          name\n        }\n      }\n    }\n  }\n": typeof types.AlbumCard_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumHeader_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    primaryType\n    coverArtUri\n    firstReleaseYear\n    credits {\n      artist {\n        id\n        name\n      }\n    }\n    mainRelease {\n      id\n      recordings {\n        id\n        length\n      }\n    }\n  }\n": typeof types.AlbumHeader_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumPanel_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumHeader_ReleaseGroup\n    ...AlbumTrackList_ReleaseGroup\n    firstReleaseYear\n    mainRelease {\n      labels {\n        id\n        name\n      }\n    }\n  }\n": typeof types.AlbumPanel_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumTrackList_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    mainRelease {\n      id\n      title\n      recordings {\n        id\n        ...RecordingPlayButton_Recording\n        title\n        length\n        statistics {\n          listeners\n        }\n        nameCredits {\n          name\n          artist {\n            id\n            name\n          }\n        }\n      }\n    }\n  }\n": typeof types.AlbumTrackList_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumList_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumCard_ReleaseGroup\n  }\n": typeof types.AlbumList_ReleaseGroupFragmentDoc,
    "\n  fragment ArtistCard_Artist on ArtistBase {\n    id\n    name\n    listeners\n    images {\n      artistThumb\n    }\n  }\n": typeof types.ArtistCard_ArtistFragmentDoc,
    "\n  fragment TopArtistCard_Artist on LastFmArtist {\n    id\n    name\n    musicBrainzArtist {\n      id\n      name\n      images {\n        artistThumb\n      }\n    }\n    statistics {\n      listeners\n    }\n  }\n": typeof types.TopArtistCard_ArtistFragmentDoc,
    "\n  fragment ArtistList_Artist on Artist {\n    id\n    ...ArtistCard_Artist\n  }\n": typeof types.ArtistList_ArtistFragmentDoc,
    "\n  fragment ArtistNotInLibraryTopTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      name\n      statistics {\n        listeners\n      }\n    }\n  }\n": typeof types.ArtistNotInLibraryTopTracks_ArtistFragmentDoc,
    "\n  mutation RefreshArtist($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n    }\n  }\n": typeof types.RefreshArtistDocument,
    "\n  query ArtistAlbumList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        albums {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": typeof types.ArtistAlbumListDocument,
    "\n  query ArtistEpList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        eps {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": typeof types.ArtistEpListDocument,
    "\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    listeners\n    albums {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    eps {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    singles {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    images {\n      artistBackground\n    }\n    serverStatus {\n      id\n      result {\n        __typename\n        ... on ArtistServerStatusResultBase {\n          topTracksVisible\n          releasesVisible\n        }\n        ... on ArtistServerStatusImportingArtistReleases {\n          numReleaseGroupsFinishedImporting\n          totalNumReleaseGroupsBeingImported\n        }\n      }\n    }\n    ...ArtistNotInLibraryTopTracks_Artist\n  }\n": typeof types.ArtistPanel_ArtistFragmentDoc,
    "\n  query ArtistSingleList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        singles {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": typeof types.ArtistSingleListDocument,
    "\n  fragment TopArtistTrackItem_LastFmTrack on LastFmTrack {\n    id\n    name\n    playCount\n    summary\n    recording {\n      id\n      title\n      length\n      ...RecordingPlayButton_Recording\n      mainAlbum {\n        id\n        title\n        coverArtUri\n      }\n    }\n  }\n": typeof types.TopArtistTrackItem_LastFmTrackFragmentDoc,
    "\n  query TopArtistTracks($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        topTracks {\n          id\n          ...TopArtistTrackItem_LastFmTrack\n        }\n      }\n    }\n  }\n": typeof types.TopArtistTracksDocument,
    "\n  query ArtistServerStatus($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        serverStatus {\n          id\n          result {\n            __typename\n            ... on ArtistServerStatusResultBase {\n              releasesVisible\n              topTracksVisible\n            }\n            ... on ArtistServerStatusImportingArtistReleases {\n              totalNumReleaseGroupsBeingImported\n              numReleaseGroupsFinishedImporting\n            }\n          }\n        }\n      }\n    }\n  }\n": typeof types.ArtistServerStatusDocument,
    "\n  subscription ArtistServerStatusSub($artistId: ID!) {\n    artistServerStatusUpdated(artistId: $artistId) {\n      id\n      result {\n        __typename\n        ... on ArtistServerStatusResultBase {\n          releasesVisible\n          topTracksVisible\n        }\n        ... on ArtistServerStatusImportingArtistReleases {\n          totalNumReleaseGroupsBeingImported\n          numReleaseGroupsFinishedImporting\n          artist {\n            id\n            albums {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n            eps {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n            singles {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n          }\n        }\n      }\n    }\n  }\n": typeof types.ArtistServerStatusSubDocument,
    "\n  mutation SignIn($username: String!, $password: String!) {\n    signIn(input: { username: $username, password: $password }) {\n      __typename\n      ... on SignInSuccess {\n        user {\n          id\n          username\n        }\n      }\n      ... on SignInError {\n        message\n      }\n    }\n  }\n": typeof types.SignInDocument,
    "\n  mutation SignOut {\n    signOut {\n      __typename\n\n      ... on SignOutSuccess {\n        success\n      }\n      ... on SignOutError {\n        message\n      }\n    }\n  }\n": typeof types.SignOutDocument,
    "\n  fragment DownloadOverview_DownloadStatus on DownloadStatus {\n    id\n    ...DownloadStatus_DownloadStatus\n  }\n": typeof types.DownloadOverview_DownloadStatusFragmentDoc,
    "\n  query DownloadOverviewQuery {\n    download {\n      all {\n        id\n        ...DownloadOverview_DownloadStatus\n      }\n    }\n  }\n": typeof types.DownloadOverviewQueryDocument,
    "\n  subscription DownloadOverviewSubscription {\n    downloadStatusUpdated {\n      id\n      ...DownloadOverview_DownloadStatus\n    }\n  }\n": typeof types.DownloadOverviewSubscriptionDocument,
    "\n  fragment DownloadStatus_DownloadStatus on DownloadStatus {\n    id\n    numberOfTracks\n    tracksDownloaded\n    statusDescription\n    release {\n      id\n      title\n      year\n      artists {\n        id\n        name\n      }\n      recordings {\n        id\n      }\n    }\n  }\n": typeof types.DownloadStatus_DownloadStatusFragmentDoc,
    "\n  mutation CreateUser($username: String!, $password: String!) {\n    createUser(input: { username: $username, password: $password }) {\n      __typename\n      ... on CreateUserSuccess {\n        # Assuming a similar success payload\n        user {\n          id\n          username\n        }\n      }\n      ... on CreateUserError {\n        # Assuming a similar error payload\n        message\n      }\n    }\n  }\n": typeof types.CreateUserDocument,
    "\n  fragment LikedSongRow_LikedSong on LikedSong {\n    id\n    recording {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n      mainAlbum {\n        id\n        title\n        coverArtUri\n        artists {\n          id\n        }\n      }\n    }\n  }\n": typeof types.LikedSongRow_LikedSongFragmentDoc,
    "\n  fragment LikedSongsList_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": typeof types.LikedSongsList_UserFragmentDoc,
    "\n  fragment RecordingPlayButton_Recording on Recording {\n    id\n    streamingServiceInfo {\n      id\n      youtubeVideoId\n    }\n  }\n": typeof types.RecordingPlayButton_RecordingFragmentDoc,
    "\n  query YoutubeVideoSearch($recordingId: ID!) {\n    musicBrainz {\n      recording {\n        byId(id: $recordingId) {\n          id\n          streamingServiceInfo {\n            id\n            youtubeSearchVideoId\n          }\n        }\n      }\n    }\n  }\n": typeof types.YoutubeVideoSearchDocument,
    "\n  mutation CreatePlaylist {\n    createPlaylist {\n      __typename\n      ... on CreatePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n": typeof types.CreatePlaylistDocument,
    "\n  query PlaylistList {\n    viewer {\n      id\n      playlists {\n        id\n        name\n        createdAt\n      }\n    }\n  }\n": typeof types.PlaylistListDocument,
    "\n  mutation RenamePlaylist($playlistId: String!, $newPlaylistName: String!) {\n    renamePlaylist(\n      input: { playlistId: $playlistId, newPlaylistName: $newPlaylistName }\n    ) {\n      __typename\n      ... on RenamePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n": typeof types.RenamePlaylistDocument,
    "\n  mutation DeletePlaylist($playlistId: String!) {\n    deletePlaylist(input: { playlistId: $playlistId }) {\n      __typename\n      ... on DeletePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n": typeof types.DeletePlaylistDocument,
    "\n  query TopArtistRecommendations {\n    recommendations {\n      topArtists {\n        id\n        ...TopArtistCard_Artist\n      }\n    }\n  }\n": typeof types.TopArtistRecommendationsDocument,
    "\n  fragment TopTrackCard_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    name\n    artist {\n      id\n      musicBrainzArtist {\n        id\n        name\n        images {\n          artistThumb\n        }\n      }\n    }\n    album {\n      id\n      imageUrl\n    }\n    images {\n      artistThumb\n    }\n  }\n": typeof types.TopTrackCard_LastFmTrackFragmentDoc,
    "\n  query TopTrackRecommendations {\n    recommendations {\n      topTracks {\n        id\n        ...TopTrackCard_LastFmTrack\n      }\n    }\n  }\n": typeof types.TopTrackRecommendationsDocument,
    "\n  query AlbumSearchResultsSearch($text: String!, $limit: Int!) {\n    musicBrainz {\n      releaseGroup {\n        searchByName(name: $text, limit: $limit) {\n          id\n          title\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": typeof types.AlbumSearchResultsSearchDocument,
    "\n  query ArtistSearchResultsSearch($searchText: String!, $limit: Int!) {\n    musicBrainz {\n      artist {\n        searchByName(name: $searchText, limit: $limit) {\n          id\n          name\n          ...ArtistCard_Artist\n          images {\n            artistThumb\n          }\n        }\n      }\n    }\n  }\n": typeof types.ArtistSearchResultsSearchDocument,
    "\n  query RecordingSearchResultsSearch($text: String!, $limit: Int!) {\n    musicBrainz {\n      recording {\n        searchByName(name: $text, limit: $limit) {\n          id\n          title\n          length\n          nameCredits {\n            artist {\n              id\n              name\n            }\n          }\n          mainAlbum {\n            id\n            title\n            coverArtUri\n          }\n        }\n      }\n    }\n  }\n": typeof types.RecordingSearchResultsSearchDocument,
    "\n  query SearchResultArtistSearch($text: String!) {\n    artist {\n      searchByName(name: $text, limit: 5) {\n        id\n        name\n        images {\n          artistThumb\n        }\n      }\n    }\n  }\n": typeof types.SearchResultArtistSearchDocument,
    "\n  query SearchResultSongSearch($text: String!) {\n    recording {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        length\n        nameCredits {\n          artist {\n            id\n            name\n          }\n        }\n        mainAlbum {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n": typeof types.SearchResultSongSearchDocument,
    "\n  query SearchResultAlbumSearch($text: String!) {\n    releaseGroup {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        mainRelease {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n": typeof types.SearchResultAlbumSearchDocument,
    "\n  fragment ServerSettingsFormContainer_ServerSettings on ServerSettings {\n    id\n    libraryPath\n  }\n": typeof types.ServerSettingsFormContainer_ServerSettingsFragmentDoc,
    "\n  mutation UpdateServerSettings($newLibraryPath: String!) {\n    updateLibraryPath(input: { newLibraryPath: $newLibraryPath }) {\n      ... on UpdateLibraryPathSuccess {\n        serverSettings {\n          id\n          libraryPath\n        }\n      }\n    }\n  }\n": typeof types.UpdateServerSettingsDocument,
    "\n  query Sidebar {\n    viewer {\n      id\n      username\n    }\n  }\n": typeof types.SidebarDocument,
    "\n  fragment SoulSeekNetworkStatus_SoulSeekStatus on SoulSeekStatus {\n    id\n    status\n  }\n": typeof types.SoulSeekNetworkStatus_SoulSeekStatusFragmentDoc,
    "\n  subscription SoulSeekNetworkStatusSubscription {\n    soulSeekStatusUpdated {\n      ...SoulSeekNetworkStatus_SoulSeekStatus\n    }\n  }\n": typeof types.SoulSeekNetworkStatusSubscriptionDocument,
    "\n  query SoulSeekNetworkStatusQuery {\n    external {\n      id\n      soulSeek {\n        id\n        status {\n          ...SoulSeekNetworkStatus_SoulSeekStatus\n        }\n      }\n    }\n  }\n": typeof types.SoulSeekNetworkStatusQueryDocument,
    "\n  mutation ImportSpotifyPlaylistById($playlistId: String!, $userId: UUID!) {\n    importSpotifyPlaylistById(playlistId: $playlistId, userId: $userId) {\n      __typename\n      ... on ImportSpotifyPlaylistSuccess {\n        success\n      }\n      ... on ImportSpotifyPlaylistError {\n        message\n      }\n    }\n  }\n": typeof types.ImportSpotifyPlaylistByIdDocument,
    "\n  fragment SpotifyPlaylistsList_SpotifyPlaylist on SpotifyPlaylist {\n    id\n    description\n    name\n    coverImageUrl\n  }\n": typeof types.SpotifyPlaylistsList_SpotifyPlaylistFragmentDoc,
    "\n  query UserPlaylistsLoader_Query($spotifyUsername: String!) {\n    playlist {\n      importPlaylists {\n        spotify {\n          spotifyPlaylistsForUser(username: $spotifyUsername) {\n            id\n            ...SpotifyPlaylistsList_SpotifyPlaylist\n          }\n        }\n      }\n    }\n  }\n": typeof types.UserPlaylistsLoader_QueryDocument,
    "\n  fragment UserProfilePanel_User on User {\n    id\n    username\n    createdAt\n    updatedAt\n    likedSongs {\n      id\n    }\n  }\n": typeof types.UserProfilePanel_UserFragmentDoc,
    "\n  query UserProfileWidget {\n    viewer {\n      id\n      username\n    }\n  }\n": typeof types.UserProfileWidgetDocument,
};
const documents: Documents = {
    "\n  query Bootstrap {\n    areThereAnyUsers\n    viewer {\n      id\n    }\n  }\n": types.BootstrapDocument,
    "\n  query AlbumListQuery {\n    releaseGroup {\n      all {\n        id\n        ...AlbumList_ReleaseGroup\n      }\n    }\n  }\n": types.AlbumListQueryDocument,
    "\n  query AlbumQuery($releaseGroupId: ID!) {\n    releaseGroup {\n      byId(id: $releaseGroupId) {\n        id\n        title\n        credits {\n          name\n        }\n        ...AlbumPanel_ReleaseGroup\n      }\n    }\n  }\n": types.AlbumQueryDocument,
    "\n  query ArtistListQuery {\n    artist {\n      all {\n        id\n        ...ArtistList_Artist\n      }\n    }\n  }\n": types.ArtistListQueryDocument,
    "\n  query ArtistQuery($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        name\n        ...ArtistPanel_Artist\n      }\n    }\n  }\n": types.ArtistQueryDocument,
    "\n  query LikedSongsQuery {\n    viewer {\n      id\n      ...LikedSongsList_User\n    }\n  }\n": types.LikedSongsQueryDocument,
    "\n  query SettingsPage {\n    serverSettings {\n      ...ServerSettingsFormContainer_ServerSettings\n    }\n  }\n": types.SettingsPageDocument,
    "\n  query ProfilePage {\n    viewer {\n      id\n      username\n      ...UserProfilePanel_User\n    }\n  }\n": types.ProfilePageDocument,
    "\n  fragment Playlist_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": types.Playlist_UserFragmentDoc,
    "\n  fragment LikedSongRow_Recoding on Recording {\n    id\n    title\n    length\n    nameCredits {\n      artist {\n        id\n        name\n      }\n    }\n    mainAlbum {\n      id\n      title\n      coverArtUri\n    }\n  }\n": types.LikedSongRow_RecodingFragmentDoc,
    "\n  mutation AddArtistToServerLibrary($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n    }\n  }\n": types.AddArtistToServerLibraryDocument,
    "\n  fragment AlbumCard_ReleaseGroup on ReleaseGroupBase {\n    id\n    title\n    firstReleaseYear\n    ... on MbReleaseGroup {\n      coverArtUri\n      credits {\n        name\n        artist {\n          id\n          name\n        }\n      }\n    }\n    ... on ReleaseGroup {\n      coverArtUri\n      credits {\n        name\n        artist {\n          id\n          name\n        }\n      }\n    }\n  }\n": types.AlbumCard_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumHeader_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    primaryType\n    coverArtUri\n    firstReleaseYear\n    credits {\n      artist {\n        id\n        name\n      }\n    }\n    mainRelease {\n      id\n      recordings {\n        id\n        length\n      }\n    }\n  }\n": types.AlbumHeader_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumPanel_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumHeader_ReleaseGroup\n    ...AlbumTrackList_ReleaseGroup\n    firstReleaseYear\n    mainRelease {\n      labels {\n        id\n        name\n      }\n    }\n  }\n": types.AlbumPanel_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumTrackList_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    mainRelease {\n      id\n      title\n      recordings {\n        id\n        ...RecordingPlayButton_Recording\n        title\n        length\n        statistics {\n          listeners\n        }\n        nameCredits {\n          name\n          artist {\n            id\n            name\n          }\n        }\n      }\n    }\n  }\n": types.AlbumTrackList_ReleaseGroupFragmentDoc,
    "\n  fragment AlbumList_ReleaseGroup on ReleaseGroup {\n    id\n    ...AlbumCard_ReleaseGroup\n  }\n": types.AlbumList_ReleaseGroupFragmentDoc,
    "\n  fragment ArtistCard_Artist on ArtistBase {\n    id\n    name\n    listeners\n    images {\n      artistThumb\n    }\n  }\n": types.ArtistCard_ArtistFragmentDoc,
    "\n  fragment TopArtistCard_Artist on LastFmArtist {\n    id\n    name\n    musicBrainzArtist {\n      id\n      name\n      images {\n        artistThumb\n      }\n    }\n    statistics {\n      listeners\n    }\n  }\n": types.TopArtistCard_ArtistFragmentDoc,
    "\n  fragment ArtistList_Artist on Artist {\n    id\n    ...ArtistCard_Artist\n  }\n": types.ArtistList_ArtistFragmentDoc,
    "\n  fragment ArtistNotInLibraryTopTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      name\n      statistics {\n        listeners\n      }\n    }\n  }\n": types.ArtistNotInLibraryTopTracks_ArtistFragmentDoc,
    "\n  mutation RefreshArtist($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n    }\n  }\n": types.RefreshArtistDocument,
    "\n  query ArtistAlbumList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        albums {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": types.ArtistAlbumListDocument,
    "\n  query ArtistEpList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        eps {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": types.ArtistEpListDocument,
    "\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    listeners\n    albums {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    eps {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    singles {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    images {\n      artistBackground\n    }\n    serverStatus {\n      id\n      result {\n        __typename\n        ... on ArtistServerStatusResultBase {\n          topTracksVisible\n          releasesVisible\n        }\n        ... on ArtistServerStatusImportingArtistReleases {\n          numReleaseGroupsFinishedImporting\n          totalNumReleaseGroupsBeingImported\n        }\n      }\n    }\n    ...ArtistNotInLibraryTopTracks_Artist\n  }\n": types.ArtistPanel_ArtistFragmentDoc,
    "\n  query ArtistSingleList($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        singles {\n          id\n          firstReleaseDate\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": types.ArtistSingleListDocument,
    "\n  fragment TopArtistTrackItem_LastFmTrack on LastFmTrack {\n    id\n    name\n    playCount\n    summary\n    recording {\n      id\n      title\n      length\n      ...RecordingPlayButton_Recording\n      mainAlbum {\n        id\n        title\n        coverArtUri\n      }\n    }\n  }\n": types.TopArtistTrackItem_LastFmTrackFragmentDoc,
    "\n  query TopArtistTracks($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        topTracks {\n          id\n          ...TopArtistTrackItem_LastFmTrack\n        }\n      }\n    }\n  }\n": types.TopArtistTracksDocument,
    "\n  query ArtistServerStatus($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        serverStatus {\n          id\n          result {\n            __typename\n            ... on ArtistServerStatusResultBase {\n              releasesVisible\n              topTracksVisible\n            }\n            ... on ArtistServerStatusImportingArtistReleases {\n              totalNumReleaseGroupsBeingImported\n              numReleaseGroupsFinishedImporting\n            }\n          }\n        }\n      }\n    }\n  }\n": types.ArtistServerStatusDocument,
    "\n  subscription ArtistServerStatusSub($artistId: ID!) {\n    artistServerStatusUpdated(artistId: $artistId) {\n      id\n      result {\n        __typename\n        ... on ArtistServerStatusResultBase {\n          releasesVisible\n          topTracksVisible\n        }\n        ... on ArtistServerStatusImportingArtistReleases {\n          totalNumReleaseGroupsBeingImported\n          numReleaseGroupsFinishedImporting\n          artist {\n            id\n            albums {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n            eps {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n            singles {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n          }\n        }\n      }\n    }\n  }\n": types.ArtistServerStatusSubDocument,
    "\n  mutation SignIn($username: String!, $password: String!) {\n    signIn(input: { username: $username, password: $password }) {\n      __typename\n      ... on SignInSuccess {\n        user {\n          id\n          username\n        }\n      }\n      ... on SignInError {\n        message\n      }\n    }\n  }\n": types.SignInDocument,
    "\n  mutation SignOut {\n    signOut {\n      __typename\n\n      ... on SignOutSuccess {\n        success\n      }\n      ... on SignOutError {\n        message\n      }\n    }\n  }\n": types.SignOutDocument,
    "\n  fragment DownloadOverview_DownloadStatus on DownloadStatus {\n    id\n    ...DownloadStatus_DownloadStatus\n  }\n": types.DownloadOverview_DownloadStatusFragmentDoc,
    "\n  query DownloadOverviewQuery {\n    download {\n      all {\n        id\n        ...DownloadOverview_DownloadStatus\n      }\n    }\n  }\n": types.DownloadOverviewQueryDocument,
    "\n  subscription DownloadOverviewSubscription {\n    downloadStatusUpdated {\n      id\n      ...DownloadOverview_DownloadStatus\n    }\n  }\n": types.DownloadOverviewSubscriptionDocument,
    "\n  fragment DownloadStatus_DownloadStatus on DownloadStatus {\n    id\n    numberOfTracks\n    tracksDownloaded\n    statusDescription\n    release {\n      id\n      title\n      year\n      artists {\n        id\n        name\n      }\n      recordings {\n        id\n      }\n    }\n  }\n": types.DownloadStatus_DownloadStatusFragmentDoc,
    "\n  mutation CreateUser($username: String!, $password: String!) {\n    createUser(input: { username: $username, password: $password }) {\n      __typename\n      ... on CreateUserSuccess {\n        # Assuming a similar success payload\n        user {\n          id\n          username\n        }\n      }\n      ... on CreateUserError {\n        # Assuming a similar error payload\n        message\n      }\n    }\n  }\n": types.CreateUserDocument,
    "\n  fragment LikedSongRow_LikedSong on LikedSong {\n    id\n    recording {\n      id\n      title\n      length\n      artists {\n        id\n        name\n      }\n      mainAlbum {\n        id\n        title\n        coverArtUri\n        artists {\n          id\n        }\n      }\n    }\n  }\n": types.LikedSongRow_LikedSongFragmentDoc,
    "\n  fragment LikedSongsList_User on User {\n    id\n    likedSongs {\n      id\n      ...LikedSongRow_LikedSong\n    }\n  }\n": types.LikedSongsList_UserFragmentDoc,
    "\n  fragment RecordingPlayButton_Recording on Recording {\n    id\n    streamingServiceInfo {\n      id\n      youtubeVideoId\n    }\n  }\n": types.RecordingPlayButton_RecordingFragmentDoc,
    "\n  query YoutubeVideoSearch($recordingId: ID!) {\n    musicBrainz {\n      recording {\n        byId(id: $recordingId) {\n          id\n          streamingServiceInfo {\n            id\n            youtubeSearchVideoId\n          }\n        }\n      }\n    }\n  }\n": types.YoutubeVideoSearchDocument,
    "\n  mutation CreatePlaylist {\n    createPlaylist {\n      __typename\n      ... on CreatePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n": types.CreatePlaylistDocument,
    "\n  query PlaylistList {\n    viewer {\n      id\n      playlists {\n        id\n        name\n        createdAt\n      }\n    }\n  }\n": types.PlaylistListDocument,
    "\n  mutation RenamePlaylist($playlistId: String!, $newPlaylistName: String!) {\n    renamePlaylist(\n      input: { playlistId: $playlistId, newPlaylistName: $newPlaylistName }\n    ) {\n      __typename\n      ... on RenamePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n": types.RenamePlaylistDocument,
    "\n  mutation DeletePlaylist($playlistId: String!) {\n    deletePlaylist(input: { playlistId: $playlistId }) {\n      __typename\n      ... on DeletePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n": types.DeletePlaylistDocument,
    "\n  query TopArtistRecommendations {\n    recommendations {\n      topArtists {\n        id\n        ...TopArtistCard_Artist\n      }\n    }\n  }\n": types.TopArtistRecommendationsDocument,
    "\n  fragment TopTrackCard_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    name\n    artist {\n      id\n      musicBrainzArtist {\n        id\n        name\n        images {\n          artistThumb\n        }\n      }\n    }\n    album {\n      id\n      imageUrl\n    }\n    images {\n      artistThumb\n    }\n  }\n": types.TopTrackCard_LastFmTrackFragmentDoc,
    "\n  query TopTrackRecommendations {\n    recommendations {\n      topTracks {\n        id\n        ...TopTrackCard_LastFmTrack\n      }\n    }\n  }\n": types.TopTrackRecommendationsDocument,
    "\n  query AlbumSearchResultsSearch($text: String!, $limit: Int!) {\n    musicBrainz {\n      releaseGroup {\n        searchByName(name: $text, limit: $limit) {\n          id\n          title\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n": types.AlbumSearchResultsSearchDocument,
    "\n  query ArtistSearchResultsSearch($searchText: String!, $limit: Int!) {\n    musicBrainz {\n      artist {\n        searchByName(name: $searchText, limit: $limit) {\n          id\n          name\n          ...ArtistCard_Artist\n          images {\n            artistThumb\n          }\n        }\n      }\n    }\n  }\n": types.ArtistSearchResultsSearchDocument,
    "\n  query RecordingSearchResultsSearch($text: String!, $limit: Int!) {\n    musicBrainz {\n      recording {\n        searchByName(name: $text, limit: $limit) {\n          id\n          title\n          length\n          nameCredits {\n            artist {\n              id\n              name\n            }\n          }\n          mainAlbum {\n            id\n            title\n            coverArtUri\n          }\n        }\n      }\n    }\n  }\n": types.RecordingSearchResultsSearchDocument,
    "\n  query SearchResultArtistSearch($text: String!) {\n    artist {\n      searchByName(name: $text, limit: 5) {\n        id\n        name\n        images {\n          artistThumb\n        }\n      }\n    }\n  }\n": types.SearchResultArtistSearchDocument,
    "\n  query SearchResultSongSearch($text: String!) {\n    recording {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        length\n        nameCredits {\n          artist {\n            id\n            name\n          }\n        }\n        mainAlbum {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n": types.SearchResultSongSearchDocument,
    "\n  query SearchResultAlbumSearch($text: String!) {\n    releaseGroup {\n      searchByName(name: $text, limit: 5) {\n        id\n        title\n        mainRelease {\n          id\n          title\n          coverArtUri\n        }\n      }\n    }\n  }\n": types.SearchResultAlbumSearchDocument,
    "\n  fragment ServerSettingsFormContainer_ServerSettings on ServerSettings {\n    id\n    libraryPath\n  }\n": types.ServerSettingsFormContainer_ServerSettingsFragmentDoc,
    "\n  mutation UpdateServerSettings($newLibraryPath: String!) {\n    updateLibraryPath(input: { newLibraryPath: $newLibraryPath }) {\n      ... on UpdateLibraryPathSuccess {\n        serverSettings {\n          id\n          libraryPath\n        }\n      }\n    }\n  }\n": types.UpdateServerSettingsDocument,
    "\n  query Sidebar {\n    viewer {\n      id\n      username\n    }\n  }\n": types.SidebarDocument,
    "\n  fragment SoulSeekNetworkStatus_SoulSeekStatus on SoulSeekStatus {\n    id\n    status\n  }\n": types.SoulSeekNetworkStatus_SoulSeekStatusFragmentDoc,
    "\n  subscription SoulSeekNetworkStatusSubscription {\n    soulSeekStatusUpdated {\n      ...SoulSeekNetworkStatus_SoulSeekStatus\n    }\n  }\n": types.SoulSeekNetworkStatusSubscriptionDocument,
    "\n  query SoulSeekNetworkStatusQuery {\n    external {\n      id\n      soulSeek {\n        id\n        status {\n          ...SoulSeekNetworkStatus_SoulSeekStatus\n        }\n      }\n    }\n  }\n": types.SoulSeekNetworkStatusQueryDocument,
    "\n  mutation ImportSpotifyPlaylistById($playlistId: String!, $userId: UUID!) {\n    importSpotifyPlaylistById(playlistId: $playlistId, userId: $userId) {\n      __typename\n      ... on ImportSpotifyPlaylistSuccess {\n        success\n      }\n      ... on ImportSpotifyPlaylistError {\n        message\n      }\n    }\n  }\n": types.ImportSpotifyPlaylistByIdDocument,
    "\n  fragment SpotifyPlaylistsList_SpotifyPlaylist on SpotifyPlaylist {\n    id\n    description\n    name\n    coverImageUrl\n  }\n": types.SpotifyPlaylistsList_SpotifyPlaylistFragmentDoc,
    "\n  query UserPlaylistsLoader_Query($spotifyUsername: String!) {\n    playlist {\n      importPlaylists {\n        spotify {\n          spotifyPlaylistsForUser(username: $spotifyUsername) {\n            id\n            ...SpotifyPlaylistsList_SpotifyPlaylist\n          }\n        }\n      }\n    }\n  }\n": types.UserPlaylistsLoader_QueryDocument,
    "\n  fragment UserProfilePanel_User on User {\n    id\n    username\n    createdAt\n    updatedAt\n    likedSongs {\n      id\n    }\n  }\n": types.UserProfilePanel_UserFragmentDoc,
    "\n  query UserProfileWidget {\n    viewer {\n      id\n      username\n    }\n  }\n": types.UserProfileWidgetDocument,
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
export function graphql(source: "\n  query Bootstrap {\n    areThereAnyUsers\n    viewer {\n      id\n    }\n  }\n"): (typeof documents)["\n  query Bootstrap {\n    areThereAnyUsers\n    viewer {\n      id\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query AlbumListQuery {\n    releaseGroup {\n      all {\n        id\n        ...AlbumList_ReleaseGroup\n      }\n    }\n  }\n"): (typeof documents)["\n  query AlbumListQuery {\n    releaseGroup {\n      all {\n        id\n        ...AlbumList_ReleaseGroup\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query AlbumQuery($releaseGroupId: ID!) {\n    releaseGroup {\n      byId(id: $releaseGroupId) {\n        id\n        title\n        credits {\n          name\n        }\n        ...AlbumPanel_ReleaseGroup\n      }\n    }\n  }\n"): (typeof documents)["\n  query AlbumQuery($releaseGroupId: ID!) {\n    releaseGroup {\n      byId(id: $releaseGroupId) {\n        id\n        title\n        credits {\n          name\n        }\n        ...AlbumPanel_ReleaseGroup\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ArtistListQuery {\n    artist {\n      all {\n        id\n        ...ArtistList_Artist\n      }\n    }\n  }\n"): (typeof documents)["\n  query ArtistListQuery {\n    artist {\n      all {\n        id\n        ...ArtistList_Artist\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ArtistQuery($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        name\n        ...ArtistPanel_Artist\n      }\n    }\n  }\n"): (typeof documents)["\n  query ArtistQuery($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        name\n        ...ArtistPanel_Artist\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query LikedSongsQuery {\n    viewer {\n      id\n      ...LikedSongsList_User\n    }\n  }\n"): (typeof documents)["\n  query LikedSongsQuery {\n    viewer {\n      id\n      ...LikedSongsList_User\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query SettingsPage {\n    serverSettings {\n      ...ServerSettingsFormContainer_ServerSettings\n    }\n  }\n"): (typeof documents)["\n  query SettingsPage {\n    serverSettings {\n      ...ServerSettingsFormContainer_ServerSettings\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ProfilePage {\n    viewer {\n      id\n      username\n      ...UserProfilePanel_User\n    }\n  }\n"): (typeof documents)["\n  query ProfilePage {\n    viewer {\n      id\n      username\n      ...UserProfilePanel_User\n    }\n  }\n"];
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
export function graphql(source: "\n  mutation AddArtistToServerLibrary($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n    }\n  }\n"): (typeof documents)["\n  mutation AddArtistToServerLibrary($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlbumCard_ReleaseGroup on ReleaseGroupBase {\n    id\n    title\n    firstReleaseYear\n    ... on MbReleaseGroup {\n      coverArtUri\n      credits {\n        name\n        artist {\n          id\n          name\n        }\n      }\n    }\n    ... on ReleaseGroup {\n      coverArtUri\n      credits {\n        name\n        artist {\n          id\n          name\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment AlbumCard_ReleaseGroup on ReleaseGroupBase {\n    id\n    title\n    firstReleaseYear\n    ... on MbReleaseGroup {\n      coverArtUri\n      credits {\n        name\n        artist {\n          id\n          name\n        }\n      }\n    }\n    ... on ReleaseGroup {\n      coverArtUri\n      credits {\n        name\n        artist {\n          id\n          name\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment AlbumHeader_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    primaryType\n    coverArtUri\n    firstReleaseYear\n    credits {\n      artist {\n        id\n        name\n      }\n    }\n    mainRelease {\n      id\n      recordings {\n        id\n        length\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment AlbumHeader_ReleaseGroup on ReleaseGroup {\n    id\n    title\n    primaryType\n    coverArtUri\n    firstReleaseYear\n    credits {\n      artist {\n        id\n        name\n      }\n    }\n    mainRelease {\n      id\n      recordings {\n        id\n        length\n      }\n    }\n  }\n"];
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
export function graphql(source: "\n  fragment ArtistCard_Artist on ArtistBase {\n    id\n    name\n    listeners\n    images {\n      artistThumb\n    }\n  }\n"): (typeof documents)["\n  fragment ArtistCard_Artist on ArtistBase {\n    id\n    name\n    listeners\n    images {\n      artistThumb\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment TopArtistCard_Artist on LastFmArtist {\n    id\n    name\n    musicBrainzArtist {\n      id\n      name\n      images {\n        artistThumb\n      }\n    }\n    statistics {\n      listeners\n    }\n  }\n"): (typeof documents)["\n  fragment TopArtistCard_Artist on LastFmArtist {\n    id\n    name\n    musicBrainzArtist {\n      id\n      name\n      images {\n        artistThumb\n      }\n    }\n    statistics {\n      listeners\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ArtistList_Artist on Artist {\n    id\n    ...ArtistCard_Artist\n  }\n"): (typeof documents)["\n  fragment ArtistList_Artist on Artist {\n    id\n    ...ArtistCard_Artist\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment ArtistNotInLibraryTopTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      name\n      statistics {\n        listeners\n      }\n    }\n  }\n"): (typeof documents)["\n  fragment ArtistNotInLibraryTopTracks_Artist on Artist {\n    id\n    topTracks {\n      id\n      name\n      statistics {\n        listeners\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation RefreshArtist($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n    }\n  }\n"): (typeof documents)["\n  mutation RefreshArtist($artistId: ID!) {\n    addArtistToServerLibrary(input: { artistId: $artistId }) {\n      __typename\n    }\n  }\n"];
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
export function graphql(source: "\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    listeners\n    albums {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    eps {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    singles {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    images {\n      artistBackground\n    }\n    serverStatus {\n      id\n      result {\n        __typename\n        ... on ArtistServerStatusResultBase {\n          topTracksVisible\n          releasesVisible\n        }\n        ... on ArtistServerStatusImportingArtistReleases {\n          numReleaseGroupsFinishedImporting\n          totalNumReleaseGroupsBeingImported\n        }\n      }\n    }\n    ...ArtistNotInLibraryTopTracks_Artist\n  }\n"): (typeof documents)["\n  fragment ArtistPanel_Artist on Artist {\n    id\n    name\n    listeners\n    albums {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    eps {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    singles {\n      id\n      firstReleaseDate\n      ...AlbumCard_ReleaseGroup\n    }\n    images {\n      artistBackground\n    }\n    serverStatus {\n      id\n      result {\n        __typename\n        ... on ArtistServerStatusResultBase {\n          topTracksVisible\n          releasesVisible\n        }\n        ... on ArtistServerStatusImportingArtistReleases {\n          numReleaseGroupsFinishedImporting\n          totalNumReleaseGroupsBeingImported\n        }\n      }\n    }\n    ...ArtistNotInLibraryTopTracks_Artist\n  }\n"];
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
export function graphql(source: "\n  query TopArtistTracks($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        topTracks {\n          id\n          ...TopArtistTrackItem_LastFmTrack\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query TopArtistTracks($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        topTracks {\n          id\n          ...TopArtistTrackItem_LastFmTrack\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ArtistServerStatus($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        serverStatus {\n          id\n          result {\n            __typename\n            ... on ArtistServerStatusResultBase {\n              releasesVisible\n              topTracksVisible\n            }\n            ... on ArtistServerStatusImportingArtistReleases {\n              totalNumReleaseGroupsBeingImported\n              numReleaseGroupsFinishedImporting\n            }\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query ArtistServerStatus($artistId: ID!) {\n    artist {\n      byId(id: $artistId) {\n        id\n        serverStatus {\n          id\n          result {\n            __typename\n            ... on ArtistServerStatusResultBase {\n              releasesVisible\n              topTracksVisible\n            }\n            ... on ArtistServerStatusImportingArtistReleases {\n              totalNumReleaseGroupsBeingImported\n              numReleaseGroupsFinishedImporting\n            }\n          }\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  subscription ArtistServerStatusSub($artistId: ID!) {\n    artistServerStatusUpdated(artistId: $artistId) {\n      id\n      result {\n        __typename\n        ... on ArtistServerStatusResultBase {\n          releasesVisible\n          topTracksVisible\n        }\n        ... on ArtistServerStatusImportingArtistReleases {\n          totalNumReleaseGroupsBeingImported\n          numReleaseGroupsFinishedImporting\n          artist {\n            id\n            albums {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n            eps {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n            singles {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  subscription ArtistServerStatusSub($artistId: ID!) {\n    artistServerStatusUpdated(artistId: $artistId) {\n      id\n      result {\n        __typename\n        ... on ArtistServerStatusResultBase {\n          releasesVisible\n          topTracksVisible\n        }\n        ... on ArtistServerStatusImportingArtistReleases {\n          totalNumReleaseGroupsBeingImported\n          numReleaseGroupsFinishedImporting\n          artist {\n            id\n            albums {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n            eps {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n            singles {\n              id\n              firstReleaseDate\n              ...AlbumCard_ReleaseGroup\n            }\n          }\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation SignIn($username: String!, $password: String!) {\n    signIn(input: { username: $username, password: $password }) {\n      __typename\n      ... on SignInSuccess {\n        user {\n          id\n          username\n        }\n      }\n      ... on SignInError {\n        message\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation SignIn($username: String!, $password: String!) {\n    signIn(input: { username: $username, password: $password }) {\n      __typename\n      ... on SignInSuccess {\n        user {\n          id\n          username\n        }\n      }\n      ... on SignInError {\n        message\n      }\n    }\n  }\n"];
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
export function graphql(source: "\n  mutation CreateUser($username: String!, $password: String!) {\n    createUser(input: { username: $username, password: $password }) {\n      __typename\n      ... on CreateUserSuccess {\n        # Assuming a similar success payload\n        user {\n          id\n          username\n        }\n      }\n      ... on CreateUserError {\n        # Assuming a similar error payload\n        message\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation CreateUser($username: String!, $password: String!) {\n    createUser(input: { username: $username, password: $password }) {\n      __typename\n      ... on CreateUserSuccess {\n        # Assuming a similar success payload\n        user {\n          id\n          username\n        }\n      }\n      ... on CreateUserError {\n        # Assuming a similar error payload\n        message\n      }\n    }\n  }\n"];
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
export function graphql(source: "\n  mutation CreatePlaylist {\n    createPlaylist {\n      __typename\n      ... on CreatePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation CreatePlaylist {\n    createPlaylist {\n      __typename\n      ... on CreatePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query PlaylistList {\n    viewer {\n      id\n      playlists {\n        id\n        name\n        createdAt\n      }\n    }\n  }\n"): (typeof documents)["\n  query PlaylistList {\n    viewer {\n      id\n      playlists {\n        id\n        name\n        createdAt\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation RenamePlaylist($playlistId: String!, $newPlaylistName: String!) {\n    renamePlaylist(\n      input: { playlistId: $playlistId, newPlaylistName: $newPlaylistName }\n    ) {\n      __typename\n      ... on RenamePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation RenamePlaylist($playlistId: String!, $newPlaylistName: String!) {\n    renamePlaylist(\n      input: { playlistId: $playlistId, newPlaylistName: $newPlaylistName }\n    ) {\n      __typename\n      ... on RenamePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation DeletePlaylist($playlistId: String!) {\n    deletePlaylist(input: { playlistId: $playlistId }) {\n      __typename\n      ... on DeletePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation DeletePlaylist($playlistId: String!) {\n    deletePlaylist(input: { playlistId: $playlistId }) {\n      __typename\n      ... on DeletePlaylistSuccess {\n        viewer {\n          id\n          playlists {\n            id\n            name\n            createdAt\n          }\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query TopArtistRecommendations {\n    recommendations {\n      topArtists {\n        id\n        ...TopArtistCard_Artist\n      }\n    }\n  }\n"): (typeof documents)["\n  query TopArtistRecommendations {\n    recommendations {\n      topArtists {\n        id\n        ...TopArtistCard_Artist\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment TopTrackCard_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    name\n    artist {\n      id\n      musicBrainzArtist {\n        id\n        name\n        images {\n          artistThumb\n        }\n      }\n    }\n    album {\n      id\n      imageUrl\n    }\n    images {\n      artistThumb\n    }\n  }\n"): (typeof documents)["\n  fragment TopTrackCard_LastFmTrack on LastFmTrack {\n    id\n    playCount\n    name\n    artist {\n      id\n      musicBrainzArtist {\n        id\n        name\n        images {\n          artistThumb\n        }\n      }\n    }\n    album {\n      id\n      imageUrl\n    }\n    images {\n      artistThumb\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query TopTrackRecommendations {\n    recommendations {\n      topTracks {\n        id\n        ...TopTrackCard_LastFmTrack\n      }\n    }\n  }\n"): (typeof documents)["\n  query TopTrackRecommendations {\n    recommendations {\n      topTracks {\n        id\n        ...TopTrackCard_LastFmTrack\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query AlbumSearchResultsSearch($text: String!, $limit: Int!) {\n    musicBrainz {\n      releaseGroup {\n        searchByName(name: $text, limit: $limit) {\n          id\n          title\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query AlbumSearchResultsSearch($text: String!, $limit: Int!) {\n    musicBrainz {\n      releaseGroup {\n        searchByName(name: $text, limit: $limit) {\n          id\n          title\n          ...AlbumCard_ReleaseGroup\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query ArtistSearchResultsSearch($searchText: String!, $limit: Int!) {\n    musicBrainz {\n      artist {\n        searchByName(name: $searchText, limit: $limit) {\n          id\n          name\n          ...ArtistCard_Artist\n          images {\n            artistThumb\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query ArtistSearchResultsSearch($searchText: String!, $limit: Int!) {\n    musicBrainz {\n      artist {\n        searchByName(name: $searchText, limit: $limit) {\n          id\n          name\n          ...ArtistCard_Artist\n          images {\n            artistThumb\n          }\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query RecordingSearchResultsSearch($text: String!, $limit: Int!) {\n    musicBrainz {\n      recording {\n        searchByName(name: $text, limit: $limit) {\n          id\n          title\n          length\n          nameCredits {\n            artist {\n              id\n              name\n            }\n          }\n          mainAlbum {\n            id\n            title\n            coverArtUri\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query RecordingSearchResultsSearch($text: String!, $limit: Int!) {\n    musicBrainz {\n      recording {\n        searchByName(name: $text, limit: $limit) {\n          id\n          title\n          length\n          nameCredits {\n            artist {\n              id\n              name\n            }\n          }\n          mainAlbum {\n            id\n            title\n            coverArtUri\n          }\n        }\n      }\n    }\n  }\n"];
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
export function graphql(source: "\n  fragment ServerSettingsFormContainer_ServerSettings on ServerSettings {\n    id\n    libraryPath\n  }\n"): (typeof documents)["\n  fragment ServerSettingsFormContainer_ServerSettings on ServerSettings {\n    id\n    libraryPath\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  mutation UpdateServerSettings($newLibraryPath: String!) {\n    updateLibraryPath(input: { newLibraryPath: $newLibraryPath }) {\n      ... on UpdateLibraryPathSuccess {\n        serverSettings {\n          id\n          libraryPath\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation UpdateServerSettings($newLibraryPath: String!) {\n    updateLibraryPath(input: { newLibraryPath: $newLibraryPath }) {\n      ... on UpdateLibraryPathSuccess {\n        serverSettings {\n          id\n          libraryPath\n        }\n      }\n    }\n  }\n"];
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
export function graphql(source: "\n  mutation ImportSpotifyPlaylistById($playlistId: String!, $userId: UUID!) {\n    importSpotifyPlaylistById(playlistId: $playlistId, userId: $userId) {\n      __typename\n      ... on ImportSpotifyPlaylistSuccess {\n        success\n      }\n      ... on ImportSpotifyPlaylistError {\n        message\n      }\n    }\n  }\n"): (typeof documents)["\n  mutation ImportSpotifyPlaylistById($playlistId: String!, $userId: UUID!) {\n    importSpotifyPlaylistById(playlistId: $playlistId, userId: $userId) {\n      __typename\n      ... on ImportSpotifyPlaylistSuccess {\n        success\n      }\n      ... on ImportSpotifyPlaylistError {\n        message\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment SpotifyPlaylistsList_SpotifyPlaylist on SpotifyPlaylist {\n    id\n    description\n    name\n    coverImageUrl\n  }\n"): (typeof documents)["\n  fragment SpotifyPlaylistsList_SpotifyPlaylist on SpotifyPlaylist {\n    id\n    description\n    name\n    coverImageUrl\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query UserPlaylistsLoader_Query($spotifyUsername: String!) {\n    playlist {\n      importPlaylists {\n        spotify {\n          spotifyPlaylistsForUser(username: $spotifyUsername) {\n            id\n            ...SpotifyPlaylistsList_SpotifyPlaylist\n          }\n        }\n      }\n    }\n  }\n"): (typeof documents)["\n  query UserPlaylistsLoader_Query($spotifyUsername: String!) {\n    playlist {\n      importPlaylists {\n        spotify {\n          spotifyPlaylistsForUser(username: $spotifyUsername) {\n            id\n            ...SpotifyPlaylistsList_SpotifyPlaylist\n          }\n        }\n      }\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  fragment UserProfilePanel_User on User {\n    id\n    username\n    createdAt\n    updatedAt\n    likedSongs {\n      id\n    }\n  }\n"): (typeof documents)["\n  fragment UserProfilePanel_User on User {\n    id\n    username\n    createdAt\n    updatedAt\n    likedSongs {\n      id\n    }\n  }\n"];
/**
 * The graphql function is used to parse GraphQL queries into a document that can be used by GraphQL clients.
 */
export function graphql(source: "\n  query UserProfileWidget {\n    viewer {\n      id\n      username\n    }\n  }\n"): (typeof documents)["\n  query UserProfileWidget {\n    viewer {\n      id\n      username\n    }\n  }\n"];

export function graphql(source: string) {
  return (documents as any)[source] ?? {};
}

export type DocumentType<TDocumentNode extends DocumentNode<any, any>> = TDocumentNode extends DocumentNode<  infer TType,  any>  ? TType  : never;