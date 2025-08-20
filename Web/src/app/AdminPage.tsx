import * as React from "react";
import { PageLayout, PageHeader } from "@/components/ui";
import { Cog } from "lucide-react";
import { Outlet } from "react-router";

export const AdminPage: React.FC = () => {
  return (
    <PageLayout>
      <PageHeader icon={Cog} title="Administration" subtitle="Manage server and users" />
      <div className="pt-2">
        <Outlet />
      </div>
    </PageLayout>
  );
};


