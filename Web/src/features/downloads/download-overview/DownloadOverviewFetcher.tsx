import { graphql } from "@/gql";
import * as React from "react";
import { useQuery, useSubscription } from "urql";

export interface DownloadOverviewFetcherProps {}

const downloadOverviewFetcherQuery = graphql(`
  query DownloadOverviewQuery {
    areThereAnyUsers
  }
`);

const downloadOverviewFetcherSubscription = graphql(`
  subscription DownloadOverviewSubscription {
    ping {
      id
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

  return null;
  // return <DownloadOverview downloadStatuses={data.download.all} />;
};
