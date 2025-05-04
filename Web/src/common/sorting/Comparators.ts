export const byStringField =
  <T>(fieldResolver: (item: T) => string) =>
  (a: T, b: T): number =>
    fieldResolver(a).localeCompare(fieldResolver(b));

export const byNumberField =
  <T>(fieldResolver: (item: T) => number) =>
  (a: T, b: T): number =>
    fieldResolver(a) - fieldResolver(b);
