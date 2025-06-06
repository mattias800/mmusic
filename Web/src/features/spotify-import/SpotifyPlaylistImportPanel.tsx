import React, { useState } from "react";
import {
  SpotifyUserInputForm,
  SpotifyUserInputFormModel,
} from "./SpotifyUserInputForm";
import { UserPlaylistsFetcher } from "./UserPlaylistsFetcher.tsx";
import { Section } from "@/components/page-body/Section.tsx";
import { SectionHeading } from "@/components/headings/SectionHeading.tsx";
import { MainPadding } from "@/components/layout/MainPadding.tsx";
import { SectionList } from "@/components/page-body/SectionList.tsx";
import { Button } from "@/components/ui/button.tsx";
import { Search } from "lucide-react";

export const SpotifyPlaylistImportPanel: React.FC = () => {
  const [spotifyUsername, setSpotifyUsername] = useState<string>("");
  const [formModel, setFormModel] = useState<SpotifyUserInputFormModel>({
    username: "",
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    const s = formModel.username.trim();
    if (!s) {
      // Basic validation, can be enhanced
      alert("Please enter a Spotify username or ID.");
      return;
    }
    setSpotifyUsername(s);
  };

  return (
    <MainPadding>
      <SectionList>
        <Section>
          <SectionHeading>Import Spotify Playlists</SectionHeading>
          <form
            onSubmit={handleSubmit}
            className={"flex flex-col gap-4 items-start w-lg"}
          >
            <SpotifyUserInputForm
              value={formModel}
              onValueChange={setFormModel}
            />

            <div className={"flex justify-start"}>
              <Button type="submit" iconLeft={Search}>
                Get playlists
              </Button>
            </div>
          </form>
        </Section>

        {spotifyUsername && (
          <UserPlaylistsFetcher spotifyUsername={spotifyUsername} />
        )}
      </SectionList>
    </MainPadding>
  );
};
