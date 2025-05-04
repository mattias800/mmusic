import * as React from "react";
import { Input } from "@/components/ui/input.tsx";
import { useState } from "react";

export interface SearchInputProps {}

export const SearchInput: React.FC<SearchInputProps> = () => {
  const [value, setValue] = useState("");
  return (
    <form>
      <Input placeholder={"Search..."} />
    </form>
  );
};
