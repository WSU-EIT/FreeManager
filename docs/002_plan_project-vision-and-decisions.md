# FreeManager: Project Vision & Key Decisions

> What we're building, why, and the major decisions that shaped it.

---

## What is FreeManager?

**FreeManager** is a web-based platform that lets users create custom applications based on the FreeCRM framework by editing only `.App.` customization files in a browser-based editor.

### The Problem

Building a FreeCRM-based application today requires:
1. Clone the repo
2. Run Windows-only `.exe` tools to rename/remove modules
3. Edit files locally in VS Code or Visual Studio
4. Manually manage Git commits
5. Run `dotnet publish` to build
6. Deploy somewhere

**This is too complex for many users** who just want to add business logic.

### The Solution

FreeManager automates the infrastructure:

| Traditional | FreeManager |
|-------------|-------------|
| Clone repo manually | Click "New Project" |
| Edit in local IDE | Edit in browser (Monaco) |
| Manage Git yourself | Automatic versioning |
| Run build commands | Click "Build" |
| Deploy manually | Download ZIP artifact |

Users focus on **business logic**. We handle **infrastructure**.

---

## Key Architectural Decisions

### Decision 1: Database-First Storage (Not Git)

**Choice:** Store `.App.` files in SQL database tables, not Git branches.

**Why:**
- Simpler - no GitHub API complexity
- Faster - database queries vs. network calls
- Full control - we own versioning, no merge conflicts
- Self-contained - single deployable application

**Trade-off:** No native Git collaboration. We implement our own version history.

### Decision 2: Clone FreeCRM at Build Time Only

**Choice:** Never store FreeCRM framework code. Clone fresh from GitHub only when building.

**Why:**
- Framework stays immutable and up-to-date
- No storage of thousands of framework files per project
- Clean build environment every time
- Easy to update framework version

**Trade-off:** Builds are slower (must clone first). Acceptable for our use case.

### Decision 3: Dogfooding with .App. Pattern

**Choice:** Build FreeManager itself using the `.App.` extension pattern it enables.

**Why:**
- Proves the pattern works for real applications
- We experience the same constraints as users
- Validates that `.App.` files are sufficient for complex features
- Single codebase - FreeManager IS a FreeCRM application

**Files we created:**
- `DataAccess.App.FreeManager.cs`
- `DataController.App.FreeManager.cs`
- `DataObjects.App.FreeManager.cs`
- `EFDataModel.App.FreeManager.cs`

### Decision 4: FM_ Prefix Convention

**Choice:** All FreeManager-specific code uses `FM_` prefix for methods and `FM` prefix for entities.

**Why:**
- Instant identification of FreeManager code vs. FreeCRM core
- Prevents naming collisions with future FreeCRM updates
- Easy to search/find all FreeManager code
- Consistent with FreeCRM's existing patterns

### Decision 5: Tenant Isolation

**Choice:** All data is scoped by `TenantId`. Every query filters by tenant.

**Why:**
- Multi-tenant from day one
- Users only see their own projects
- Security built into the data layer
- Follows existing FreeCRM patterns

---

## Success Criteria

### Minimum Viable Product (MVP)

- [ ] Can create a project with custom name
- [ ] Can edit `.App.` files in browser with Monaco editor
- [ ] Can view file version history
- [ ] Can build and download artifact ZIP
- [ ] Built entirely using `.App.` pattern (dogfooding proof)

### Future Enhancements

- Live preview of Razor components
- IntelliSense in Monaco editor
- Template library for common patterns
- Direct deployment to Azure/IIS
- Collaboration features

---

## Technical Stack

| Layer | Technology |
|-------|------------|
| Frontend | Blazor WebAssembly |
| Backend | ASP.NET Core 10 |
| Database | SQL Server (also PostgreSQL, MySQL, SQLite) |
| ORM | Entity Framework Core 10 |
| Editor | Monaco (VS Code's editor) |
| Build | `dotnet publish` + Windows tools |

---

## Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Windows-only `.exe` tools | Run builds in Windows container/runner |
| Large file content in DB | Use `NVARCHAR(MAX)`, consider compression later |
| Concurrent edits | Optimistic concurrency with version field |
| Build failures | Detailed logs, clear error messages |
| Framework updates break builds | Pin to known-good FreeCRM commit |
