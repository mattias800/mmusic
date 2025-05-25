export const getTypename = <T extends { __typename: string }>(
  entity: T,
): T["__typename"] => entity.__typename;

export const isTypename = <
  T extends { __typename: string },
  TTypename extends T["__typename"],
>(
  entity: T | null | undefined,
  type: TTypename,
): entity is T & { __typename: TTypename } => entity?.__typename === type;

export const getUnionValue = <
  T extends { __typename: string },
  TTypename extends T["__typename"],
>(
  entity: T | null | undefined,
  type: TTypename,
): Extract<T, { __typename: TTypename }> | undefined => {
  if (entity == null) {
    return undefined;
  }
  if (entity.__typename === type) {
    return entity as Extract<T, { __typename: TTypename }>;
  } else {
    return undefined;
  }
};

type When<TTypename extends string, T extends { __typename: TTypename }> = {
  is: <R, TExactTypename extends TTypename>(
    prediction: TExactTypename,
    producer: (entity: Extract<T, { __typename: TExactTypename }>) => R,
  ) => Chain<TTypename, T, R>;
};

type Chain<TTypename extends string, T extends { __typename: TTypename }, R> = {
  is: <TExactTypename extends TTypename>(
    prediction: TExactTypename,
    producer: (entity: Extract<T, { __typename: TExactTypename }>) => R,
  ) => Chain<T["__typename"], T, R>;
  default: (producer: () => R) => R;
  elseUndefined: () => R | undefined;
  elseNull: () => R | null;
};

const match = <
  TTypename extends string,
  T extends { __typename: TTypename },
  R,
>(
  value: R,
): Chain<T["__typename"], T, R> => ({
  is: () => match<T["__typename"], T, R>(value),
  default: (): R => value,
  elseUndefined: (): R => value,
  elseNull: (): R => value,
});

const chain = <
  TTypename extends string,
  T extends { __typename: TTypename },
  R,
>(
  value: T | null | undefined,
): Chain<T["__typename"], T, R> => {
  return {
    is: <TExactTypename extends TTypename>(
      prediction: TExactTypename,
      producer: (entity: Extract<T, { __typename: TExactTypename }>) => R,
    ) => {
      if (value == null) {
        return chain<T["__typename"], T, R>(value);
      }
      return prediction === value.__typename
        ? match(producer(value as Extract<T, { __typename: TExactTypename }>))
        : chain<T["__typename"], T, R>(value);
    },
    default: (producer: () => R) => producer(),
    elseUndefined: () => undefined,
    elseNull: () => null,
  };
};

export const whenTypename = <
  TTypename extends string,
  T extends { __typename: TTypename },
>(
  value: T | null | undefined,
): When<T["__typename"], T> => ({
  is: <R, TExactTypename extends TTypename>(
    prediction: TExactTypename,
    producer: (entity: Extract<T, { __typename: TExactTypename }>) => R,
  ) => {
    if (value == null) {
      return chain<T["__typename"], T, R>(value);
    }
    return prediction === value.__typename
      ? match<T["__typename"], T, R>(
          producer(value as Extract<T, { __typename: TExactTypename }>),
        )
      : chain<T["__typename"], T, R>(value);
  },
});
