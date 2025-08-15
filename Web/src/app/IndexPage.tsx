import { SectionList } from "@/components/page-body/SectionList.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { TopArtistRecommendations } from "@/features/recommendations/top-artists/TopArtistRecommendations.tsx";
import { TopTrackRecommendations } from "@/features/recommendations/top-tracks/TopTrackRecommendations.tsx";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { ServerLibraryStatisticsHeader } from "@/features/server-library/ServerLibraryStatisticsHeader.tsx";
import { graphql } from "@/gql";
import { useQuery } from "urql";

const indexPageQuery = graphql(`
  query IndexPageQuery {
    serverLibrary {
      ...ServerLibraryStatisticsHeader_ServerLibrary
    }
  }
`);

export const IndexPage = () => {
  const [{ data, fetching, error }] = useQuery({
    query: indexPageQuery,
  });

  if (fetching) return <div className="p-6">Loading...</div>;
  if (error) return <div className="p-6">Error: {error.message}</div>;
  if (!data?.serverLibrary) return <div className="p-6">No data</div>;

  return (
    <>
      <title>Server Library Overview</title>
      <ServerLibraryStatisticsHeader serverLibrary={data.serverLibrary} />
      <MainPadding>
        <SectionList>
          <Section>
            <SectionHeading>Top artists</SectionHeading>
            <TopArtistRecommendations />
          </Section>
          <Section>
            <SectionHeading>Top tracks</SectionHeading>
            <TopTrackRecommendations />
          </Section>
        </SectionList>
      </MainPadding>
    </>
  );
};
