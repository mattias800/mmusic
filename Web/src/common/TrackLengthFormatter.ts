export const formatTrackLength = (length: number) => {
  // Length is in milliseconds, format as HH:MM:SS
  const totalSeconds = Math.floor(length / 1000);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;

  const padNumber = (num: number) => num.toString().padStart(2, "0");

  return hours > 0
    ? `${hours}:${padNumber(minutes)}:${padNumber(seconds)}`
    : `${minutes}:${padNumber(seconds)}`;
};
