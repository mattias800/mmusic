import * as React from "react";
import { useMutation, useQuery } from "urql";
import { GlassCard } from "@/components/ui";
import { Switch } from "@/components/ui/switch.tsx";
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip.tsx";
import {
  Cloud,
  HardDriveDownload,
  Magnet,
  RefreshCcw,
  Server,
  Settings,
} from "lucide-react";
import { graphql } from "@/gql";
import {
  downloadersTogglesCardQuery,
  updateDownloaderSettingsMutation,
} from "@/features/settings/DownloadersTogglesCardMutations.tsx";

const testSabnzbdConnectivityQuery = graphql(`
  query TestSabnzbdConnectivity {
    external {
      testSabnzbdConnectivity {
        ok
        message
      }
    }
  }
`);

const testProwlarrConnectivityQuery = graphql(`
  query TestProwlarrConnectivity {
    external {
      testProwlarrConnectivity {
        ok
        message
      }
    }
  }
`);

const testQBittorrentConnectivityQuery = graphql(`
  query TestQBittorrentConnectivity {
    external {
      testQBittorrentConnectivity {
        ok
        message
      }
    }
  }
`);

export const DownloadersTogglesCard: React.FC = () => {
  const [{ data, fetching }] = useQuery({
    query: downloadersTogglesCardQuery,
  });

  const [{ fetching: saving }, update] = useMutation(
    updateDownloaderSettingsMutation,
  );

  const [{ data: prow, fetching: prowLoading }, reexecProw] = useQuery({
    query: testProwlarrConnectivityQuery,
    pause: false,
    context: { requestPolicy: "network-only" },
  });

  const [{ data: qbitConn, fetching: qbitLoading }, reexecQbit] = useQuery({
    query: testQBittorrentConnectivityQuery,
    pause: false,
    context: { requestPolicy: "network-only" },
  });

  const [{ data: sabConn, fetching: sabLoading }, reexecSab] = useQuery({
    query: testSabnzbdConnectivityQuery,
    pause: false,
    context: { requestPolicy: "network-only" },
  });

  const serverSettings = data?.serverSettings;
  const sab = !!serverSettings?.enableSabnzbdDownloader;
  const qbit = !!serverSettings?.enableQBittorrentDownloader;
  const soul = !!serverSettings?.enableSoulSeekDownloader;

  const disabled = fetching || saving || !serverSettings;

  const toggleSab = async (next: boolean) => {
    const res = await update({
      input: {
        enableSabnzbdDownloader: next,
        enableQBittorrentDownloader: qbit,
        enableSoulSeekDownloader: soul,
      },
    });
    if (res.error) alert(res.error.message);
  };

  const toggleQbit = async (next: boolean) => {
    const res = await update({
      input: {
        enableSabnzbdDownloader: sab,
        enableQBittorrentDownloader: next,
        enableSoulSeekDownloader: soul,
      },
    });
    if (res.error) alert(res.error.message);
  };

  const toggleSoul = async (next: boolean) => {
    const res = await update({
      input: {
        enableSabnzbdDownloader: sab,
        enableQBittorrentDownloader: qbit,
        enableSoulSeekDownloader: next,
      },
    });
    if (res.error) alert(res.error.message);
  };

  return (
    <GlassCard title="Downloaders" icon={Settings} iconBgColor="bg-blue-500/20">
      <div className="space-y-4">
        <StatusRow
          icon={Server}
          title="Prowlarr"
          availability={deriveStatus(
            prowLoading,
            prow?.external?.testProwlarrConnectivity,
          )}
          onTest={() => reexecProw({ requestPolicy: "network-only" })}
        />
        <ServiceRow
          icon={HardDriveDownload}
          title="SABnzbd (Usenet)"
          enabled={sab}
          onToggle={toggleSab}
          disabled={disabled}
          availability={deriveStatus(
            sabLoading,
            sabConn?.external?.testSabnzbdConnectivity,
          )}
          onTest={() => reexecSab({ requestPolicy: "network-only" })}
        />
        <ServiceRow
          icon={Magnet}
          title="qBittorrent (Torrents)"
          enabled={qbit}
          onToggle={toggleQbit}
          disabled={disabled}
          availability={deriveStatus(
            qbitLoading,
            qbitConn?.external?.testQBittorrentConnectivity,
          )}
          onTest={() => reexecQbit({ requestPolicy: "network-only" })}
        />
        <ServiceRow
          icon={Cloud}
          title="Soulseek"
          enabled={soul}
          onToggle={toggleSoul}
          disabled={disabled}
          availability={{
            state: "info",
            message: "Status shown on Soulseek panel",
          }}
        />
      </div>
    </GlassCard>
  );
};

