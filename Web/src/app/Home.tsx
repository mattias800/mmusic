import * as React from "react";
import { LoginPanel } from "@/features/auth/components/LoginPanel.tsx";

export interface HomeProps {}

export const Home: React.FC<HomeProps> = () => {
  return <LoginPanel />;
};
