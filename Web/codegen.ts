import type { CodegenConfig } from "@graphql-codegen/cli";
import { resolve } from "path";

const config: CodegenConfig = {
  schema: "http://localhost:5095/graphql",
  documents: ["src/**/*.{ts,tsx}"],
  generates: {
    "./src/gql/": {
      preset: "client",
    },
    "./src/gql/introspection.json": {
      plugins: ["introspection"],
      config: {
        minify: false,
      },
    },
    "./src/gql/types-with-no-id.ts": {
      plugins: [
        resolve(__dirname, "./scripts/cache-keys-plugin/cache-keys-plugin.ts"),
      ],
    },
  },
};

export default config;