type Availability = {
  state: "ok" | "error" | "unknown" | "info";
  message?: string;
};

function deriveStatus(
  loading: boolean,
  payload?: { ok: boolean; message: string } | null,
): Availability {
  if (loading) return { state: "info", message: "Checking..." };
  if (!payload) return { state: "unknown", message: "Not tested" };
  if (payload.ok)
    return { state: "ok", message: payload.message || "Available" };
  return { state: "error", message: payload.message || "Unavailable" };
}

interface ServiceRowProps {
  icon: React.ComponentType<{ className?: string }>;
  title: string;
  enabled: boolean;
  onToggle: (v: boolean) => void;
  disabled?: boolean;
  availability: Availability;
  onTest?: () => void;
}

const ServiceRow: React.FC<ServiceRowProps> = ({
  icon: Icon,
  title,
  enabled,
  onToggle,
  disabled,
  availability,
  onTest,
}) => {
  const pill =
    availability.state === "ok"
      ? "bg-green-500/20 text-green-300 border-green-500/30"
      : availability.state === "error"
        ? "bg-red-500/20 text-red-300 border-red-500/30"
        : availability.state === "info"
          ? "bg-blue-500/20 text-blue-300 border-blue-500/30"
          : "bg-gray-500/20 text-gray-300 border-gray-500/30";

  return (
    <div
      className={
        "flex items-center justify-between rounded-lg border px-3 py-2 " +
        (enabled
          ? "border-green-500/30 bg-green-500/5"
          : "border-white/10 bg-white/5")
      }
    >
      <div className="flex items-center gap-3">
        <div
          className={
            "w-8 h-8 rounded-md flex items-center justify-center " +
            (enabled ? "bg-green-500/20" : "bg-white/10")
          }
        >
          <Icon
            className={
              "w-4 h-4 " + (enabled ? "text-green-300" : "text-gray-300")
            }
          />
        </div>
        <div>
          <div className="text-white text-sm font-medium">{title}</div>
          {availability.message && (
            <Tooltip>
              <TooltipTrigger asChild>
                <div
                  className={`inline-flex items-center gap-2 mt-1 text-[11px] px-2 py-0.5 rounded-full border ${pill}`}
                >
                  {availability.message}
                  {onTest && (
                    <button
                      type="button"
                      title="Test now"
                      onClick={() => onTest()}
                      className="ml-1 inline-flex items-center text-xs text-white/80 hover:text-white"
                    >
                      <RefreshCcw className="w-3 h-3" />
                    </button>
                  )}
                </div>
              </TooltipTrigger>
              <TooltipContent sideOffset={8}>
                {availability.message}
              </TooltipContent>
            </Tooltip>
          )}
        </div>
      </div>
      <Switch
        checked={enabled}
        onCheckedChange={onToggle}
        disabled={disabled}
        ariaLabel={`Enable ${title}`}
      />
    </div>
  );
};

interface StatusRowProps {
  icon: React.ComponentType<{ className?: string }>;
  title: string;
  availability: Availability;
  onTest?: () => void;
}

const StatusRow: React.FC<StatusRowProps> = ({
  icon: Icon,
  title,
  availability,
  onTest,
}) => {
  const pill =
    availability.state === "ok"
      ? "bg-green-500/20 text-green-300 border-green-500/30"
      : availability.state === "error"
        ? "bg-red-500/20 text-red-300 border-red-500/30"
        : availability.state === "info"
          ? "bg-blue-500/20 text-blue-300 border-blue-500/30"
          : "bg-gray-500/20 text-gray-300 border-gray-500/30";

  return (
    <div className="flex items-center justify-between rounded-lg border px-3 py-2 border-white/10 bg-white/5">
      <div className="flex items-center gap-3">
        <div className="w-8 h-8 rounded-md flex items-center justify-center bg-white/10">
          <Icon className="w-4 h-4 text-gray-300" />
        </div>
        <div>
          <div className="text-white text-sm font-medium">{title}</div>
          {availability.message && (
            <Tooltip>
              <TooltipTrigger asChild>
                <div
                  className={`inline-flex items-center gap-2 mt-1 text-[11px] px-2 py-0.5 rounded-full border ${pill}`}
                >
                  {availability.message}
                  {onTest && (
                    <button
                      type="button"
                      title="Test now"
                      onClick={() => onTest()}
                      className="ml-1 inline-flex items-center text-xs text-white/80 hover:text-white"
                    >
                      <RefreshCcw className="w-3 h-3" />
                    </button>
                  )}
                </div>
              </TooltipTrigger>
              <TooltipContent sideOffset={8}>
                {availability.message}
              </TooltipContent>
            </Tooltip>
          )}
        </div>
      </div>
      <div />
    </div>
  );
};
