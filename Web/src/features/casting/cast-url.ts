export const getCastAbsoluteUrl = (
  relativePath: string,
  configuredBase?: string | null,
): string => {
  const base =
    configuredBase && configuredBase.length > 0
      ? configuredBase
      : window.location.origin;
  return new URL(relativePath, base).toString();
};
