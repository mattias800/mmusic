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
import { describe, it, expect } from 'vitest';
import { capitalizeFirstLetter, truncateString } from './string-utils';

describe('String Utils', () => {
  describe('capitalizeFirstLetter', () => {
    it('should capitalize the first letter of a string', () => {
      expect(capitalizeFirstLetter('hello')).toBe('Hello');
    });
    
    // More tests...
  });
});
```

#### Component Tests
Here's an example of testing a React component:

```typescript
// src/components/ui/Alert.test.tsx
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { Alert } from './Alert';

describe('Alert Component', () => {
  it('renders with default variant', () => {
    render(<Alert>This is an alert</Alert>);
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
