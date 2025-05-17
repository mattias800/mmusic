/**
 * Formats album length in milliseconds to a string in the format "X min Y sec"
 * @param milliseconds Total length in milliseconds
 * @returns Formatted string
 */
export const formatAlbumLength = (milliseconds: number): string => {
  const totalSeconds = Math.floor(milliseconds / 1000);
  const minutes = Math.floor(totalSeconds / 60);
  const remainingSeconds = totalSeconds % 60;
  const paddedSeconds = remainingSeconds.toString().padStart(2, "0");

  return `${minutes} min ${paddedSeconds} sec`;
};
