import * as React from "react";
import { SectionList } from "@/components/page-body/SectionList.tsx";

import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";

export interface ServerSettingsPanelProps {}

export const ServerSettingsPanel: React.FC<ServerSettingsPanelProps> = () => {
  return (
    <SectionList>
      <Section>
        <SectionHeading>Download settings</SectionHeading>
        Well hello!
      </Section>
    </SectionList>
  );
};
