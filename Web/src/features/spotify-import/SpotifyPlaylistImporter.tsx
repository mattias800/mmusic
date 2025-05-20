import React, { useState } from "react";
import { SpotifyUserInputForm } from "./SpotifyUserInputForm";
import { UserPlaylistsFetcher } from "./UserPlaylistsFetcher.tsx";

export const SpotifyPlaylistImporter: React.FC = () => {
  const [spotifyUsername, setSpotifyUsername] = useState<string>("");

  return (
    <div style={{ padding: "20px", fontFamily: "Arial, sans-serif" }}>
      <h1>Import Spotify Playlists</h1>

      <SpotifyUserInputForm onUsernameSubmit={setSpotifyUsername} />

      {spotifyUsername && (
        <UserPlaylistsFetcher spotifyUsername={spotifyUsername} />
      )}
    </div>
  );
};
