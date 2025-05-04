import * as React from "react";

export const PlaylistHeader: React.FC = () => {
  return (
    <div className="grid grid-cols-[40px_48px_1fr_1fr_150px_48px] gap-4 items-center px-4 py-2 text-sm text-neutral-400 uppercase tracking-wider border-b border-neutral-700">
      <span>#</span>
      <span></span>
      <span>Title</span>
      <span className="hidden md:block">Album</span>
      <span className="hidden sm:inline">Added</span>
      <span className="text-right">Time</span>
    </div>
  );
};
