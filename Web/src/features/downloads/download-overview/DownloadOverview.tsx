import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";

export interface DownloadOverviewProps {
  downloadStatuses: Array<
    FragmentType<typeof downloadOverviewDownloadStatusFragment>
  >;
}

const downloadOverviewDownloadStatusFragment = graphql(`
  fragment DownloadOverview_DownloadStatus on Query {
    areThereAnyUsers
  }
`);

export const DownloadOverview: React.FC<DownloadOverviewProps> = (props) => {
  const downloadStatuses = useFragment(
    downloadOverviewDownloadStatusFragment,
    props.downloadStatuses,
  );

  /*
  {downloadStatuses.map((downloadStatus) => (
        <DownloadStatus
          key={downloadStatus.id}
          downloadStatus={downloadStatus}
        />
      ))}
   */

  return <div className={"flex flex-col gap-12"}></div>;
};
