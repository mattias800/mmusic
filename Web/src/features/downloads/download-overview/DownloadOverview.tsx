import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";

export interface DownloadOverviewProps {
  downloadStatuses: Array<
    FragmentType<typeof downloadOverviewDownloadStatusFragment>
  >;
}

export const downloadOverviewDownloadStatusFragment = graphql(`
  fragment DownloadOverview_DownloadStatus on DownloadStatus {
    id
  }
`);

export const DownloadOverview: React.FC<DownloadOverviewProps> = (props) => {
  const downloadStatuses = useFragment(
    downloadOverviewDownloadStatusFragment,
    props.downloadStatuses,
  );

  return <div></div>;
};
