import { UserProfileWidgetContent } from "@/widgets/user-profile-widget/UserProfileWidgetContent.tsx";
import { ThemeProvider } from "@/components/theme-provider.tsx";
import { Provider as ReduxProvider } from "react-redux";
import { store } from "@/Store.ts";
import { Provider as UrqlProvider } from "urql";
import { urqlClient } from "@/UrqlClient.ts";
import { Bootstrap } from "@/Bootstrap.tsx";
import { SetupPage } from "@/app/SetupPage.tsx";
import { SignInPage } from "@/app/SignInPage.tsx";

const UserProfileWidget = () => {
  return (
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme">
      <ReduxProvider store={store}>
        <UrqlProvider value={urqlClient}>
          <Bootstrap
            renderNoUsers={() => <SetupPage />}
            renderAuthenticated={() => <UserProfileWidgetContent />}
            renderNotAuthenticated={() => <SignInPage />}
          />
        </UrqlProvider>
      </ReduxProvider>
    </ThemeProvider>
  );
};

export default UserProfileWidget;
