import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { PageLoading, PageError, PageNoArtists } from "@/components/ui";
import { ArtistList } from "@/features/artist/artist-list/ArtistList.tsx";
import { Users, AlertTriangle } from "lucide-react";

export interface ArtistListPageProps {}

const artistListQuery = graphql(`
  query ArtistListQuery {
    serverLibrary {
      allArtists {
        id
        ...ArtistList_Artist
      }
    }
  }
`);

export const ArtistListPage: React.FC<ArtistListPageProps> = () => {
  const [{ error, data, fetching, stale }] = useQuery({
    query: artistListQuery,
  });

  if (fetching || stale)
    return (
      <PageLoading
        title="Loading Artists"
        subtitle="Fetching your artist collection"
        icon={Users}
        iconBgColor="bg-blue-500/20"
      />
    );

  if (error)
    return (
      <PageError
        title="Failed to Load Artists"
        message="We couldn't load your artist collection"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );

  if (!data?.serverLibrary.allArtists?.length) return <PageNoArtists />;

  return (
    <>
      <title>Artists</title>
      <ArtistList artists={data.serverLibrary.allArtists} />
    </>
  );
};
