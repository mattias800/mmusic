import { createTRPCContext } from "@trpc/tanstack-react-query";
import { AppRouter } from "../../../api/server";

const t = createTRPCContext<AppRouter>();

export const { TRPCProvider, useTRPC, useTRPCClient } = t;
