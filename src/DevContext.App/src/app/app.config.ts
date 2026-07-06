import { ApplicationConfig, isDevMode, provideBrowserGlobalErrorListeners, provideZonelessChangeDetection } from '@angular/core';
import { provideRouter, Routes, type RedirectFunction } from '@angular/router';

/** Deep-link compat (proposal §8.3): old per-view routes redirect into the Workbench,
 * preserving `?focus` where present so bookmarked/shared trace links keep working. */
const redirectToExplore: RedirectFunction = ({ queryParams }) =>
  queryParams['focus'] ? `/explore?focus=${encodeURIComponent(queryParams['focus'])}` : '/explore';

const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./shell/workspace-shell').then((m) => m.WorkspaceShell),
    children: [
      { path: '', loadComponent: () => import('./features/pages/home-page').then((m) => m.HomePage) },
      { path: 'overview', redirectTo: '/' },
      { path: 'entries', redirectTo: redirectToExplore },
      { path: 'trace', redirectTo: redirectToExplore },
      { path: 'graph', redirectTo: redirectToExplore },
      { path: 'export', redirectTo: '/explore' },
      // F-proposal Workbench (deck │ stage │ inspector) — canonical since the W4 cutover.
      { path: 'explore', loadComponent: () => import('./features/pages/workbench-page').then((m) => m.WorkbenchPage) },
      { path: 'atlas', loadComponent: () => import('./features/pages/atlas-page').then((m) => m.AtlasPage) },
      { path: 'insights', loadComponent: () => import('./features/pages/insights-page').then((m) => m.InsightsPage) },
      { path: 'mcp', loadComponent: () => import('./features/pages/mcp-page').then((m) => m.McpPage) },
      { path: 'settings', loadComponent: () => import('./features/settings/settings-view').then((m) => m.SettingsView) },
      // Dev-only token sheet + component gallery (proposal W0.4) — never in a prod build's nav.
      ...(isDevMode()
        ? [{ path: 'styleguide', loadComponent: () => import('./features/pages/styleguide-page').then((m) => m.StyleguidePage) }]
        : []),
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
