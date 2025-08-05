import * as React from "react";

export interface MmusicLogoProps {
  width?: string;
}

const iconUrl = new URL("./mmusic.png", import.meta.url).href;

export const MmusicLogo: React.FC<MmusicLogoProps> = ({ width }) => {
  return <img src={iconUrl} alt={"mmusic logo"} style={{ width }} />;
};
