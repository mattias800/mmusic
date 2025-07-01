import path from "path";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig, ProxyOptions } from "vite";
import react from "@vitejs/plugin-react";
import federation from "@originjs/vite-plugin-federation";
import checker from "vite-plugin-checker";

const proxyOptions: ProxyOptions = {
  target: "http://[::1]:5095",
  ws: true,
};

export default defineConfig({
  build: {
    target: ["chrome89", "edge89", "es2022", "firefox89", "safari15"],
  },
  plugins: [
    react(),
    tailwindcss(),
    federation({
      name: "remote-app",
      filename: "remoteEntry.js",
      // Modules to expose
      exposes: {
        "./UserProfileWidget":
          "./src/widgets/user-profile-widget/UserProfileWidget.tsx",
      },
      shared: ["react"],
    }),
    checker({
      typescript: true,
      eslint: {
        lintCommand: 'eslint "./src/**/*.{ts,tsx}"',
        useFlatConfig: true,
      },
    })
  ],
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
