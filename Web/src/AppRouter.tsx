import * as React from "react";
import { Route, Routes } from "react-router";
import SetupPage from "@/app/Setup.tsx"; // Import the SetupPage component
import ProfilePage from "@/app/ProfilePage.tsx"; // Import the actual ProfilePage component
import { IndexPage } from "@/app/IndexPage.tsx";
import { LikedSongsPage } from "@/app/LikedSongsPage.tsx";
import { AlbumPage } from "@/app/AlbumPage.tsx";
import { ArtistPage } from "@/app/ArtistPage.tsx";
import { ImportSpotifyPlaylistPage } from "@/app/ImportSpotifyPlaylistPage.tsx";

export interface AppRouterProps {}

export const AppRouter: React.FC<AppRouterProps> = () => {
  return (
    <Routes>
      <Route path="/" element={<IndexPage />} />
      <Route path="/album/:releaseGroupId" element={<AlbumPage />} />
      <Route path="/artist/:artistId" element={<ArtistPage />} />
      <Route path="/liked-songs" element={<LikedSongsPage />} />
      <Route
        path="/playlists/import/spotify"
        element={<ImportSpotifyPlaylistPage />}
      />
      <Route path="/setup" element={<SetupPage />} /> {/* Add the /setup route */}
      <Route path="/profile" element={<ProfilePage />} />
    </Routes>
  );
};
