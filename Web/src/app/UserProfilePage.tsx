import { graphql } from "@/gql";
import { useQuery } from "urql";
import { PageLoading, PageError, PageNoData } from "@/components/ui";
import { UserProfilePanel } from "@/features/user-profile/UserProfilePanel.tsx";
import { User, AlertTriangle, Users } from "lucide-react";

const profilePageQuery = graphql(`
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

  if (fetching || stale)
    return (
      <PageLoading
        title="Loading Profile"
        subtitle="Fetching your user profile information"
        icon={User}
        iconBgColor="bg-blue-500/20"
      />
    );

  if (error)
    return (
      <PageError
        title="Failed to Load Profile"
        message="We couldn't load your user profile"
        error={error}
        icon={AlertTriangle}
        iconBgColor="bg-red-500/20"
      />
    );

  if (!data?.viewer)
    return (
      <PageNoData
        title="Profile Not Available"
        message="Your user profile couldn't be loaded"
        icon={Users}
        iconBgColor="bg-yellow-500/20"
      />
    );

  return (
    <>
      <title>{data.viewer.username}'s profile</title>
      <UserProfilePanel user={data.viewer} />
    </>
  );
};
