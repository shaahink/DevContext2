use std::net::TcpListener;
use std::process::{Child, Command, Stdio};
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::{Arc, Mutex};
use std::thread;
use std::time::Duration;

use tauri::webview::{Color, WebviewWindowBuilder};
use tauri::{Emitter, Manager, RunEvent, Theme, WebviewUrl};
use tauri_plugin_window_state::{StateFlags, WindowExt};

/// Backoff schedule for server crash-restart attempts (§7.1) — indexed by `attempt - 2`
/// (attempt 1 is the initial, unthrottled start). Capped at 5 total attempts so a genuinely
/// broken server DLL doesn't retry forever.
const BACKOFF_SECS: [u64; 3] = [1, 5, 15];
const MAX_ATTEMPTS: u32 = 5;

/// Holds the managed .NET server child process so we can terminate it when the app exits, plus
/// a flag telling the crash-restart supervisor thread to stop retrying once shutdown starts.
struct ServerProcess {
    child: Mutex<Option<Child>>,
    shutting_down: AtomicBool,
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        // Must be the first plugin registered (Tauri requirement). Second launch focuses the
        // existing window instead of opening a duplicate; a path argument (e.g. "Open with…"
        // from Explorer, or a second CLI invocation) is forwarded to the frontend as a
        // "single-instance-path" event to open as a new tab, not replace the current session.
        .plugin(tauri_plugin_single_instance::init(|app, args, _cwd| {
            if let Some(window) = app.get_webview_window("main") {
                let _ = window.unminimize();
                let _ = window.show();
                let _ = window.set_focus();
            }
            if let Some(path) = args.get(1) {
                let _ = app.emit("single-instance-path", path.clone());
            }
        }))
        .plugin(tauri_plugin_window_state::Builder::default().build())
        .plugin(tauri_plugin_fs::init())
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_clipboard_manager::init())
        .setup(|app| {
            if cfg!(debug_assertions) {
                app.handle().plugin(
                    tauri_plugin_log::Builder::default()
                        .level(log::LevelFilter::Info)
                        .build(),
                )?;
            }

            let server_process = Arc::new(ServerProcess {
                child: Mutex::new(None),
                shutting_down: AtomicBool::new(false),
            });

            // Only own the server's lifecycle when DEVCONTEXT_SERVER_DLL points at the built
            // assembly (packaged builds, or a dev override). In local dev (`pnpm dev`) the
            // server is run separately via concurrently on the fixed port 5179, this stays
            // unset, and the frontend's `config.ts` fallback to 127.0.0.1:5179 applies.
            let server_url = std::env::var("DEVCONTEXT_SERVER_DLL").ok().map(|dll| {
                let port = pick_free_port();
                let supervised = server_process.clone();
                thread::spawn(move || supervise(dll, port, supervised));
                format!("http://127.0.0.1:{port}")
            });

            app.manage(server_process);

            // No-flash startup (§7.2): window starts hidden with the dark base color instead
            // of the WebView2 default white, so there's nothing to flash before first paint.
            // Angular calls `getCurrentWindow().show()` once the app has rendered.
            let mut builder = WebviewWindowBuilder::new(app, "main", WebviewUrl::default())
                .title("DevContext")
                .inner_size(1280.0, 820.0)
                .min_inner_size(960.0, 640.0)
                .resizable(true)
                .decorations(false)
                .theme(Some(Theme::Dark))
                .visible(false)
                .background_color(Color(0x16, 0x18, 0x1d, 0xff));

            // Inject the sidecar's dynamically-picked port before any frontend script runs
            // (config.ts reads this global; falls back to :5179 when it's absent).
            if let Some(url) = &server_url {
                builder = builder.initialization_script(format!(
                    "window.__DEVCONTEXT_SERVER__ = {url:?};"
                ));
            }

            let window = builder.build()?;
            // Restore size/position/maximized from the previous session (§7.2). No-op on
            // first launch (nothing saved yet) — the builder's inner_size above still applies.
            let _ = window.restore_state(StateFlags::all());

            Ok(())
        })
        .build(tauri::generate_context!())
        .expect("error while building tauri application")
        .run(|app, event| {
            if let RunEvent::Exit = event {
                if let Some(state) = app.try_state::<Arc<ServerProcess>>() {
                    // Stop the supervisor from restarting before we kill its current child.
                    state.shutting_down.store(true, Ordering::SeqCst);
                    if let Ok(mut guard) = state.child.lock() {
                        if let Some(mut child) = guard.take() {
                            let _ = child.kill();
                        }
                    }
                }
            }
        });
}

