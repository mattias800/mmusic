import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";
import { AlbumList } from "@/features/album/album-list/AlbumList.tsx";

export interface AlbumListPageProps {}

export const albumListQuery = graphql(`
  query AlbumListQuery {
    releaseGroup {
      all {
        id
        ...AlbumList_ReleaseGroup
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
  if (!data?.releaseGroup.all) return <div>No data</div>;

  return (
    <>
      <title>Albums</title>
      <AlbumList releaseGroups={data.releaseGroup.all} />
    </>
  );
};
