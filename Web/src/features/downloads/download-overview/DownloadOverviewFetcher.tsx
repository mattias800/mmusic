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

  if (fetching) {
    return <div>Loading...</div>;
  }
  if (error) {
    return <div>Error: {error.message}</div>;
  }
  if (!data) {
    return <div>Error: No data</div>;
  }

  return <DownloadOverview downloadStatuses={data.download.all} />;
};
