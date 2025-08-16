import * as React from "react";
import { useCallback, useEffect, useRef, useState } from "react";
import { Input } from "@/components/ui/input.tsx";
import { SearchPanel } from "@/features/search/search-input/SearchPanel.tsx";
import { Search } from "lucide-react";

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
        <div className="relative">
          <Search className="absolute left-4 top-1/2 transform -translate-y-1/2 w-5 h-5 text-gray-300 pointer-events-none z-10" />
          <Input
            placeholder="Search your music library..."
            value={value}
            onChange={(ev) => setValue(ev.target.value)}
            className="w-full pl-12 pr-4 py-3 bg-gray-800/60 border-white/20 text-white placeholder-gray-400 focus:bg-gray-800/80 focus:border-blue-400/60 focus:ring-blue-400/30 focus:ring-2 transition-all duration-200 rounded-2xl text-base backdrop-blur-sm"
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
