import { gql } from 'urql';

export const ARE_THERE_ANY_USERS_QUERY = gql`
  query AreThereAnyUsers {
    areThereAnyUsers
  }
`;

export const CREATE_USER_MUTATION = gql`
  mutation CreateUser($username: String!, $password: String!) {
    createUser(input: { username: $username, password: $password }) {
      __typename
      ... on CreateUserSuccess { # Assuming a similar success payload
        user {
          id
          username
        }
      }
      ... on CreateUserError { # Assuming a similar error payload
        message
      }
    }
  }
`;

export const SIGN_IN_MUTATION = gql`
  mutation SignIn($username: String!, $password: String!) {
    signIn(input: { username: $username, password: $password }) {
      __typename
      ... on SignInSuccess {
        user {
          id
          username
        }
      }
      ... on SignInError {
        message
      }
    }
  }
`;
