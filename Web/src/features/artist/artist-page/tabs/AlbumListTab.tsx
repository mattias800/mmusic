import * as React from "react";
import { ReleaseList } from "@/features/artist/artist-page/ReleaseList.tsx";
import { ReleaseType } from "@/gql/graphql.ts";
import { useParams } from "react-router";

export interface AlbumListTabProps {}

export const AlbumListTab: React.FC<AlbumListTabProps> = () => {
  const { artistId } = useParams<{ artistId: string }>();

  if (!artistId) {
    return null;
  }

  return <ReleaseList artistId={artistId} releaseType={ReleaseType.Album} />;
};
