import * as React from "react";
import { useEffect, useState } from "react";
import { debounce } from "lodash-es";
import { SearchResult } from "@/features/search/search-result-popover/SearchResult.tsx";

export interface SearchPanelProps {
  searchText: string;
  onClickSearchResult: () => void;
}

export const SearchPanel: React.FC<SearchPanelProps> = ({
  searchText,
  onClickSearchResult,
}) => {
  const [debouncedText, setDebouncedText] = useState(searchText);

  useEffect(() => {
    const debouncedUpdate = debounce(() => {
      setDebouncedText(searchText);
    }, 1000);

    debouncedUpdate();

    return () => {
      debouncedUpdate.cancel();
    };
  }, [searchText]);

  return (
    <div className={"relative"}>
      <div className={"absolute z-20"}>
        {debouncedText.length > 2 && (
          <SearchResult
            onClickSearchResult={onClickSearchResult}
            searchText={debouncedText}
          />
        )}
      </div>
    </div>
  );
};
