# Example: Intent Inference

**Intent**: Let the tool figure out the right mode and profile from natural language.

**Command**:
```bash
# Debug a failing endpoint
devcontext analyze . --task "debug why is this endpoint returning 500"

# Architecture overview
devcontext analyze . --task "architecture overview of the blog platform"

# DI audit
devcontext analyze . --task "di injection wiring middleware pipeline"
```

---

## How intent inference works

DevContext uses keyword matching against your intent text:

| Keywords in your intent | Mode | Profile | Why |
|-------------------------|------|---------|-----|
| `debug`, `why`, `failing`, `error`, `exception`, `500`, `trace`, `call graph` | Trace | Debug | These suggest you're troubleshooting a specific issue — call graph helps trace the problem |
| `add`, `implement`, `similar`, `like`, `crud`, `new endpoint`, `architecture`, `overview`, `structure`, `layers`, `map` | Overview | Focused | These suggest you want a broad picture or are adding a new feature |
| `di`, `injection`, `reflect`, `activator`, `register`, `middleware`, `pipeline`, `wiring` | Overview | Debug | DI/middleware analysis benefits from detailed dependency tracing |
| `event`, `message`, `publish`, `consume`, `queue`, `bus` | Trace | Focused | Event/message flow is entry-point focused |

**Fallback**: If no keywords match, defaults to Overview + Focused.

---

## Desktop UI behavior

In the Desktop UI, the **Intent** field is in the ConfigPanel sidebar:

```
Intent (optional)
[debug why is this endpoint returning 500        ]
```

As you type and press Enter or move focus away, the Mode toggle and Section checkboxes update automatically:

| Intent text | Mode auto-selects | Sections auto-checked |
|------------|-------------------|----------------------|
| `"debug why..."` | **Trace** toggled | Call graph, Endpoints, MediatR Handlers, DI/Wiring |
| `"architecture..."` | **Overview** toggled | Architecture overview, Endpoints, MediatR Handlers, Data model, DI/Wiring, Related types |
| `"di wiring..."` | Overview toggled | DI/Wiring, Architecture overview |

The profile is derived automatically: checking Call graph enables Debug, checking Source code enables Full.

---

## When to use `--task` vs explicit flags

| Use `--task` when... | Use explicit flags when... |
|---------------------|--------------------------|
| You're not sure which mode to pick | You know exactly what you want |
| You want a quick, reasonable default | You need precise control over sections |
| You're using a natural language prompt anyway | You're scripting/automating |
| You're new to the tool | You've learned the modes and want fine-grained control |

The `--task` field does NOT replace explicit flags — if both are provided, `--task` sets defaults and explicit flags (`--scenario`, `--profile`) override them.
