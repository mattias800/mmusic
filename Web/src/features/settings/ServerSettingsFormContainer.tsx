import * as React from "react";
import { useCallback, useState } from "react";
import { FragmentType, graphql, useFragment } from "@/gql";
import {
  ServerSettingsForm,
  ServerSettingsFormModel,
} from "@/features/settings/ServerSettingsForm.tsx";
import { useMutation } from "urql";
import { Button } from "@/components/ui/button.tsx";

export interface ServerSettingsFormContainerProps {
  serverSettings: FragmentType<
    typeof serverSettingsFormContainerServerSettingsFragment
  >;
}

const serverSettingsFormContainerServerSettingsFragment = graphql(`
  fragment ServerSettingsFormContainer_ServerSettings on ServerSettings {
    id
    libraryPath
  }
`);

const updateServerSettingsMutation = graphql(`
  mutation UpdateServerSettings($newLibraryPath: String!) {
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

export const ServerSettingsFormContainer: React.FC<
  ServerSettingsFormContainerProps
> = (props) => {
  const serverSettings = useFragment(
    serverSettingsFormContainerServerSettingsFragment,
    props.serverSettings,
  );

  const [model, setModel] = useState<ServerSettingsFormModel>({
    libraryPath: serverSettings.libraryPath,
  });

  const [{ fetching }, updateServerSettings] = useMutation(
    updateServerSettingsMutation,
  );

  const onSubmit = useCallback(async () => {
    await updateServerSettings({ newLibraryPath: model.libraryPath });
  }, [model.libraryPath, updateServerSettings]);

  return (
    <div className={"flex flex-col gap-4"}>
      <ServerSettingsForm value={model} onValueChange={setModel} />
      <div>
        <Button onClick={onSubmit} loading={fetching}>
          Save
        </Button>
      </div>
    </div>
  );
};
