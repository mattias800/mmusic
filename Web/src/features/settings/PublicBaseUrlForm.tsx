import * as React from "react";
import { useMutation } from "urql";
import { Button } from "@/components/ui/button";
import { Alert } from "@/components/ui/Alert";
import { Globe } from "lucide-react";
import { graphql } from "@/gql";

type ServerSettingsShape = { id: string; publicBaseUrl?: string | null };

const updateMutation = graphql(`
  mutation UpdatePublicBaseUrl($input: UpdatePublicBaseUrlInput!) {
    updatePublicBaseUrl(input: $input) {
      ... on UpdatePublicBaseUrlSuccess {
        serverSettings {
          id
          publicBaseUrl
        }
      }
      ... on UpdatePublicBaseUrlError {
        message
      }
    }
  }
`);

export interface PublicBaseUrlFormProps {
  serverSettings: ServerSettingsShape;
}

export const PublicBaseUrlForm: React.FC<PublicBaseUrlFormProps> = ({
  serverSettings,
}) => {
  const [value, setValue] = React.useState(serverSettings.publicBaseUrl ?? "");
  const [error, setError] = React.useState<string | null>(null);
  const [, update] = useMutation(updateMutation);

  const onSave = async () => {
    setError(null);
    const trimmed = value.trim();
    if (trimmed.length > 0) {
      try {
        const url = new URL(trimmed);
        if (!/^https?:$/.test(url.protocol))
          throw new Error("Must be http or https");
      } catch {
        setError("Please enter a valid URL starting with http:// or https://");
        return;
      }
    }
    const res = await update({ input: { publicBaseUrl: trimmed } });
    const payload = res.data?.updatePublicBaseUrl;
    if (payload && "serverSettings" in payload) {
      // ok
    } else if (payload && "message" in payload) {
      setError(payload.message ?? "Failed to update");
    } else if (res.error) {
      setError(res.error.message);
    }
  };

  return (
    <div className="space-y-3">
      <div className="flex items-center gap-3">
        <div className="w-10 h-10 bg-blue-500/20 rounded-lg flex items-center justify-center">
          <Globe className="w-5 h-5 text-blue-400" />
        </div>
        <div className="flex-1">
          <label className="block text-sm text-gray-300 mb-1">
            Public Base URL
          </label>
          <input
            type="url"
            placeholder="https://mmusic.com"
            value={value}
            onChange={(e) => setValue(e.target.value)}
            className="w-full bg-gray-800/50 border border-gray-700 rounded px-3 py-2 text-white"
          />
          <p className="text-xs text-gray-400 mt-1">
            Used for Google Cast and external links. Leave empty to use request
            origin.
          </p>
        </div>
      </div>
      {error && <Alert variant="warning" title={error} />}
      <div className="flex justify-end">
        <Button
          onClick={onSave}
          className="bg-blue-600 hover:bg-blue-700 text-white"
        >
          Save
        </Button>
      </div>
    </div>
  );
};
