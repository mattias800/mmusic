import * as React from "react";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { SectionList } from "@/components/page-body/SectionList.tsx";

export interface ArtistNotInLibrarySectionListProps {
  artistName: string;
  renderTopTracks?: () => React.ReactNode;
  renderImportBox?: () => React.ReactNode;
}

export const ArtistNotInLibrarySectionList: React.FC<
  ArtistNotInLibrarySectionListProps
> = ({ artistName, renderTopTracks, renderImportBox }) => {
  return (
    <SectionList>
      <Section>
        <div className={"flex gap-32 justify-between"}>
          {renderTopTracks && (
            <div className={"flex flex-col"}>
              <SectionHeading>Popular tracks by {artistName}</SectionHeading>
              {renderTopTracks?.()}
            </div>
          )}

          {renderImportBox?.()}
        </div>
      </Section>
    </SectionList>
  );
};
