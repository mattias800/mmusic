import * as React from "react";
import { ReleaseList } from "@/features/artist/artist-page/ReleaseList.tsx";
import { ReleaseType } from "@/gql/graphql.ts";
import { useParams } from "react-router";

export interface SingleListTabProps {}

export const SingleListTab: React.FC<SingleListTabProps> = () => {
  const { artistId } = useParams<{ artistId: string }>();

  if (!artistId) {
    return null;
  }

  return <ReleaseList artistId={artistId} releaseType={ReleaseType.Single} />;
};
