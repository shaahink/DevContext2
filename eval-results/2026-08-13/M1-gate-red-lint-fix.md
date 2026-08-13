# M1 — gate RED after s15, fixed (session #16)

Conductor ran the gate battery independently after session #15. Both `fast-app` and `battery`
came back **FAIL (exit 5, step 5 — app check)**, each having failed twice (retried once under SC4.1).
Everything before step 5 was green in both runs: build succeeded, contract sweep clean.

## The failure, as the battery reported it

```
--- Step 5: App check (pnpm check) ---
 > ng lint
 Linting "devcontext-app"...
 C:\Code\DevContext2-desktop\src\DevContext.App\src\app\features\pages\workbench-page.spec.ts
   136:24  error  Type literal only has a call signature, you should use a function type instead
                  @typescript-eslint/prefer-function-type
 ✖ 1 problem (1 error, 0 warnings)
  FAIL  pnpm check failed

GATE: FAIL (step 5 - app check)
```

## Root cause (measured, not inferred)

`workbench-page.spec.ts` is the file s15 **added** for the M1.2 dock resizer. Its `DockTestSurface`
helper interface declared:

```ts
dockWidthOverride: { (): number | null };     // line 136 — sole member is a call signature
```

`@typescript-eslint/prefer-function-type` rejects a type literal whose **only** member is a call
signature; it must be written as a function type. The near-miss one line up is worth recording,
because it explains why the pattern was not obviously wrong to the author:

```ts
dockLevel: { (): number; set(v: number): void };   // line 134 — LEGAL, it has a second member
```

Same brace-wrapped shape, different verdict. The rule fires on arity of the type literal, not on
the call signature.

Why s15 missed it: the handoff records that it typechecked specs with `tsc -p tsconfig.spec.json`
(~20s) instead of running the full app check. `tsc` accepts both spellings — they are the *same
type*. Only `ng lint` distinguishes them, and `pnpm build` never sees spec files at all. So no
check s15 ran could have caught this.

## The fix

One line, in the spec s15 added:

```ts
-  dockWidthOverride: { (): number | null };
+  dockWidthOverride: () => number | null;
```

Type-identical rewrite. **No test deleted or skipped, no expectation relaxed, no lint rule
disabled or downgraded, no eslint-disable comment, no gate command softened, no golden touched.**
The measurement is exactly as strong as it was before.

## Verification — `pnpm check` (the gate's step 5) re-run in full

`pnpm check` is `lint && test && build`, short-circuiting on the first failure. Because lint is
first, the red gate had **never executed** the test or build steps — so all three were re-run, not
just the one that was red.

```
> ng lint
Linting "devcontext-app"...
All files pass linting.

> ng test --watch=false
Application bundle generation complete. [11.277 seconds]
 RUN  v4.1.9 C:/Code/DevContext2-desktop/src/DevContext.App
 Test Files  25 passed (25)
      Tests  224 passed (224)
   Duration  21.60s

> ng build
Application bundle generation complete. [111.844 seconds] - 2026-08-13T23:11:36.745Z
Output location: C:\Code\DevContext2-desktop\src\DevContext.App\dist\devcontext-app
```

Full log: `.conductor/bg-logs/appcheck2-20260813-230839311.log` (process exited after 3m 1s).

Per trap #1 (filtered output can look clean while the command failed), the verdict is **not** read
off the happy-path lines above. Two independent exit-code-grade checks:

- `pnpm lint` re-run in the foreground: `All files pass linting.` / **`LINT-EXITCODE=0`** — a real
  exit code from the exact step that was red.
- The complete check log scanned for failure markers (`ELIFECYCLE`, `Command failed`, `error TS`,
  the `✖` lint marker, `FAIL`): **none present**. The red gate's log contained
  `ELIFECYCLE Command failed with exit code 1` twice; this one contains it zero times.

## Scope

This session changed exactly one line of one file. The five M1.2 hygiene deliverables themselves
were sound — `MapResponse.stack`, the lens-slot gating, the `createTab` cap fix, the dock resizer,
and theme selection all built and tested clean. What was untrue was the surrounding claim that the
stage was green; the lint gate had never been run against the new file.
