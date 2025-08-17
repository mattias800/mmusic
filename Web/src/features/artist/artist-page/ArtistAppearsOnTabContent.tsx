import * as React from "react";
import { useQuery } from "urql";
import { graphql } from "@/gql";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { AppearsOnReleaseCard } from "@/features/artist/artist-page/AppearsOnReleaseCard.tsx";
import { PlaylistCard } from "@/features/playlists/PlaylistCard.tsx";
import { Section } from "@/components/page-body/Section.tsx";

interface ArtistAppearsOnTabContentProps {
  artistId: string;
  appearsOn: Array<{
    coverArtUrl?: string | null;
    firstReleaseDate?: string | null;
    firstReleaseYear?: string | null;
    musicBrainzReleaseGroupId?: string | null;
    primaryArtistMusicBrainzId?: string | null;
    primaryArtistName: string;
    releaseTitle: string;
    releaseType: string;
    role?: string | null;
  }>;
}

const artistPlaylistsQuery = graphql(`
  query ArtistAppearsOn_Playlists($artistId: ID!) {
    viewer {
      id
    }
    playlist {
      searchPlaylists(limit: 50, searchTerm: "") {
        id
        name
        coverImageUrl
        createdAt
        items {
          id
          artist {
            id
          }
        }
      }
    }
    serverLibrary {
      artistById(id: $artistId) { id }
    }
  }
`);

export const ArtistAppearsOnTabContent: React.FC<ArtistAppearsOnTabContentProps> = ({
  artistId,
  appearsOn,
}) => {
  const [{ data }] = useQuery({
    query: artistPlaylistsQuery,
    variables: { artistId },
  });

  const playlists = React.useMemo(() => {
    const list = data?.playlist?.searchPlaylists ?? [];
    return list.filter((pl) => (pl.items ?? []).some((it) => it?.artist?.id === artistId));
  }, [data, artistId]);

  return (
    <div className="flex flex-col gap-8">
      <Section>
        <h3 className="text-lg font-semibold">Playlists</h3>
        <CardFlexList>
          {playlists.map((pl) => (
            <PlaylistCard key={pl.id} playlist={pl} />
          ))}
        </CardFlexList>
      </Section>

      <Section>
        <h3 className="text-lg font-semibold">Other Artists' Releases</h3>
        <CardFlexList>
          {appearsOn.map((a, idx) => (
            <AppearsOnReleaseCard
              key={`${a.musicBrainzReleaseGroupId ?? a.releaseTitle}-${idx}`}
              item={a}
            />
          ))}
        </CardFlexList>
      </Section>
    </div>
  );
};


