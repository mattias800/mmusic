import * as React from "react";
import { SearchResultArtist } from "./SearchResultArtist";
import { SearchResultReleaseGroup } from "./SearchResultReleaseGroup";
import { SearchResultRecording } from "./SearchResultRecording";

export interface SearchResultProps {
  searchText: string;
  onClickSearchResult: () => void;
}

export const SearchResult: React.FC<SearchResultProps> = ({
  searchText,
  onClickSearchResult,
}) => {
  return (
    <div className="w-96 rounded-2xl bg-black shadow-lg border border-white/10 max-h-[80vh] overflow-y-auto p-4 pb-10 gap-8 flex flex-col">
      <SearchResultArtist
        searchText={searchText}
        onClickSearchResult={onClickSearchResult}
      />

      <SearchResultReleaseGroup
        searchText={searchText}
        onClickSearchResult={onClickSearchResult}
      />

      <SearchResultRecording
        searchText={searchText}
        onClickSearchResult={onClickSearchResult}
      />
    </div>
  );
};
