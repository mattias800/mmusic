import * as React from "react";
import { Route, Routes } from "react-router";
import { Home } from "@/app/Home.tsx";
import { AlbumPanel } from "@/features/album/AlbumPanel.tsx";
import { LikedSongs } from "@/app/LikedSongs.tsx";
import { Album } from "@/app/Album.tsx";

export interface AppRouterProps {}

export const AppRouter: React.FC<AppRouterProps> = () => {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/album-panel" element={<AlbumPanel />} />
      <Route path="/album/:releaseId" element={<Album />} />
      <Route path="/liked-songs" element={<LikedSongs />} />
    </Routes>
  );
};
