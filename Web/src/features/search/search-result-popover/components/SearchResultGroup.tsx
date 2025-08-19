import * as React from "react";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { NoResultsFound } from "@/features/search/search-result-popover/components/NoResultsFound.tsx";

export interface SearchResultGroupProps<T extends { id: string }> {
  heading: string;
  fetching: boolean;
  items: T[] | undefined;
  renderItem: (item: T) => React.ReactNode;
}

export function SearchResultGroup<T extends { id: string }>({
  heading,
  fetching,
  items,
  renderItem,
}: SearchResultGroupProps<T>) {
  return (
    <div className="space-y-4">
      {/* Section Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-2">
          <h3 className="text-base font-semibold text-white tracking-tight">
            {heading}
          </h3>
          {fetching && items && (
            <div className="flex items-center gap-2">
              <Spinner size="sm" />
              <span className="text-xs text-gray-400">Updating...</span>
            </div>
          )}
        </div>
        {items && items.length > 0 && (
          <div className="text-xs text-gray-400 bg-gray-800/80 px-3 py-1.5 rounded-full border border-white/10">
            {items.length} result{items.length !== 1 ? "s" : ""}
          </div>
        )}
      </div>

      {/* Content */}
      <div className="space-y-2">
        {fetching && !items && (
          <div className="flex items-center justify-center py-8">
            <SpinnerSpacing>
              <Spinner />
            </SpinnerSpacing>
          </div>
        )}

        {!fetching && (!items || items.length === 0) && <NoResultsFound />}

        {items && items.length > 0 && (
          <div className="space-y-2">
            {items.map((item) => (
              <div key={item.id} className="group">
                {renderItem(item)}
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}
