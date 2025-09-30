# One-paragraph

We’re adding a self-contained “.app file explorer” to the FreeManager app that lives entirely in the existing extension points—no framework changes. The feature introduces a new `Index.app.razor` page that lists all `*.app.*` files in the repo and previews their contents with the built-in Monaco editor, plus a small `DataController.App.*.cs` partial that exposes read-only endpoints to enumerate and fetch those files. This gives us a safe, non-invasive foundation for later in-browser editing while keeping to the core rule: only touch `.app.` files or add new ones.

---

# Two paragraphs

The goal is to centralize all app-specific customization inside `.app.` files and provide a first-class UI to discover and view them. We’ll add an `Index.app.razor` component that scans for `*.app.*` files, groups them by folder, and shows a Monaco preview of the selected file. On the server, we’ll add a small `DataController.App.{Feature}.cs` partial with minimal endpoints—`/api/AppFiles/List` and `/api/AppFiles/Contents`—that are path-validated and read-only. This mirrors the existing extension pattern and avoids any modification to the base framework or shared plumbing.

Why this matters: teams often inherit the framework (auth, layout, plumbing) but only control the `.app.` layer. By shipping a discoverable, safe, and extensible viewer inside that layer, we accelerate onboarding, reduce “where is this customized?” time, and create a natural place to evolve toward editing, diffs, and git workflow—all without touching the core. It’s a pragmatic, low-risk step that immediately helps today and sets us up for tomorrow.

---

# One-page

## What we’re doing

We’re implementing a self-contained “.app file explorer” inside FreeManager that lists and previews all `*.app.*` files in the repository. The UI lives in a new `Index.app.razor` page and uses Monaco for read-only viewing. A small server-side `DataController.App.{Feature}.cs` partial provides two endpoints for listing and fetching files. Everything is additive and constrained to the `.app.` layer—no framework code is modified.

## Why we’re doing it

FreeManager’s architecture intentionally isolates product customization in `.app.` files so multiple solutions can share a common framework (auth, API wiring, layouts). In practice, engineers spend time hunting for those customization points across projects. By surfacing a built-in explorer and previewer:

* New contributors can immediately see “what’s actually customized here.”
* Reviewers get one place to sanity-check changes.
* We lay groundwork for future editing/diff without architecture churn.

## Design constraints

* **Zero framework changes**: only add or edit `.app.` files.
* **Security first**: server validates all paths against the content root; no directory traversal; only `*.app.*` files are returned.
* **Minimal surface**: two endpoints—`/api/AppFiles/List` and `/api/AppFiles/Contents`—and one page—`Index.app.razor`.
* **Familiar patterns**: reuse the existing `Helpers.GetOrPost<T>` pattern and Monaco component already present in the stack.

## How it works (high level)

* **Server (partial controller):**

  * `GET /api/AppFiles/List` enumerates `*.app.*` under the content root (excluding `bin/`, `obj/`, `.git/`, `node_modules/`, and typically `wwwroot/`), returning `FileName`, relative `FullPath`, and basic metrics (lines, chars).
  * `POST /api/AppFiles/Contents` accepts a list of relative paths, validates each path stays under the content root and matches the `.app.` pattern, and returns file contents.

* **Client (Index.app.razor):**

  * Loads the list, groups by folder, filters by path/name, and shows Monaco read-only preview for the selected file.
  * UI is intentionally simple and fast; later we can add Save/Diff without reshaping the page.

## Non-goals (for this phase)

* Editing/saving changes, git commits, or PR automation.
* Searching across non-`.app.` files or exposing raw framework internals.

## Roadmap (next steps)

1. **Save + Diff**: add `/api/AppFiles/Save` with optimistic concurrency; enable Monaco write mode.
2. **Search & Replace**: server-side grep with match previews and navigation.
3. **Git QoL**: surface `git status/diff` for `.app.` files; quick commit from UI.
4. **Live reload**: SignalR notifications when files change on disk.

---

# Full write-up (README style)

## Overview

This feature adds an in-app explorer for `.app.` customization files in FreeManager. It lists every `*.app.*` file, groups by folder, and displays a read-only preview using Monaco. The implementation is **fully additive** and respects the project rule: **do not modify the base framework**—only add or adjust `.app.` files.

