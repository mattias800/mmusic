import { graphql } from "@/gql";
import { useQuery } from "urql";
import { ScreenSpinner } from "@/components/spinner/ScreenSpinner.tsx";
import { UserProfilePanel } from "@/features/user-profile/UserProfilePanel.tsx";

export const profilePageQuery = graphql(`
  query ProfilePage {
    viewer {
      id
      username
      ...UserProfilePanel_User
    }
  }
`);

export const UserProfilePage = () => {
  const [{ error, data, fetching, stale }] = useQuery({
    query: profilePageQuery,
  });
  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;
  if (!data?.viewer) return <div>No data</div>;

  return (
    <>
      <title>{data.viewer.username}'s profile</title>
      <UserProfilePanel user={data.viewer} />
    </>
  );
};
