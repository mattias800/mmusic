import type { CodegenConfig } from "@graphql-codegen/cli";
import { resolve } from "path";

const scalarConfig = {
  strictScalars: true,
  defaultScalarType: "unknown",
  scalars: {
    DateTime: "string",
    LocalDate: "string",
    LocalTime: "string",
    Duration: "string",
    MoneyAmount: "string",
    Instant: "string",
  },
};

const config: CodegenConfig = {
  schema: "http://localhost:3100/graphql",
  documents: ["src/**/*.{ts,tsx}"],
  generates: {
    "./schema.graphql": {
      config: scalarConfig,
      plugins: ["schema-ast"],
    },
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
      plugins: [resolve(__dirname, "./scripts/cache-keys-plugin/cache-keys-plugin.ts")],
    },
  },
};

export default config;
