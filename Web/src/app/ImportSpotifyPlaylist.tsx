import * as React from "react";
import { SpotifyPlaylistImporter } from "@/features/spotify-import/SpotifyPlaylistImporter.tsx";

export interface ImportSpotifyPlaylistProps {}

export const ImportSpotifyPlaylist: React.FC<
  ImportSpotifyPlaylistProps
> = () => {
  return <SpotifyPlaylistImporter />;
};
