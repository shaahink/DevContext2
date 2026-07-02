import { ApplicationConfig, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter, Routes } from '@angular/router';

const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./shell/workspace-shell').then((m) => m.WorkspaceShell),
    children: [
      { path: '', loadComponent: () => import('./features/pages/overview-page').then((m) => m.OverviewPage) },
      { path: 'overview', loadComponent: () => import('./features/pages/overview-page').then((m) => m.OverviewPage) },
      { path: 'entries', loadComponent: () => import('./features/pages/entries-page').then((m) => m.EntriesPage) },
      { path: 'trace', loadComponent: () => import('./features/pages/trace-page').then((m) => m.TracePage) },
      { path: 'graph', loadComponent: () => import('./features/pages/graph-page').then((m) => m.GraphPage) },
      { path: 'insights', loadComponent: () => import('./features/pages/insights-page').then((m) => m.InsightsPage) },
      { path: 'export', loadComponent: () => import('./features/pages/export-page').then((m) => m.ExportPage) },
      { path: 'settings', loadComponent: () => import('./features/settings/settings-view').then((m) => m.SettingsView) },
    ],
  },
];

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideZonelessChangeDetection(),
    provideRouter(routes),
  ],
};
