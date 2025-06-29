import * as React from "react";
import { Input } from "@/components/ui/input.tsx";
import { Label } from "@/components/ui/label.tsx";

export interface ServerSettingsFormProps {
  value: ServerSettingsFormModel;
  onValueChange: (value: ServerSettingsFormModel) => void;
}

export interface ServerSettingsFormModel {
  libraryPath: string;
}

export const ServerSettingsForm: React.FC<ServerSettingsFormProps> = ({
  onValueChange,
  value,
}) => {
  return (
      <div>
        <Label
          htmlFor="library-path"
          style={{ marginBottom: "8px", display: "block" }}
        >
          Library path
        </Label>
        <div className={"w-xl"}>
          <Input
            id={"library-path"}
            value={value.libraryPath}
            onChange={(ev) =>
              onValueChange({ ...value, libraryPath: ev.target.value ?? "" })
            }
          />
        </div>
      </div>

  );
};
