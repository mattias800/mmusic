import React from 'react';
import { useQuery } from 'urql';
import {
  GetSpotifyPlaylistsForUserDocument,
  type GetSpotifyPlaylistsForUserQuery,
  type GetSpotifyPlaylistsForUserQueryVariables,
  type SpotifyPlaylist,
} from '@/gql/graphql';
import { UserPlaylistsDisplay } from './UserPlaylistsDisplay';

interface UserPlaylistsLoaderProps {
  username: string;
  setCurrentStep: (step: 'USERNAME_INPUT' | 'LOADING_PLAYLISTS' | 'DISPLAY_PLAYLISTS') => void;
}

export const UserPlaylistsLoader: React.FC<UserPlaylistsLoaderProps> = ({ username, setCurrentStep }) => {
  const [fetchUserPlaylistsState, executeFetchUserPlaylists] = useQuery<
    GetSpotifyPlaylistsForUserQuery,
    GetSpotifyPlaylistsForUserQueryVariables
  >({
    query: GetSpotifyPlaylistsForUserDocument,
    variables: { username },
    requestPolicy: 'network-only',
  });

  if (fetchUserPlaylistsState.fetching) {
    return <p>Loading playlists for {username}...</p>;
  }

  if (fetchUserPlaylistsState.error) {
    return (
      <div>
        <p style={{ color: 'red' }}>
          Error fetching playlists: {fetchUserPlaylistsState.error.message}
        </p>
        <button onClick={() => setCurrentStep('USERNAME_INPUT')}>
          Try a different username
        </button>
        <button onClick={() => executeFetchUserPlaylists({ requestPolicy: 'network-only' })}>
          Retry
        </button>
      </div>
    );
  }

  const playlistsData = fetchUserPlaylistsState.data?.playlist?.importPlaylists?.spotify?.spotifyPlaylistsForUser;

  if (playlistsData) {
    if (playlistsData.length === 0) {
      return (
        <div>
          <p>No playlists found for user: {username}.</p>
          <button onClick={() => setCurrentStep('USERNAME_INPUT')}>
            Try another username
          </button>
        </div>
      );
    }
    return <UserPlaylistsDisplay playlists={playlistsData as SpotifyPlaylist[]} username={username} />;
  }

  return <p>Preparing to load playlists...</p>; 
}; 