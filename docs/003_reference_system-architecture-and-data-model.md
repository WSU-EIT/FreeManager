# FreeManager: System Architecture & Data Model

> Technical reference for system design, data model, and project structure.

---

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         BROWSER                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │              CRM.Client (Blazor WebAssembly)              │  │
│  │                                                           │  │
│  │   Projects List → Project Editor → Build Status          │  │
│  │                      ↓                                    │  │
│  │                 Monaco Editor                             │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                         HTTP/SignalR
                              │
┌─────────────────────────────────────────────────────────────────┐
│                          SERVER                                  │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                 CRM (ASP.NET Core)                        │  │
│  │                                                           │  │
│  │   DataController.App.FreeManager.cs                       │  │
│  │   └── FM_GetProjects, FM_SaveAppFile, FM_StartBuild, etc  │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │              CRM.DataAccess                               │  │
│  │                                                           │  │
│  │   DataAccess.App.FreeManager.cs                           │  │
│  │   └── Business logic, validation, versioning              │  │
│  └───────────────────────────────────────────────────────────┘  │
│                              │                                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │              CRM.EFModels (Entity Framework)              │  │
│  │                                                           │  │
│  │   FMProject │ FMAppFile │ FMAppFileVersion │ FMBuild      │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                              │
                             SQL
                              │
┌─────────────────────────────────────────────────────────────────┐
│                        DATABASE                                  │
│   FMProjects │ FMAppFiles │ FMAppFileVersions │ FMBuilds        │
└─────────────────────────────────────────────────────────────────┘
```

---

## Data Model (ERD)

```
┌─────────────────────────┐
│       FMProject         │
├─────────────────────────┤
│ FMProjectId (PK)        │
│ TenantId (FK)           │──────────┐
│ Name                    │          │
│ DisplayName             │          │
│ Description             │          │
│ IncludedModules         │          │
│ Status                  │          │
│ CreatedAt               │          │
│ UpdatedAt               │          │
│ CreatedBy               │          │
│ Deleted                 │          │
│ DeletedAt               │          │
└───────────┬─────────────┘          │
            │                        │
            │ 1:N                    │ (Tenant isolation)
            │                        │
┌───────────▼─────────────┐          │
│       FMAppFile         │          │
├─────────────────────────┤          │
│ FMAppFileId (PK)        │          │
│ FMProjectId (FK)        │──────────┘
│ FilePath                │
│ FileType                │
│ CurrentVersion          │
│ CreatedAt               │
│ UpdatedAt               │
│ Deleted                 │
│ DeletedAt               │
└───────────┬─────────────┘
            │
            │ 1:N
            │
┌───────────▼─────────────┐
│   FMAppFileVersion      │
├─────────────────────────┤
│ FMAppFileVersionId (PK) │
│ FMAppFileId (FK)        │
│ Version                 │
│ Content (NVARCHAR MAX)  │
│ ContentHash (SHA256)    │
│ CreatedAt               │
│ CreatedBy               │
│ Comment                 │
└─────────────────────────┘

┌─────────────────────────┐
│        FMBuild          │
├─────────────────────────┤
│ FMBuildId (PK)          │
│ FMProjectId (FK)        │─────────► FMProject
│ BuildNumber             │
│ Status                  │
│ CreatedAt               │
│ StartedAt               │
│ CompletedAt             │
│ ArtifactPath            │
│ ArtifactSizeBytes       │
│ LogOutput               │
│ ErrorMessage            │
│ CreatedBy               │
└─────────────────────────┘
```

### Enums/Constants

**Project Status:** `Draft`, `Active`, `Archived`

**Build Status:** `Queued`, `Running`, `Succeeded`, `Failed`

**File Types:** `DataObjects`, `DataAccess`, `Controller`, `RazorComponent`, `Stylesheet`, `GlobalSettings`, `Other`

---

## Project Dependencies

```
┌─────────────────────┐
│   CRM.DataObjects   │  ◄── DTOs, shared models (no dependencies)
└──────────┬──────────┘
           │
     ┌─────┴─────┬─────────────────┐
     │           │                 │
     ▼           ▼                 ▼
