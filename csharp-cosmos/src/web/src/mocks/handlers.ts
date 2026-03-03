import { http, HttpResponse } from 'msw';

const API_BASE = 'http://localhost:3100';

// In-memory store for mocked entities (e.g. items). Extend as needed for your API.
const store: Map<string, { id: string; [key: string]: unknown }> = new Map();

export const handlers = [
  // GET list
  http.get(`${API_BASE}/api/items`, () => {
    const items = Array.from(store.values());
    return HttpResponse.json(items);
  }),

  // GET by id
  http.get(`${API_BASE}/api/items/:id`, ({ params }) => {
    const item = store.get(params.id as string);
    if (!item) return new HttpResponse(null, { status: 404 });
    return HttpResponse.json(item);
  }),

  // POST create
  http.post(`${API_BASE}/api/items`, async ({ request }) => {
    const body = (await request.json()) as { id?: string; [key: string]: unknown };
    const id = body.id ?? crypto.randomUUID();
    const item = { ...body, id, created: new Date().toISOString(), updated: new Date().toISOString() };
    store.set(id, item);
    return HttpResponse.json(item, { status: 201 });
  }),

  // PUT update
  http.put(`${API_BASE}/api/items/:id`, async ({ params, request }) => {
    const existing = store.get(params.id as string);
    if (!existing) return new HttpResponse(null, { status: 404 });
    const body = (await request.json()) as { [key: string]: unknown };
    const item = { ...existing, ...body, updated: new Date().toISOString() };
    store.set(params.id as string, item);
    return HttpResponse.json(item);
  }),

  // PATCH
  http.patch(`${API_BASE}/api/items/:id`, async ({ params, request }) => {
    const existing = store.get(params.id as string);
    if (!existing) return new HttpResponse(null, { status: 404 });
    const body = (await request.json()) as { [key: string]: unknown };
    const item = { ...existing, ...body, updated: new Date().toISOString() };
    store.set(params.id as string, item);
    return HttpResponse.json(item);
  }),

  // DELETE
  http.delete(`${API_BASE}/api/items/:id`, ({ params }) => {
    if (!store.has(params.id as string)) return new HttpResponse(null, { status: 404 });
    store.delete(params.id as string);
    return new HttpResponse(null, { status: 204 });
  }),
];
