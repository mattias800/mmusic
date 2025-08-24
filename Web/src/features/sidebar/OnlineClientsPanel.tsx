import * as React from "react";
import { useEffect, useState } from "react";
import { useMutation, useQuery, useSubscription } from "urql";
import { graphql } from "@/gql";
import { GlassCard } from "@/components/ui";
import { Switch } from "@/components/ui/switch.tsx";

const onlineClientsQuery = graphql(`
  query OnlineClients {
    user {
      onlineClients {
        userId
        clientId
        name
        lastSeenAt
        playback {
          artistId
          releaseFolderName
          trackNumber
          trackTitle
        }
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
      playback {
        artistId
        releaseFolderName
        trackNumber
        trackTitle
      }
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
  const [{ data }, reexec] = useQuery({
    query: onlineClientsQuery,
    requestPolicy: "network-only",
    pause: false,
  });
  const [, sendHeartbeat] = useMutation(heartbeatMutation);
  const [clientName, setClientName] = useState<string>(
    getStoredClientName() || getDefaultClientName(),
  );
  const [allowRemote, setAllowRemote] = useState<boolean>(
    getStoredAllowRemote(),
  );

  useSubscription({ query: clientsUpdated }, () => {
    reexec({ requestPolicy: "network-only" });
    return null;
  });

  useEffect(() => {
    const clientId = getOrCreateClientId();
    persistClientName(clientName);
    persistAllowRemote(allowRemote);
    const beat = () =>
      sendHeartbeat({ clientId, name: clientName }).catch(() => {});
    const interval = setInterval(beat, 15000);
    beat();
    return () => clearInterval(interval);
  }, [sendHeartbeat, clientName, allowRemote]);

  const clients = data?.user?.onlineClients ?? [];

  return (
    <GlassCard title="Online Clients">
      <div className="space-y-3 text-sm">
        <div className="space-y-2">
          <div className="font-medium">This client</div>
          <div className="flex items-center gap-2">
            <input
              className="border rounded px-2 py-1 w-60"
              value={clientName}
              onChange={(e) => setClientName(e.target.value)}
              placeholder="Client name"
            />
            <label className="flex items-center gap-2 text-xs">
              <Switch
                checked={allowRemote}
                onCheckedChange={setAllowRemote}
                ariaLabel="Allow remote playback"
              />
              Allow remote playback
            </label>
          </div>
        </div>
        {clients.length === 0 && (
          <div className="opacity-60">No clients online</div>
        )}
        {clients.map((c) => (
          <div
            key={`${c.userId}-${c.clientId}`}
            className="flex items-center justify-between"
          >
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
  try {
    const navAny = navigator as unknown as {
      userAgentData?: { platform?: string };
    };
    const platform =
      navAny.userAgentData?.platform || navigator.platform || "Unknown";
    return `${platform} / ${navigator.userAgent}`;
  } catch {
    return "Unknown Client";
  }
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

function getStoredClientName(): string | null {
  try {
    return localStorage.getItem("mmusic_client_name");
  } catch {
    return null;
  }
}

function persistClientName(name: string) {
  try {
    localStorage.setItem("mmusic_client_name", name);
  } catch {
    /* ignore */
  }
}

function getStoredAllowRemote(): boolean {
  try {
    const v = localStorage.getItem("mmusic_allow_remote_playback");
    return v == null ? true : v === "true";
  } catch {
    return true;
  }
}

function persistAllowRemote(allow: boolean) {
  try {
    localStorage.setItem(
      "mmusic_allow_remote_playback",
      allow ? "true" : "false",
    );
  } catch {
    /* ignore */
  }
}
