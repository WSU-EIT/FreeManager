# FreeManager Project Vision

## What Is FreeManager?

FreeManager is a **web-based development platform** that enables users to create custom applications based on the FreeCRM framework by editing only the `.App.` customization files.

Think of it as **"FreeCRM as a Service"** - users focus purely on their business logic while the platform handles all infrastructure concerns.

## The Problem

Creating a new project from FreeCRM today requires:

1. Cloning the FreeCRM repository
2. Running Windows-only rename/remove tools
3. Understanding the full codebase structure
4. Managing Git operations manually
5. Setting up build/deploy pipelines

This creates friction and requires technical expertise that limits adoption.

## The Solution

A web application that:

1. **Abstracts complexity** - Click "Create Project", enter a name, select modules
2. **Leverages existing automation** - Uses FreeCRM's GitHub Actions for rename/remove
3. **Exposes only `.App.` files** - Enforces the modular pattern by design
4. **Provides a code editor** - Monaco-based editing in the browser
5. **Handles builds** - Triggers GitHub Actions, provides artifact downloads

## Core Principles

### 1. FreeCRM Is Immutable

We never store or modify core FreeCRM files. The existing GitHub Actions workflow clones `main`, runs the tools, and pushes to a `{ProjectName}_base` branch. We build on top of that.

### 2. `.App.` Files ARE The Project

A user's project is defined entirely by their `.App.` files:
- `DataObjects.App.{ProjectName}.cs` - Custom DTOs
- `DataAccess.App.{ProjectName}.cs` - Business logic
- `DataController.App.{ProjectName}.cs` - API endpoints
- `*.App.razor` - UI customizations
- `GlobalSettings.App.cs` - Configuration

Everything else comes from FreeCRM automatically.

### 3. Git Is The Source of Truth

Every project is a Git branch. Changes are commits. History is preserved. Collaboration is built-in.

### 4. Leverage Existing Infrastructure

FreeCRM already has GitHub Actions that:
- Run the rename/remove executables
- Create and push to `*_base` branches
- Build and test projects
- Upload artifacts

