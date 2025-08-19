import { useQuery } from "urql";
import { graphql } from "@/gql";

const q = graphql(`
  query ServerPublicBaseUrl {
    serverSettings {
      id
      publicBaseUrl
    }
  }
`);

export const useServerPublicBaseUrl = () => {
  const [{ data }] = useQuery({ query: q, requestPolicy: "cache-first" });
  return data?.serverSettings?.publicBaseUrl ?? null;
};

import { useEffect, useState } from "react";
import {
  CastState,
  getCastState,
  ensureCastInitialized,
  onCastStateChange,
} from "@/features/casting/cast-sender.ts";

export const useCast = () => {
  const [state, setState] = useState<CastState>(() => getCastState());
  useEffect(() => {
    const off = onCastStateChange(setState);
    ensureCastInitialized();
    return () => off();
  }, []);
  return state;
};
