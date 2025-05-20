import React from "react";
import { useQuery } from "urql";
import { UserPlaylistsList } from "./UserPlaylistsList.tsx";
import { graphql } from "@/gql";

interface UserPlaylistsFetcherProps {
  spotifyUsername: string;
}

export const userPlaylistsFetcherQuery = graphql(`
  query UserPlaylistsLoader_Query($spotifyUsername: String!) {
    playlist {
      importPlaylists {
        spotify {
          spotifyPlaylistsForUser(username: $spotifyUsername) {
            id
            ...UserPlaylistsList_SpotifyPlaylist
          }
        }
      }
    }
  }
`);

export const UserPlaylistsFetcher: React.FC<UserPlaylistsFetcherProps> = ({
  spotifyUsername,
}) => {
  const [fetchUserPlaylistsState, executeFetchUserPlaylists] = useQuery({
    query: userPlaylistsFetcherQuery,
    variables: { spotifyUsername },
    requestPolicy: "network-only",
  });

  if (fetchUserPlaylistsState.fetching) {
    return <p>Loading playlists for {spotifyUsername}...</p>;
  }

  if (fetchUserPlaylistsState.error) {
    return (
      <div>
        <p style={{ color: "red" }}>
          Error fetching playlists: {fetchUserPlaylistsState.error.message}
        </p>
        <button
          onClick={() =>
            executeFetchUserPlaylists({ requestPolicy: "network-only" })
          }
        >
          Retry
        </button>
      </div>
    );
  }

  const playlists =
    fetchUserPlaylistsState.data?.playlist?.importPlaylists?.spotify
      ?.spotifyPlaylistsForUser;

  if (playlists) {
    if (playlists.length === 0) {
      return (
        <div>
          <p>No playlists found for user: {spotifyUsername}.</p>
        </div>
      );
    }
    return (
      <UserPlaylistsList playlists={playlists} username={spotifyUsername} />
    );
  }

  return <p>Preparing to load playlists...</p>;
};
