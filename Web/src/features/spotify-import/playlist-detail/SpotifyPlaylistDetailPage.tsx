import React from "react";
import { useParams } from "react-router";
import { SpotifyPlaylistPanel } from "./SpotifyPlaylistPanel.tsx";

export const SpotifyPlaylistDetailPage: React.FC = () => {
  const { playlistId } = useParams<{ playlistId: string }>();
  if (!playlistId) return <div className="p-4">Invalid playlist id</div>;
  return <SpotifyPlaylistPanel playlistId={playlistId} />;
};


