import {
  QueryClient,
  QueryClientProvider,
  useQuery,
} from "@tanstack/react-query";
import type { AppRouter } from "../../api/server";
import { createTRPCClient, httpBatchLink } from "@trpc/client";
import { useState } from "react";
import { TRPCProvider, useTRPC } from "./utils/trpc";

const queryClient = new QueryClient();
const TEST_ID = "11111111-1111-1111-1111-111111111111";

function Media() {
  const trpc = useTRPC();
  const { data, isLoading, error } = useQuery(
    trpc.getMediaById.queryOptions({ id: TEST_ID }),
  );

  if (isLoading) return <p>Loading...</p>;
  if (error) return <p>Error: {(error as unknown as Error).message}</p>;
  if (!data) return <p>No media found.</p>;

  return (
    <div className="border p-4 rounded bg-gray-100">
      <div>
        <b>Title:</b> {data.title}
      </div>
      <div>
        <b>Artist:</b> {data.artist}
      </div>
      <div>
        <b>Album:</b> {data.album}
      </div>
      <div>
        <b>Year:</b> {data.year}
      </div>
      <div>
        <b>Path:</b> {data.path}
      </div>
      <div>
        <b>ID:</b> {data.id}
      </div>
    </div>
  );
}

function App() {
  const [trpcClient] = useState(() =>
    createTRPCClient<AppRouter>({
      links: [
        httpBatchLink({
          url: "/trpc",
        }),
      ],
    }),
  );
  return (
    <QueryClientProvider client={queryClient}>
      <TRPCProvider trpcClient={trpcClient} queryClient={queryClient}>
        <div
          style={{
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            marginTop: 40,
          }}
        >
          <h1>Vite + React + tRPC Example</h1>
          <Media />
        </div>
      </TRPCProvider>
    </QueryClientProvider>
  );
}

export default App;
