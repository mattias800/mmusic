import React from "react";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Music, User } from "lucide-react";

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
    <div className="space-y-4">
      <div className="text-center">
        <div className="inline-flex items-center justify-center w-12 h-12 bg-gradient-to-br from-green-500/20 to-emerald-500/20 rounded-xl border border-green-500/30 mb-3">
          <User className="w-6 h-6 text-green-400" />
        </div>
        <Label
          htmlFor={inputId}
          className="text-lg font-semibold text-white block mb-2"
        >
          Spotify Username
        </Label>
        <p className="text-gray-400 text-sm mb-4">
          Enter your Spotify username or user ID to discover your playlists
        </p>
      </div>

      <div className="relative">
        <Music className="absolute left-4 top-1/2 transform -translate-y-1/2 w-5 h-5 text-green-400 pointer-events-none z-10" />
        <Input
          type="text"
          id={inputId}
          value={value.username}
          onChange={(e) => onValueChange({ username: e.target.value })}
          placeholder="Enter your Spotify username..."
          className="w-full pl-12 pr-4 py-4 bg-white/5 border-white/20 text-white placeholder-gray-400 focus:bg-white/10 focus:border-green-400/50 focus:ring-green-400/30 focus:ring-2 transition-all duration-200 rounded-2xl text-base backdrop-blur-sm"
        />
      </div>
    </div>
  );
};
