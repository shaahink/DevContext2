import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter, Routes } from '@angular/router';

const routes: Routes = [
  { path: '', loadComponent: () => import('./features/source/source-view').then((m) => m.SourceView) },
  { path: 'overview', loadComponent: () => import('./features/overview/overview-view').then((m) => m.OverviewView) },
  { path: 'entries', loadComponent: () => import('./features/entries/entries-view').then((m) => m.EntriesView) },
  { path: 'browse', loadComponent: () => import('./features/browse/browse-view').then((m) => m.BrowseView) },
  { path: 'trace', loadComponent: () => import('./features/trace/trace-view').then((m) => m.TraceView) },
  { path: 'document', loadComponent: () => import('./features/document/document-view').then((m) => m.DocumentView) },
  { path: 'stats', loadComponent: () => import('./features/stats/stats-view').then((m) => m.StatsView) },
  { path: 'insights', loadComponent: () => import('./features/insights/insights-view').then((m) => m.InsightsView) },
  { path: 'graph', loadComponent: () => import('./features/graph/graph-view').then((m) => m.GraphView) },
  { path: 'cache', loadComponent: () => import('./features/cache/cache-view').then((m) => m.CacheView) },
  { path: '**', redirectTo: '' },
];

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
  ],
};
