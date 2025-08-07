import * as React from "react";
import { TopArtistTracks } from "@/features/artist/artist-page/TopArtistTracks.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistHeader } from "@/features/artist/artist-page/ArtistHeader.tsx";
import { GradientContent } from "@/components/page-body/GradientContent.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { whenTypename } from "@/common/utils/TypenameMatcher.ts";
import { ArtistNotInLibrarySectionList } from "@/features/artist/artist-not-in-library/ArtistNotInLibrarySectionList.tsx";
import { ArtistServerStatus } from "@/features/artist/artist-server-status/ArtistServerStatus.tsx";
import { ArtistNotInLibraryTopTracks } from "@/features/artist/artist-not-in-library/ArtistNotInLibraryTopTracks.tsx";
import { AddArtistToLibraryBox } from "@/features/artist/artist-not-in-library/AddArtistToLibraryBox.tsx";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { ArtistActionButtons } from "@/features/artist/artist-page/ArtistActionButtons.tsx";
import { CardFlexList } from "@/components/page-body/CardFlexList.tsx";
import { byStringField } from "@/common/sorting/Comparators.ts";
import { AlbumCard } from "@/features/album/AlbumCard.tsx";

interface ArtistPanelProps {
  artist: FragmentType<typeof artistPanelArtistFragment>;
}

const artistPanelArtistFragment = graphql(`
  fragment ArtistPanel_Artist on Artist {
    id
    name
    listeners
    albums {
      id
      firstReleaseDate
      ...AlbumCard_Release
    }
    eps {
      id
      firstReleaseDate
      ...AlbumCard_Release
    }
    singles {
      id
      firstReleaseDate
      ...AlbumCard_Release
    }
    images {
      backgrounds
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

  const isImporting = whenTypename(artist.serverStatus.result)
    .is("ArtistServerStatusImportingArtist", () => true)
    .is("ArtistServerStatusImportingArtistReleases", () => true)
    .default(() => false);

  const isInLibrary =
    artist.serverStatus.result.__typename !== "ArtistServerStatusNotInLibrary";

  const totalNumReleaseGroups =
    artist.albums.length + artist.eps.length + artist.singles.length;

  return (
    <GradientContent>
      <ArtistHeader
        artistName={artist.name}
        artistBackgroundUrl={artist.images?.artistBackground ?? ""}
        listeners={artist.listeners}
        renderServerStatus={() => <ArtistServerStatus artistId={artist.id} />}
      />

      {!isInLibrary || (isImporting && totalNumReleaseGroups < 1) ? (
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
          <ArtistActionButtons artistId={artist.id} isImporting={isImporting} />

          <MainPadding>
            <SectionList>
              {topTracksVisible && <TopArtistTracks artistId={artist.id} />}

              {releasesVisible && (
                <>
                  <Section>
                    <SectionHeading>Albums</SectionHeading>
                    <CardFlexList>
                      {artist.albums
                        .toSorted(
                          byStringField((a) => a.firstReleaseDate ?? ""),
                        )
                        .toReversed()
                        .map((release) => (
                          <AlbumCard releaseGroup={release} key={release.id} />
                        ))}
                    </CardFlexList>
                  </Section>

                  <Section>
                    <SectionHeading>EPs</SectionHeading>
                    <CardFlexList>
                      {artist.eps
                        .toSorted(
                          byStringField((a) => a.firstReleaseDate ?? ""),
                        )
                        .toReversed()
                        .map((release) => (
                          <AlbumCard releaseGroup={release} key={release.id} />
                        ))}
                    </CardFlexList>
                  </Section>

                  <Section>
                    <SectionHeading>Singles</SectionHeading>
                    <CardFlexList>
                      {artist.singles
                        .toSorted(
                          byStringField((a) => a.firstReleaseDate ?? ""),
                        )
                        .toReversed()
                        .map((release) => (
                          <AlbumCard releaseGroup={release} key={release.id} />
                        ))}
                    </CardFlexList>
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
