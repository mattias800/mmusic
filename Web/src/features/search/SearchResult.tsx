import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { SearchResultArtist } from "./SearchResultArtist";
import { SearchResultReleaseGroup } from "./SearchResultReleaseGroup";
import { SearchResultRecording } from "./SearchResultRecording";

export interface SearchResultProps {
  query?: FragmentType<typeof searchResultQueryFragment>;
  fetching?: boolean;
  skipped?: boolean;
  onClickSearchResult: () => void;
}

export const searchResultQueryFragment = graphql(`
  fragment SearchResult_Query on Query {
    artist {
      searchByName(name: $text, limit: 5) {
        ...SearchResultArtist_Artist
      }
    }
    releaseGroup {
      searchByName(name: $text, limit: 5) {
        ...SearchResultReleaseGroup_ReleaseGroup
      }
    }
    recording {
      searchByName(name: $text, limit: 5) {
        ...SearchResultRecording_Recording
      }
    }
  }
`);

export const SearchResult: React.FC<SearchResultProps> = ({
  fetching,
  skipped,
  onClickSearchResult,
  ...props
}) => {
  const query = useFragment(searchResultQueryFragment, props.query);

  if (skipped) {
    return null;
  }

  if (fetching) {
    return (
      <div className="w-96 rounded-2xl bg-black p-4 shadow-lg border border-white/10">
        <div className="text-white text-center py-4">Searching...</div>
      </div>
    );
  }

  if (!query) {
    return (
      <div className="w-96 rounded-2xl bg-black p-4 shadow-lg border border-white/10">
        <div className="text-white text-center py-4">
          {JSON.stringify(query)}
        </div>
      </div>
    );
  }

  const hasResults =
    (query.artist?.searchByName?.length || 0) > 0 ||
    (query.releaseGroup?.searchByName?.length || 0) > 0 ||
    (query.recording?.searchByName?.length || 0) > 0;

  if (!hasResults) {
    return (
      <div className="w-96 rounded-2xl bg-black p-4 shadow-lg border border-white/10">
        <div className="text-white text-center py-4">No results found</div>
      </div>
    );
  }

  return (
    <div className="w-96 rounded-2xl bg-black shadow-lg border border-white/10 max-h-[80vh] overflow-y-auto">
      {query.artist?.searchByName && query.artist.searchByName.length > 0 && (
        <SearchResultArtist
          artists={query.artist.searchByName}
          onClickSearchResult={onClickSearchResult}
        />
      )}

      {query.releaseGroup?.searchByName &&
        query.releaseGroup.searchByName.length > 0 && (
          <SearchResultReleaseGroup
            releaseGroups={query.releaseGroup.searchByName}
            onClickSearchResult={onClickSearchResult}
          />
        )}

      {query.recording?.searchByName &&
        query.recording.searchByName.length > 0 && (
          <SearchResultRecording
            recordings={query.recording.searchByName}
            onClickSearchResult={onClickSearchResult}
          />
        )}
    </div>
  );
};
