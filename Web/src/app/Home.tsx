import * as React from "react";
import { ModeToggle } from "@/components/mode-toggle.tsx";

export interface HomeProps {}

export const Home: React.FC<HomeProps> = () => {
  return <ModeToggle />;
};
