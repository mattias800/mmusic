import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";
import { AlbumList } from "@/features/album/album-list/AlbumList.tsx";

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

  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;
  if (!data?.serverLibrary.allReleases) return <div>No data</div>;

  return (
    <>
      <title>Albums</title>
      <AlbumList releaseGroups={data.serverLibrary.allReleases} />
    </>
  );
};
