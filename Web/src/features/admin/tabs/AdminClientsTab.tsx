import * as React from "react";
import { OnlineClientsPanel } from "@/features/sidebar/OnlineClientsPanel.tsx";
import { PageLayout, PageHeader } from "@/components/ui";
import { MonitorDot } from "lucide-react";

export const AdminClientsTab: React.FC = () => {
  return (
    <PageLayout>
      <PageHeader
        icon={MonitorDot}
        title="Online Clients"
        subtitle="Active clients connected to your server"
      />
      <div className="pt-4">
        <OnlineClientsPanel />
      </div>
    </PageLayout>
  );
};


