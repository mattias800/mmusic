import * as React from "react";
import { graphql } from "@/gql";
import { useMutation, useQuery } from "urql";
import { Label } from "@/components/ui/label.tsx";
import { Input } from "@/components/ui/input.tsx";
import { Button } from "@/components/ui/button.tsx";

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
    <div className="flex flex-col gap-2 w-xs">
      <Label htmlFor="slsk-time">Soulseek search time limit (seconds)</Label>
      <Input
        id="slsk-time"
        type="number"
        min={5}
        max={600}
        value={value}
        onChange={(e) => setValue(parseInt(e.target.value || "0", 10))}
      />
      <div>
        <Button onClick={onSave} loading={fetching} disabled={fetching}>
          Save
        </Button>
      </div>
    </div>
  );
};


