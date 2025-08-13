import * as React from "react";
import { useCallback } from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { useMutation } from "urql";
import { Alert } from "@/components/ui/Alert.tsx";
import { DiskUsagePanel } from "./components/DiskUsagePanel.tsx";
import { ChangeLibraryFolderControl } from "./components/ChangeLibraryFolderControl.tsx";

export interface LibraryPathFormProps {
  serverSettings: FragmentType<typeof libraryPathFormServerSettingsFragment>;
}

const libraryPathFormServerSettingsFragment = graphql(`
  fragment LibraryPathForm_ServerSettings on ServerSettings {
    id
    libraryPath
    storageStats {
      totalDiskBytes
      availableFreeBytes
      librarySizeBytes
    }
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

  const [{ fetching }, updateServerSettings] = useMutation(
    updateLibraryPathMutation,
  );

  const onPathChanged = useCallback(
    async (path: string) => {
      await updateServerSettings({ newLibraryPath: path });
    },
    [updateServerSettings],
  );

  return (
    <div className="flex flex-col gap-12">
      <ChangeLibraryFolderControl
        currentPath={serverSettings.libraryPath}
        loading={fetching}
        onPathChanged={onPathChanged}
      />

      {serverSettings.storageStats && (
        <DiskUsagePanel
          totalBytes={serverSettings.storageStats.totalDiskBytes ?? undefined}
          freeBytes={
            serverSettings.storageStats.availableFreeBytes ?? undefined
          }
          libraryBytes={serverSettings.storageStats.librarySizeBytes}
        />
      )}

      {!serverSettings.libraryPath && (
        <Alert variant="warning" title="Library path not set">
          Please set a library path to enable downloading and scanning.
        </Alert>
      )}
    </div>
  );
};
