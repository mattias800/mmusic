import * as React from "react";
import { PageLayout, PageHeader, TabMenu } from "@/components/ui";
import { Cog, Settings, PlugZap, Users, MonitorDot, Gauge } from "lucide-react";
import { useLocation, useNavigate, Outlet } from "react-router";
import { TabItem } from "@/components/ui/TabItem.tsx";

export const AdminPage: React.FC = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const tab = React.useMemo(() => {
    const parts = location.pathname.split("/").filter(Boolean); // ["admin", "server"]
    return parts[1] ?? "overview";
  }, [location.pathname]);

  return (
    <PageLayout>
      <PageHeader icon={Cog} title="Administration" subtitle="Manage server and users" />
      <TabMenu className="mt-6">
        <TabItem
          label="Overview"
          icon={Gauge}
          onClick={() => navigate("/admin", { replace: true })}
          isActive={tab === "overview"}
        />
        <TabItem
          label="Server Settings"
          icon={Settings}
          onClick={() => navigate("/admin/server", { replace: true })}
          isActive={tab === "server"}
        />
        <TabItem
          label="Integrations"
          icon={PlugZap}
          onClick={() => navigate("/admin/integrations", { replace: true })}
          isActive={tab === "integrations"}
        />
        <TabItem
          label="Online Clients"
          icon={MonitorDot}
          onClick={() => navigate("/admin/clients", { replace: true })}
          isActive={tab === "clients"}
        />
        <TabItem
          label="Users"
          icon={Users}
          onClick={() => navigate("/admin/users", { replace: true })}
          isActive={tab === "users"}
        />
      </TabMenu>
      <div className="pt-6">
        <Outlet />
      </div>
    </PageLayout>
  );
};


