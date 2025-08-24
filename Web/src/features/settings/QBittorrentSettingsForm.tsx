import * as React from "react";
import { useQuery, useMutation } from "urql";
import { graphql } from "@/gql";
import { Label } from "@/components/ui/label.tsx";
import { Input } from "@/components/ui/input.tsx";
import { Button } from "@/components/ui/button.tsx";
import { Globe, User, Folder } from "lucide-react";
import { useClient } from "urql";

const query = graphql(`
  query QBittorrentSettings_Query {
    serverSettings {
      id
      qBittorrentBaseUrl
      qBittorrentUsername
      qBittorrentSavePath
    }
  }
`);

const mutation = graphql(`
  mutation UpdateQBittorrentSettings($input: UpdateQBittorrentSettingsInput!) {
    updateQBittorrentSettings(input: $input) {
      __typename
      ... on UpdateQBittorrentSettingsSuccess {
        serverSettings {
          id
          qBittorrentBaseUrl
          qBittorrentUsername
          qBittorrentSavePath
        }
      }
      ... on UpdateQBittorrentSettingsError {
        message
      }
    }
  }
`);

export const QBittorrentSettingsForm: React.FC = () => {
  const [{ data }] = useQuery({ query, requestPolicy: "cache-and-network" });
  const [{ fetching }, update] = useMutation(mutation);
  const client = useClient();
  const s = data?.serverSettings;
  const [baseUrl, setBaseUrl] = React.useState<string>(
    s?.qBittorrentBaseUrl ?? "",
  );
  const [username, setUsername] = React.useState<string>(
    s?.qBittorrentUsername ?? "",
  );
  const [savePath, setSavePath] = React.useState<string>(
    s?.qBittorrentSavePath ?? "",
  );

  React.useEffect(() => {
    if (!s) return;
    setBaseUrl(s.qBittorrentBaseUrl ?? "");
    setUsername(s.qBittorrentUsername ?? "");
    setSavePath(s.qBittorrentSavePath ?? "");
  }, [s]);

  const onSave = async () => {
    const input = {
      baseUrl: baseUrl.trim() || null,
      username: username.trim() || null,
      savePath: savePath.trim() || null,
    };
    await update({ input });
  };

  const testQuery = graphql(`
    query TestQBittorrentConnectivity {
      external {
        testQBittorrentConnectivity {
          ok
          message
        }
      }
    }
  `);
  const [testMsg, setTestMsg] = React.useState<string | null>(null);
  const [testing, setTesting] = React.useState(false);
  const onTest = async () => {
    setTesting(true);
    setTestMsg(null);
    const res = await client.query(testQuery, {}).toPromise();
    const payload = res.data?.external?.testQBittorrentConnectivity;
    if (payload)
      setTestMsg(`${payload.ok ? "OK" : "FAIL"}: ${payload.message}`);
    else setTestMsg(res.error ? `Error: ${res.error.message}` : "No response");
    setTesting(false);
  };

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-2 gap-4">
        <div>
          <Label className="text-gray-300">Base URL</Label>
          <div className="relative">
            <Input
              value={baseUrl}
              onChange={(e) => setBaseUrl(e.target.value)}
              className="pl-9 bg-white/5 border-white/10"
              placeholder="http://localhost:8080"
            />
            <Globe className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" />
          </div>
        </div>
        <div>
          <Label className="text-gray-300">Username</Label>
          <div className="relative">
            <Input
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              className="pl-9 bg-white/5 border-white/10"
            />
            <User className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" />
          </div>
        </div>
      </div>
      <div>
        <Label className="text-gray-300">Save Path</Label>
        <div className="relative">
          <Input
            value={savePath}
            onChange={(e) => setSavePath(e.target.value)}
            className="pl-9 bg-white/5 border-white/10"
            placeholder="/downloads"
          />
          <Folder className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" />
        </div>
      </div>
      <div className="flex items-center justify-between">
        <div className="text-xs text-gray-400 min-h-4">{testMsg}</div>
        <div className="flex gap-2">
          <Button
            onClick={onTest}
            disabled={testing}
            className="bg-gray-600 hover:bg-gray-700 text-white"
          >
            {testing ? "Testing..." : "Test connection"}
          </Button>
          <Button
            onClick={onSave}
            disabled={fetching}
            className="bg-blue-600 hover:bg-blue-700 text-white"
          >
            {fetching ? "Saving..." : "Save"}
          </Button>
        </div>
      </div>
      <p className="text-xs text-gray-400">
        Password should be provided via environment/secret store; not editable
        here.
      </p>
    </div>
  );
};
