import * as React from "react";

export interface DownloadOverviewProps {}

// Minimal placeholder component; real fragment wiring comes later

export const DownloadOverview: React.FC<DownloadOverviewProps> = () => {
  // const downloadStatuses = useFragment(
  //   downloadOverviewDownloadStatusFragment,
  //   props.downloadStatuses,
  // );

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
