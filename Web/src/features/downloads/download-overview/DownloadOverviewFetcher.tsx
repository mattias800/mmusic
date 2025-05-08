import { graphql } from "@/gql";
import * as React from "react";
import { useQuery, useSubscription } from "urql";
import { DownloadOverview } from "@/features/downloads/download-overview/DownloadOverview.tsx";

export interface DownloadOverviewFetcherProps {}

export const downloadOverviewFetcherQuery = graphql(`
  query DownloadOverviewQuery {
    download {
      id
      all {
        id
        ...DownloadOverview_DownloadStatus
      }
    }
  }
`);

export const downloadOverviewFetcherSubscription = graphql(`
  subscription DownloadOverviewSubscription {
    downloadStatusUpdated {
      id
      ...DownloadOverview_DownloadStatus
    }
  }
`);

export const DownloadOverviewFetcher: React.FC<
  DownloadOverviewFetcherProps
> = () => {
  const [{ data, error, fetching }] = useQuery({
    query: downloadOverviewFetcherQuery,
  });

  useSubscription({ query: downloadOverviewFetcherSubscription });

  if (fetching || error || !data) {
    return null;
  }

  return <DownloadOverview downloadStatuses={data.download.all} />;
};
