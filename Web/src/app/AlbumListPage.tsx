import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { PageLoading, PageError, PageNoAlbums } from "@/components/ui";
import { AlbumList } from "@/features/album/album-list/AlbumList.tsx";
import { Music, AlertTriangle } from "lucide-react";

export interface AlbumListPageProps {}

const albumListQuery = graphql(`
  query AlbumListQuery {
    serverLibrary {
      allReleases {
        id
        ...AlbumList_Release
      }
    }
  }
`);

export const AlbumListPage: React.FC<AlbumListPageProps> = () => {
  const [{ error, data, fetching, stale }] = useQuery({
    query: albumListQuery,
  });

  if (fetching || stale)
    return (
      <PageLoading
        title="Loading Albums"
        subtitle="Fetching your music collection"
        icon={Music}
        iconBgColor="bg-purple-500/20"
      />
    );

  if (error)
    return (
      <PageError
        title="Failed to Load Albums"
        message="We couldn't load your album collection"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );

  if (!data?.serverLibrary.allReleases?.length) return <PageNoAlbums />;

  return (
    <>
      <title>Albums</title>
      <AlbumList releaseGroups={data.serverLibrary.allReleases} />
    </>
  );
};
