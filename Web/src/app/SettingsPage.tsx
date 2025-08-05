import * as React from "react";
import { ServerSettingsPanel } from "@/features/settings/ServerSettingsPanel.tsx";
import { useQuery } from "urql";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";
import { graphql } from "@/gql";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { LibraryPathForm } from "@/features/settings/LibraryPathForm.tsx";
import { DownloadPathForm } from "@/features/settings/DownloadPathForm.tsx";

export interface SettingsPageProps {}

const settingsPageQuery = graphql(`
  query SettingsPage {
    serverSettings {
      ...LibraryPathForm_ServerSettings
      ...DownloadPathForm_ServerSettings
    }
  }
`);

export const SettingsPage: React.FC<SettingsPageProps> = () => {
  const [{ error, data, fetching, stale }] = useQuery({
    query: settingsPageQuery,
  });
  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;
  if (!data?.serverSettings) return <div>No data</div>;

  return (
    <MainPadding>
      <title>Settings</title>
      <ServerSettingsPanel>
        <LibraryPathForm serverSettings={data.serverSettings} />
        <DownloadPathForm serverSettings={data.serverSettings} />
      </ServerSettingsPanel>
    </MainPadding>
  );
};
