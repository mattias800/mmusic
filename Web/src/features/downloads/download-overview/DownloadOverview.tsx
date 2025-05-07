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
    <div className={"flex flex-col"}>
      {downloadStatuses.length === 0 && <span>No downloads in progress</span>}
      {downloadStatuses.length > 0 && (
        <span className={"mb-2"}>
          Downloads in progress: {downloadStatuses.length}
        </span>
      )}
      {downloadStatuses.map((downloadStatus) => (
        <DownloadStatus downloadStatus={downloadStatus} />
      ))}
    </div>
  );
};
