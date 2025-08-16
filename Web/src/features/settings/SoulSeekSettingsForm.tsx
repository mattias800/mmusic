import * as React from "react";
import { graphql } from "@/gql";
import { useMutation, useQuery } from "urql";
import { Label } from "@/components/ui/label.tsx";
import { Input } from "@/components/ui/input.tsx";
import { GradientButton } from "@/components/ui";
import { Search, Clock } from "lucide-react";

const query = graphql(`
  query SoulSeekSettings_Query {
    serverSettings {
      id
      soulSeekSearchTimeLimitSeconds
    }
  }
`);

const mutation = graphql(`
  mutation UpdateSoulSeekSearchTimeLimit($seconds: Int!) {
    updateSoulSeekSearchTimeLimit(seconds: $seconds) {
      __typename
      ... on UpdateSoulSeekSearchTimeLimitSuccess {
        serverSettings { id soulSeekSearchTimeLimitSeconds }
      }
      ... on UpdateSoulSeekSearchTimeLimitError { message }
    }
  }
`);

export const SoulSeekSettingsForm: React.FC = () => {
  const [{ data }] = useQuery({ query });
  const current = data?.serverSettings?.soulSeekSearchTimeLimitSeconds ?? 60;
  const [value, setValue] = React.useState<number>(current);
  const [{ fetching }, update] = useMutation(mutation);

  React.useEffect(() => { setValue(current); }, [current]);

  const onSave = async () => {
    const seconds = Math.max(5, Math.min(600, Math.floor(value)));
    await update({ seconds });
  };

  return (
    <div className="space-y-6">
      {/* Description */}
      <div className="p-4 bg-white/5 rounded-lg border border-white/10">
        <div className="flex items-center gap-3 mb-3">
          <div className="p-2 bg-purple-500/20 rounded-lg">
            <Search className="w-5 h-5 text-purple-400" />
          </div>
          <h3 className="text-lg font-semibold text-white">SoulSeek Search Configuration</h3>
        </div>
        <p className="text-gray-300 text-sm">
          Configure how long SoulSeek searches can run before timing out. Longer timeouts allow for more thorough searches but may impact performance.
        </p>
      </div>

      {/* Settings Form */}
      <div className="space-y-4">
        <div>
          <Label htmlFor="slsk-time" className="text-gray-200 text-sm font-medium">
            Search Time Limit
          </Label>
          <div className="mt-2 flex items-center gap-3">
            <div className="relative">
              <Input
                id="slsk-time"
                type="number"
                min={5}
                max={600}
                value={value}
                onChange={(e) => setValue(parseInt(e.target.value || "0", 10))}
                className="w-24 bg-white/10 border-white/20 text-white placeholder-gray-400 focus:border-purple-400 focus:ring-purple-400"
              />
              <Clock className="absolute right-2 top-1/2 transform -translate-y-1/2 w-4 h-4 text-gray-400" />
            </div>
            <span className="text-sm text-gray-400">seconds (5-600)</span>
          </div>
          <p className="mt-2 text-xs text-gray-400">
            Current setting: {current} seconds
          </p>
        </div>
        
        <GradientButton 
          onClick={onSave} 
          disabled={fetching}
          variant="primary"
          className="w-full"
        >
          {fetching ? 'Saving...' : 'Save Settings'}
        </GradientButton>
      </div>
    </div>
  );
};


