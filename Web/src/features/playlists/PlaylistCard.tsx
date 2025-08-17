import * as React from "react";
import { PhotoCard } from "@/components/cards/PhotoCard.tsx";
import { PhotoCardCenterHeading } from "@/components/cards/PhotoCardCenterHeading.tsx";
import { PhotoCardBottomText } from "@/components/cards/PhotoCardBottomText.tsx";
import { useNavigate } from "react-router";

export interface PlaylistCardProps {
  playlist: {
    id: string;
    name?: string | null;
    coverImageUrl?: string | null;
    createdAt?: string | null;
  };
}

export const PlaylistCard: React.FC<PlaylistCardProps> = ({ playlist }) => {
  const navigate = useNavigate();
  return (
    <PhotoCard
      imageUrl={playlist.coverImageUrl ?? ""}
      imageAlt={playlist.name ?? "Playlist cover"}
      onClick={() => navigate(`/playlist/${playlist.id}`)}
    >
      <PhotoCardCenterHeading>
        {playlist.name ?? "Untitled Playlist"}
      </PhotoCardCenterHeading>
      {playlist.createdAt && (
        <PhotoCardBottomText>
          {new Date(playlist.createdAt).toLocaleDateString()}
        </PhotoCardBottomText>
      )}
    </PhotoCard>
  );
};

