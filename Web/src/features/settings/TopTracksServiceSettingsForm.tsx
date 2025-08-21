import * as React from "react";
import { useQuery, useMutation } from "urql";
import { graphql } from "@/gql";
import { Button } from "@/components/ui/button";
import { Alert } from "@/components/ui/Alert";
import { Music } from "lucide-react";
import { toast } from "sonner";
import { Switch } from "@/components/ui/switch.tsx";

const topTracksServiceSettingsQuery = graphql(`
  query TopTracksServiceSettings {
    serverSettings {
      id
      listenBrainzTopTracksEnabled
      spotifyTopTracksEnabled
      lastFmTopTracksEnabled
    }
  }
`);

const updateTopTracksServiceSettingsMutation = graphql(`
  mutation UpdateTopTracksServiceSettings(
    $input: UpdateTopTracksServiceSettingsInput!
  ) {
    updateTopTracksServiceSettings(input: $input) {
      ... on UpdateTopTracksServiceSettingsSuccess {
        success
        message
      }
      ... on UpdateTopTracksServiceSettingsError {
        message
      }
    }
  }
`);

export const TopTracksServiceSettingsForm: React.FC = () => {
  const [listenBrainzEnabled, setListenBrainzEnabled] = React.useState(false);
  const [spotifyEnabled, setSpotifyEnabled] = React.useState(false);
  const [lastFmEnabled, setLastFmEnabled] = React.useState(false);
  const [isUpdating, setIsUpdating] = React.useState(false);

  const [{ data, fetching }] = useQuery({
    query: topTracksServiceSettingsQuery,
  });

  const [, updateSettings] = useMutation(
    updateTopTracksServiceSettingsMutation,
  );

  React.useEffect(() => {
    if (data?.serverSettings) {
      setListenBrainzEnabled(data.serverSettings.listenBrainzTopTracksEnabled);
      setSpotifyEnabled(data.serverSettings.spotifyTopTracksEnabled);
      setLastFmEnabled(data.serverSettings.lastFmTopTracksEnabled);
    }
  }, [data]);

  const handleSave = async () => {
    setIsUpdating(true);
    try {
      const result = await updateSettings({
        input: {
          listenBrainzTopTracksEnabled: listenBrainzEnabled,
          spotifyTopTracksEnabled: spotifyEnabled,
          lastFmTopTracksEnabled: lastFmEnabled,
        },
      });

      if (result.data?.updateTopTracksServiceSettings) {
        const resultData = result.data.updateTopTracksServiceSettings;
        if ("success" in resultData && resultData.success) {
          toast.success(
            resultData.message ||
              "Top tracks service settings updated successfully",
          );
        } else if ("message" in resultData) {
          toast.error(resultData.message);
        }
      }
    } catch {
      toast.error("Failed to update top tracks service settings");
    } finally {
      setIsUpdating(false);
    }
  };

  if (fetching) {
    return <div>Loading...</div>;
  }

  return (
    <div className="space-y-6">
      <div>
        <h3 className="text-lg font-semibold text-white mb-2">
          Top Tracks Data Sources
        </h3>
        <p className="text-sm text-gray-400">
          Configure which services to use for fetching top tracks when importing
          artists. At least one service should be enabled for top tracks
          functionality.
        </p>
      </div>

      <div className="space-y-4">
        {/* ListenBrainz */}
        <div className="bg-gray-800/50 border border-gray-700 rounded-lg p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-3">
              <div className="w-10 h-10 bg-orange-500/20 rounded-lg flex items-center justify-center">
                <Music className="w-5 h-5 text-orange-400" />
              </div>
              <div>
                <h4 className="text-white text-base font-medium">
                  ListenBrainz
                </h4>
                <p className="text-gray-400 text-sm">
                  Community-driven music listening database
                </p>
              </div>
            </div>
            <label className="flex items-center cursor-pointer">
              <Switch checked={listenBrainzEnabled} onCheckedChange={setListenBrainzEnabled} ariaLabel="Enable ListenBrainz" />
            </label>
          </div>
        </div>

        {/* Spotify */}
        <div className="bg-gray-800/50 border border-gray-700 rounded-lg p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-3">
              <div className="w-10 h-10 bg-green-500/20 rounded-lg flex items-center justify-center">
                <Music className="w-5 h-5 text-green-400" />
              </div>
              <div>
                <h4 className="text-white text-base font-medium">Spotify</h4>
                <p className="text-gray-400 text-sm">
                  Popular music streaming platform data
                </p>
              </div>
            </div>
            <label className="flex items-center cursor-pointer">
              <Switch checked={spotifyEnabled} onCheckedChange={setSpotifyEnabled} ariaLabel="Enable Spotify" />
            </label>
          </div>
        </div>

        {/* Last.fm */}
        <div className="bg-gray-800/50 border border-gray-700 rounded-lg p-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center space-x-3">
              <div className="w-10 h-10 bg-red-500/20 rounded-lg flex items-center justify-center">
                <Music className="w-5 h-5 text-red-400" />
              </div>
              <div>
                <h4 className="text-white text-base font-medium">Last.fm</h4>
                <p className="text-gray-400 text-sm">
                  Music discovery and recommendation service
                </p>
              </div>
            </div>
            <label className="flex items-center cursor-pointer">
              <Switch checked={lastFmEnabled} onCheckedChange={setLastFmEnabled} ariaLabel="Enable Last.fm" />
            </label>
          </div>
        </div>
      </div>

      {/* Warning if no services enabled */}
      {!listenBrainzEnabled && !spotifyEnabled && !lastFmEnabled && (
        <Alert variant="warning" title="No services enabled">
          No top tracks services are enabled. Top tracks functionality will not
          work.
        </Alert>
      )}

      {/* Save Button */}
      <div className="flex justify-end">
        <Button
          onClick={handleSave}
          disabled={isUpdating}
          className="bg-blue-600 hover:bg-blue-700 text-white"
        >
          {isUpdating ? "Saving..." : "Save Settings"}
        </Button>
      </div>
    </div>
  );
};
