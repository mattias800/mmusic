import * as React from "react";
import { buildCoverArtPlaceholder } from "@/components/images/placeholderCoverArt.ts";

export interface ReleaseCoverArtProps
  extends React.ImgHTMLAttributes<HTMLImageElement> {
  // Primary cover art URL if available
  srcUrl?: string | null | undefined;
  // Optional artist thumbnail URL to use as fallback
  artistThumbUrl?: string | null | undefined;
  // Title used to generate placeholder SVG when no image is available or when all fallbacks fail
  titleForPlaceholder: string;
  // Optional className for the wrapper img element
  className?: string;
}

/**
 * ReleaseCoverArt
 * Renders release cover art with fallbacks:
 * 1) srcUrl (explicit cover art)
 * 2) artistThumbUrl (artist thumbnail)
 * 3) generated SVG placeholder based on title
 */
export const ReleaseCoverArt: React.FC<ReleaseCoverArtProps> = ({
  srcUrl,
  artistThumbUrl,
  titleForPlaceholder,
  alt,
  className,
  ...imgProps
}) => {
  const [url, setUrl] = React.useState<string | undefined>(() => {
    return (
      (srcUrl ?? undefined) ||
      (artistThumbUrl ?? undefined) ||
      buildCoverArtPlaceholder(titleForPlaceholder)
    );
  });

  const onError = React.useCallback(
    (e: React.SyntheticEvent<HTMLImageElement, Event>) => {
      const target = e.currentTarget as HTMLImageElement;

      // First fallback: try artist thumbnail if we haven't tried it yet and it's available
      if (!target.dataset.artistTried && artistThumbUrl) {
        target.dataset.artistTried = "1";
        setUrl(artistThumbUrl);
        return;
      }

      // Final fallback: placeholder
      if (!target.dataset.placeholderTried) {
        target.dataset.placeholderTried = "1";
        setUrl(buildCoverArtPlaceholder(titleForPlaceholder));
        return;
      }

      // Prevent infinite loops if even placeholder fails
    },
    [artistThumbUrl, titleForPlaceholder],
  );

  return (
    <img
      src={url}
      alt={alt}
      className={className}
      onError={onError}
      {...imgProps}
    />
  );
};
