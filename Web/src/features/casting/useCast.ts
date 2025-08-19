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


