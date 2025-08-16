import * as React from "react";
import { useCallback, useEffect, useRef, useState } from "react";
import { Input } from "@/components/ui/input.tsx";
import { SearchPanel } from "@/features/search/search-input/SearchPanel.tsx";

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
    <div>
      <form ref={formRef} onFocus={() => setInFocus(true)}>
        <Input
          placeholder="Search your music library..."
          value={value}
          onChange={(ev) => setValue(ev.target.value)}
          className="bg-white/10 border-white/20 text-white placeholder-gray-400 focus:border-blue-400 focus:ring-blue-400 transition-all duration-200"
        />
      </form>
      {value && inFocus && (
        <div ref={popupRef} className="mt-2">
          <SearchPanel
            searchText={value}
            onClickSearchResult={onClickSearchResult}
          />
        </div>
      )}
    </div>
  );
};
