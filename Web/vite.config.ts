import path from "path";
import tailwindcss from "@tailwindcss/vite";
import { defineConfig, ProxyOptions } from "vite";
import react from "@vitejs/plugin-react";
import federation from "@originjs/vite-plugin-federation";

const proxyOptions: ProxyOptions = {
  target: "http://[::1]:5095",
  ws: true,
};

export default defineConfig({
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
