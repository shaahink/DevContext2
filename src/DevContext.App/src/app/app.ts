import { afterNextRender, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { isTauri } from './core/tauri-env';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  template: '<router-outlet />',
})
export class App {
  constructor() {
    // No-flash startup (§7.2): the Rust side creates the window hidden; show it once the
    // shell has actually painted instead of the WebView2-default blank/white frame.
    afterNextRender(() => {
      if (!isTauri()) return;
      void import('@tauri-apps/api/window').then(({ getCurrentWindow }) => getCurrentWindow().show());
    });
  }
}
