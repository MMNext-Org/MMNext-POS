import { describe, test, expect } from 'vitest';

// Sample unit test for domain logic
describe('sample logic', () => {
  test('should calculate correct total', () => {
    const items = [10, 20, 30];
    const total = items.reduce((sum, item) => sum + item, 0);
    expect(total).toBe(60);
  });
});