import * as React from "react";
import { RefreshCcw } from "lucide-react";
import { Button } from "@/components/ui/button.tsx";

export interface RefreshButtonProps {
  loading?: boolean;
  onClick?: () => void;
}

export const RefreshButton: React.FC<RefreshButtonProps> = ({
  loading,
  onClick,
}) => {
  return (
    <Button
      variant={"secondary"}
      size={"icon"}
      loading={loading}
      iconLeft={RefreshCcw}
      onClick={onClick}
    />
  );
};
