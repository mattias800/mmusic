import * as React from "react";
import { AdminUsersPage } from "@/app/AdminUsersPage.tsx";
import { PageLayout, PageHeader } from "@/components/ui";
import { Users } from "lucide-react";

export const AdminUsersTab: React.FC = () => {
  return (
    <PageLayout>
      <PageHeader
        icon={Users}
        title="Users"
        subtitle="Manage users and roles"
      />
      <div className="pt-4">
        <AdminUsersPage />
      </div>
    </PageLayout>
  );
};


