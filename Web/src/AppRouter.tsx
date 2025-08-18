import * as React from "react";
import { Route, Routes } from "react-router";
import { IndexPage } from "@/app/IndexPage.tsx";
import { LikedSongsPage } from "@/app/LikedSongsPage.tsx";
import { AlbumPage } from "@/app/AlbumPage.tsx";
import { ArtistPage } from "@/app/ArtistPage.tsx";
import { ImportSpotifyPlaylistPage } from "@/app/ImportSpotifyPlaylistPage.tsx";
import { SpotifyPlaylistDetailPage } from "@/app/SpotifyPlaylistDetailPage.tsx";
import { UserProfilePage } from "@/app/UserProfilePage.tsx";
import { SettingsPage } from "@/app/SettingsPage.tsx";
import { ArtistListPage } from "@/app/ArtistListPage.tsx";
import { AlbumListPage } from "@/app/AlbumListPage.tsx";
import { SearchResultPage } from "@/app/SearchResultPage.tsx";
import { MbArtistPage } from "@/app/MbArtistPage.tsx";
import { PlaylistPage } from "@/app/PlaylistPage.tsx";
import { QueuesPage } from "@/app/QueuesPage.tsx";
import { AdminUsersPage } from "@/app/AdminUsersPage.tsx";
import { TopArtistTracks } from "@/features/artist/artist-page/TopArtistTracks.tsx";
import { SimilarArtistsTab } from "@/features/artist/artist-page/tabs/SimilarArtistsTab.tsx";
import { ArtistAppearsOnTab } from "@/features/artist/artist-page/tabs/ArtistAppearsOnTab.tsx";
import { AlbumListTab } from "@/features/artist/artist-page/tabs/AlbumListTab.tsx";
import { EpListTab } from "@/features/artist/artist-page/tabs/EpListTab.tsx";
import { SingleListTab } from "@/features/artist/artist-page/tabs/SingleListTab.tsx";
import { ArtistStatisticsHeaderTab } from "@/features/artist/artist-page/tabs/ArtistStatisticsHeaderTab.tsx";

export interface AppRouterProps {}

export const AppRouter: React.FC<AppRouterProps> = () => {
  return (
    <Routes>
      <Route path="/" element={<IndexPage />} />
      <Route path="/albums" element={<AlbumListPage />} />
      <Route path="/artists" element={<ArtistListPage />} />
      <Route path="/artist/:artistId" element={<ArtistPage />}>
        <Route path={""} element={<TopArtistTracks />} />
        <Route path={"albums"} element={<AlbumListTab />} />
        <Route path={"eps"} element={<EpListTab />} />
        <Route path={"singles"} element={<SingleListTab />} />
        <Route path={"similar-artists"} element={<SimilarArtistsTab />} />
        <Route path={"appears-on"} element={<ArtistAppearsOnTab />} />
        <Route
          path={"media-availability"}
          element={<ArtistStatisticsHeaderTab />}
        />
      </Route>
      <Route path="/mb-artist/:mbArtistId" element={<MbArtistPage />} />
      <Route
        path="/artist/:artistId/release/:releaseFolderName"
        element={<AlbumPage />}
      />
      <Route path="/liked-songs" element={<LikedSongsPage />} />
      <Route
        path="/playlists/import/spotify"
        element={<ImportSpotifyPlaylistPage />}
      />
      <Route
        path="/playlists/import/spotify/:playlistId"
        element={<SpotifyPlaylistDetailPage />}
      />
      <Route path="/profile" element={<UserProfilePage />} />
      <Route path="/settings" element={<SettingsPage />} />
      <Route path="/search" element={<SearchResultPage />} />
      <Route path="/playlist/:playlistId" element={<PlaylistPage />} />
      <Route path="/queues" element={<QueuesPage />} />
      <Route path="/admin/users" element={<AdminUsersPage />} />
    </Routes>
  );
};
