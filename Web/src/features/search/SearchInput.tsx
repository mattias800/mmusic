import * as React from "react";
import { Input } from "@/components/ui/input.tsx";
import { useState } from "react";
import { SearchPanel } from "@/features/search/SearchPanel.tsx";

export interface SearchInputProps {}

export const SearchInput: React.FC<SearchInputProps> = () => {
  const [value, setValue] = useState("");
  const [inFocus, setInFocus] = useState(false);
  return (
    <form>
      <Input
        placeholder={"Search..."}
        value={value}
        onFocus={() => setInFocus(true)}
        onBlur={() => setInFocus(false)}
        onChange={(ev) => setValue(ev.target.value)}
      />
      {value && inFocus && <SearchPanel searchText={value} />}
    </form>
  );
};
