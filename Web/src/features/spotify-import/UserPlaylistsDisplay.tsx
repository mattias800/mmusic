import React, { useState, useCallback, useEffect } from 'react';
import { useMutation } from 'urql';
import {
  ImportSpotifyPlaylistByIdDocument,
  type SpotifyPlaylist,
  type ImportSpotifyPlaylistByIdMutation,
  type ImportSpotifyPlaylistByIdMutationVariables,
} from '@/gql/graphql';

interface UserPlaylistsDisplayProps {
  playlists: SpotifyPlaylist[];
  username: string; // For context or display
  // Add a callback to go back if needed: onBack: () => void;
}

export const UserPlaylistsDisplay: React.FC<UserPlaylistsDisplayProps> = ({ playlists, username }) => {
  const [selectedPlaylistId, setSelectedPlaylistId] = useState<string | null>(null);
  const [importError, setImportError] = useState<string | null>(null);
  const [importSuccessMessage, setImportSuccessMessage] = useState<string | null>(null);

  const [importPlaylistState, executeImportPlaylist] = useMutation<
    ImportSpotifyPlaylistByIdMutation,
    ImportSpotifyPlaylistByIdMutationVariables
  >(ImportSpotifyPlaylistByIdDocument);

  const handleSelectPlaylist = (playlistId: string) => {
    setSelectedPlaylistId(playlistId);
    setImportError(null);
    setImportSuccessMessage(null);
  };

  const handleImportSelectedPlaylist = useCallback(() => {
    if (!selectedPlaylistId) {
      setImportError('Please select a playlist to import.');
      return;
    }
    setImportError(null);
    setImportSuccessMessage(null);

    const MOCK_USER_ID = 1; // Replace with actual user ID from auth context or prop
    executeImportPlaylist({ playlistId: selectedPlaylistId, userId: MOCK_USER_ID });
  }, [selectedPlaylistId, executeImportPlaylist]);

  useEffect(() => {
    if (importPlaylistState.data?.importSpotifyPlaylistById) {
      const importedPlaylist = playlists.find(p => p.id === selectedPlaylistId);
      setImportSuccessMessage(
        `Playlist "${importedPlaylist?.name || selectedPlaylistId}" imported successfully!`
      );
      setSelectedPlaylistId(null); // Clear selection after successful import
    } else if (importPlaylistState.data?.importSpotifyPlaylistById === false) {
      setImportError("Failed to import playlist. The operation returned false.");
    }
  }, [importPlaylistState.data, playlists, selectedPlaylistId]);

  useEffect(() => {
    if (importPlaylistState.error) {
      setImportError(importPlaylistState.error.message || "Failed to import playlist");
    }
  }, [importPlaylistState.error]);

  return (
    <div style={{ marginTop: '20px' }}>
      <h3>Playlists for {username}:</h3>
      {importError && <p style={{ color: 'red' }}>Error: {importError}</p>}
      {importSuccessMessage && <p style={{ color: 'green' }}>{importSuccessMessage}</p>}
      
      <ul style={{ listStyleType: 'none', padding: 0 }}>
        {playlists.map((playlist) => (
          <li
            key={playlist.id}
            style={{
              marginBottom: '10px',
              padding: '15px',
              border: selectedPlaylistId === playlist.id ? '2px solid #007bff' : '1px solid #ccc',
              borderRadius: '4px',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
            }}
          >
            <div>
              {playlist.coverImageUrl && 
                <img 
                  src={playlist.coverImageUrl} 
                  alt={playlist.name} 
                  style={{ width: '50px', height: '50px', marginRight: '10px', borderRadius: '4px' }} 
                />
              }
              <strong>{playlist.name}</strong>
              {playlist.description && <p style={{ fontSize: '0.9em', color: '#555' }}>{playlist.description}</p>}
              {/* Display track count if available in SpotifyPlaylist type */}
            </div>
            <button
              onClick={() => handleSelectPlaylist(playlist.id)}
              disabled={importPlaylistState.fetching || selectedPlaylistId === playlist.id}
              style={{ padding: '8px 12px' }}
            >
              {selectedPlaylistId === playlist.id ? 'Selected' : 'Select'}
            </button>
          </li>
        ))}
      </ul>
      
      {selectedPlaylistId && (
        <button
          onClick={handleImportSelectedPlaylist}
          disabled={importPlaylistState.fetching}
          style={{
            marginTop: '20px',
            padding: '10px 20px',
            fontSize: '16px',
            backgroundColor: '#28a745',
            color: 'white',
            border: 'none',
            borderRadius: '4px',
          }}
        >
          {importPlaylistState.fetching ? 'Importing...' : `Import "${playlists.find(p=>p.id === selectedPlaylistId)?.name}"`}
        </button>
      )}
    </div>
  );
}; 