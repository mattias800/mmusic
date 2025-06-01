import * as React from "react";
import { TopArtistTracks } from "@/features/artist/artist-page/TopArtistTracks.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistAlbumList } from "@/features/artist/artist-page/ArtistAlbumList.tsx";
import { ArtistHeader } from "@/features/artist/artist-page/ArtistHeader.tsx";
import { ArtistSingleList } from "@/features/artist/artist-page/ArtistSingleList.tsx";
import { GradientContent } from "@/components/page-body/GradientContent.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { ArtistEpList } from "@/features/artist/artist-page/ArtistEpList.tsx";
import { whenTypename } from "@/common/utils/TypenameMatcher.ts";
import { ArtistNotInLibrarySectionList } from "@/features/artist/artist-not-in-library/ArtistNotInLibrarySectionList.tsx";
import { ArtistServerStatus } from "@/features/artist/artist-server-status/ArtistServerStatus.tsx";
import { ArtistNotInLibraryTopTracks } from "@/features/artist/artist-not-in-library/ArtistNotInLibraryTopTracks.tsx";
import { AddArtistToLibraryBox } from "@/features/artist/artist-not-in-library/AddArtistToLibraryBox.tsx";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { ArtistActionButtons } from "@/features/artist/artist-page/ArtistActionButtons.tsx";

interface ArtistPanelProps {
  artist: FragmentType<typeof artistPanelArtistFragment>;
}

export const artistPanelArtistFragment = graphql(`
  fragment ArtistPanel_Artist on Artist {
    id
    name
    listeners
    images {
      artistBackground
    }
    serverStatus {
      id
      result {
        __typename
        ... on ArtistServerStatusResultBase {
          topTracksVisible
          releasesVisible
        }
        ... on ArtistServerStatusImportingArtistReleases {
          numReleaseGroupsFinishedImporting
          totalNumReleaseGroupsBeingImported
        }
      }
    }
    ...ArtistNotInLibraryTopTracks_Artist
  }
`);

export const ArtistPanel: React.FC<ArtistPanelProps> = (props) => {
  const artist = useFragment(artistPanelArtistFragment, props.artist);

  const { releasesVisible, topTracksVisible } = artist.serverStatus.result;

  const importIsStarting = whenTypename(artist.serverStatus.result)
    .is("ArtistServerStatusImportingArtist", () => true)
    .is(
      "ArtistServerStatusImportingArtistReleases",
      (p) => p.numReleaseGroupsFinishedImporting < 1,
    )
    .default(() => false);

  const isInLibrary =
    artist.serverStatus.result.__typename !== "ArtistServerStatusNotInLibrary";

  return (
    <GradientContent>
      <ArtistHeader
        artistName={artist.name}
        artistBackgroundUrl={artist.images?.artistBackground ?? ""}
        listeners={artist.listeners}
        renderServerStatus={() => <ArtistServerStatus artistId={artist.id} />}
      />

      {!isInLibrary || importIsStarting ? (
        <MainPadding>
          <ArtistNotInLibrarySectionList
            artistName={artist.name}
            renderTopTracks={() => (
              <ArtistNotInLibraryTopTracks artist={artist} />
            )}
            renderImportBox={() => (
              <AddArtistToLibraryBox
                artistId={artist.id}
                artistName={artist.name}
                isInLibrary={isInLibrary}
              />
            )}
          />
        </MainPadding>
      ) : (
        <>
          <ArtistActionButtons artistId={artist.id} />

          <MainPadding>
            <SectionList>
              {topTracksVisible && <TopArtistTracks artistId={artist.id} />}

              {releasesVisible && (
                <>
                  <Section>
                    <SectionHeading>Albums</SectionHeading>
                    <ArtistAlbumList artistId={artist.id} />
                  </Section>

                  <Section>
                    <SectionHeading>EPs</SectionHeading>
                    <ArtistEpList artistId={artist.id} />
                  </Section>

                  <Section>
                    <SectionHeading>Singles</SectionHeading>
                    <ArtistSingleList artistId={artist.id} />
                  </Section>
                </>
              )}
            </SectionList>
          </MainPadding>
        </>
      )}
    </GradientContent>
  );
};
