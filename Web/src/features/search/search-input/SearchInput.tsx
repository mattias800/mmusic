import * as React from "react";
import { useCallback, useEffect, useRef, useState } from "react";
import { SearchPanel } from "@/features/search/search-input/SearchPanel.tsx";
import { Search } from "lucide-react";
import { cn } from "@/lib/utils.ts";

export interface SearchInputProps {}

export const SearchInput: React.FC<SearchInputProps> = () => {
  const [value, setValue] = useState("");
  const [inFocus, setInFocus] = useState(false);
  const formRef = useRef<HTMLFormElement>(null);
  const popupRef = useRef<HTMLDivElement>(null);

  const onClickSearchResult = useCallback(() => {
    setTimeout(() => {
      setInFocus(false);
    }, 100);
  }, []);

  // Handle clicks outside the form and popup
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        formRef.current &&
        !formRef.current.contains(event.target as Node) &&
        popupRef.current &&
        !popupRef.current.contains(event.target as Node)
      ) {
        setInFocus(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, []);

  return (
    <div className="relative z-[9999]">
      <form ref={formRef} onFocus={() => setInFocus(true)}>
        <div
          className={cn(
            // IMPORTANT: no 'isolate' here — allow blending with the page/photo
            "relative w-full rounded-4xl overflow-hidden px-4",
            // Subtle frame
            "ring-1 ring-white/25 shadow-lg backdrop-blur-sm",
          )}
        >
          {/* Frosted glass layer */}
          <div
            aria-hidden
            className={cn(
              "pointer-events-none absolute inset-0",
              // Liquid glass feel
              "bg-white/10 backdrop-blur-xl backdrop-saturate-150",
              // Slight normalization helps mid-tones without killing the photo
              "backdrop-brightness-95 backdrop-contrast-110",
            )}
          />
          {/* Inner highlight (gives depth) */}
          <div
            aria-hidden
            className="pointer-events-none absolute inset-0 [mask-image:linear-gradient(#000,transparent)]"
            style={{ boxShadow: "inset 0 1px 0 0 rgba(255,255,255,0.35)" }}
          />

          <Search
            className={cn(
              "pointer-events-none absolute left-4 top-1/2 -translate-y-1/2 h-5 w-5 z-10",
              // Dynamic contrast
              "text-white mix-blend-difference",
            )}
          />

          <input
            type="text"
            value={value}
            onChange={(ev) => setValue(ev.target.value)}
            placeholder="Search your music library..."
            className={cn(
              "relative z-10 w-full min-w-0 pl-12 pr-3 bg-transparent border-0 outline-none",
              "text-base leading-6 py-2",
              // Dynamic contrast over the photo
              "text-white mix-blend-difference",
              // Cursor & selection
              "caret-white selection:bg-white/30 selection:text-black",
              // Smooth focus ring (kept inside)
              "focus-visible:ring-2 focus-visible:ring-white/30 rounded-3xl",
            )}
          />
        </div>
      </form>
      {value && inFocus && (
        <div ref={popupRef} className="mt-3 relative z-[9999]">
          <SearchPanel
            searchText={value}
            onClickSearchResult={onClickSearchResult}
          />
        </div>
      )}
    </div>
  );
};
