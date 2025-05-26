import * as React from "react";
import { ArtistInLibraryButton } from "@/features/add-artist-to-server-library/ArtistInLibraryButton.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { CircleAlert } from "lucide-react";

export interface ArtistNotInLibrarySectionListProps {
  artistId: string;
  artistName: string;
  isInLibrary: boolean;
  renderTopTracks?: () => React.ReactNode;
}

export const ArtistNotInLibrarySectionList: React.FC<
  ArtistNotInLibrarySectionListProps
> = ({ artistId, artistName, isInLibrary, renderTopTracks }) => {
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

          <div
            className={
              "flex flex-col gap-4 p-4 bg-black/70 rounded-md justify-between"
            }
          >
            <div className={"flex gap-4 items-center"}>
              <CircleAlert />
              <span className={"text-2xl"}>Not in library</span>
            </div>
            <span>{artistName} is not in your library, click to add.</span>
            <div className={"flex justify-end"}>
              <ArtistInLibraryButton
                artistId={artistId}
                isInLibrary={isInLibrary}
              />
            </div>
          </div>
        </div>
      </Section>
    </SectionList>
  );
};
