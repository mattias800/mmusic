import { FragmentType, graphql } from "@/gql";
import * as React from "react";

export interface DownloadStatusProps {
  downloadStatus: FragmentType<typeof downloadStatusDownloadStatusFragment>;
}

export const downloadStatusDownloadStatusFragment = graphql(`
  fragment DownloadStatus_DownloadStatus on DownloadStatus {
    id
    release {
      id
      title
      year
      artists {
        id
        name
      }
      recordings {
        id
      }
    }
  }
`);

export const DownloadStatus: React.FC<DownloadStatusProps> = () => {
  return <div></div>;
};
