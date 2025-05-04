import * as React from "react";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { PlusCircle } from "lucide-react";
import { Separator } from "@/components/ui/separator";
import { ScrollArea, ScrollBar } from "@/components/ui/scroll-area.tsx";
import { listenNowAlbums, madeForYouAlbums } from "@/components/albums.ts";
import { AlbumArtwork } from "@/components/album-artwork.tsx";
import { PodcastEmptyPlaceholder } from "@/components/podcast-empty-placeholder.tsx";

export interface AlbumPanelProps {}

export const AlbumPanel: React.FC<AlbumPanelProps> = () => {
  return (
    <div/>
  );
};
