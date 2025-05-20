import React, { useState } from 'react';
import { SpotifyUserInputForm } from './SpotifyUserInputForm';
import { UserPlaylistsLoader } from './UserPlaylistsLoader';

type ImportStep = 'USERNAME_INPUT' | 'LOADING_PLAYLISTS' | 'DISPLAY_PLAYLISTS';

export const SpotifyPlaylistImporter: React.FC = () => {
  const [currentStep, setCurrentStep] = useState<ImportStep>('USERNAME_INPUT');
  const [username, setUsername] = useState<string>('');
  // TODO: Add state for the direct playlist ID import flow later
  // const [directPlaylistId, setDirectPlaylistId] = useState<string>('');

  const handleUsernameSubmit = (submittedUsername: string) => {
    setUsername(submittedUsername);
    setCurrentStep('LOADING_PLAYLISTS'); // Or directly to UserPlaylistsLoader which handles its own loading state
  };

  const handlePlaylistsLoaded = () => {
    setCurrentStep('DISPLAY_PLAYLISTS');
  };

  const handleReset = () => {
    setUsername('');
    setCurrentStep('USERNAME_INPUT');
  };

  // The direct import by playlist ID functionality can be a separate section or integrated
  // For now, focusing on the username -> playlists flow

  return (
    <div style={{ padding: '20px', fontFamily: 'Arial, sans-serif' }}>
      <h1>Import Spotify Playlists</h1>
      <button onClick={handleReset} style={{ marginBottom: '20px' }}>Start Over</button>

      {currentStep === 'USERNAME_INPUT' && (
        <SpotifyUserInputForm onUsernameSubmit={handleUsernameSubmit} />
      )}

      {currentStep === 'LOADING_PLAYLISTS' && username && (
        <UserPlaylistsLoader username={username} setCurrentStep={setCurrentStep} />
      )}

    </div>
  );
};
