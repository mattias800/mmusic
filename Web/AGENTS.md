# MMusic Project Development Guidelines

This document provides essential information for developers working on the MMusic project.

## Build and Configuration

### Prerequisites

- bun

### Setup

1. Install dependencies:

   ```bash
   bun install
   ```

2. Start the development server:

   ```bash
   bun dev
   ```

   This will start the Vite development server on port 3100.

3. Build for production:
   ```bash
   bun build
   ```

### Project Structure

- `/src`: Main source code
  - `/app`: Page components
  - `/assets`: Static assets
  - `/common`: Shared utilities
  - `/components`: Reusable UI components
  - `/features`: Feature-specific components
  - `/gql`: GraphQL related files
  - `/hooks`: Custom React hooks
  - `/lib`: Utility libraries
  - `/test`: Test setup and utilities

### Configuration Files

- `vite.config.ts`: Vite configuration
- `tsconfig.json`: TypeScript configuration
- `eslint.config.js`: ESLint configuration
- `graphql.config.yml`: GraphQL configuration
- `codegen.ts`: GraphQL code generation configuration

### GraphQL Integration

- The project uses GraphQL with the URQL client
- GraphQL schema is defined in `schema.graphql`
- Generate TypeScript types from GraphQL schema:

  ```bash
  bun types
  ```

- No functions or documents are generated, only types.

## Testing

### Testing Framework

The project uses Vitest as the testing framework, along with React Testing Library for component testing.

### Running Tests

- Run all tests once:

  ```bash
  bun test
  ```

- Run tests in watch mode (for development):

  ```bash
  bun test:watch
  ```

- Run tests with coverage report:
  ```bash
  bun test:coverage
  ```

### Test File Structure

- Test files should be placed next to the files they test
- Use the naming convention `*.test.ts` or `*.test.tsx`
- Example directory structure:
  ```
  src/
  ├── components/
  │   └── ui/
  │       ├── Alert.tsx
  │       └── Alert.test.tsx
  ```

### Writing Tests

#### Utility Tests

Here's an example of testing utility functions:

```typescript
// src/common/utils/string-utils.test.ts
import { describe, it, expect } from "vitest";
import { capitalizeFirstLetter, truncateString } from "./string-utils";

describe("String Utils", () => {
  describe("capitalizeFirstLetter", () => {
    it("should capitalize the first letter of a string", () => {
      expect(capitalizeFirstLetter("hello")).toBe("Hello");
    });

    // More tests...
  });
});
```

#### Component Tests

Here's an example of testing a React component:

```typescript
// src/components/ui/Alert.test.tsx
import {describe, it, expect} from 'vitest';
import {render, screen} from '@testing-library/react';
import {Alert} from './Alert';

describe('Alert Component', () => {
    it('renders with default variant', () => {
        render(<Alert>This
        is
        an
        alert < /Alert>);
        const alert = screen.getByRole('alert');
        expect(alert).toBeInTheDocument();
        expect(alert.textContent).toBe('This is an alert');
    });

    // More tests...
});
```

### Adding New Tests

1. Create a new test file next to the file you want to test
2. Import the necessary testing utilities
3. Write your tests using the describe/it pattern
4. Run the tests to verify they pass

## Code Style and Development Practices

### TypeScript

- The project uses TypeScript for type safety
- Follow the TypeScript configuration in `tsconfig.json`
- Use explicit types for function parameters and return values

### React

- The project uses React 19
- Use functional components with hooks
- Follow the component structure in the existing codebase

### Styling

- The project uses Tailwind CSS for styling
- Use the `cn` utility function from `src/lib/utils.ts` for conditional class names
- Follow the component styling patterns in the existing UI components

### State Management

- The project uses Redux Toolkit for global state management
- Use React hooks for component-level state

### Code Formatting

- Use Prettier for code formatting:
  ```bash
  bun prettier
  ```

### Linting

- Use ESLint for code linting:
  ```bash
  bun lint
  ```

### GraphQL

- Use the generated types from GraphQL schema
- Follow the query/mutation patterns in the existing code
- Use the URQL client for GraphQL operations

## Debugging

### Development Server

- The development server runs on port 3100
- GraphQL requests are proxied to port 5095

### Browser DevTools

- Use React DevTools for component debugging
- Use Redux DevTools for state debugging
- Use Network tab for API request debugging

## Deployment

- The project is built using Vite
- The build output is in the `dist` directory
- Preview the production build:
  ```bash
  bun preview
  ```

# Folder structure

The application uses vertical slice architecture.
All features are placed in the `src/features` folder.
All pages of the app are placed in the `src/app` folder.
All shared components are placed in the `src/common` folder.
The `src/gql` folder is generated, and should never be modified manually.

# Components

## Page components

All pages should be placed in app/pages/.
They must be named with the suffix "Page" and should be exported as default.
They should be added to AppRouter so the user can navigate to them.

## Data dependencies

### Fragments

The application uses fragment co-location, which means that fragments for a component should be placed together with the components.
All components that rely on data from GraphQL should define its own dependencies in GraphQL fragments. 
Please the fragment documents in the same file.
The data should then be sent to the component as props.
Use the `useFragment` function from @/gql to unmask the fragment.
Use the `FragmentType<typeof fragmentVariable>` to define the type of the props.
The name of the fragment should be the same as the component, but with GraphQL type and "Fragment" as suffixes.
For example, UserList that uses the User type would have a fragment named `UserListUser_Fragment` and
the variable would be named `userListUserFragment`.

### Queries

Any component that sends a GraphQL query should have "Fetcher" as suffix in its name.
The query should be named like the component, but with "Query" as suffix.
For example, the component AlbumPanelFetcher would have a query named AlbumPanelQuery,
and the query variable should be named albumPanelQuery.

There is no need to manually specify types for queries, they are inferred from the generated types.

Here is an example of useQuery:

```typescript
const [{ error, data, fetching, stale }] = useQuery({
  query: likedSongsQuery,
});
```

And here is an example of a full fetcher component:

```typescript jsx
export interface LikedSongsProps {}

export const likedSongsQuery = graphql(`
  query LikedSongsQuery {
    viewer {
      id
      ...LikedSongsList_User
    }
  }
`);

export const LikedSongs: React.FC<LikedSongsProps> = () => {
  const [{ error, data, fetching, stale }] = useQuery({
    query: likedSongsQuery,
  });
  if (fetching || stale) return <ScreenSpinner />;
  if (error) return <div>Error: {error.message}</div>;
  if (!data?.viewer) return <div>No data</div>;
  return <LikedSongsList user={data?.viewer} />;
};
```

## Mutations

Mutations should be placed in the same file as the component that uses them.
They should be named after the logical operation that it performs, with "Mutation" as suffix.

There is no need to manually specify types for mutations, 
the generated types include declarations for the graphql function.

Here is an example of usage:

```typescript
const [{ fetching }, signIn] = useMutation(signInMutation);
```

## GraphQL types

All Typescript types for GraphQL are generated by GraphQL codegen, using this command:

```bash
bun types
```

These types are never to be modified manually.
They are also never imported directly,
since the types will be applied to all calls to the `graphql()` function.
GraphQL documents should never be imported from the generated source,
instead, always use the variable created next to the React component.

You must generate the types after changing the GraphQL schema, or any fragment, query or mutation.

### Code quality

### useEffect

Avoid useEffect when possible. They are almost never needed.

## UI components

Shared dumb UI components should be placed in common/components/.
They should use Tailwind CSS for styling.

## Comments

Please avoid comments in the code when possible.
If you feel like you need to add a comment, consider if the code can be refactored to be more readable instead.
Never add comments in the imports section.
