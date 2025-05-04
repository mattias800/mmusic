import { FragmentType, graphql, useFragment } from "@/gql";
import * as React from "react";
import { Link } from "react-router";

export interface SearchResultReleaseGroupProps {
  releaseGroups: Array<FragmentType<typeof searchResultReleaseGroupFragment>>;
  onClickSearchResult: () => void;
}

export const searchResultReleaseGroupFragment = graphql(`
  fragment SearchResultReleaseGroup_ReleaseGroup on ReleaseGroup {
    id
    title
    mainRelease {
      id
      title
      coverArtUri
    }
  }
`);

export const SearchResultReleaseGroup: React.FC<
  SearchResultReleaseGroupProps
> = ({ onClickSearchResult, ...props }) => {
  const releaseGroups = useFragment(
    searchResultReleaseGroupFragment,
    props.releaseGroups,
  );

  if (!releaseGroups || releaseGroups.length === 0) {
    return null;
  }

  return (
    <div className="p-4">
      <h3 className="text-lg font-semibold mb-3 text-white">Albums</h3>
      <div className="space-y-2">
        {releaseGroups.map((releaseGroup) => (
          <Link
            to={`/album/${releaseGroup.mainRelease?.id || releaseGroup.id}`}
            key={releaseGroup.id}
            className="flex items-center p-2 hover:bg-white/10 rounded-md transition-colors"
            onClick={onClickSearchResult}
          >
            {releaseGroup.mainRelease?.coverArtUri ? (
              <img
                src={releaseGroup.mainRelease.coverArtUri}
                alt={releaseGroup.title}
                className="w-10 h-10 object-cover mr-3"
              />
            ) : (
              <div className="w-10 h-10 bg-neutral-700 flex items-center justify-center mr-3">
                <span className="text-white text-xs">🎵</span>
              </div>
            )}
            <div>
              <p className="text-white font-medium">{releaseGroup.title}</p>
              <p className="text-xs text-white/60">Album</p>
            </div>
          </Link>
        ))}
      </div>
    </div>
  );
};
