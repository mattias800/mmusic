import { createHTTPServer } from "@trpc/server/adapters/standalone";
import { appRouter } from "./server";

const server = createHTTPServer({
  router: appRouter,
});

const port = process.env.PORT || 5505;

server.listen(port);

console.log(`Server is running on http://localhost:${port}`);
