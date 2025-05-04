interface PopularTrackProps {
  index: number;
  track: {
    title: string;
    plays: string;
    duration: string;
    active?: boolean;
  };
}

export const PopularTrackRow: React.FC<PopularTrackProps> = ({
  index,
  track,
}) => {
  return (
    <div
      className={`grid grid-cols-[40px_1fr_150px_50px] items-center px-4 py-2 rounded hover:bg-neutral-800 ${
        track.active ? "text-green-400 font-semibold" : "text-white"
      }`}
    >
      <span>{track.active ? "▶" : index}</span>
      <span className="truncate">{track.title}</span>
      <span className="text-sm text-neutral-400 text-right">{track.plays}</span>
      <span className="text-sm text-neutral-400 text-right">
        {track.duration}
      </span>
    </div>
  );
};
