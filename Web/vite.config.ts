import path from "path";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig, ProxyOptions } from "vite";
import react from "@vitejs/plugin-react";

const proxyOptions: ProxyOptions = {
  target: "http://[::1]:5095",
  ws: true,
};

export default defineConfig({
  plugins: [react(), tailwindcss()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    port: 3100,
    strictPort: true,
    proxy: {
      "/graphql": proxyOptions,
    },
  },
});
