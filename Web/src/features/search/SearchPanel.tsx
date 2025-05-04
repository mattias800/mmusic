import { graphql } from "@/gql";
import * as React from "react";
import { useEffect, useState } from "react";
import { debounce } from "lodash-es";
import { useQuery } from "urql";
import { SearchResult } from "@/features/search/SearchResult.tsx";

export interface SearchPanelProps {
  searchText: string;
}

const query = graphql(`
  query SearchPanelQuery($text: String!) {
    ...SearchResult_Query
  }
`);

export const SearchPanel: React.FC<SearchPanelProps> = ({ searchText }) => {
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

  const skipped = debouncedText.length < 3;

  const [{ data, fetching }] = useQuery({
    query,
    variables: { text: debouncedText },
    pause: skipped,
  });

  return (
    <div className={"relative"}>
      <div className={"absolute z-20"}>
        <SearchResult fetching={fetching} query={data} skipped={skipped} />
      </div>
    </div>
  );
};
