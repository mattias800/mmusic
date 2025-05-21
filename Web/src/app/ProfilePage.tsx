import * as React from "react";
import { gql, useQuery } from "@urql/next"; // Assuming @urql/next based on task description
import { Center, Loader, Text, Title, List, ThemeIcon, Stack } from "@mantine/core";
import { IconCircleCheck, IconCircleDashed } from "@tabler/icons-react";

// Define the GraphQL query
const GET_VIEWER_PROFILE = gql`
  query GetViewerProfile {
    viewer {
      id
      username
      createdAt
      updatedAt
      likedSongs {
        id
        recording { # Assuming 'recording' resolves to an object with a 'title'
          id
          title
        }
      }
    }
  }
`;

const ProfilePage: React.FC = () => {
  const [result] = useQuery({ query: GET_VIEWER_PROFILE });
  const { data, fetching, error } = result;

  if (fetching) {
    return (
      <Center style={{ height: "100%" }}>
        <Loader />
      </Center>
    );
  }

  if (error) {
    return (
      <Center style={{ height: "100%" }}>
        <Text color="red">Error fetching profile: {error.message}</Text>
      </Center>
    );
  }

  if (!data?.viewer) {
    return (
      <Center style={{ height: "100%" }}>
        <Text>No profile data found.</Text>
      </Center>
    );
  }

  const { username, createdAt, updatedAt, likedSongs } = data.viewer;

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString(undefined, {
      year: "numeric",
      month: "long",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  return (
    <Stack p="md">
      <Title order={2}>Profile</Title>
      <Text>
        <strong>Username:</strong> {username}
      </Text>
      <Text>
        <strong>Joined:</strong> {formatDate(createdAt)}
      </Text>
      <Text>
        <strong>Last Updated:</strong> {formatDate(updatedAt)}
      </Text>

      <Title order={3} mt="lg">Liked Songs</Title>
      {likedSongs && likedSongs.length > 0 ? (
        <List
          spacing="xs"
          size="sm"
          center
          icon={
            <ThemeIcon color="teal" size={24} radius="xl">
              <IconCircleCheck size="1rem" />
            </ThemeIcon>
          }
        >
          {likedSongs.map((song: any) => (
            <List.Item key={song.id}>
              {song.recording?.title || `Song ID: ${song.id}`}
            </List.Item>
          ))}
        </List>
      ) : (
        <Text>No liked songs yet.</Text>
      )}
    </Stack>
  );
};

export default ProfilePage;
