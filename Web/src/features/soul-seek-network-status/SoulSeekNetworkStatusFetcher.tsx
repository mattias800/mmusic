import { graphql } from "@/gql";
import * as React from "react";
import { useQuery } from "urql";
import { SoulSeekNetworkStatus } from "@/features/soul-seek-network-status/SoulSeekNetworkStatus.tsx";

export interface SoulSeekNetworkStatusFetcherProps {}

const query = graphql(`
  query SoulSeekNetworkStatusQuery {
    external {
      id
      soulSeek {
        id
        status {
          ...SoulSeekNetworkStatus_SoulSeekStatus
        }
      }
    }
  }
`);

export const SoulSeekNetworkStatusFetcher: React.FC<
  SoulSeekNetworkStatusFetcherProps
> = () => {
  const [{ data, fetching, error }] = useQuery({ query: query });

  if (fetching || error || !data) {
    return null;
  }

  return (
    <SoulSeekNetworkStatus soulSeekStatus={data.external.soulSeek.status} />
  );
};
