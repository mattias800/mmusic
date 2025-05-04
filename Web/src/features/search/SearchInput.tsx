import * as React from "react";
import { Input } from "@/components/ui/input.tsx";
import { useState, useRef, useEffect, useCallback } from "react";
import { SearchPanel } from "@/features/search/SearchPanel.tsx";

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
          placeholder={"Search..."}
          value={value}
          onChange={(ev) => setValue(ev.target.value)}
        />
      </form>
      {value && inFocus && (
        <div ref={popupRef}>
          <SearchPanel
            searchText={value}
            onClickSearchResult={onClickSearchResult}
          />
        </div>
      )}
    </div>
  );
};
