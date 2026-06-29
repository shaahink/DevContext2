# DevContext Desktop

Cross-platform desktop client for DevContext — Tauri (native shell) + Angular 22 (zoneless) talking to
`DevContext.Server` over gRPC-Web, which wraps the `DevContext.Core` engine.

See **[AGENTS.md](./AGENTS.md)** for prerequisites, run/test commands, codegen, and architecture.

Quick start:

```bash
pnpm install
pnpm dev        # desktop window (server + tauri)
# or
pnpm dev:web    # browser at http://localhost:4200 (server + ng serve)
```
