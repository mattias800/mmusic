import * as React from "react";
import { ArtistSearchResults } from "./ArtistSearchResults";
import { AlbumSearchResults } from "./AlbumSearchResults";
import { RecordingSearchResults } from "./RecordingSearchResults";

export interface SearchResultBodyProps {
  searchText: string;
}

export const SearchResultBody: React.FC<SearchResultBodyProps> = ({
  searchText,
}) => {
  return (
    <div className="p-4 pb-10 gap-8 flex flex-col">
      <ArtistSearchResults searchText={searchText} />
      <AlbumSearchResults searchText={searchText} />
      <RecordingSearchResults searchText={searchText} />
    </div>
  );
};
