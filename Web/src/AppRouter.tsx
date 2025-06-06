import * as React from "react";
import { Route, Routes } from "react-router";
import { IndexPage } from "@/app/IndexPage.tsx";
import { LikedSongsPage } from "@/app/LikedSongsPage.tsx";
import { AlbumPage } from "@/app/AlbumPage.tsx";
import { ArtistPage } from "@/app/ArtistPage.tsx";
import { ImportSpotifyPlaylistPage } from "@/app/ImportSpotifyPlaylistPage.tsx";
import { UserProfilePage } from "@/app/UserProfilePage.tsx";
import { SettingsPage } from "@/app/SettingsPage.tsx";
import { ArtistListPage } from "@/app/ArtistListPage.tsx";
import { AlbumListPage } from "@/app/AlbumListPage.tsx";
import { SearchResultPage } from "@/app/SearchResultPage.tsx";

export interface AppRouterProps {}

export const AppRouter: React.FC<AppRouterProps> = () => {
  return (
    <Routes>
      <Route path="/" element={<IndexPage />} />
      <Route path="/albums" element={<AlbumListPage />} />
      <Route path="/album/:releaseGroupId" element={<AlbumPage />} />
      <Route path="/artists" element={<ArtistListPage />} />
      <Route path="/artist/:artistId" element={<ArtistPage />} />
      <Route path="/liked-songs" element={<LikedSongsPage />} />
      <Route
        path="/playlists/import/spotify"
        element={<ImportSpotifyPlaylistPage />}
      />
      <Route path="/profile" element={<UserProfilePage />} />
      <Route path="/settings" element={<SettingsPage />} />
      <Route path="/search" element={<SearchResultPage />} />
    </Routes>
  );
};
