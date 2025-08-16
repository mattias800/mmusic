import {
  Cog,
  Heart,
  Import,
  Library,
  ListOrdered,
  MicVocal,
  Music2,
  User,
  Search,
  Home,
  Play,
  Download,
} from "lucide-react";
import { SearchInput } from "../search/search-input/SearchInput.tsx";
import { DownloadOverviewFetcher } from "@/features/downloads/download-overview/DownloadOverviewFetcher.tsx";
import { useQuery } from "urql";
import { graphql } from "@/gql";
import { cn } from "@/lib/utils.ts";
import { SidebarNavButton } from "@/features/sidebar/SidebarNavButton.tsx";
import { SidebarSection } from "@/features/sidebar/SidebarSection.tsx";
import { SoulSeekNetworkStatusFetcher } from "@/features/soul-seek-network-status/SoulSeekNetworkStatusFetcher.tsx";
import { MmusicLogo } from "@/components/logo/MmusicLogo.tsx";
import { PlaylistList } from "@/features/playlists/playlist-list/PlaylistList.tsx";
import { RootState } from "@/Store.ts";
import { useSelector } from "react-redux";

export interface SidebarProps {
  className?: string;
}

const sidebarQuery = graphql(`
  query Sidebar {
    viewer {
      id
      username
    }
  }
`);

const selector = (state: RootState) => state.musicPlayers.isOpen;

export const Sidebar = ({ className }: SidebarProps) => {
  const [{ data }] = useQuery({
    query: sidebarQuery,
  });

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
      {/* Top Section - Logo and Search */}
      <div className="flex flex-col gap-8 p-8">
        {/* Logo */}
        <div className="flex justify-center">
          <div className="p-4 bg-gradient-to-br from-blue-500/20 to-purple-500/20 rounded-xl border border-white/10">
            <MmusicLogo width={"90px"} />
          </div>
        </div>

        {/* Search Section */}
        <div className="space-y-4">
          <div className="flex items-center gap-3 px-2">
            <Search className="w-5 h-5 text-blue-400" />
            <h3 className="text-base font-semibold text-white">Search Library</h3>
          </div>
          <div className="px-2">
            <SearchInput />
          </div>
        </div>
      </div>

      {/* Main Navigation */}
      <div className="flex-1 flex flex-col gap-8 px-8">
        {/* Discover Section */}
        <SidebarSection heading="Discover" icon={Home} iconColor="text-pink-400">
          <SidebarNavButton path="/" label="Popular artists" icon={Heart} />
        </SidebarSection>
        
        {/* Library Section */}
        <SidebarSection heading="Library" icon={Library} iconColor="text-blue-400">
          <SidebarNavButton path="/liked-songs" label="Liked songs" icon={Heart} />
          <SidebarNavButton path="/songs" label="Songs" icon={Music2} />
          <SidebarNavButton path="/artists" label="Artists" icon={MicVocal} />
          <SidebarNavButton path="/albums" label="Albums" icon={Library} />
        </SidebarSection>
        
        {/* Playlists Section */}
        <SidebarSection heading="Playlists" icon={Play} iconColor="text-green-400">
          <SidebarNavButton path="/playlists/import/spotify" label="Import from Spotify" icon={Import} />
          <PlaylistList />
        </SidebarSection>
        
        {/* Downloads Section */}
        <SidebarSection 
          heading="Downloads" 
          icon={Download} 
          iconColor="text-orange-400"
          right={<SoulSeekNetworkStatusFetcher />}
        >
          <div className="pt-2">
            <DownloadOverviewFetcher />
          </div>
        </SidebarSection>
        
        {/* Queues & History Section */}
        <SidebarSection heading="Queues & History" icon={ListOrdered} iconColor="text-indigo-400">
          <SidebarNavButton path="/queues" label="Open queues" icon={ListOrdered} />
        </SidebarSection>
      </div>

      {/* Bottom Section - User */}
      <div className="p-8">
        <SidebarSection heading="You" icon={User} iconColor="text-emerald-400">
          <SidebarNavButton path="/profile" icon={User} label={username} />
          <SidebarNavButton path="/settings" icon={Cog} label="Settings" />
        </SidebarSection>
      </div>
    </div>
  );
};
