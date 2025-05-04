import { graphql } from "@/gql";
import * as React from "react";

export interface SearchPanelProps {}

const query = graphql(`
query SearchPanelQuery($text: String!) {
  artist {
    searchByName(name: $text) {
      id
      name
      images {
        artistThumb
      }
    }
  }
  release {
    searchByName(name: $text) {
      id
      title
      coverArtUri
    }
  }
}
`);

export const SearchPanel: React.FC<SearchPanelProps> = () => {
  return <div></div>;
};
