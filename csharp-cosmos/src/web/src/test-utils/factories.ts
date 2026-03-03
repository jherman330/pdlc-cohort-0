import type { Entity } from '../services/restService';

let idCounter = 0;

/**
 * Creates a unique string ID for test entities.
 */
export function createId(prefix = 'id'): string {
  idCounter += 1;
  return `${prefix}-${idCounter}-${Date.now()}`;
}

/**
 * Builds an Entity with optional overrides. Useful for unit tests and MSW handlers.
 */
export function buildEntity<T extends Entity>(overrides: Partial<T> = {}): T {
  const now = new Date();
  return {
    id: createId(),
    created: now,
    updated: now,
    ...overrides,
  } as T;
}

/**
 * Builds multiple entities. Optional second argument to customize each item.
 */
export function buildEntityList<T extends Entity>(
  count: number,
  overridesOrFn: Partial<T> | ((index: number) => Partial<T>) = {}
): T[] {
  return Array.from({ length: count }, (_, i) => {
    const overrides = typeof overridesOrFn === 'function' ? overridesOrFn(i) : overridesOrFn;
    return buildEntity<T>(overrides);
  });
}
