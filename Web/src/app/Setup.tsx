import React from "react";
import { Center, Paper, Stack, Title } from "@mantine/core";
import { CreateAdminUserPanel } from "@/features/create-admin-user/CreateAdminUserPanel.tsx";

const SetupPage: React.FC = () => {
  return (
    <Center style={{ minHeight: "100vh" }}>
      <Paper
        withBorder
        shadow="md"
        p="xl"
        radius="md"
        style={{ width: "100%", maxWidth: "400px" }}
      >
        <Stack align="center">
          <Title order={1} style={{ marginBottom: "24px" }}>
            Create Admin User
          </Title>
        </Stack>
        <CreateAdminUserPanel />
      </Paper>
    </Center>
  );
};

export default SetupPage;
