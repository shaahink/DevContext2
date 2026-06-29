use std::process::{Child, Command};
use std::sync::Mutex;

use tauri::{Manager, RunEvent};

/// Holds the managed .NET server child process so we can terminate it when the app exits.
struct ServerProcess(Mutex<Option<Child>>);

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .setup(|app| {
            if cfg!(debug_assertions) {
                app.handle().plugin(
                    tauri_plugin_log::Builder::default()
                        .level(log::LevelFilter::Info)
                        .build(),
                )?;
            }
            app.manage(ServerProcess(Mutex::new(spawn_server())));
            Ok(())
        })
        .build(tauri::generate_context!())
        .expect("error while building tauri application")
        .run(|app, event| {
            if let RunEvent::Exit = event {
                if let Some(state) = app.try_state::<ServerProcess>() {
                    if let Ok(mut guard) = state.0.lock() {
                        if let Some(mut child) = guard.take() {
                            let _ = child.kill();
                        }
                    }
                }
            }
        });
}

/// Spawns the DevContext .NET server as a managed child, when `DEVCONTEXT_SERVER_DLL` points at the
/// built server assembly. In local development the server is run separately (via `pnpm dev`), so the
/// variable is unset and we skip spawning — the app simply connects to the already-running server.
/// Packaged builds set the variable (P5: bundled self-contained sidecar) to own the lifecycle here.
fn spawn_server() -> Option<Child> {
    let dll = std::env::var("DEVCONTEXT_SERVER_DLL").ok()?;
    let urls = std::env::var("DEVCONTEXT_SERVER_URLS")
        .unwrap_or_else(|_| "http://127.0.0.1:5179".to_string());

    match Command::new("dotnet")
        .arg(&dll)
        .arg("--urls")
        .arg(&urls)
        .stdin(std::process::Stdio::null())
        .spawn()
    {
        Ok(child) => {
            log::info!("Spawned DevContext server from {dll} on {urls}");
            Some(child)
        }
        Err(err) => {
            log::error!("Failed to spawn DevContext server: {err}");
            None
        }
    }
}
