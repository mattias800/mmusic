import { ScrollArea } from "@/components/ui/scroll-area";
import {
  Cog,
  Heart,
  Import,
  Library,
  MicVocal,
  Music2,
  User,
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

export const Sidebar = ({ className }: SidebarProps) => {
  const [{ data }] = useQuery({
    query: sidebarQuery,
  });

  const username = data?.viewer?.username ?? "Profile";

  return (
    <div
      id="sidebar"
      className={cn("h-full flex flex-col justify-between", className)}
    >
      <div className={"flex flex-col gap-4"}>
        <div className={"flex justify-center"}>
          <MmusicLogo width={"100px"} />
        </div>

        <SidebarSection heading={"Search library"}>
          <SearchInput />
        </SidebarSection>

        <SidebarSection heading={"Discover"}>
          <SidebarNavButton path={"/"} label={"Popular artists"} icon={Heart} />
        </SidebarSection>

        <SidebarSection heading={"Library"}>
          <SidebarNavButton
            path={"/liked-songs"}
            label={"Liked songs"}
            icon={Heart}
          />

          <SidebarNavButton path={"/songs"} label={"Songs"} icon={Music2} />
          <SidebarNavButton
            path={"/artists"}
            label={"Artists"}
            icon={MicVocal}
          />
          <SidebarNavButton path={"/albums"} label={"Albums"} icon={Library} />
        </SidebarSection>

        <SidebarSection heading={"Playlists"}>
          <SidebarNavButton
            path={"/playlists/import/spotify"}
            label={"Import from Spotify"}
            icon={Import}
          />
          <PlaylistList />
        </SidebarSection>

        <SidebarSection
          heading={"Downloads"}
          right={<SoulSeekNetworkStatusFetcher />}
        >
          <ScrollArea className="h-[300px] px-1">
            <div className="pt-4 px-6">
              <DownloadOverviewFetcher />
            </div>
          </ScrollArea>
        </SidebarSection>
      </div>

      <SidebarSection heading={"You"}>
        <SidebarNavButton path={"/profile"} icon={User} label={username} />
        <SidebarNavButton path={"/settings"} icon={Cog} label={"Settings"} />
      </SidebarSection>
    </div>
  );
};
