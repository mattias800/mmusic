import * as React from "react";
import { PropsWithChildren } from "react";
import { SectionList } from "@/components/page-body/SectionList.tsx";

import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { Section } from "@/components/page-body/Section.tsx";

export interface ServerSettingsPanelProps extends PropsWithChildren {}

export const ServerSettingsPanel: React.FC<ServerSettingsPanelProps> = ({
  children,
}) => {
  return (
    <SectionList>
      <Section>
        <SectionHeading>Download settings</SectionHeading>
        {children}
      </Section>
    </SectionList>
  );
};
