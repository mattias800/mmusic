import { SectionList } from "@/components/page-body/SectionList.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { TopArtistRecommendations } from "@/features/recommendations/top-artists/TopArtistRecommendations.tsx";

export const IndexPage = () => {
  return (
    <SectionList>
      <Section>
        <SectionHeading>Top artists</SectionHeading>
        <TopArtistRecommendations />
      </Section>
    </SectionList>
  );
};
