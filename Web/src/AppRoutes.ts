export const getRouteToRelease = (
  artistId: string,
  releaseFolderName: string,
): string => `/artist/${artistId}/release/${releaseFolderName}`;

export const getRouteToArtist = (artistId: string): string =>
  `/artist/${artistId}`;
