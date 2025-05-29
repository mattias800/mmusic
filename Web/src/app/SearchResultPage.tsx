import * as React from "react";
import { useParams } from "react-router";
import { SearchResultBody } from "@/features/search/search-result-body/SearchResultBody.tsx";
import { InvalidSearchText } from "@/features/search/search-result-body/InvalidSearchText.tsx";

export interface SearchResultPageProps {}

export const SearchResultPage: React.FC<SearchResultPageProps> = () => {
  const { searchText } = useParams<{ searchText: string }>();

  if (!searchText) {
    return <InvalidSearchText />;
  }

  return <SearchResultBody searchText={searchText} />;
};
