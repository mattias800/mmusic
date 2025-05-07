import type { CodegenConfig } from "@graphql-codegen/cli";

const config: CodegenConfig = {
  schema: "http://localhost:3100/graphql",
  documents: ["src/**/*.tsx"],
  generates: {
    "./src/gql/": {
      preset: "client",
    },
    "schema.json": {
      plugins: ["introspection"],
      config: {
        output: "schema.json",
      },
    },
  },
};

export default config;