┌─────────┐ ┌─────────────┐ ┌───────────┐
│CRM.EF   │ │CRM.Data     │ │CRM.Client │
│Models   │ │Access       │ │           │
└────┬────┘ └──────┬──────┘ └───────────┘
     │             │
     └──────┬──────┘
            │
            ▼
     ┌──────────────┐
     │     CRM      │  ◄── Server, references all
     │   (Server)   │
     └──────────────┘
```

---

## API Endpoints

### Projects

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Data/FM_GetProjects` | List all projects |
| GET | `/api/Data/FM_GetProject?projectId={id}` | Get single project |
| POST | `/api/Data/FM_CreateProject` | Create project |
| PUT | `/api/Data/FM_UpdateProject` | Update project |
| DELETE | `/api/Data/FM_DeleteProject?projectId={id}` | Soft delete |

### Files

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/Data/FM_GetAppFiles?projectId={id}` | List project files |
| GET | `/api/Data/FM_GetAppFile?fileId={id}` | Get file with content |
| PUT | `/api/Data/FM_SaveAppFile` | Save with versioning |
| POST | `/api/Data/FM_CreateAppFile` | Create new file |
| DELETE | `/api/Data/FM_DeleteAppFile?fileId={id}` | Soft delete |
| GET | `/api/Data/FM_GetFileVersions?fileId={id}` | Version history |
| GET | `/api/Data/FM_GetFileVersion?versionId={id}` | Specific version |

### Builds

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Data/FM_StartBuild` | Queue new build |
| GET | `/api/Data/FM_GetBuilds?projectId={id}` | Build history |
| GET | `/api/Data/FM_GetBuild?buildId={id}` | Build details + log |
| GET | `/api/Data/FM_DownloadArtifact?buildId={id}` | Download ZIP |

---

## File Locations

### FreeManager Code (What We Added)

```
CRM.EFModels/EFModels/
├── FMProject.cs                      # Project entity
├── FMAppFile.cs                      # File entity
├── FMAppFileVersion.cs               # Version entity
├── FMBuild.cs                        # Build entity
└── EFDataModel.App.FreeManager.cs    # DbContext extension

CRM.DataObjects/
└── DataObjects.App.FreeManager.cs    # DTOs (~200 lines)

CRM.DataAccess/
└── DataAccess.App.FreeManager.cs     # Business logic (~650 lines)

CRM/Controllers/
└── DataController.App.FreeManager.cs # API endpoints (~220 lines)
```

### Blazor UI (Phase 2 - Not Yet Created)

```
CRM.Client/Shared/AppComponents/
├── FMProjects.App.razor              # Project list page
├── FMNewProject.App.razor            # Create wizard
├── FMProjectEditor.App.razor         # Editor with Monaco
├── FMFileTree.App.razor              # File sidebar
└── FMBuildStatus.App.razor           # Build progress
```

---

## Build Process Flow

```
User clicks "Build"
        │
        ▼
┌───────────────────┐
│ 1. Create FMBuild │
│    Status=Queued  │
└─────────┬─────────┘
          │
          ▼ (Background worker)
┌───────────────────┐
│ 2. Clone FreeCRM  │
│    from GitHub    │
└─────────┬─────────┘
          │
          ▼
┌───────────────────┐
│ 3. Run Remove     │
│    Modules.exe    │
└─────────┬─────────┘
          │
          ▼
┌───────────────────┐
│ 4. Run Rename     │
│    FreeCRM.exe    │
└─────────┬─────────┘
          │
          ▼
┌───────────────────┐
│ 5. Copy .App.     │
│    files from DB  │
└─────────┬─────────┘
          │
          ▼
┌───────────────────┐
│ 6. dotnet publish │
└─────────┬─────────┘
          │
          ▼
┌───────────────────┐
│ 7. ZIP artifact   │
│    Update FMBuild │
│    Status=Success │
└───────────────────┘
```
