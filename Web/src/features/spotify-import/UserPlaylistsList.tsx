import React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";

const importSpotifyPlaylistByIdMutation = graphql(`
  mutation ImportSpotifyPlaylistById($playlistId: String!, $userId: UUID!) {
    importSpotifyPlaylistById(playlistId: $playlistId, userId: $userId) {
      __typename
      ... on ImportSpotifyPlaylistSuccess {
        success
      }
      ... on ImportSpotifyPlaylistError {
        message
      }
    }
  }
`);

export const userPlaylistsListSpotifyPlaylistFragment = graphql(`
  fragment UserPlaylistsList_SpotifyPlaylist on SpotifyPlaylist {
    id
    description
    name
    coverImageUrl
  }
`);

interface UserPlaylistsListProps {
  playlists: Array<
    FragmentType<typeof userPlaylistsListSpotifyPlaylistFragment>
  >;
  username: string;
}

export const UserPlaylistsList: React.FC<UserPlaylistsListProps> = ({
  username,
  ...props
}) => {
  const playlists = useFragment(
    userPlaylistsListSpotifyPlaylistFragment,
    props.playlists,
  );

  const [{ fetching }] = useMutation(importSpotifyPlaylistByIdMutation);

  return (
    <div style={{ marginTop: "20px" }}>
      <h3>Playlists for {username}:</h3>

      {fetching && <p>Importing playlists...</p>}

      <ul style={{ listStyleType: "none", padding: 0 }}>
        {playlists.map((playlist) => (
          <li
            key={playlist.id}
            style={{
              marginBottom: "10px",
              padding: "15px",
              border: "1px solid #ccc",
              borderRadius: "4px",
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
            }}
          >
            <div>
              {playlist.coverImageUrl && (
                <img
                  src={playlist.coverImageUrl}
                  alt={playlist.name}
                  style={{
                    width: "50px",
                    height: "50px",
                    marginRight: "10px",
                    borderRadius: "4px",
                  }}
                />
              )}
              <strong>{playlist.name}</strong>
              {playlist.description && (
                <p style={{ fontSize: "0.9em", color: "#555" }}>
                  {playlist.description}
                </p>
              )}
            </div>
            <button style={{ padding: "8px 12px" }}>Select</button>
          </li>
        ))}
      </ul>
    </div>
  );
};
