import { useState } from "react";
import { useQuery } from "urql";
import { TypedDocumentNode } from "@graphql-typed-document-node/core";

export const useLimitQuery = <TResult, TVariables extends { limit: number }>(
  {
    query,
    variables,
  }: {
    query: TypedDocumentNode<TResult, TVariables>;
    variables: Omit<TVariables, "limit">;
  },
  initialLimit: number = 5,
  maxLimit: number = 50,
  stepSize: number = 5,
) => {
  const [limit, setLimit] = useState(initialLimit);

  const [{ data, fetching }] = useQuery({
    query,
    variables: { ...variables, limit } as TVariables,
  });

  const onClickMore = () => {
    setLimit(limit + stepSize);
  };

  const showMoreButtonVisible = Boolean(data) && (limit < maxLimit || fetching);

  return [
    {
      limit,
      data,
      fetching,
      onClickMore,
      showMoreButtonVisible,
    },
  ];
};