We trigger these via API rather than rebuilding the functionality.

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                     FreeManager Web UI                           │
│                    (Blazor WebAssembly)                         │
│                                                                  │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌───────────┐ │
│  │  Dashboard  │ │   Project   │ │   Monaco    │ │   Build   │ │
│  │   (list)    │ │   Wizard    │ │   Editor    │ │   Status  │ │
│  └─────────────┘ └─────────────┘ └─────────────┘ └───────────┘ │
├─────────────────────────────────────────────────────────────────┤
│                     FreeManager API                              │
│                   (ASP.NET Core)                                │
│                                                                  │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌───────────┐ │
│  │  Project    │ │   File      │ │   GitHub    │ │   Build   │ │
│  │  Service    │ │   Service   │ │   Service   │ │   Service │ │
│  └──────┬──────┘ └──────┬──────┘ └──────┬──────┘ └─────┬─────┘ │
│         │               │               │               │       │
├─────────┴───────────────┴───────────────┴───────────────┴───────┤
│                                                                  │
│                    GitHub API (Octokit)                         │
│                                                                  │
│  • workflow_dispatch (trigger rename/build)                     │
│  • repos/contents (read/write .App. files)                      │
│  • repos/branches (list project branches)                       │
│  • actions/runs (check workflow status)                         │
│  • actions/artifacts (download builds)                          │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                                                                  │
│                    GitHub: WSU-EIT/FreeCRM                       │
│                                                                  │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │ main branch                                                  ││
│  │ └── rename-and-branch.yml (existing workflow)               ││
│  │     └── Runs: Remove modules → Rename → Push to *_base      ││
│  └─────────────────────────────────────────────────────────────┘│
│                              │                                   │
│                              ▼                                   │
│  ┌─────────────────────────────────────────────────────────────┐│
│  │ {ProjectName}_base branches                                  ││
│  │                                                              ││
│  │ CustomerApp_base/                                            ││
│  │ ├── CustomerApp/           (renamed server project)         ││
│  │ ├── CustomerApp.Client/    (renamed client project)         ││
│  │ ├── CustomerApp.DataAccess/                                 ││
│  │ │   ├── DataAccess.cs                 (core - untouched)    ││
│  │ │   ├── DataAccess.App.cs             (hooks)               ││
│  │ │   └── DataAccess.App.CustomerApp.cs (USER'S CODE)         ││
│  │ └── ...                                                      ││
│  └─────────────────────────────────────────────────────────────┘│
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

## User Workflow

### Creating a New Project

```
User                          FreeManager                      GitHub
  │                               │                               │
  │  "Create Project:             │                               │
  │   CustomerApp,                │                               │
  │   keep: Tags, Appointments"   │                               │
  │ ─────────────────────────────►│                               │
  │                               │  POST workflow_dispatch       │
  │                               │  {name: CustomerApp,          │
  │                               │   selections: keep:Tags,...}  │
  │                               │ ─────────────────────────────►│
  │                               │                               │
  │                               │         (Action runs)         │
  │                               │          Clone main           │
  │                               │          Remove modules       │
  │                               │          Rename project       │
  │                               │          Push to branch       │
  │                               │                               │
  │                               │◄───── Workflow complete ──────│
  │                               │                               │
  │                               │  GET branch contents          │
  │                               │ ─────────────────────────────►│
  │                               │◄───── .App. file list ────────│
  │                               │                               │
  │◄───── Project ready! ─────────│                               │
```

### Editing Files

```
User                          FreeManager                      GitHub
  │                               │                               │
  │  Open DataAccess.App.cs       │                               │
  │ ─────────────────────────────►│                               │
  │                               │  GET repos/contents/path      │
  │                               │ ─────────────────────────────►│
  │                               │◄───── File content ───────────│
  │◄───── Monaco editor ──────────│                               │
  │                               │                               │
  │  Edit file, click Save        │                               │
  │ ─────────────────────────────►│                               │
  │                               │  PUT repos/contents/path      │
  │                               │  {content: base64, sha: ...}  │
  │                               │ ─────────────────────────────►│
  │                               │◄───── Commit created ─────────│
  │◄───── Saved! ─────────────────│                               │
```

### Building

```
User                          FreeManager                      GitHub
  │                               │                               │
  │  Click "Build"                │                               │
  │ ─────────────────────────────►│                               │
  │                               │  POST workflow_dispatch       │
  │                               │  (build-test workflow)        │
  │                               │ ─────────────────────────────►│
  │                               │                               │
  │◄───── Build started ──────────│                               │
  │                               │                               │
  │  (poll for status)            │  GET actions/runs/{id}        │
  │                               │ ─────────────────────────────►│
  │                               │◄───── status: completed ──────│
  │                               │                               │
  │                               │  GET actions/artifacts/{id}   │
  │                               │ ─────────────────────────────►│
  │                               │◄───── artifact URL ───────────│
  │                               │                               │
  │◄───── Download ready! ────────│                               │
```

## Data Model

### What We Store (SQL Server)

```
Project
├── Id: GUID
├── Name: string (e.g., "CustomerApp")
├── DisplayName: string (e.g., "Customer Management App")
├── UserId: GUID (owner)
├── GitHubBranch: string (e.g., "CustomerApp_base")
├── IncludedModules: string[] (e.g., ["Tags", "Appointments"])
├── Status: enum (Creating, Active, Building, Archived)
├── CreatedAt: DateTime
├── UpdatedAt: DateTime
└── LastBuildAt: DateTime?
```

### What GitHub Stores

- Full renamed project in `{ProjectName}_base` branch
- User's `.App.` files (committed via API)
- Build artifacts (via Actions)
- Full Git history

## API Endpoints

### Projects
```
POST   /api/projects                    Create (triggers GitHub workflow)
GET    /api/projects                    List user's projects
GET    /api/projects/{id}               Get details + status
DELETE /api/projects/{id}               Archive (optionally delete branch)
```

### Files
```
GET    /api/projects/{id}/files         List .App. files in branch
GET    /api/projects/{id}/files/{path}  Get file content
PUT    /api/projects/{id}/files/{path}  Update file (commits to branch)
POST   /api/projects/{id}/files         Create new .App. file
```

### Builds
```
POST   /api/projects/{id}/builds        Trigger build workflow
GET    /api/projects/{id}/builds        List builds
GET    /api/projects/{id}/builds/{id}   Get build status
GET    /api/projects/{id}/builds/{id}/download   Get artifact
```

## Implementation Phases

### Phase 1: Project Creation (MVP)
- [ ] GitHub OAuth authentication
- [ ] Project creation form (name, modules)
- [ ] Trigger `workflow_dispatch` to create branch
- [ ] Poll for workflow completion
- [ ] Display project status

**Deliverable:** Users can create a named, module-configured project

### Phase 2: File Editing
- [ ] List `.App.` files from branch
- [ ] Monaco editor integration
- [ ] Read file via GitHub API
- [ ] Write file via GitHub API (commit)
- [ ] Show commit history

**Deliverable:** Users can edit `.App.` files in browser

### Phase 3: Build System
- [ ] Trigger build workflow
- [ ] Stream/poll build logs
- [ ] Download artifacts
- [ ] Build history

**Deliverable:** Users can build and download their project

### Phase 4: Templates & Scaffolding
- [ ] Template library (CRUD entity, API endpoint, Razor page)
- [ ] Parameter input (entity name, properties)
- [ ] Generate `.App.` files from templates
- [ ] Multi-file scaffolding

**Deliverable:** Users can add new features via templates

### Phase 5: Advanced Features
- [ ] Live preview (iframe with built app?)
- [ ] IntelliSense (C# Language Server)
- [ ] Collaboration (multiple users on project)
- [ ] Direct deployment (Azure Container Apps)

## Security Considerations

| Risk | Mitigation |
|------|------------|
| Malicious code in `.App.` files | Builds run in GitHub's isolated runners |
| Path traversal attempts | Whitelist `.App.` file patterns server-side |
| Unauthorized project access | GitHub branch permissions + our auth layer |
| API abuse | Rate limiting, per-user quotas |
| Secrets exposure | Never store GitHub tokens client-side |

## Success Metrics

| Metric | Target |
|--------|--------|
| Projects created | 50 in first month |
| Time to first edit | < 5 minutes from signup |
| Build success rate | > 90% |
| User retention (weekly active) | > 30% |

## Open Questions

1. **GitHub vs Azure DevOps?** - FreeCICD has Azure DevOps integration. Support both?
2. **Multi-repo or single repo?** - All projects in FreeCRM repo, or fork per user?
3. **Real-time collaboration?** - SignalR for concurrent editing?
4. **Pricing model?** - Free tier? Pay per build?

## Resources

- [FreeCRM Repository](https://github.com/WSU-EIT/FreeCRM)
- [Existing rename-and-branch.yml workflow](https://github.com/WSU-EIT/FreeCRM/blob/main/.github/workflows/rename-and-branch.yml)
- [GitHub REST API](https://docs.github.com/en/rest)
- [Octokit.NET](https://github.com/octokit/octokit.net)
- [Monaco Editor for Blazor](https://github.com/nicknow/BlazorMonaco)
