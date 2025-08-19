import { useQuery } from "urql";
import { graphql } from "@/gql";

const serverPublicBaseUrlQuery = graphql(`
  query ServerPublicBaseUrl {
    serverSettings {
      id
      publicBaseUrl
    }
  }
`);

export const useServerPublicBaseUrl = () => {
  const [{ data }] = useQuery({
    query: serverPublicBaseUrlQuery,
    requestPolicy: "cache-first",
  });
  return data?.serverSettings?.publicBaseUrl ?? null;
};
