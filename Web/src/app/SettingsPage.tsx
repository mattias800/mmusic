import * as React from "react";
import { ServerSettingsPanel } from "@/features/settings/ServerSettingsPanel.tsx";
import { useQuery } from "urql";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";
import { graphql } from "@/gql";
import { ServerSettingsFormContainer } from "@/features/settings/ServerSettingsFormContainer.tsx";
import { MainPadding } from "@/components/layout/MainPadding.tsx";

export interface SettingsPageProps {}

export const settingsPageQuery = graphql(`
  query SettingsPage {
    serverSettings {
      ...ServerSettingsFormContainer_ServerSettings
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
        <ServerSettingsFormContainer serverSettings={data.serverSettings} />
      </ServerSettingsPanel>
    </MainPadding>
  );
};
