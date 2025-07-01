import { LikedSong } from "@/gql/graphql";
import { FragmentType, graphql, useFragment } from "@/gql";
import React from "react";
import { SignOutButton } from "@/features/auth/components/SignOutButton.tsx";

export interface UserProfilePanelProps {
  user: FragmentType<typeof userProfilePanelUserFragment>;
}

const userProfilePanelUserFragment = graphql(`
  fragment UserProfilePanel_User on User {
    id
    username
    createdAt
    updatedAt
    likedSongs {
      id
    }
  }
`);

export const UserProfilePanel: React.FC<UserProfilePanelProps> = (props) => {
  const user = useFragment(userProfilePanelUserFragment, props.user);

  const { username, createdAt, updatedAt, likedSongs } = user;

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-2xl font-bold mb-4">My Profile</h1>
      <div className="bg-card text-card-foreground p-6 rounded-lg shadow">
        <SignOutButton />
        <p className="mb-2">
          <strong>Username:</strong> {username}
        </p>
        <p className="mb-2">
          <strong>Joined:</strong> {new Date(createdAt).toLocaleDateString()}
        </p>
        <p className="mb-4">
          <strong>Last Updated:</strong>{" "}
          {new Date(updatedAt).toLocaleDateString()}
        </p>

        <h2 className="text-xl font-semibold mt-6 mb-3 border-t pt-4">
          Liked Songs ({likedSongs.length})
        </h2>
        {likedSongs.length > 0 ? (
          <ul className="list-disc pl-5 space-y-1">
            {likedSongs.map((song: Pick<LikedSong, "id">) => (
              <li key={song.id}>{song.id}</li>
            ))}
          </ul>
        ) : (
          <p>You haven't liked any songs yet.</p>
        )}
      </div>
    </div>
  );
};
