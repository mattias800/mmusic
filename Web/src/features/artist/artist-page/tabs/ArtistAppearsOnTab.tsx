import * as React from "react";
import { useQuery } from "urql";
import { graphql } from "@/gql";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { AppearsOnReleaseCard } from "@/features/artist/artist-page/AppearsOnReleaseCard.tsx";
import { PlaylistCard } from "@/features/playlists/PlaylistCard.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { useParams } from "react-router";

interface ArtistAppearsOnTabProps {}

const artistAppearsOnQuery = graphql(`
  query ArtistAppearsOnQuery($artistId: ID!) {
    viewer {
      id
    }
    artist {
      byId(artistId: $artistId) {
        id
        name
        appearsOn {
          releases {
            musicBrainzReleaseGroupId
            ...AppearsOnReleaseCard_ArtistAppearsOnRelease
          }
          playlists {
            id
            ...PlaylistCard_Playlist
          }
        }
      }
    }
  }
`);

export const ArtistAppearsOnTab: React.FC<ArtistAppearsOnTabProps> = () => {
  const { artistId } = useParams<{ artistId: string }>();

  const [{ data, fetching, error }] = useQuery({
    query: artistAppearsOnQuery,
    variables: { artistId: artistId ?? "" },
    pause: !artistId,
  });

  if (fetching) {
    return <div>Loading...</div>;
  }

  if (error) {
    return <div>Error: {error.message}</div>;
  }

  if (!data?.artist?.byId) {
    return <div>No data..</div>;
  }

  const artist = data?.artist?.byId;

  return (
    <div className="flex flex-col gap-8">
      <Section>
        <h3 className="text-lg font-semibold">Playlists</h3>
        <CardFlexList>
          {artist?.appearsOn.playlists.map((playlist) => (
            <PlaylistCard key={playlist.id} playlist={playlist} />
          ))}
        </CardFlexList>
      </Section>

      <Section>
        <h3 className="text-lg font-semibold">Other Artists' Releases</h3>
        <CardFlexList>
          {artist.appearsOn.releases.map((release) => (
            <AppearsOnReleaseCard
              key={release.musicBrainzReleaseGroupId}
              release={release}
            />
          ))}
        </CardFlexList>
      </Section>
    </div>
  );
};
