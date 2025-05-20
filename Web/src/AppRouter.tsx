import * as React from "react";
import { Route, Routes } from "react-router";
import { Home } from "@/app/Home.tsx";
import { LikedSongs } from "@/app/LikedSongs.tsx";
import { Album } from "@/app/Album.tsx";
import { Artist } from "@/app/Artist.tsx";
import { SpotifyImportPage } from "@/features/spotify-import/SpotifyImportPage.tsx";

export interface AppRouterProps {}

export const AppRouter: React.FC<AppRouterProps> = () => {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/album/:releaseGroupId" element={<Album />} />
      <Route path="/artist/:artistId" element={<Artist />} />
      <Route path="/liked-songs" element={<LikedSongs />} />
      <Route path="/import/spotify" element={<SpotifyImportPage />} />
    </Routes>
  );
};
