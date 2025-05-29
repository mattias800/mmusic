import * as React from "react";
import { useSearchParams } from "react-router-dom";
import { SearchResultBody } from "@/features/search/search-result-body/SearchResultBody";

export const SearchResultPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const searchText = searchParams.get("q") ?? "";

  // TODO: Add a search input to allow the user to change the search text on this page?

  return (
    <div className="container mx-auto p-4">
      <h1 className="text-2xl font-bold mb-4">Search Results for "{searchText}"</h1>
      <SearchResultBody searchText={searchText} />
    </div>
  );
}; 