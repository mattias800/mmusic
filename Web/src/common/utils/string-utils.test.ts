import { describe, it, expect } from 'vitest';
import { capitalizeFirstLetter, truncateString } from './string-utils';

describe('String Utils', () => {
  describe('capitalizeFirstLetter', () => {
    it('should capitalize the first letter of a string', () => {
      expect(capitalizeFirstLetter('hello')).toBe('Hello');
      expect(capitalizeFirstLetter('world')).toBe('World');
    });

    it('should return empty string when given empty string', () => {
      expect(capitalizeFirstLetter('')).toBe('');
    });

    it('should handle already capitalized strings', () => {
      expect(capitalizeFirstLetter('Hello')).toBe('Hello');
    });

    it('should handle single character strings', () => {
      expect(capitalizeFirstLetter('a')).toBe('A');
    });
  });

  describe('truncateString', () => {
    it('should truncate strings longer than maxLength', () => {
      expect(truncateString('Hello world', 5)).toBe('Hello...');
    });

    it('should not truncate strings shorter than or equal to maxLength', () => {
      expect(truncateString('Hello', 5)).toBe('Hello');
      expect(truncateString('Hi', 5)).toBe('Hi');
    });

    it('should handle empty strings', () => {
      expect(truncateString('', 5)).toBe('');
    });

    it('should handle maxLength of 0', () => {
      expect(truncateString('Hello', 0)).toBe('...');
    });
  });
});
