import React from "react";
import { useQuery } from "urql";
import { SpotifyPlaylistsList } from "./SpotifyPlaylistsList.tsx";
import { graphql } from "@/gql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { MessageBox } from "@/components/errors/MessageBox.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { Button } from "@/components/ui/button.tsx";

interface UserPlaylistsFetcherProps {
  spotifyUsername: string;
}

const userPlaylistsFetcherQuery = graphql(`
  query UserPlaylistsLoader_Query($spotifyUsername: String!) {
    playlist {
      importPlaylists {
        spotify {
          spotifyPlaylistsForUser(username: $spotifyUsername) {
            id
            ...SpotifyPlaylistsList_SpotifyPlaylist
          }
        }
      }
    }
  }
`);

export const UserPlaylistsFetcher: React.FC<UserPlaylistsFetcherProps> = ({
  spotifyUsername,
}) => {
  const [{ error, data, fetching }, executeFetchUserPlaylists] = useQuery({
    query: userPlaylistsFetcherQuery,
    variables: { spotifyUsername },
    requestPolicy: "network-only",
  });

  if (fetching) {
    return (
      <SpinnerSpacing>
        <Spinner />
      </SpinnerSpacing>
    );
  }

  if (error) {
    return (
      <MessageBox message={error.message}>
        <Button
          variant={"secondary"}
          onClick={() =>
            executeFetchUserPlaylists({ requestPolicy: "network-only" })
          }
        >
          Retry
        </Button>
      </MessageBox>
    );
  }

  const playlists =
    data?.playlist?.importPlaylists?.spotify?.spotifyPlaylistsForUser;

  if (!playlists) {
    return (
      <MessageBox message={"Something went wrong."}>
        <Button
          variant={"secondary"}
          onClick={() =>
            executeFetchUserPlaylists({ requestPolicy: "network-only" })
          }
        >
          Retry
        </Button>{" "}
      </MessageBox>
    );
  }

  if (playlists.length === 0) {
    return (
      <MessageBox
        message={`No playlists found for user: ${spotifyUsername}.`}
      />
    );
  }

  return (
    <Section>
      <SectionHeading>{spotifyUsername}'s playlists</SectionHeading>
      <SpotifyPlaylistsList
        playlists={playlists}
        spotifyUsername={spotifyUsername}
      />
    </Section>
  );
};
