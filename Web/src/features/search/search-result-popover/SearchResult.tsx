import * as React from "react";
import { SearchResultArtist } from "./SearchResultArtist.tsx";
import { SearchResultRelease } from "./SearchResultRelease.tsx";
import { SearchResultTrack } from "./SearchResultTrack.tsx";
import { Link } from "react-router";

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

      <SearchResultRelease
        searchText={searchText}
        onClickSearchResult={onClickSearchResult}
      />

      <SearchResultTrack
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
