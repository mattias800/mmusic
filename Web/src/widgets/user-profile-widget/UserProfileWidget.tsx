import { graphql } from "@/gql";
import { useQuery } from "urql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";

const userProfileWidgetQuery = graphql(`
  query UserProfileWidget {
    viewer {
      id
      username
    }
  }
`);

export const UserProfileWidget = () => {
  const [{ fetching, data }] = useQuery({ query: userProfileWidgetQuery });

  const username = data?.viewer?.username;

  return (
    <div className="bg-card text-card-foreground p-6 rounded-lg shadow">
      {fetching || !data ? (
        <SpinnerSpacing>
          <Spinner />
        </SpinnerSpacing>
      ) : (
        <p className="mb-2">
          <strong>Username:</strong> {username}
        </p>
      )}
    </div>
  );
};