## Rationale

* **Clarity**: Immediately see what’s customized in this deployment without spelunking.
* **Onboarding**: New teammates can orient faster by browsing all `.app.` touchpoints.
* **Extensibility**: Establishes the natural home for future in-browser editing, diffs, and git operations.
* **Safety**: Stays within the guardrails (path validation, read-only first) and doesn’t couple to framework internals.

## Scope & Non-Goals

**In scope**

* A new page `Index.app.razor` that lists and previews `.app.` files.
* A small server partial controller exposing:

  * `GET /api/AppFiles/List`
  * `POST /api/AppFiles/Contents`
* File enumeration limited to `*.app.*` under the content root, excluding `bin`, `obj`, `.git`, `node_modules` (and typically `wwwroot` assets).

**Out of scope (initially)**

* Saving/formatting files, git commits, branching, or PR creation.
* Modifying framework code or cross-cutting plumbing.

## Architecture

### Client

* **Component**: `Shared/AppComponents/Index.app.razor`

* **Responsibilities**:

  * Fetch list of `.app.` files (`/api/AppFiles/List`)
  * Render grouped list with filter
  * Fetch and preview content for the selected file (`/api/AppFiles/Contents`)
  * Use Monaco in read-only mode

* **UX behaviors**:

  * Deterministic sorting (folder → file)
  * Simple filter (path or filename substring)
  * Busy/empty states and safe fallbacks

### Server

* **Partial controller**: `Controllers/DataController.App.{Feature}.cs`

* **Endpoints**:

  * `GET /api/AppFiles/List`
    Returns `[ { FileName, FullPath (relative), LineCount, CharCount } ]`.
  * `POST /api/AppFiles/Contents`
    Body: `{ "FilePaths": [ "relative/path/one", ... ] }`
    Returns `[ { FileName, FullPath, Content } ]`.

* **Security & validation**:

  * Rebuild absolute paths from the content root and ensure `StartsWith(contentRoot)`.
  * Only allow files that match `*.app.*` (case-insensitive).
  * Ignore requests to non-existent or excluded locations.
  * No writes in this phase.

## Implementation Notes

* **No framework edits**: New functionality is delivered via `.app.` partials/pages only.
* **Performance**: Enumeration is lightweight; we can add caching if needed later.
* **Consistency**: Uses `Helpers.GetOrPost<T>` and existing Monaco integration, mirroring patterns from other `.app.` pages.
* **Accessibility**: The list groups improve scanability; Monaco’s built-in features aid readability.

## Setup / Usage

1. Build and run FreeManager as usual.
2. Navigate to the page hosting `Index.app.razor` (typically the home view when `Model.View == "home"`).
3. Use the filter to narrow file lists; click a file to preview its contents in Monaco.

> If namespaces or paths differ in your environment, adjust the partial controller’s namespace to match the project’s `DataController` home.

## Future Work

* **Write path**: Add `/api/AppFiles/Save` with ETag/hash for optimistic concurrency, plus Monaco write mode and diff preview.
* **Search**: Server-side regex with match previews; jump between occurrences.
* **Git integration**: Show changed `.app.` files first; one-click commit with message; optional branch/PR creation.
* **Live updates**: File watcher + SignalR to refresh list/preview on disk changes.

## FAQ

**Q: Why not just let folks use their IDE?**
A: They still can. This viewer is about **discoverability** and shared context, especially for reviewers and non-IDE stakeholders.

**Q: Can we view non-`.app.` files?**
A: Intentionally no. This tool is for app-layer customization only, which keeps scope focused and risk low.

**Q: How hard is it to enable editing later?**
A: Straightforward—add a `Save` endpoint and flip Monaco to writeable. We’ll include concurrency checks and optional git hooks to keep it safe.

**Q: Is this safe to expose anonymously?**
A: You can gate the endpoints with the same auth policy as other app pages. By default we recommend allowing **authenticated** access only.

---

If you want these wrapped into actual files (README.md snippets or an ADR), I can spit out ready-to-commit markdown next.
