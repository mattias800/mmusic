import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { AlertTriangle, CheckCircle2, Network } from "lucide-react";

const query = graphql(`
  query SoulSeekReachabilitySummary_Query {
    soulSeekSharingStatistics {
      isSharingEnabled
      listeningPort
      observedIp
      observedPort
      observedAtUtc
      isPrivateIp
      portMatches
    }
  }
`);

export const SoulSeekReachabilitySummary: React.FC = () => {
  const [{ data, fetching, error }] = useQuery({ query, requestPolicy: "cache-and-network" });

  if (fetching || error || !data?.soulSeekSharingStatistics) return null;
  const s = data.soulSeekSharingStatistics;

  const observedEndpoint = s.observedIp && s.observedPort ? `${s.observedIp}:${s.observedPort}` : "—";
  const observedAtText = s.observedAtUtc ? new Date(s.observedAtUtc).toLocaleString() : "—";
  const warning = s.isPrivateIp || !s.portMatches;

  return (
    <div
      className={`mt-4 rounded-xl border p-4 ${
        warning
          ? "bg-orange-500/10 border-orange-500/20 text-orange-100"
          : "bg-green-500/10 border-green-500/20 text-green-100"
      }`}
    >
      <div className="flex items-center justify-between gap-4">
        <div className="flex items-center gap-3">
          <div className={`p-2 rounded-lg ${warning ? "bg-orange-500/20" : "bg-green-500/20"}`}>
            <Network className={`w-5 h-5 ${warning ? "text-orange-300" : "text-green-300"}`} />
          </div>
          <div>
            <div className="flex items-center gap-2">
              {warning ? (
                <AlertTriangle className="w-4 h-4" />
              ) : (
                <CheckCircle2 className="w-4 h-4" />
              )}
              <span className="font-semibold">
                {warning ? "SoulSeek Reachability: Action recommended" : "SoulSeek Reachability: OK"}
              </span>
            </div>
            <div className="text-sm opacity-90 mt-1">
              Observed: <span className="font-mono">{observedEndpoint}</span> · Listen port: {s.listeningPort} · Last checked: {observedAtText}
            </div>
          </div>
        </div>
        {!s.isSharingEnabled && (
          <div className="text-xs italic opacity-80">Sharing disabled</div>
        )}
      </div>
    </div>
  );
};