/// Picks an OS-assigned free loopback port by binding to port 0 and reading it back, then
/// dropping the listener before the caller spawns the real server on that number. There's a
/// brief window where another process could grab it first, but this is the standard portpicker
/// pattern and avoids pulling in an extra crate for it.
fn pick_free_port() -> u16 {
    TcpListener::bind("127.0.0.1:0")
        .and_then(|listener| listener.local_addr())
        .map(|addr| addr.port())
        .unwrap_or(5179)
}

/// Owns the server's full lifecycle on a background thread: spawn, detect exit (via polling
/// `try_wait` rather than a blocking `wait`, so the mutex is never held across a long wait —
/// `RunEvent::Exit` needs to acquire it at any time to kill the child on quit), and restart with
/// backoff on an unexpected exit. Runs until `shutting_down` is set or `MAX_ATTEMPTS` is hit.
fn supervise(dll: String, port: u16, state: Arc<ServerProcess>) {
    let mut attempt = 0u32;
    loop {
        attempt += 1;
        if attempt > MAX_ATTEMPTS {
            log::error!("DevContext server failed to stay up after {attempt} attempts, giving up");
            return;
        }
        if attempt > 1 {
            let backoff = BACKOFF_SECS[(attempt - 2).min(2) as usize];
            log::warn!("DevContext server down, restarting in {backoff}s (attempt {attempt})");
            thread::sleep(Duration::from_secs(backoff));
        }
        if state.shutting_down.load(Ordering::SeqCst) {
            return;
        }

        let child = spawn_child(&dll, port);
        *state.child.lock().unwrap() = child;

        loop {
            if state.shutting_down.load(Ordering::SeqCst) {
                return;
            }
            let exited = {
                let mut guard = state.child.lock().unwrap();
                match guard.as_mut() {
                    None => true, // spawn_child failed outright
                    Some(child) => matches!(child.try_wait(), Ok(Some(_)) | Err(_)),
                }
            };
            if exited {
                break;
            }
            thread::sleep(Duration::from_millis(500));
        }
    }
}

/// Spawns the DevContext .NET server as a managed child on the given port, with
/// below-normal process priority so the UI stays responsive under Roslyn compilation.
fn spawn_child(dll: &str, port: u16) -> Option<Child> {
    match Command::new("dotnet")
        .arg(dll)
        .arg("--urls")
        .arg(format!("http://127.0.0.1:{port}"))
        .stdin(Stdio::null())
        .spawn()
    {
        Ok(mut child) => {
            log::info!("Spawned DevContext server from {dll} on port {port}");

            #[cfg(target_os = "windows")]
            set_below_normal_priority(&mut child);

            Some(child)
        }
        Err(err) => {
            log::error!("Failed to spawn DevContext server: {err}");
            None
        }
    }
}

#[cfg(target_os = "windows")]
fn set_below_normal_priority(child: &mut Child) {
    use std::os::windows::io::AsRawHandle;
    use windows::Win32::System::Threading::{
        SetPriorityClass, BELOW_NORMAL_PRIORITY_CLASS,
    };
    use windows::Win32::Foundation::HANDLE;

    let handle = child.as_raw_handle();
    if handle.is_null() { return; }
    unsafe {
        let _ = SetPriorityClass(
            HANDLE(handle as *mut _),
            BELOW_NORMAL_PRIORITY_CLASS,
        );
    }
}
