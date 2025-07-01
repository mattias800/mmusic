import { FragmentType, graphql, useFragment } from "@/gql";
import { SoulSeekStatusType } from "@/gql/graphql";
import * as React from "react";
import { useSubscription } from "urql";

export interface SoulSeekNetworkStatusProps {
  soulSeekStatus: FragmentType<
    typeof soulSeekNetworkStatusSoulSeekStatusFragment
  >;
}

const soulSeekNetworkStatusSoulSeekStatusFragment = graphql(`
  fragment SoulSeekNetworkStatus_SoulSeekStatus on SoulSeekStatus {
    id
    status
  }
`);

const subscription = graphql(`
  subscription SoulSeekNetworkStatusSubscription {
    soulSeekStatusUpdated {
      ...SoulSeekNetworkStatus_SoulSeekStatus
    }
  }
`);

export const SoulSeekNetworkStatus: React.FC<SoulSeekNetworkStatusProps> = (
  props,
) => {
  const soulSeekStatus = useFragment(
    soulSeekNetworkStatusSoulSeekStatusFragment,
    props.soulSeekStatus,
  );

  useSubscription({ query: subscription });
  return (
    <div className="flex items-center space-x-2 text-sm text-white">
      <span
        className={`w-3 h-3 rounded-full ${getIndicatorColor(soulSeekStatus.status)}`}
        aria-label={getText(soulSeekStatus.status)}
      />
      <span className="text-zinc-300">{getText(soulSeekStatus.status)}</span>
    </div>
  );
};

const getText = (status: SoulSeekStatusType): string => {
  switch (status) {
    case "OFFLINE":
      return "Offline";
    case "CONNECTING":
      return "Connecting...";
    case "ONLINE":
      return "Online";
    default:
      return "";
  }
};

const getIndicatorColor = (status: SoulSeekStatusType): string => {
  switch (status) {
    case "OFFLINE":
      return "bg-red-500";
    case "CONNECTING":
      return "bg-yellow-500";
    case "ONLINE":
      return "bg-green-500";
    default:
      return "";
  }
};
