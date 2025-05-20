import * as React from "react";
import logo from "./mmusic.png";

export interface MmusicLogoProps {
  width?: string;
}

export const MmusicLogo: React.FC<MmusicLogoProps> = ({ width }) => {
  return <img src={logo} alt={"mmusic logo"} style={{ width }} />;
};
