import * as React from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import { Alert } from "@/components/ui/Alert.tsx";
import { DiskUsagePanel } from "./components/DiskUsagePanel.tsx";
import { ChangeLibraryFolderControl } from "./components/ChangeLibraryFolderControl.tsx";

export interface LibraryPathFormProps {
  serverSettings: FragmentType<typeof libraryPathFormServerSettingsFragment>;
}

const libraryPathFormServerSettingsFragment = graphql(`
  fragment LibraryPathForm_ServerSettings on ServerSettings {
    id
    ...ChangeLibraryFolderControl_ServerSettings
    libraryPath
    storageStats {
      totalDiskBytes
      availableFreeBytes
      librarySizeBytes
      estimatedTotalLibrarySizeBytes
    }
  }
`);

export const LibraryPathForm: React.FC<LibraryPathFormProps> = (props) => {
  const serverSettings = useFragment(
    libraryPathFormServerSettingsFragment,
    props.serverSettings,
  );

  return (
    <div className="space-y-8">
      <ChangeLibraryFolderControl serverSettings={serverSettings} />

      {serverSettings.storageStats && (
        <DiskUsagePanel
          totalBytes={serverSettings.storageStats.totalDiskBytes ?? undefined}
          freeBytes={serverSettings.storageStats.availableFreeBytes ?? undefined}
          libraryBytes={serverSettings.storageStats.librarySizeBytes ?? 0}
          estimatedTotalLibraryBytes={serverSettings.storageStats.estimatedTotalLibrarySizeBytes ?? 0}
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
