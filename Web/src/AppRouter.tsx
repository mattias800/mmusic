import * as React from "react";
import { Route, Routes } from "react-router";
import { Home } from "@/app/Home.tsx";
import { AlbumPanel } from "@/features/album/AlbumPanel.tsx";

export interface AppRouterProps {}

export const AppRouter: React.FC<AppRouterProps> = () => {
  return (
    <Routes>
      <Route path="/" element={<Home />} />
      <Route path="/album" element={<AlbumPanel />} />
    </Routes>
  );
};
