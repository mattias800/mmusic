import * as React from "react";
import { Spinner } from "@/components/spinner/Spinner.tsx";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { NoResultsFound } from "./NoResultsFound.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { PropsWithChildren } from "react"; // Adjusted path

export interface SearchResultGroupProps<T extends { id: string }>
  extends PropsWithChildren {
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
  children,
}: SearchResultGroupProps<T>) {
  return (
    <Section>
      <SectionHeading loading={Boolean(fetching && items)}>
        {heading}
      </SectionHeading>
      <div className={"flex flex-wrap gap-8"}>
        {fetching && !items && (
          <SpinnerSpacing>
            <Spinner />
          </SpinnerSpacing>
        )}
        {!items || (items.length === 0 && <NoResultsFound />)}

        {items?.map((item) => <div key={item.id}>{renderItem(item)}</div>)}
      </div>
      {children}
    </Section>
  );
}
