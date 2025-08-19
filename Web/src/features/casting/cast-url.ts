export const getCastAbsoluteUrl = (relativePath: string): string => {
  const env = (import.meta as unknown as { env?: Record<string, string> }).env;
  const configuredBase = env?.VITE_SERVER_BASE_URL as string | undefined;
  const base = configuredBase || window.location.origin;
  return new URL(relativePath, base).toString();
};


