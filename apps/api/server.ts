import { z } from "zod";
import { db } from "./db";
import { media } from "./schema";
import { eq } from "drizzle-orm";
import { procedure, router } from "./trpc";

export const appRouter = router({
  hello: procedure.input(z.object({ name: z.string() })).query(({ input }) => {
    return { greeting: `Hello, ${input.name}!` };
  }),
  getMediaById: procedure
    .input(z.object({ id: z.string().uuid() }))
    .query(async ({ input }) => {
      const result = await db
        .select()
        .from(media)
        .where(eq(media.id, input.id));
      return result[0] || null;
    }),
});

export type AppRouter = typeof appRouter;
