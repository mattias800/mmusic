import { graphql } from "@/gql";

export const addArtistToServerLibraryMutation = graphql(`
mutation AddArtistToServerLibrary($artistId: ID!) {
  addArtistToServerLibrary(input: { artistId: $artistId }) {
    __typename
    ... on AddArtistToServerLibrarySuccess {
      serverAvailability {
        id
        isInServerLibrary
      }
    }
  }
}`);
