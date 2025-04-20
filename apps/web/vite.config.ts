import { defineConfig, ProxyOptions } from "vite";
import react from "@vitejs/plugin-react";
import checker from "vite-plugin-checker";

const proxyOptions: ProxyOptions = {
  target: "http://[::1]:5505",
  ws: true,
  rewrite: (path) => {
    console.log(`Proxying request to: ${path}`);
    return path.replace(/^\/trpc/, "");
  },
};

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(),
    checker({
      typescript: true,
      eslint: {
        lintCommand: "eslint ./src --ext .ts,.tsx",
        useFlatConfig: true,
      },
    }),
  ],
  server: {
    port: 3000,
    strictPort: true,
    proxy: {
      "/trpc": proxyOptions,
    },
  },
});
