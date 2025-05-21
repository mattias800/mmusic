import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import App from "./App.tsx";

// Import Mantine core styles
import '@mantine/core/styles.css';
// Import Mantine notifications styles
import '@mantine/notifications/styles.css';

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <App />
  </StrictMode>,
);
