import * as React from "react";
import { useLocation } from "react-router";
import { useQuery } from "urql";
import { SidebarDocument } from "@/gql/graphql.ts";
import { Sidebar } from "@/features/sidebar/Sidebar.tsx";
import { AdminSidebar } from "@/features/sidebar/AdminSidebar.tsx";

export interface SidebarSwitcherProps {}

export const SidebarSwitcher: React.FC<SidebarSwitcherProps> = () => {
  const location = useLocation();
  const [{ data }] = useQuery({ query: SidebarDocument });

  const isAdmin = data?.viewer?.isAdmin ?? false;
  const isInAdmin = React.useMemo(() => {
    return (
      location.pathname === "/admin" || location.pathname.startsWith("/admin/")
    );
  }, [location.pathname]);

  if (isInAdmin && isAdmin) return <AdminSidebar />;
  return <Sidebar />;
};
