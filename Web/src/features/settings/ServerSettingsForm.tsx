import * as React from "react";
import { useState } from "react";
import { Input } from "@/components/ui/input.tsx";
import { Label } from "@/components/ui/label.tsx";
import { Button } from "@/components/ui/button.tsx";
import { DirectoryBrowserModal } from "./components/DirectoryBrowser/DirectoryBrowserModal.tsx";

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
  const [isModalOpen, setIsModalOpen] = useState(false);

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
          value={value.libraryPath}
          readOnly
        />
        <Button onClick={() => setIsModalOpen(true)}>Browse...</Button>
      </div>
      <DirectoryBrowserModal
        open={isModalOpen}
        onOpenChange={setIsModalOpen}
        onSelect={(path) => onValueChange({ ...value, libraryPath: path })}
      />
    </div>
  );
};
