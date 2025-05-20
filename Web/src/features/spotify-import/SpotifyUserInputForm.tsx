import React, { useState } from 'react';

interface SpotifyUserInputFormProps {
  onUsernameSubmit: (username: string) => void;
  // Potentially pass down a disabled flag if another operation is in progress globally
}

export const SpotifyUserInputForm: React.FC<SpotifyUserInputFormProps> = ({ onUsernameSubmit }) => {
  const [usernameInput, setUsernameInput] = useState('');

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!usernameInput.trim()) {
      // Basic validation, can be enhanced
      alert('Please enter a Spotify username or ID.');
      return;
    }
    onUsernameSubmit(usernameInput);
  };

  return (
    <form onSubmit={handleSubmit} style={{ marginBottom: '20px' }}>
      <h2>Get User's Playlists</h2>
      <input
        type="text"
        value={usernameInput}
        onChange={(e) => setUsernameInput(e.target.value)}
        placeholder="Spotify Username or ID"
        style={{ marginRight: '10px', padding: '8px' }}
      />
      <button type="submit" style={{ padding: '8px 15px' }}>
        Get Playlists
      </button>
    </form>
  );
}; 