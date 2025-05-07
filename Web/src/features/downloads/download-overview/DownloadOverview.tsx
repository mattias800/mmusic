import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { DownloadStatus } from "@/features/downloads/download-overview/DownloadStatus.tsx";

export interface DownloadOverviewProps {
  downloadStatuses: Array<
    FragmentType<typeof downloadOverviewDownloadStatusFragment>
  >;
}

export const downloadOverviewDownloadStatusFragment = graphql(`
  fragment DownloadOverview_DownloadStatus on DownloadStatus {
    id
    ...DownloadStatus_DownloadStatus
  }
`);

export const DownloadOverview: React.FC<DownloadOverviewProps> = (props) => {
  const downloadStatuses = useFragment(
    downloadOverviewDownloadStatusFragment,
    props.downloadStatuses,
  );

  return (
    <div className={"flex flex-col gap-12"}>
      {downloadStatuses.map((downloadStatus) => (
        <DownloadStatus
          key={downloadStatus.id}
          downloadStatus={downloadStatus}
        />
      ))}
    </div>
  );
};
