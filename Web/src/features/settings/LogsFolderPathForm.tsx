import * as React from "react";
import { useState } from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { DirectoryBrowserModal } from "@/features/settings/components/DirectoryBrowser/DirectoryBrowserModal";

export interface LogsFolderPathFormProps {
  serverSettings: FragmentType<typeof logsFolderPathFormServerSettingsFragment>;
}

const logsFolderPathFormServerSettingsFragment = graphql(`
  fragment LogsFolderPathForm_ServerSettings on ServerSettings {
    id
    logsFolderPath
  }
`);

const updateLogsFolderPathMutation = graphql(`
  mutation UpdateLogsFolderPath($newPath: String) {
    updateLogsFolderPath(input: { newPath: $newPath }) {
      ... on UpdateLogsFolderPathSuccess {
        serverSettings {
          id
          logsFolderPath
        }
      }
      ... on UpdateLogsFolderPathError {
        message
      }
    }
  }
`);

export const LogsFolderPathForm: React.FC<LogsFolderPathFormProps> = (props) => {
  const serverSettings = useFragment(
    logsFolderPathFormServerSettingsFragment,
    props.serverSettings,
  );

  const [isModalOpen, setIsModalOpen] = useState(false);
  const [{ fetching }, updateSetting] = useMutation(updateLogsFolderPathMutation);

  return (
    <div>
      <Label htmlFor="logs-folder-path" style={{ marginBottom: "8px", display: "block" }}>
        Logs folder path
      </Label>
      <div className={"flex w-xl items-center space-x-2"}>
        <Input id={"logs-folder-path"} value={serverSettings.logsFolderPath ?? ""} readOnly />
        <Button loading={fetching} disabled={fetching} onClick={() => setIsModalOpen(true)}>
          Browse...
        </Button>
        <Button
          variant="outline"
          loading={fetching}
          disabled={fetching}
          onClick={() => updateSetting({ newPath: null })}
        >
          Clear
        </Button>
      </div>
      <div className="text-sm text-muted-foreground mt-2">
        When set, per-release logs are written under <code>Downloads/&lt;Artist&gt;/&lt;Release&gt;.log</code>.
      </div>
      <DirectoryBrowserModal
        open={isModalOpen}
        onOpenChange={setIsModalOpen}
        onSelect={(path) => updateSetting({ newPath: path })}
        heading={"Select logs folder"}
      />
    </div>
  );
};



