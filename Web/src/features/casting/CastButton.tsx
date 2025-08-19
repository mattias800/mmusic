import * as React from "react";
import { Button } from "@/components/ui/button.tsx";
import { Cast } from "lucide-react";
import { useCast } from "@/features/casting/useCast.ts";
import {
  endCastSession,
  startCastSession,
} from "@/features/casting/cast-sender.ts";

export interface CastButtonProps {
  className?: string;
}

export const CastButton: React.FC<CastButtonProps> = ({ className }) => {
  const { isReady, hasSession } = useCast();
  if (!isReady) return null;
  return (
    <Button
      variant={hasSession ? "default" : "ghost"}
      size="icon"
      className={className ?? "h-10 w-10"}
      onClick={() => (hasSession ? endCastSession() : startCastSession())}
      aria-label={hasSession ? "Disconnect Cast" : "Cast"}
      title={hasSession ? "Disconnect Cast" : "Cast to device"}
    >
      <Cast className="h-5 w-5" />
    </Button>
  );
};
