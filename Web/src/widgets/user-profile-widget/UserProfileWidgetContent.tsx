import * as React from "react";
import { graphql } from "@/gql";
import { useQuery } from "urql";
import { SpinnerSpacing } from "@/components/spinner/SpinnerSpacing.tsx";
import { Spinner } from "@/components/spinner/Spinner.tsx";

export interface UserProfileWidgetContentProps {}

const userProfileWidgetContentQuery = graphql(`
  query UserProfileWidget {
    viewer {
      id
      username
    }
  }
`);

export const UserProfileWidgetContent: React.FC<
  UserProfileWidgetContentProps
> = () => {
  const [{ fetching, data }] = useQuery({
    query: userProfileWidgetContentQuery,
  });

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
