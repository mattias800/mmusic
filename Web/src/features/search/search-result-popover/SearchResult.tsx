import * as React from "react";
import { SearchResultArtist } from "./SearchResultArtist.tsx";
import { SearchResultReleaseGroup } from "./SearchResultReleaseGroup.tsx";
import { SearchResultRecording } from "./SearchResultRecording.tsx";
import { Link } from "react-router-dom";

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

      {searchText && (
        <Link
          to={`/search?q=${encodeURIComponent(searchText)}`}
          onClick={onClickSearchResult}
          className="mt-4 px-4 py-2 bg-neutral-700 hover:bg-neutral-600 text-white text-sm rounded-md text-center transition-colors"
        >
          Show more
        </Link>
      )}
    </div>
  );
};
