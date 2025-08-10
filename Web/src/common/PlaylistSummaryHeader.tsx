import React from "react";

export interface PlaylistSummaryHeaderProps {
  title: string;
  description?: string | null;
  coverImageUrl?: string | null;
  trackCount: number;
  totalMinutes?: number;
  actions?: React.ReactNode;
}

export const PlaylistSummaryHeader: React.FC<PlaylistSummaryHeaderProps> = ({
  title,
  description,
  coverImageUrl,
  trackCount,
  totalMinutes,
  actions,
}) => {
  return (
    <div className="flex items-center gap-6">
      {coverImageUrl ? (
        <img src={coverImageUrl} alt={title} className="h-32 w-32 rounded shadow" />
      ) : null}
      <div className="flex flex-col gap-2">
        <h1 className="text-2xl font-bold">{title}</h1>
        {description ? (
          <p className="text-white/60 max-w-xl">{description}</p>
        ) : null}
        <div className="text-sm text-white/60">
          {trackCount} tracks{totalMinutes != null ? ` • ${Math.round(totalMinutes)} min` : ""}
        </div>
        {actions ? <div className="flex gap-2 mt-2">{actions}</div> : null}
      </div>
    </div>
  );
};


