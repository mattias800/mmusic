import * as React from "react";
import { ServerSettingsPanel } from "@/features/settings/ServerSettingsPanel.tsx";

export interface SettingsPageProps {}

export const SettingsPage: React.FC<SettingsPageProps> = () => {
  return (
    <>
      <title>Settings</title>
      <ServerSettingsPanel />
    </>
  );
};
