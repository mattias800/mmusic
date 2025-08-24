export const formatBytes = (bytes: number): string => {
  const units = ["B", "KB", "MB", "GB", "TB", "PB"] as const;
  let i = 0;
  let n = bytes;
  while (n >= 1024 && i < units.length - 1) {
    n /= 1024;
    i++;
  }
  const formatted = n >= 100 ? Math.round(n).toString() : n.toFixed(1);
  return `${formatted} ${units[i]}`;
};
