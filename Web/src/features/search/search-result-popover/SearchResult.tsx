import * as React from "react";
import { SearchResultArtist } from "./SearchResultArtist.tsx";
import { SearchResultRelease } from "./SearchResultRelease.tsx";
import { SearchResultTrack } from "./SearchResultTrack.tsx";
import { Link } from "react-router";
import { Search, TrendingUp } from "lucide-react";

export interface SearchResultProps {
  searchText: string;
  onClickSearchResult: () => void;
}

export const SearchResult: React.FC<SearchResultProps> = ({
  searchText,
  onClickSearchResult,
}) => {
  return (
    <div className="w-[56rem] rounded-2xl bg-gray-900/95 backdrop-blur-md shadow-2xl border border-white/20 max-h-[85vh] overflow-hidden relative z-[9999]">
      {/* Header */}
      <div className="p-6 pb-4 border-b border-white/10">
        <div className="flex items-center gap-3 mb-2">
          <div className="p-2 bg-blue-500/20 rounded-lg border border-blue-500/30">
            <Search className="w-5 h-5 text-blue-400" />
          </div>
          <div>
            <h3 className="text-lg font-semibold text-white">Search Results</h3>
            <p className="text-sm text-gray-300">"{searchText}"</p>
          </div>
        </div>
      </div>

      {/* Three Column Layout */}
      <div className="p-6 space-y-6 max-h-[calc(85vh-120px)] overflow-y-auto">
        <div className="grid grid-cols-3 gap-8">
          {/* Artists Column */}
          <div className="space-y-4">
            <SearchResultArtist
              searchText={searchText}
              onClickSearchResult={onClickSearchResult}
            />
          </div>

          {/* Albums Column */}
          <div className="space-y-4">
            <SearchResultRelease
              searchText={searchText}
              onClickSearchResult={onClickSearchResult}
            />
          </div>

          {/* Songs Column */}
          <div className="space-y-4">
            <SearchResultTrack
              searchText={searchText}
              onClickSearchResult={onClickSearchResult}
            />
          </div>
        </div>
      </div>

      {/* Footer */}
      {searchText && (
        <div className="p-6 pt-4 border-t border-white/10 bg-gradient-to-r from-blue-500/5 to-purple-500/5">
          <Link
            to={`/search?q=${encodeURIComponent(searchText)}`}
            onClick={onClickSearchResult}
            className="w-full px-4 py-3 bg-gradient-to-r from-blue-500/20 to-purple-500/20 hover:from-blue-500/30 hover:to-purple-500/30 border border-blue-500/30 text-white text-sm font-medium rounded-xl text-center transition-all duration-200 hover:scale-[1.02] flex items-center justify-center gap-2"
          >
            <TrendingUp className="w-4 h-4" />
            View All Results
          </Link>
        </div>
      )}
    </div>
  );
};
