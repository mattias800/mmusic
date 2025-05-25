import * as React from "react";
import { LargePlayButton } from "@/components/buttons/LargePlayButton.tsx";
import { TopArtistTracks } from "@/features/artist/artist-page/TopArtistTracks.tsx";
import { ShuffleButton } from "@/components/buttons/ShuffleButton.tsx";
import { DotsButton } from "@/components/buttons/DotsButton.tsx";
import { FollowButton } from "@/components/buttons/FollowButton.tsx";
import { FragmentType, graphql, useFragment } from "@/gql";
import { ArtistAlbumList } from "@/features/artist/artist-page/ArtistAlbumList.tsx";
import { ArtistHeader } from "@/features/artist/artist-page/ArtistHeader.tsx";
import { ArtistInLibraryButton } from "@/features/add-artist-to-server-library/ArtistInLibraryButton.tsx";
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
    ...TopArtistTracks_Artist
    ...ArtistNotInLibraryTopTracks_Artist
    ...ArtistServerStatus_Artist
  }
`);

export const ArtistPanel: React.FC<ArtistPanelProps> = (props) => {
  const artist = useFragment(artistPanelArtistFragment, props.artist);

  const { releasesVisible, topTracksVisible } = artist.serverStatus.result;

  const importInProgress = whenTypename(artist.serverStatus.result)
    .is("ArtistServerStatusImportingArtist", () => true)
    .is(
      "ArtistServerStatusImportingArtistReleases",
      (p) => p.numReleaseGroupsFinishedImporting < 1,
    )
    .default(() => false);

  const notInLibrary =
    artist.serverStatus.result.__typename === "ArtistServerStatusNotInLibrary";

  return (
    <GradientContent>
      <ArtistHeader
        artistName={artist.name}
        artistBackgroundUrl={artist.images?.artistBackground ?? ""}
        listeners={artist.listeners}
        renderServerStatus={() => <ArtistServerStatus artist={artist} />}
      />

      {notInLibrary || importInProgress ? (
        <ArtistNotInLibrarySectionList
          artistId={artist.id}
          artistName={artist.name}
          isInLibrary={!notInLibrary}
          renderTopTracks={() => (
            <ArtistNotInLibraryTopTracks artist={artist} />
          )}
        />
      ) : (
        <>
          <div className="px-6 md:px-10 py-6 flex items-center gap-4">
            <LargePlayButton />
            <ShuffleButton />
            <FollowButton />
            <DotsButton />
            <ArtistInLibraryButton artistId={artist.id} isInLibrary={true} />
          </div>

          <SectionList>
            {topTracksVisible && <TopArtistTracks artist={artist} />}

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
        </>
      )}
    </GradientContent>
  );
};
