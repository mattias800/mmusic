import * as React from "react";
import { useMutation, useQuery } from "urql";
import { UpdateDownloaderSettingsDocument, DownloadersTogglesCardDocument } from "@/features/admin/graphql/Downloaders.gql.ts";
import { GlassCard, GradientButton } from "@/components/ui";
import { Settings } from "lucide-react";

// Use generated documents

export const DownloadersTogglesCard: React.FC = () => {
  const [{ data, fetching }] = useQuery({ query: DownloadersTogglesCardDocument, requestPolicy: "network-only" });
  const [, update] = useMutation(UpdateDownloaderSettingsDocument);

  const [sab, setSab] = React.useState(false);
  const [qbit, setQbit] = React.useState(false);
  const [soul, setSoul] = React.useState(false);
  const [saving, setSaving] = React.useState(false);

  React.useEffect(() => {
    if (data?.serverSettings) {
      setSab(!!data.serverSettings.enableSabnzbdDownloader);
      setQbit(!!data.serverSettings.enableQBittorrentDownloader);
      setSoul(!!data.serverSettings.enableSoulSeekDownloader);
    }
  }, [data?.serverSettings]);

  const onSave = async () => {
    setSaving(true);
    const res = await update({
      input: {
        enableSabnzbdDownloader: sab,
        enableQBittorrentDownloader: qbit,
        enableSoulSeekDownloader: soul,
      },
    });
    setSaving(false);
    if (res.error) alert(res.error.message);
    // optimistic enough; query is network-only
  };

  return (
    <GlassCard title="Downloaders" icon={Settings} iconBgColor="bg-blue-500/20">
      <div className="space-y-4">
        <label className="flex items-center gap-3 text-gray-200">
          <input
            type="checkbox"
            checked={sab}
            onChange={(e) => setSab(e.target.checked)}
            disabled={fetching || saving}
          />
          Enable SABnzbd (Usenet)
        </label>
        <label className="flex items-center gap-3 text-gray-200">
          <input
            type="checkbox"
            checked={qbit}
            onChange={(e) => setQbit(e.target.checked)}
            disabled={fetching || saving}
          />
          Enable qBittorrent (Torrents)
        </label>
        <label className="flex items-center gap-3 text-gray-200">
          <input
            type="checkbox"
            checked={soul}
            onChange={(e) => setSoul(e.target.checked)}
            disabled={fetching || saving}
          />
          Enable Soulseek
        </label>
        <div>
          <GradientButton onClick={onSave} disabled={fetching || saving}>
            {saving ? "Saving..." : "Save"}
          </GradientButton>
        </div>
      </div>
    </GlassCard>
  );
};


