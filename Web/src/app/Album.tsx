import { graphql } from "@/gql";
import * as React from "react";
import { useQuery } from "urql";
import { useParams } from "react-router";
import { AlbumPanel } from "@/features/album/AlbumPanel.tsx";

export interface AlbumProps {}

export const albumQuery = graphql(`
  query AlbumQuery($releaseId: ID!) {
    release {
      id
      byId(id: $releaseId) {
        id
        ...AlbumPanel_Release
      }
    }
  }
`);

export const Album: React.FC<AlbumProps> = () => {
  const { releaseId } = useParams<{ releaseId: string }>();
  const [{ error, data, fetching }] = useQuery({
    query: albumQuery,
    variables: { releaseId: releaseId! },
    pause: !releaseId,
  });

  if (fetching) return <div>Loading...</div>;
  if (error) return <div>Error: {error.message}</div>;
  if (!data?.release.byId) return <div>No data</div>;

  return <AlbumPanel release={data.release.byId} />;
};
