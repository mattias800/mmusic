import { migrate } from "drizzle-orm/node-postgres/migrator";
import { db, pool } from "./db";

async function main() {
  await migrate(db, { migrationsFolder: "./drizzle" });
  await pool.end();
}

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
