import * as React from "react";
import { Button } from "@/components/ui/button.tsx";
import { ReleaseCoverArt } from "@/components/images/ReleaseCoverArt.tsx";
import { Tag } from "@/components/text/Tag.tsx";

export interface ArtistMatchListItemProps {
  imageUrl?: string | null;
  name: string;
  disambiguation?: string | null;
  typeLabel?: string | null;
  country?: string | null;
  listeners?: number | string | null;
  summary?: string | null;
  selected?: boolean;
  disabled?: boolean;
  buttonLabel?: string;
  onSelect: () => void;
}

export const ArtistMatchListItem: React.FC<ArtistMatchListItemProps> = ({
  imageUrl,
  name,
  disambiguation,
  typeLabel,
  country,
  listeners,
  summary,
  selected,
  disabled,
  buttonLabel = "Select",
  onSelect,
}) => {
  const listenersText = React.useMemo(() => {
    if (listeners == null) return null;
    if (typeof listeners === "number") return listeners.toLocaleString();
    const asNumber = Number(listeners);
    if (!Number.isNaN(asNumber)) return asNumber.toLocaleString();
    return String(listeners);
  }, [listeners]);

  return (
    <div
      className={
        "flex items-center justify-between gap-3 rounded-md border p-2 transition-colors " +
        (selected
          ? "border-white/30 bg-white/[0.07]"
          : "border-white/10 hover:bg-white/5")
      }
    >
      <div className="flex items-center gap-3 min-w-0">
        <ReleaseCoverArt
          className="h-10 w-10 rounded"
          srcUrl={imageUrl ?? undefined}
          titleForPlaceholder={name}
          alt={name}
        />
        <div className="min-w-0">
          <div className={"flex items-center gap-2"}>
            <span className="font-medium truncate">
              {name} {selected ? "selected" : "not selected"}
            </span>
            {selected && <Tag variant={"info"}>Selected</Tag>}
          </div>
          {disambiguation && (
            <div className="text-[11px] text-white/60 truncate">
              {disambiguation}
            </div>
          )}
          {(typeLabel || country || listenersText) && (
            <div className="text-xs text-white/60 truncate">
              {(typeLabel ?? "").toString()} {country ? `• ${country}` : ""}
              {listenersText ? ` • ${listenersText} listeners` : ""}
            </div>
          )}
          {summary && (
            <div className="text-[11px] text-white/60 line-clamp-2">
              {summary}
            </div>
          )}
        </div>
      </div>
      <Button
        size="sm"
        className="shrink-0"
        onClick={onSelect}
        disabled={disabled}
      >
        {buttonLabel}
      </Button>
    </div>
  );
};
