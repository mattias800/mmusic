import * as React from "react";
import { useEffect } from "react";
import { useMutation, useQuery, useSubscription } from "urql";
import { graphql } from "@/gql";
import { GlassCard } from "@/components/ui";

const onlineClientsQuery = graphql(`
  query OnlineClients {
    user {
      onlineClients {
        userId
        clientId
        name
        lastSeenAt
        playback { artistId releaseFolderName trackNumber trackTitle }
      }
    }
  }
`);

const clientsUpdated = graphql(`
  subscription ClientsUpdated {
    onClientsUpdated {
      userId
      clientId
      name
      lastSeenAt
      playback { artistId releaseFolderName trackNumber trackTitle }
    }
  }
`);

const heartbeatMutation = graphql(`
  mutation Heartbeat(
    $clientId: String!
    $name: String
    $artistId: String
    $releaseFolderName: String
    $trackNumber: Int
    $trackTitle: String
  ) {
    heartbeat(
      clientId: $clientId
      name: $name
      artistId: $artistId
      releaseFolderName: $releaseFolderName
      trackNumber: $trackNumber
      trackTitle: $trackTitle
    ) {
      __typename
    }
  }
`);

export const OnlineClientsPanel: React.FC = () => {
  const [{ data }, reexec] = useQuery({ query: onlineClientsQuery, requestPolicy: "network-only", pause: false });
  const [, sendHeartbeat] = useMutation(heartbeatMutation);

  useSubscription({ query: clientsUpdated }, () => {
    reexec({ requestPolicy: "network-only" });
    return null;
  });

  useEffect(() => {
    const clientId = getOrCreateClientId();
    const name = getDefaultClientName();
    const interval = setInterval(() => {
      sendHeartbeat({ clientId, name }).catch(() => {});
    }, 15000);
    // initial beat
    sendHeartbeat({ clientId, name }).catch(() => {});
    return () => clearInterval(interval);
  }, [sendHeartbeat]);

  const clients = data?.user?.onlineClients ?? [];

  return (
    <GlassCard title="Online Clients">
      <div className="space-y-2 text-sm">
        {clients.length === 0 && <div className="opacity-60">No clients online</div>}
        {clients.map((c) => (
          <div key={`${c.userId}-${c.clientId}`} className="flex items-center justify-between">
            <div className="truncate">
              <div className="font-medium">{c.name}</div>
              <div className="opacity-70 text-xs">
                {c.playback?.trackTitle ?? "Idle"}
              </div>
            </div>
            <div className="opacity-60 text-xs">{formatTime(c.lastSeenAt)}</div>
          </div>
        ))}
      </div>
    </GlassCard>
  );
};

function getOrCreateClientId(): string {
  const key = "mmusic_client_id";
  let id = localStorage.getItem(key);
  if (!id) {
    id = crypto.randomUUID();
    localStorage.setItem(key, id);
  }
  return id;
}

function getDefaultClientName(): string {
  return `${navigator.platform || "Unknown"} / ${navigator.userAgent}`;
}

function formatTime(value: unknown): string {
  try {
    if (typeof value === "string" || typeof value === "number") {
      const d = new Date(value);
      if (!isNaN(d.getTime())) return d.toLocaleTimeString();
    }
  } catch {
    // ignore formatting errors
  }
  return "";
}


