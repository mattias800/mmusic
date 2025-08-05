import * as React from "react";
import { useState } from "react";
import { Input } from "@/components/ui/input.tsx";
import { Label } from "@/components/ui/label.tsx";
import { Button } from "@/components/ui/button.tsx";
import { DirectoryBrowserModal } from "./components/DirectoryBrowser/DirectoryBrowserModal.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";

export interface DownloadPathFormProps {
  serverSettings: FragmentType<typeof downloadPathFormServerSettingsFragment>;
}

const downloadPathFormServerSettingsFragment = graphql(`
  fragment DownloadPathForm_ServerSettings on ServerSettings {
    id
    downloadPath
  }
`);

const updateDownloadPathMutation = graphql(`
  mutation UpdateDownloadPath($newDownloadPath: String!) {
    updateDownloadPath(input: { newDownloadPath: $newDownloadPath }) {
      ... on UpdateDownloadPathSuccess {
        serverSettings {
          id
          downloadPath
        }
      }
    }
  }
`);

export const DownloadPathForm: React.FC<DownloadPathFormProps> = (props) => {
  const serverSettings = useFragment(
    downloadPathFormServerSettingsFragment,
    props.serverSettings,
  );

  const [isModalOpen, setIsModalOpen] = useState(false);

  const [{ fetching }, updateServerSettings] = useMutation(
    updateDownloadPathMutation,
  );

  return (
    <div>
      <Label
        htmlFor="download-path"
        style={{ marginBottom: "8px", display: "block" }}
      >
        Download path
      </Label>
      <div className={"flex w-xl items-center space-x-2"}>
        <Input
          id={"download-path"}
          value={serverSettings.downloadPath}
          readOnly
        />
        <Button
          loading={fetching}
          disabled={fetching}
          onClick={() => setIsModalOpen(true)}
        >
          Browse...
        </Button>
      </div>
      <DirectoryBrowserModal
        open={isModalOpen}
        onOpenChange={setIsModalOpen}
        onSelect={(path) => updateServerSettings({ newDownloadPath: path })}
        heading={"Select download folder"}
      />
    </div>
  );
};
