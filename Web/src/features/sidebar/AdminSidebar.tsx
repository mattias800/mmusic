import {
  Cog,
  Download,
  Gauge,
  Home,
  MonitorDot,
  PlugZap,
  Settings,
  User,
  Users,
} from "lucide-react";
import { useQuery } from "urql";
import { cn } from "@/lib/utils.ts";
import { SidebarSection } from "@/features/sidebar/SidebarSection.tsx";
import { SidebarNavButton } from "@/features/sidebar/SidebarNavButton.tsx";
import { MmusicLogo } from "@/components/logo/MmusicLogo.tsx";
import { RootState } from "@/Store.ts";
import { useSelector } from "react-redux";
import { SidebarDocument } from "@/gql/graphql.ts";

export interface AdminSidebarProps {
  className?: string;
}

const selector = (state: RootState) => state.musicPlayers.isOpen;

export const AdminSidebar = ({ className }: AdminSidebarProps) => {
  const [{ data }] = useQuery({ query: SidebarDocument });

  const username = data?.viewer?.username ?? "Profile";
  const musicPlayerIsOpen = useSelector(selector);

  return (
    <div
      id="sidebar"
      className={cn(
        "h-full flex flex-col justify-between relative z-10 bg-gradient-to-b from-gray-900 via-gray-800 to-gray-900 border-r border-white/10",
        musicPlayerIsOpen && "pb-24",
        className,
      )}
    >
      <div className="flex flex-col gap-8 p-8">
        <div className="flex justify-center">
          <div className="p-4 bg-gradient-to-br from-blue-500/20 to-purple-500/20 rounded-xl border border-white/10">
            <MmusicLogo width={"90px"} />
          </div>
        </div>
      </div>

      <div className="flex-1 flex flex-col gap-8 px-8">
        <SidebarSection heading="App" icon={Home} iconColor="text-pink-400">
          <SidebarNavButton path="/" label="Back to app" icon={Home} />
        </SidebarSection>

        <SidebarSection
          heading="Administration"
          icon={Cog}
          iconColor="text-blue-400"
        >
          <SidebarNavButton path="/admin" label="Overview" icon={Gauge} />
          <SidebarNavButton
            path="/admin/downloads"
            label="Downloads"
            icon={Download}
          />
          <SidebarNavButton
            path="/admin/server"
            label="Server Settings"
            icon={Settings}
          />
          <SidebarNavButton
            path="/admin/integrations"
            label="Integrations"
            icon={PlugZap}
          />
          <SidebarNavButton
            path="/admin/clients"
            label="Online Clients"
            icon={MonitorDot}
          />
          <SidebarNavButton path="/admin/users" label="Users" icon={Users} />
        </SidebarSection>
      </div>

      <div className="p-8">
        <SidebarSection heading="You" icon={User} iconColor="text-emerald-400">
          <SidebarNavButton path="/profile" icon={User} label={username} />
        </SidebarSection>
      </div>
    </div>
  );
};


