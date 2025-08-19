import React from "react";
import { useMutation } from "urql";
import { graphql } from "@/gql";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useState } from "react";
import { GradientButton } from "@/components/ui";
import { Download, AlertTriangle } from "lucide-react";

const updateDownloadSlotCountMutation = graphql(`
  mutation UpdateDownloadSlotCount($input: UpdateDownloadSlotCountInput!) {
    updateDownloadSlotCount(input: $input) {
      ... on UpdateDownloadSlotCountSuccess {
        serverSettings {
          downloadSlotCount
        }
      }
      ... on UpdateDownloadSlotCountError {
        message
      }
    }
  }
`);

interface DownloadSlotSettingsFormProps {
  serverSettings: {
    downloadSlotCount: number;
  };
}

export const DownloadSlotSettingsForm: React.FC<
  DownloadSlotSettingsFormProps
> = ({ serverSettings }) => {
  const [slotCount, setSlotCount] = useState(serverSettings.downloadSlotCount);
  const [isUpdating, setIsUpdating] = useState(false);

  const [, updateDownloadSlotCount] = useMutation(
    updateDownloadSlotCountMutation,
  );

  const handleSave = async () => {
    if (slotCount < 1 || slotCount > 10) {
      alert("Download slot count must be between 1 and 10");
      return;
    }

    setIsUpdating(true);
    try {
      const result = await updateDownloadSlotCount({
        input: { newSlotCount: slotCount },
      });

      if (result.error) {
        alert(`Error updating download slot count: ${result.error.message}`);
      } else if (
        result.data?.updateDownloadSlotCount.__typename ===
        "UpdateDownloadSlotCountError"
      ) {
        alert(`Error: ${result.data.updateDownloadSlotCount.message}`);
      } else {
        // Success - the query will automatically refetch
        alert("Download slot count updated successfully!");
      }
    } catch (error) {
      alert(`Error updating download slot count: ${error}`);
    } finally {
      setIsUpdating(false);
    }
  };

  const handleReset = () => {
    setSlotCount(serverSettings.downloadSlotCount);
  };

  const hasChanges = slotCount !== serverSettings.downloadSlotCount;

  return (
    <div className="space-y-6">
      {/* Description */}
      <div className="p-4 bg-white/5 rounded-lg border border-white/10">
        <div className="flex items-center gap-3 mb-3">
          <div className="p-2 bg-green-500/20 rounded-lg">
            <Download className="w-5 h-5 text-green-400" />
          </div>
          <h3 className="text-lg font-semibold text-white">
            Download Configuration
          </h3>
        </div>
        <p className="text-gray-300 text-sm">
          Configure the number of concurrent downloads that can run
          simultaneously. Each slot can download one release at a time.
        </p>
      </div>

      {/* Settings Form */}
      <div className="space-y-4">
        <div>
          <Label
            htmlFor="slotCount"
            className="text-gray-200 text-sm font-medium"
          >
            Number of Download Slots
          </Label>
          <div className="mt-2 flex items-center gap-3">
            <Input
              id="slotCount"
              type="number"
              min="1"
              max="10"
              value={slotCount}
              onChange={(e) => setSlotCount(parseInt(e.target.value) || 1)}
              className="w-24 bg-white/10 border-white/20 text-white placeholder-gray-400 focus:border-green-400 focus:ring-green-400"
            />
            <span className="text-sm text-gray-400">slots (1-10)</span>
          </div>
          <p className="mt-2 text-xs text-gray-400">
            More slots mean faster downloads but higher resource usage. Current
            setting: {serverSettings.downloadSlotCount} slots
          </p>
        </div>

        {/* Action Buttons */}
        <div className="flex gap-3">
          <GradientButton
            onClick={handleSave}
            disabled={!hasChanges || isUpdating}
            variant="success"
            className="flex-1"
          >
            {isUpdating ? "Updating..." : "Save Changes"}
          </GradientButton>
          {hasChanges && (
            <Button
              variant="outline"
              onClick={handleReset}
              disabled={isUpdating}
              className="border-white/20 text-white hover:bg-white/10 hover:border-white/30"
            >
              Reset
            </Button>
          )}
        </div>

        {/* Warning Message */}
        {hasChanges && (
          <div className="p-3 bg-gradient-to-r from-amber-500/10 to-orange-500/10 border border-amber-500/20 rounded-lg">
            <div className="flex items-center gap-2">
              <AlertTriangle className="w-4 h-4 text-amber-400" />
              <span className="text-sm text-amber-200">
                Changes will take effect immediately. Active downloads may be
                affected.
              </span>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
