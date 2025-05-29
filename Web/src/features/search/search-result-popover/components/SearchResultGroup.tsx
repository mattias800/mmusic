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
    <div className="flex flex-col gap-2">
      <div className={"flex items-center gap-4"}>
        <h3 className="text-lg font-semibold text-white">{heading}</h3>
        {fetching && items && <Spinner size={"sm"} />}
      </div>
      <div className="space-y-2">
        {fetching && !items && (
          <SpinnerSpacing>
            <Spinner />
          </SpinnerSpacing>
        )}
        {!items || (items.length === 0 && <NoResultsFound />)}

        {items?.map((item) => <div key={item.id}>{renderItem(item)}</div>)}
      </div>
    </div>
  );
}
