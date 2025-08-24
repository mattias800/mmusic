import * as React from "react";
import { graphql } from "@/gql";
import { useMutation, useQuery } from "urql";
import { GradientButton, StatusCard, StatusGrid } from "@/components/ui";
import {
  AlertTriangle,
  CheckCircle2,
  Globe,
  Network,
  RefreshCw,
} from "lucide-react";

const reachabilityQuery = graphql(`
  query SoulSeekReachabilityCard_Query {
    soulSeekSharingStatistics {
      isSharingEnabled
      sharedFileCount
      libraryPath
      listeningPort
      totalLibrarySize
      observedIp
      observedPort
      observedAtUtc
      isPrivateIp
      portMatches
    }
  }
`);

const reachabilityMutation = graphql(`
  mutation SoulSeekReachabilityCard_Check {
    checkSoulSeekReachability {
      __typename
      ... on CheckSoulSeekReachabilitySuccess {
        statistics {
          isSharingEnabled
          sharedFileCount
          libraryPath
          listeningPort
          totalLibrarySize
          observedIp
          observedPort
          observedAtUtc
          isPrivateIp
          portMatches
        }
      }
      ... on CheckSoulSeekReachabilityError {
        message
      }
    }
  }
`);

export const SoulSeekReachabilityCard: React.FC = () => {
  const [{ data, error }, reexec] = useQuery({
    query: reachabilityQuery,
    requestPolicy: "cache-and-network",
  });
  const [{ fetching: checking }, runCheck] = useMutation(reachabilityMutation);

  const stats = data?.soulSeekSharingStatistics;

  const onCheck = async () => {
    await runCheck({});
    reexec({ requestPolicy: "network-only" });
  };

  const observedAtText = stats?.observedAtUtc
    ? new Date(stats.observedAtUtc).toLocaleString()
    : "—";
  const observedEndpoint =
    stats?.observedIp && stats?.observedPort
      ? `${stats.observedIp}:${stats.observedPort}`
      : "—";

  const showWarning = !!stats && (stats.isPrivateIp || !stats.portMatches);

  return (
    <div className="space-y-4">
      {/* Header and Action */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <div className="p-2 bg-blue-500/20 rounded-lg">
            <Network className="w-5 h-5 text-blue-400" />
          </div>
          <div>
            <div className="text-white font-semibold">
              SoulSeek Reachability
            </div>
            <div className="text-gray-400 text-sm">
              Check if peers can likely reach your share
            </div>
          </div>
        </div>
        <GradientButton
          onClick={onCheck}
          disabled={checking}
          className="min-w-40"
          variant="primary"
        >
          {checking ? (
            <span className="inline-flex items-center gap-2">
              <RefreshCw className="w-4 h-4 animate-spin" /> Checking...
            </span>
          ) : (
            <span className="inline-flex items-center gap-2">
              <RefreshCw className="w-4 h-4" /> Check Reachability
            </span>
          )}
        </GradientButton>
      </div>

      {/* Error state */}
      {error && (
        <div className="p-3 rounded-lg bg-red-500/10 border border-red-500/20 text-sm text-red-200">
          Failed to load reachability: {error.message}
        </div>
      )}

      {/* Sharing disabled */}
      {stats && !stats.isSharingEnabled && (
        <div className="p-3 rounded-lg bg-yellow-500/10 border border-yellow-500/20 text-sm text-yellow-200">
          Sharing appears to be disabled. Enable SoulSeek library sharing in
          Server Settings to make your library browseable.
        </div>
      )}

      {/* Status metrics */}
      <StatusGrid columns={2}>
        <StatusCard
          label="Listening Port"
          value={stats?.listeningPort ?? "—"}
          icon={Globe}
        />
        <StatusCard
          label="Observed Endpoint"
          value={observedEndpoint}
          icon={Globe}
        />
        <StatusCard
          label="Private IP Detected"
          value={stats ? (stats.isPrivateIp ? "Yes" : "No") : "—"}
        />
        <StatusCard
          label="Port Matches"
          value={stats ? (stats.portMatches ? "Yes" : "No") : "—"}
        />
        <StatusCard
          label="Shared Files"
          value={stats?.sharedFileCount ?? "—"}
        />
        <StatusCard label="Last Observed" value={observedAtText} />
      </StatusGrid>

      {/* Guidance */}
      {stats && (
        <div
          className={`p-4 rounded-lg border ${
            showWarning
              ? "bg-orange-500/10 border-orange-500/20 text-orange-100"
              : "bg-green-500/10 border-green-500/20 text-green-100"
          }`}
        >
          <div className="flex items-center gap-2 mb-1">
            {showWarning ? (
              <AlertTriangle className="w-4 h-4" />
            ) : (
              <CheckCircle2 className="w-4 h-4" />
            )}
            <span className="font-semibold">
              {showWarning
                ? "Potential NAT/Port mismatch detected"
                : "Reachability looks OK"}
            </span>
          </div>
          {showWarning ? (
            <p className="text-sm">
              Your observed IP appears private or the observed port doesn’t
              match your configured listen port. Other users may not reach your
              share directly. Configure port forwarding for the listen port on
              your router, or enable UPnP/NAT-PMP. Then run "Check Reachability"
              again.
            </p>
          ) : (
            <p className="text-sm">
              Public IP and listen port match the server’s observation. This is
              a good sign, though it doesn’t guarantee reachability.
            </p>
          )}
        </div>
      )}
    </div>
  );
};
