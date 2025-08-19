import * as React from "react";
import { useMemo } from "react";
import { graphql } from "@/gql";
import type { TypedDocumentNode } from "@graphql-typed-document-node/core";
import { useQuery } from "urql";
import { Button } from "@/components/ui/button.tsx";
import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip.tsx";
import { Alert } from "@/components/ui/Alert.tsx";

const statusQuery = graphql(`
  query AuthTokensStatusPanel {
    serverSettings {
      id
      isListenBrainzConfigured
      isYouTubeConfigured
      isSpotifyConfigured
      isLastfmConfigured
      isFanartConfigured
      listenBrainzConfiguredSource
      youTubeConfiguredSource
      spotifyConfiguredSource
      lastfmConfiguredSource
      fanartConfiguredSource
    }
    external {
      id
    }
  }
`);

const testListenBrainzQuery = graphql(`
  query TestListenBrainzConnectivity {
    external {
      testListenBrainzConnectivity {
        ok
        message
      }
    }
  }
`);

const testYouTubeQuery = graphql(`
  query TestYouTubeConnectivity {
    external { testYouTubeConnectivity { ok message } }
  }
`);

const testSpotifyQuery = graphql(`
  query TestSpotifyConnectivity {
    external { testSpotifyConnectivity { ok message } }
  }
`);

const testLastfmQuery = graphql(`
  query TestLastfmConnectivity {
    external { testLastfmConnectivity { ok message } }
  }
`);

const testFanartQuery = graphql(`
  query TestFanartConnectivity {
    external { testFanartConnectivity { ok message } }
  }
`);

export const AuthTokensStatusPanel: React.FC = () => {
  const [{ data, fetching, error }] = useQuery({ query: statusQuery, requestPolicy: "network-only" });

  const configured = useMemo(() => {
    const s = data?.serverSettings;
    return {
      listenBrainz: { ok: !!s?.isListenBrainzConfigured, source: s?.listenBrainzConfiguredSource ?? "none" },
      youTube: { ok: !!s?.isYouTubeConfigured, source: s?.youTubeConfiguredSource ?? "none" },
      spotify: { ok: !!s?.isSpotifyConfigured, source: s?.spotifyConfiguredSource ?? "none" },
      lastfm: { ok: !!s?.isLastfmConfigured, source: s?.lastfmConfiguredSource ?? "none" },
      fanart: { ok: !!s?.isFanartConfigured, source: s?.fanartConfiguredSource ?? "none" },
    };
  }, [data]);


  if (fetching) return <div className="text-sm opacity-70">Loading token status…</div>;
  if (error) return <div className="text-sm text-red-500">Failed to load token status</div>;

  return (
    <div className="space-y-3">
      <TestRow label="ListenBrainz" state={configured.listenBrainz} query={testListenBrainzQuery} />
      {!configured.listenBrainz.ok && (
        <FixHelp
          serviceName="ListenBrainz"
          envVars={["ListenBrainz__ApiKey"]}
          extra="Also set ListenBrainz username in Server settings if needed."
        />
      )}
      <TestRow label="YouTube" state={configured.youTube} query={testYouTubeQuery} />
      {!configured.youTube.ok && (
        <FixHelp serviceName="YouTube" envVars={["YouTube__ApiKey"]} />
      )}
      <TestRow label="Spotify" state={configured.spotify} query={testSpotifyQuery} />
      {!configured.spotify.ok && (
        <FixHelp serviceName="Spotify" envVars={["Spotify__ClientId", "Spotify__ClientSecret"]} />
      )}
      <TestRow label="Last.fm" state={configured.lastfm} query={testLastfmQuery} />
      {!configured.lastfm.ok && (
        <FixHelp serviceName="Last.fm" envVars={["Lastfm__ApiKey"]} />
      )}
      <TestRow label="Fanart.tv" state={configured.fanart} query={testFanartQuery} />
      {!configured.fanart.ok && (
        <FixHelp
          serviceName="Fanart.tv"
          envVars={["Fanart__ApiKey"]}
          extra="BaseAddress is optional; default will be used if omitted."
        />
      )}
    </div>
  );
};

const Row: React.FC<{ label: string; ok: boolean; source?: string; children?: React.ReactNode }> = ({ label, ok, source, children }) => {
  return (
    <div className="flex items-center justify-between py-1">
      <div className="flex items-center gap-2">
        <span className={`inline-block h-2.5 w-2.5 rounded-full ${ok ? "bg-green-500" : "bg-zinc-400"}`} />
        <span className="text-sm">{label}</span>
        {source && (
          <Tooltip>
            <TooltipTrigger asChild>
              <span className="text-xs opacity-60 cursor-help">({source})</span>
            </TooltipTrigger>
            <TooltipContent>
              <div className="max-w-xs">
                <div className="font-semibold mb-1">Configured source</div>
                <div>
                  {source === "appsettings"
                    ? "Read from appsettings.development.json or environment variables."
                    : source === "database"
                    ? "Stored in server settings (database)."
                    : "Not configured. Add credentials to appsettings or environment."}
                </div>
              </div>
            </TooltipContent>
          </Tooltip>
        )}
      </div>
      <div className="flex items-center gap-2">{children}</div>
    </div>
  );
};

type ConnectivityQueryDoc = TypedDocumentNode<unknown, Record<string, never>>;

function extractConnectivityStatus(data: unknown): { ok: boolean; message: string } | null {
  if (!data || typeof data !== "object") return null;
  const dataObj = data as { external?: unknown };
  const external = dataObj.external;
  if (!external || typeof external !== "object") return null;
  const externalObj = external as Record<string, unknown>;
  for (const key of Object.keys(externalObj)) {
    if (key.startsWith("test")) {
      const value = externalObj[key];
      if (value && typeof value === "object") {
        const v = value as Record<string, unknown>;
        if ("ok" in v && typeof v.ok === "boolean" && "message" in v && typeof v.message === "string") {
          return { ok: v.ok, message: v.message };
        }
      }
    }
  }
  return null;
}

const TestRow: React.FC<{
  label: string;
  state: { ok: boolean; source: string };
  query: ConnectivityQueryDoc;
}> = ({ label, state, query }) => {
  const [{ data, fetching, error }, reexecuteQuery] = useQuery<unknown, Record<string, never>>({ query, pause: true, requestPolicy: "network-only" });

  const status = React.useMemo(() => extractConnectivityStatus(data), [data]);

  const message = error
    ? `Error: ${error.message}`
    : status
    ? status.ok
      ? `OK: ${status.message}`
      : `Failed: ${status.message}`
    : null;

  return (
    <Row label={label} ok={state.ok} source={state.source}>
      <Button size="sm" onClick={() => reexecuteQuery({ requestPolicy: "network-only" })} disabled={!state.ok || fetching}>
        {fetching ? "Testing…" : "Test connectivity"}
      </Button>
      {message && !fetching && <div className="text-xs opacity-70 mt-1">{message}</div>}
    </Row>
  );
};

const FixHelp: React.FC<{
  serviceName: string;
  envVars: string[];
  extra?: string;
}> = ({ serviceName, envVars, extra }) => {
  return (
    <Alert variant="warning" title={`${serviceName} is not configured`}>
      <div className="space-y-2">
        <div>Set the following environment variable(s) on the server:</div>
        <ul className="list-disc pl-6">
          {envVars.map((v) => (
            <li key={v} className="font-mono text-xs">{v}</li>
          ))}
        </ul>
        {extra && <div className="text-xs opacity-80">{extra}</div>}
      </div>
    </Alert>
  );
};


