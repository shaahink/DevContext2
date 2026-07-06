import type { EntryVm } from '../../models/view-models';

export interface TableColumn {
  readonly key: string;
  readonly label: string;
  readonly tooltip: string;
  readonly width: number; // px
  readonly sortable: boolean;
  /** Extract the display value for this column from an entry. */
  readonly value: (entry: EntryVm) => string;
}

/** Web/microservices archetype (the dogfood repo default). */
export const WEB_COLUMNS: readonly TableColumn[] = [
  {
    key: 'method', label: 'Method', tooltip: 'HTTP method or entry trigger', width: 72,
    sortable: true,
    value: (e) => e.httpMethod ?? '',
  },
  {
    key: 'route', label: 'Route / Entry', tooltip: 'The entry point route or identifier', width: 200,
    sortable: true,
    value: (e) => e.route || e.title,
  },
  {
    key: 'target', label: 'Target Handler', tooltip: 'The resolved handler this entry flows to', width: 180,
    sortable: true,
    value: (e) => e.target ?? '',
  },
  {
    key: 'project', label: 'Service', tooltip: 'The runnable service this entry belongs to', width: 120,
    sortable: true,
    value: (e) => e.project ?? '',
  },
  {
    key: 'kind', label: 'Kind', tooltip: 'Entry kind (HTTP, Bus consumer, gRPC…)', width: 100,
    sortable: true,
    value: (e) => e.kind,
  },
  {
    key: 'provenance', label: 'Resolution', tooltip: 'How the target was resolved (Semantic / Syntactic)', width: 90,
    sortable: true,
    value: (e) => e.provenance ?? '',
  },
  {
    key: 'auth', label: 'Auth', tooltip: 'Authorization attributes on this entry', width: 80,
    sortable: true,
    value: (e) => (e.authAttributes?.length ? e.authAttributes.join(', ') : '—'),
  },
];

/** Library archetype. */
export const LIBRARY_COLUMNS: readonly TableColumn[] = [
  {
    key: 'name', label: 'Type', tooltip: 'Public type name', width: 200,
    sortable: true,
    value: (e) => e.title,
  },
  {
    key: 'kind', label: 'Kind', tooltip: 'Entry kind', width: 100,
    sortable: true,
    value: (e) => e.kind,
  },
  {
    key: 'project', label: 'Assembly', tooltip: 'The assembly this type lives in', width: 140,
    sortable: true,
    value: (e) => e.project ?? '',
  },
  {
    key: 'provenance', label: 'Resolution', tooltip: 'Resolution tier', width: 90,
    sortable: true,
    value: (e) => e.provenance ?? '',
  },
];

/** Desktop archetype. */
export const DESKTOP_COLUMNS: readonly TableColumn[] = [
  {
    key: 'name', label: 'Entry', tooltip: 'View / command / event name', width: 200,
    sortable: true,
    value: (e) => e.title,
  },
  {
    key: 'kind', label: 'Kind', tooltip: 'Entry kind', width: 120,
    sortable: true,
    value: (e) => e.kind,
  },
  {
    key: 'project', label: 'Module', tooltip: 'Module this belongs to', width: 140,
    sortable: true,
    value: (e) => e.project ?? '',
  },
  {
    key: 'target', label: 'ViewModel', tooltip: 'Target viewmodel / command handler', width: 180,
    sortable: true,
    value: (e) => e.target ?? '',
  },
];
