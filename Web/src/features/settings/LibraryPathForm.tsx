import * as React from "react";
import { useState } from "react";
import { Input } from "@/components/ui/input.tsx";
import { Label } from "@/components/ui/label.tsx";
import { Button } from "@/components/ui/button.tsx";
import { DirectoryBrowserModal } from "./components/DirectoryBrowser/DirectoryBrowserModal.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";

export interface LibraryPathFormProps {
  serverSettings: FragmentType<typeof libraryPathFormServerSettingsFragment>;
}

const libraryPathFormServerSettingsFragment = graphql(`
  fragment LibraryPathForm_ServerSettings on ServerSettings {
    id
    libraryPath
  }
`);

const updateLibraryPathMutation = graphql(`
  mutation UpdateLibraryPath($newLibraryPath: String!) {
    updateLibraryPath(input: { newLibraryPath: $newLibraryPath }) {
      ... on UpdateLibraryPathSuccess {
        serverSettings {
          id
          libraryPath
        }
      }
    }
  }
`);

export const LibraryPathForm: React.FC<LibraryPathFormProps> = (props) => {
  const serverSettings = useFragment(
    libraryPathFormServerSettingsFragment,
    props.serverSettings,
  );

  const [isModalOpen, setIsModalOpen] = useState(false);

  const [{ fetching }, updateServerSettings] = useMutation(
    updateLibraryPathMutation,
  );

  return (
    <div>
      <Label
        htmlFor="library-path"
        style={{ marginBottom: "8px", display: "block" }}
      >
        Library path
      </Label>
      <div className={"flex w-xl items-center space-x-2"}>
        <Input
          id={"library-path"}
          value={serverSettings.libraryPath}
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
        onSelect={(path) => updateServerSettings({ newLibraryPath: path })}
        heading={"Select library folder"}
      />
    </div>
  );
};
