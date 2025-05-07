import { graphql } from "@/gql";
import * as React from "react";
import { useQuery } from "urql";

export interface DownloadOverviewFetcherProps {}

export const downloadOverviewFetcherQuery = graphql(`
  query DownloadOverviewQuery {
    download {
      id
      all {
        id
        release {
          id
          artists {
            name
          }
        }
      }
    }
  }
`);

export const downloadOverviewFetcherSubscription = graphql(`
  subscription DownloadOverviewSubscription {
    downloadStatusUpdated {
      id
      release {
        id
        artists {
          name
        }
      }
    }
  }
`);

export const DownloadOverviewFetcher: React.FC<
  DownloadOverviewFetcherProps
> = () => {
  const [{ data, error, fetching }] = useQuery({
    query: downloadOverviewFetcherQuery,
  });

  if (fetching) {
    return <div>Loading...</div>;
  }
  if (error) {
    return <div>Error: {error.message}</div>;
  }

  return <div></div>;
};
