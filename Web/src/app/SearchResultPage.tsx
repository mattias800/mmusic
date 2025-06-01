import * as React from "react";
import { useSearchParams } from "react-router-dom";
import { SearchResultBody } from "@/features/search/search-result-body/SearchResultBody.tsx";
import { MainPadding } from "@/components/layout/MainPadding.tsx";

export const SearchResultPage: React.FC = () => {
  const [searchParams] = useSearchParams();
  const searchText = searchParams.get("q") ?? "";

  return (
    <MainPadding>
      <h1 className="text-2xl font-bold mb-4">
        Search Results for "{searchText}"
      </h1>
      <SearchResultBody searchText={searchText} />
    </MainPadding>
  );
};
