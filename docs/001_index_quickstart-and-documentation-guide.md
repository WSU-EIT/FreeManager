# FreeManager Quickstart & Documentation Guide

> **Read this first** when starting a new chat or task session.

---

## How to Use This Documentation

### File Naming Convention

All docs follow this pattern:
```
{###}_{tasktype}_{brief-description}.md
```

| Component | Purpose | Example |
|-----------|---------|---------|
| `###` | Order created (chronological) | `000`, `001`, `002` |
| `tasktype` | Category for grouping | `guide`, `plan`, `status` |
| `description` | What the file contains | `development-patterns-and-conventions` |

### Task Types

| Type | Use For | Example |
|------|---------|---------|
| `guide` | How-to documentation, conventions | Development patterns |
| `index` | Entry points, quickstarts | This file |
| `plan` | Vision, decisions, strategy | Project goals |
| `reference` | Architecture, diagrams, ERD, style guides | System design |
| `status` | Progress tracking, checklists | Implementation status |
| `roleplay` | Team discussion transcripts | Architecture reviews |
| `standup` | Daily/weekly status updates | Progress notes |
| `review` | Code review notes | PR feedback |
| `transcript` | Meeting/discussion records | Planning sessions |
| `brief` | CTO/stakeholder presentations | Decision requests |

### Reading Order for New Sessions

1. **This file (001)** - Understand the project structure
2. **000_guide** - FreeManager-specific patterns and conventions
3. **004_status** - Current progress and next tasks
4. **005_reference (style guide)** - C# coding conventions (reference when writing code)
5. **002_plan** - Vision and key decisions (if needed)
6. **003_reference** - Architecture details (if needed)

---

## Code Style Quick Reference

See [005_reference_csharp-style-guide.md](005_reference_csharp-style-guide.md) for full details. Key points:

| Topic | Rule |
|-------|------|
| Braces (classes) | New line |
| Braces (if/for/while) | Same line |
| Variables | Explicit type + `new()` not `var` |
| Private fields | `_camelCase` |
| Async methods | No `Async` suffix |
| File size | 0-300 ideal, 600 max |
| LINQ | Fluent syntax, one op per line |
| Null checks | Explicit `if (x == null)` |

---

## Project Structure: Two Codebases

This repo contains **two versions** of FreeCRM:

```
FreeManager/
+-- CRM/                    # <-- WORKING COPY (modify this)
+-- CRM.Client/             # <-- WORKING COPY
+-- CRM.DataAccess/         # <-- WORKING COPY
+-- CRM.DataObjects/        # <-- WORKING COPY
+-- CRM.EFModels/           # <-- WORKING COPY
+-- CRM.Plugins/            # <-- WORKING COPY
|
+-- FreeCRM-main/           # <-- STOCK REFERENCE (read-only)
|   +-- CRM/                #     Original unmodified FreeCRM
|   +-- CRM.Client/         #     Use to compare/reference
|   +-- ...                 #     DO NOT MODIFY
|
+-- docs/                   # <-- DOCUMENTATION
```

### When to Use Each

| Codebase | Purpose | Modify? |
|----------|---------|---------|
| Root projects (`CRM/`, etc.) | Active development | YES |
| `FreeCRM-main/` | Reference original patterns | NO |
| `docs/` | Documentation | YES |

### Why Two Copies?

- **FreeManager** adds features to FreeCRM using the `.App.` extension pattern
- The stock copy lets us see what's original vs. what we've added
- Helps when debugging: "Is this behavior from FreeCRM or our code?"

### FreeCRM-main as Source of Inspiration

**When you have questions about architecture, style, or structure, use `FreeCRM-main/` as your reference:**

- Look at existing Blazor components for UI patterns
- Check DataAccess methods for business logic patterns
- Review DataController for API endpoint conventions
- Examine EFModels for entity design patterns

This ensures FreeManager code stays consistent with the FreeCRM framework it extends.

---

## Key Files to Read First

### Understanding the Codebase

