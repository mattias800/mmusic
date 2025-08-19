import * as React from "react";
import { useMemo, useState } from "react";
import { graphql } from "@/gql";
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
  const [testing, setTesting] = useState(false);
  const [testResult, setTestResult] = useState<string | null>(null);

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

  async function testListenBrainz() {
    setTesting(true);
    setTestResult(null);
    try {
      const res = await fetch("/graphql", {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({ query: testListenBrainzQuery.loc?.source.body }),
      });
      const json = await res.json();
      const status = json?.data?.external?.testListenBrainzConnectivity;
      if (!status) setTestResult("Test failed");
      else setTestResult(status.ok ? `OK: ${status.message}` : `Failed: ${status.message}`);
    } catch (e) {
      const message = e instanceof Error ? e.message : String(e);
      setTestResult(`Error: ${message}`);
    } finally {
      setTesting(false);
    }
  }

  if (fetching) return <div className="text-sm opacity-70">Loading token status…</div>;
  if (error) return <div className="text-sm text-red-500">Failed to load token status</div>;

  return (
    <div className="space-y-3">
      <Row label="ListenBrainz" ok={configured.listenBrainz.ok} source={configured.listenBrainz.source}>
        <Button size="sm" onClick={testListenBrainz} disabled={!configured.listenBrainz.ok || testing}>
          {testing ? "Testing…" : "Test connectivity"}
        </Button>
        {testResult && <div className="text-xs opacity-70 mt-1">{testResult}</div>}
      </Row>
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

type ConnectivityQueryDoc = { loc?: { source: { body: string } } };

const TestRow: React.FC<{
  label: string;
  state: { ok: boolean; source: string };
  query: ConnectivityQueryDoc;
}> = ({ label, state, query }) => {
  const [testing, setTesting] = useState(false);
  const [result, setResult] = useState<string | null>(null);
  async function onTest() {
    setTesting(true);
    setResult(null);
    try {
      const res = await fetch("/graphql", {
        method: "POST",
        headers: { "content-type": "application/json" },
        body: JSON.stringify({ query: query.loc?.source.body }),
      });
      const json = await res.json();
      const status = json?.data?.external?.[Object.keys(json?.data?.external || {}).find(k => k.startsWith("test")) as string];
      if (!status) setResult("Test failed");
      else setResult(status.ok ? `OK: ${status.message}` : `Failed: ${status.message}`);
    } catch (e) {
      const message = e instanceof Error ? e.message : String(e);
      setResult(`Error: ${message}`);
    } finally {
      setTesting(false);
    }
  }
  return (
    <Row label={label} ok={state.ok} source={state.source}>
      <Button size="sm" onClick={onTest} disabled={!state.ok || testing}>
        {testing ? "Testing…" : "Test connectivity"}
      </Button>
      {result && <div className="text-xs opacity-70 mt-1">{result}</div>}
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


