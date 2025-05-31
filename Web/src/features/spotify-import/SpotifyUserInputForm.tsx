import React from "react";
import { TextInput } from "@/components/inputs/TextInput.tsx";

interface SpotifyUserInputFormProps {
  value: SpotifyUserInputFormModel;
  onValueChange: (value: SpotifyUserInputFormModel) => void;
}

export interface SpotifyUserInputFormModel {
  username: string;
}

export const SpotifyUserInputForm: React.FC<SpotifyUserInputFormProps> = ({
  value,
  onValueChange,
}) => {
  const inputId = "spotify-username-input";

  return (
    <>
      <label
        htmlFor={inputId}
        className="block text-sm font-medium text-gray-300 mb-1"
      >
        Enter Spotify username
      </label>
      <TextInput
        type="text"
        id={inputId}
        value={value.username}
        onChange={(e) => onValueChange({ username: e.target.value })}
        placeholder="Spotify username"
      />
    </>
  );
};
