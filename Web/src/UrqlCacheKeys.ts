import introspection from "./gql/introspection.json";
import { KeyGenerator } from "@urql/exchange-graphcache";

type Schema = typeof introspection;
type AllTypes = Schema["__schema"]["types"];

// Filter to only OBJECT types
type ObjectTypes = Extract<AllTypes[number], { kind: "OBJECT"; name: string }>;

// Types that have an `id` field
type TypesWithId = Extract<
  ObjectTypes,
  { fields: { name: "id" }[] | ({ name: string } & { name: "id" })[] }
>;

// Types without `id` field
type TypesWithoutId = Exclude<ObjectTypes, TypesWithId>["name"];

export const cacheKeys: Record<TypesWithoutId, KeyGenerator> = {
  NameCredit: () => null,
};
