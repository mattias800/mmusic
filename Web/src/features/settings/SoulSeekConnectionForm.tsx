import * as React from "react";
import { useQuery, useMutation } from "urql";
import { graphql } from "@/gql";
import { Label } from "@/components/ui/label.tsx";
import { Input } from "@/components/ui/input.tsx";
import { Button } from "@/components/ui/button.tsx";
import { Globe, Network, User } from "lucide-react";

const query = graphql(`
  query SoulSeekConnectionSettings_Query {
    serverSettings {
      id
      soulSeekHost
      soulSeekPort
      soulSeekUsername
    }
  }
`);

const mutation = graphql(`
  mutation UpdateSoulSeekConnectionSettings(
    $input: UpdateSoulSeekConnectionSettingsInput!
  ) {
    updateSoulSeekConnectionSettings(input: $input) {
      __typename
      ... on UpdateSoulSeekConnectionSettingsSuccess {
        serverSettings {
          id
          soulSeekHost
          soulSeekPort
          soulSeekUsername
        }
      }
      ... on UpdateSoulSeekConnectionSettingsError {
        message
      }
    }
  }
`);

export const SoulSeekConnectionForm: React.FC = () => {
  const [{ data }] = useQuery({ query, requestPolicy: "cache-and-network" });
  const [{ fetching }, update] = useMutation(mutation);

  const [host, setHost] = React.useState(
    data?.serverSettings?.soulSeekHost ?? "vps.slsknet.org",
  );
  const [port, setPort] = React.useState<number>(
    data?.serverSettings?.soulSeekPort ?? 2271,
  );
  const [username, setUsername] = React.useState(
    data?.serverSettings?.soulSeekUsername ?? "",
  );

  React.useEffect(() => {
    if (data?.serverSettings) {
      setHost(data.serverSettings.soulSeekHost ?? "vps.slsknet.org");
      setPort(data.serverSettings.soulSeekPort ?? 2271);
      setUsername(data.serverSettings.soulSeekUsername ?? "");
    }
  }, [data?.serverSettings]);

  const onSave = async () => {
    const input = {
      host: host.trim(),
      port: Math.max(1, Math.min(65535, Math.floor(port))),
      username: username.trim(),
    };
    await update({ input });
  };

  return (
    <div className="space-y-4">
      <div className="grid grid-cols-3 gap-4">
        <div className="col-span-2">
          <Label className="text-gray-300">Host</Label>
          <div className="relative">
            <Input
              value={host}
              onChange={(e) => setHost(e.target.value)}
              className="pl-9 bg-white/5 border-white/10"
            />
            <Globe className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" />
          </div>
        </div>
        <div>
          <Label className="text-gray-300">Port</Label>
          <div className="relative">
            <Input
              type="number"
              value={port}
              onChange={(e) => setPort(parseInt(e.target.value || "0", 10))}
              className="pl-9 bg-white/5 border-white/10"
            />
            <Network className="w-4 h-4 text-gray-400 absolute left-3 top-1/2 -translate-y-1/2" />
          </div>
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
      <div className="flex justify-end">
        <Button
          onClick={onSave}
          disabled={fetching}
          className="bg-blue-600 hover:bg-blue-700 text-white"
        >
          {fetching ? "Saving..." : "Save"}
        </Button>
      </div>
    </div>
  );
};
