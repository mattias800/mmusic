import * as React from "react";
import { useQuery, useMutation } from "urql";
import { graphql } from "@/gql";
import { Label } from "@/components/ui/label.tsx";
import { Input } from "@/components/ui/input.tsx";
import { Button } from "@/components/ui/button.tsx";
import { Globe, RefreshCcw, Activity } from "lucide-react";
import { useClient } from "urql";
import { Switch } from "@/components/ui/switch.tsx";

const query = graphql(`
  query ProwlarrSettings_Query {
    serverSettings {
      id
      prowlarrBaseUrl
      prowlarrTimeoutSeconds
      prowlarrMaxRetries
      prowlarrRetryDelaySeconds
      prowlarrTestConnectivityFirst
      prowlarrEnableDetailedLogging
      prowlarrMaxConcurrentRequests
    }
  }
`);

const mutation = graphql(`
  mutation UpdateProwlarrSettings($input: UpdateProwlarrSettingsInput!) {
    updateProwlarrSettings(input: $input) {
      __typename
      ... on UpdateProwlarrSettingsSuccess {
        serverSettings {
          id
          prowlarrBaseUrl
          prowlarrTimeoutSeconds
          prowlarrMaxRetries
          prowlarrRetryDelaySeconds
          prowlarrTestConnectivityFirst
          prowlarrEnableDetailedLogging
          prowlarrMaxConcurrentRequests
        }
      }
      ... on UpdateProwlarrSettingsError { message }
    }
  }
`);

export const ProwlarrSettingsForm: React.FC = () => {
  const [{ data }] = useQuery({ query, requestPolicy: "cache-and-network" });
  const [{ fetching }, update] = useMutation(mutation);
  const client = useClient();

  const s = data?.serverSettings;
  const [baseUrl, setBaseUrl] = React.useState<string>(s?.prowlarrBaseUrl ?? "");
  const [timeout, setTimeout] = React.useState<number>(s?.prowlarrTimeoutSeconds ?? 30);
  const [maxRetries, setMaxRetries] = React.useState<number>(s?.prowlarrMaxRetries ?? 2);
  const [retryDelay, setRetryDelay] = React.useState<number>(s?.prowlarrRetryDelaySeconds ?? 1);
  const [testFirst, setTestFirst] = React.useState<boolean>(s?.prowlarrTestConnectivityFirst ?? true);
  const [detailed, setDetailed] = React.useState<boolean>(s?.prowlarrEnableDetailedLogging ?? false);
  const [maxConc, setMaxConc] = React.useState<number>(s?.prowlarrMaxConcurrentRequests ?? 1);

  React.useEffect(() => {
    if (!s) return;
    setBaseUrl(s.prowlarrBaseUrl ?? "");
    setTimeout(s.prowlarrTimeoutSeconds ?? 30);
    setMaxRetries(s.prowlarrMaxRetries ?? 2);
    setRetryDelay(s.prowlarrRetryDelaySeconds ?? 1);
    setTestFirst(s.prowlarrTestConnectivityFirst ?? true);
    setDetailed(s.prowlarrEnableDetailedLogging ?? false);
    setMaxConc(s.prowlarrMaxConcurrentRequests ?? 1);
  }, [s]);

  const onSave = async () => {
    const input = {
      baseUrl: baseUrl.trim() || null,
      timeoutSeconds: Math.max(1, timeout),
      maxRetries: Math.max(0, maxRetries),
      retryDelaySeconds: Math.max(0, retryDelay),
      testConnectivityFirst: !!testFirst,
      enableDetailedLogging: !!detailed,
      maxConcurrentRequests: Math.max(1, maxConc),
    };
    await update({ input });
  };

  const testQuery = graphql(`
    query TestProwlarrConnectivity {
      external { testProwlarrConnectivity { ok message } }
    }
  `);
  const [testMsg, setTestMsg] = React.useState<string | null>(null);
  const [testing, setTesting] = React.useState(false);
  const onTest = async () => {
    setTesting(true);
    setTestMsg(null);
    const res = await client.query(testQuery, {}).toPromise();
    const payload = res.data?.external?.testProwlarrConnectivity;
    if (payload) setTestMsg(`${payload.ok ? "OK" : "FAIL"}: ${payload.message}`);
    else setTestMsg(res.error ? `Error: ${res.error.message}` : "No response");
    setTesting(false);
  };

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-3 gap-4">
        <div className="col-span-2">
          <Label className="text-gray-300">Base URL</Label>
          <div className="relative">
            <Input value={baseUrl} onChange={(e) => setBaseUrl(e.target.value)} className="pl-9 bg-white/5 border-white/10" placeholder="http://localhost:9696" />
            <Globe className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" />
          </div>
        </div>
        <div>
          <Label className="text-gray-300">Timeout (s)</Label>
          <Input type="number" value={timeout} onChange={(e) => setTimeout(parseInt(e.target.value || "0", 10))} className="bg-white/5 border-white/10" />
        </div>
      </div>
      <div className="grid grid-cols-3 gap-4">
        <div>
          <Label className="text-gray-300">Max Retries</Label>
          <div className="relative">
            <Input type="number" value={maxRetries} onChange={(e) => setMaxRetries(parseInt(e.target.value || "0", 10))} className="pl-9 bg-white/5 border-white/10" />
            <RefreshCcw className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" />
          </div>
        </div>
        <div>
          <Label className="text-gray-300">Retry Delay (s)</Label>
          <Input type="number" value={retryDelay} onChange={(e) => setRetryDelay(parseInt(e.target.value || "0", 10))} className="bg-white/5 border-white/10" />
        </div>
        <div>
          <Label className="text-gray-300">Max Concurrent</Label>
          <div className="relative">
            <Input type="number" value={maxConc} onChange={(e) => setMaxConc(parseInt(e.target.value || "0", 10))} className="pl-9 bg-white/5 border-white/10" />
            <Activity className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" />
          </div>
        </div>
      </div>
      <div className="flex items-center gap-6">
        <label className="flex items-center gap-2 text-gray-300">
          <Switch checked={testFirst} onCheckedChange={setTestFirst} ariaLabel="Test connectivity first" /> Test connectivity first
        </label>
        <label className="flex items-center gap-2 text-gray-300">
          <Switch checked={detailed} onCheckedChange={setDetailed} ariaLabel="Enable detailed logging" /> Enable detailed logging
        </label>
      </div>
      <div className="flex items-center justify-between">
        <div className="text-xs text-gray-400 min-h-4">{testMsg}</div>
        <div className="flex gap-2">
          <Button onClick={onTest} disabled={testing} className="bg-gray-600 hover:bg-gray-700 text-white">
            {testing ? "Testing..." : "Test connection"}
          </Button>
          <Button onClick={onSave} disabled={fetching} className="bg-blue-600 hover:bg-blue-700 text-white">
            {fetching ? "Saving..." : "Save"}
          </Button>
        </div>
      </div>
      <p className="text-xs text-gray-400">API key should be provided via environment/secret store; not editable here.</p>
    </div>
  );
};


