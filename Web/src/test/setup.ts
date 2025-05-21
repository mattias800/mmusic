import "@testing-library/jest-dom";

// Mock global objects or APIs if needed
// For example, if you need to mock window.matchMedia:
/*
Object.defineProperty(window, 'matchMedia', {
  writable: true,
  value: vi.fn().mockImplementation(query => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: vi.fn(),
    removeListener: vi.fn(),
    addEventListener: vi.fn(),
    removeEventListener: vi.fn(),
    dispatchEvent: vi.fn(),
  })),
});
*/

// Add any global setup needed for tests