| File | Why Read It |
|------|-------------|
| `CRM/Program.cs` | Server startup, DI configuration |
| `CRM/CRM.csproj` | Server dependencies |
| `CRM.Client/CRM.Client.csproj` | Blazor WASM dependencies |
| `CRM.DataAccess/CRM.DataAccess.csproj` | Business logic dependencies |
| `CRM.EFModels/CRM.EFModels.csproj` | EF Core setup |

### Understanding FreeManager Additions

| File | What It Contains |
|------|------------------|
| `CRM.DataAccess/DataAccess.App.FreeManager.cs` | All FM_ business logic (~650 lines) |
| `CRM/Controllers/DataController.App.FreeManager.cs` | All FM_ API endpoints |
| `CRM.DataObjects/DataObjects.App.FreeManager.cs` | All FM DTOs |
| `CRM.EFModels/EFModels/FM*.cs` | FreeManager entities (4 files) |
| `CRM.EFModels/EFModels/EFDataModel.App.FreeManager.cs` | DbContext extension |

### Project READMEs

| File | Content |
|------|---------|
| `README.md` (root) | Project overview |
| `CRM/README.md` | Server project details |
| `CRM.Client/README.md` | Blazor client details |
| `CRM.DataAccess/README.md` | Business logic patterns |

---

## Documentation Index

| # | File | Type | Description |
|---|------|------|-------------|
| 000 | [Development Guide](000_guide_development-patterns-and-conventions.md) | Guide | FreeManager patterns, team roles |
| 001 | [Quickstart](001_index_quickstart-and-documentation-guide.md) | Index | This file |
| 002 | [Vision](002_plan_project-vision-and-decisions.md) | Plan | What we're building and why |
| 003 | [Architecture](003_reference_system-architecture-and-data-model.md) | Reference | Diagrams, ERD, structure |
| 004 | [Status](004_status_implementation-progress-and-checklist.md) | Status | Current phase, checklist |
| 005 | [C# Style Guide](005_reference_csharp-style-guide.md) | Reference | Coding conventions |
| 006 | [LLM Onboarding](006_roleplay_llm-onboarding-strategies.md) | Roleplay | Team discussion on prompts |
| 007 | [CTO Briefing](007_brief_phase2-kickoff-cto-briefing.md) | Brief | Phase 2 decisions needed |

---

## Starting a New Task

When beginning a new chat session, say:

```
Read the quickstart guide at docs/001_index_quickstart-and-documentation-guide.md,
then check docs/000_guide_development-patterns-and-conventions.md for code patterns,
and docs/004_status_implementation-progress-and-checklist.md for current progress.
When writing code, follow docs/005_reference_csharp-style-guide.md conventions.
For architecture/style questions, reference FreeCRM-main/ as the source of inspiration.

The task is: [describe your task here]
```

This ensures the assistant understands:
- Project structure (two codebases)
- FreeManager conventions (FM_ prefix, .App. pattern)
- Current status (what's done, what's next)
- C# style rules (braces, naming, file size limits)
- Where to look for patterns (FreeCRM-main)

---

## Quick Reference

### Commands

```bash
# Build
dotnet build

# Run server
cd CRM && dotnet run

# Add migration
cd CRM.EFModels && dotnet ef migrations add FM_MigrationName --startup-project ../CRM

# Apply migration
dotnet ef database update --startup-project ../CRM
```

### Key Conventions

| Convention | Rule |
|------------|------|
| FreeManager methods | `FM_` prefix |
| FreeManager entities | `FM` prefix (e.g., `FMProject`) |
| Extension files | `.App.FreeManager.cs` suffix |
| File length | 0-300 ideal, 500 large, 600 max |
| Never modify | Core FreeCRM files or `FreeCRM-main/` |
| Style reference | Use `FreeCRM-main/` for patterns |

### Current Status

| Phase | Status |
|-------|--------|
| **Phase 1: Data Foundation** | COMPLETE |
| **Phase 2: Editor UI** | NEXT |
| **Phase 3: Build System** | Pending |
| **Phase 4: Polish & Templates** | Pending |

See [004_status](004_status_implementation-progress-and-checklist.md) for details.
