export const buildCoverArtPlaceholder = (title?: string): string => {
  const display = (title ?? "").trim();
  const initials =
    display
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 3)
      .map((w) => w[0]?.toUpperCase())
      .join("") || "♪";

  const svg = `<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="512" height="512" viewBox="0 0 512 512">
  <defs>
    <linearGradient id="g" x1="0" y1="0" x2="1" y2="1">
      <stop offset="0%" stop-color="#1f2937"/>
      <stop offset="100%" stop-color="#374151"/>
    </linearGradient>
  </defs>
  <rect width="512" height="512" fill="url(#g)"/>
  <g>
    <circle cx="380" cy="180" r="10" fill="#10b981"/>
    <rect x="180" y="200" width="16" height="130" rx="3" fill="#9ca3af"/>
    <rect x="196" y="200" width="8" height="60" rx="2" fill="#9ca3af"/>
    <circle cx="188" cy="345" r="28" fill="#111827" stroke="#9ca3af" stroke-width="4"/>
  </g>
  <text x="50%" y="66%" text-anchor="middle" fill="#e5e7eb" font-family="Inter, system-ui, -apple-system, Segoe UI, Roboto, Arial" font-size="96" font-weight="700">${initials}</text>
  ${display ? `<text x="50%" y="88%" text-anchor="middle" fill="#d1d5db" font-family="Inter, system-ui, -apple-system, Segoe UI, Roboto, Arial" font-size="28" font-weight="600">${escapeXml(display)}</text>` : ""}
</svg>`;
  return `data:image/svg+xml;utf8,${encodeURIComponent(svg)}`;
};

function escapeXml(unsafe: string): string {
  return unsafe
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&apos;");
}
