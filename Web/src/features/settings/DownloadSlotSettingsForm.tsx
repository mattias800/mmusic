import React from "react";
import { useMutation } from "urql";
import { graphql } from "@/gql";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useState } from "react";

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

export const DownloadSlotSettingsForm: React.FC<DownloadSlotSettingsFormProps> = ({ serverSettings }) => {
  const [slotCount, setSlotCount] = useState(serverSettings.downloadSlotCount);
  const [isUpdating, setIsUpdating] = useState(false);

  const [, updateDownloadSlotCount] = useMutation(updateDownloadSlotCountMutation);

  const handleSave = async () => {
    if (slotCount < 1 || slotCount > 10) {
      alert("Download slot count must be between 1 and 10");
      return;
    }

    setIsUpdating(true);
    try {
      const result = await updateDownloadSlotCount({
        input: { newSlotCount: slotCount }
      });

      if (result.error) {
        alert(`Error updating download slot count: ${result.error.message}`);
      } else if (result.data?.updateDownloadSlotCount.__typename === "UpdateDownloadSlotCountError") {
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
    <div className="flex flex-col gap-12">
      <div className="space-y-4">
        <div>
          <h3 className="text-lg font-semibold">Download Slots</h3>
          <p className="text-sm text-zinc-500">
            Configure the number of concurrent downloads that can run simultaneously.
          </p>
        </div>
        
        <div className="space-y-2">
          <Label htmlFor="slotCount">Number of Download Slots</Label>
          <div className="flex items-center gap-2">
            <Input
              id="slotCount"
              type="number"
              min="1"
              max="10"
              value={slotCount}
              onChange={(e) => setSlotCount(parseInt(e.target.value) || 1)}
              className="w-24"
            />
            <span className="text-sm text-zinc-500">slots (1-10)</span>
          </div>
          <p className="text-xs text-zinc-500">
            Each slot can download one release at a time. More slots mean faster downloads but higher resource usage.
          </p>
        </div>

        <div className="flex gap-2">
          <Button
            onClick={handleSave}
            disabled={!hasChanges || isUpdating}
            className="bg-blue-600 hover:bg-blue-700"
          >
            {isUpdating ? "Updating..." : "Save Changes"}
          </Button>
          {hasChanges && (
            <Button
              variant="outline"
              onClick={handleReset}
              disabled={isUpdating}
            >
              Reset
            </Button>
          )}
        </div>

        {hasChanges && (
          <div className="text-sm text-blue-500">
            ⚠️ Changes will take effect immediately. Active downloads may be affected.
          </div>
        )}
      </div>
    </div>
  );
};
