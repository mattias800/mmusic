import * as React from "react";
import { SignInPanel } from "@/features/auth/components/SignInPanel.tsx";

export interface HomeProps {}

export const Home: React.FC<HomeProps> = () => {
  return <SignInPanel />;
};
