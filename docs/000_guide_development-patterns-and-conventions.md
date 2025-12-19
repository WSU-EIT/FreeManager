# FreeManager Development Guide

How to work on this project: patterns, conventions, and team collaboration.

**For C# coding style (braces, naming, etc.), see [005_reference_csharp-style-guide.md](005_reference_csharp-style-guide.md)**

**For architecture/style questions, reference `FreeCRM-main/` as the source of inspiration.**

---

## File Length Guidelines

| Category | Lines | Description |
|----------|-------|-------------|
| **Ideal** | 0-300 | Target size for most files |
| **Large** | 300-500 | Acceptable, consider splitting |
| **Maximum** | 500-600 | Hard limit unless operational need |
| **Override** | 600+ | Requires justification (e.g., generated code, migrations) |

**When to split a file:**
- Multiple unrelated concerns in one file
- File exceeds 400 lines and growing
- Hard to find things without Ctrl+F

**How to split:**
- Use partial classes: `DataAccess.Users.cs`, `DataAccess.Appointments.cs`
- Use the `.App.{Feature}.cs` pattern for extensions
- Group by feature, not by type

---

## FreeManager Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| FreeManager methods | `FM_` prefix | `FM_GetProjects()` |
| FreeManager entities | `FM` prefix | `FMProject`, `FMAppFile` |
| Extension files | `.App.{Feature}.cs` | `DataAccess.App.FreeManager.cs` |
| Razor components | `.App.razor` | `Projects.App.razor` |
| DTOs | Descriptive suffix | `FMProjectInfo`, `FMCreateProjectRequest` |

---

## Code Patterns

**Always pass `CurrentUser` to methods requiring tenant context:**
```csharp
// GOOD - follows FreeCRM pattern
public async Task<List<FMProjectInfo>> FM_GetProjects(DataObjects.User CurrentUser)
{
    var tenantId = CurrentUser.TenantId;
    // ...
}

// BAD - no tenant isolation
public async Task<List<FMProjectInfo>> FM_GetProjects()
{
    // Where does tenantId come from?
}
```

**Use soft deletes with audit fields:**
```csharp
// GOOD
entity.Deleted = true;
entity.DeletedAt = DateTime.UtcNow;
entity.UpdatedAt = DateTime.UtcNow;

// BAD - hard delete loses audit trail
data.FMProjects.Remove(entity);
```

**Return response objects, not exceptions for expected failures:**
```csharp
// GOOD - caller can handle gracefully
if (project == null) {
    output.Messages.Add("Project not found");
    return output;
}

// BAD for expected cases - forces try/catch
throw new NotFoundException("Project not found");
```

---

## Team Roleplay Guide

When discussing architecture or design, use these roles to ensure thorough consideration:

| Role | Focus Area | Key Questions |
|------|------------|---------------|
| **[Architect]** | System design, boundaries | "Does this scale? Is it secure by default?" |
| **[Data]** | EF Core, entities, queries | "N+1 queries? Indexes needed?" |
| **[API]** | Endpoints, DTOs, validation | "RESTful? Consistent error responses?" |
| **[Blazor]** | UI, components, UX | "Loading states? Error handling?" |
| **[Quality]** | Testing, CI/CD, security | "How do we test this? Security gaps?" |
| **[Sanity]** | Reality check, scope | "Are we overengineering? Does this solve the actual problem?" |

**Every discussion should end with:**
1. `[Sanity]` final check
2. `[Summary]` with decisions, actions, and owners

---

## Project Structure

### FreeManager-Specific Files

```
CRM.EFModels/EFModels/
+-- FMProject.cs                    # Project entity
+-- FMAppFile.cs                    # File metadata entity
+-- FMAppFileVersion.cs             # Version content entity
+-- FMBuild.cs                      # Build job entity
+-- EFDataModel.App.FreeManager.cs  # DbContext extension

CRM.DataObjects/
+-- DataObjects.App.FreeManager.cs  # DTOs and constants

CRM.DataAccess/
+-- DataAccess.App.FreeManager.cs   # Business logic (~650 lines - override justified)

CRM/Controllers/
+-- DataController.App.FreeManager.cs # API endpoints

CRM.Client/Shared/AppComponents/     # (Phase 2 - not yet created)
+-- FMProjects.App.razor            # Project list
+-- FMProjectEditor.App.razor       # Editor with Monaco
+-- FMBuildStatus.App.razor         # Build progress
```

### The `.App.` Pattern

**Never modify core FreeCRM files.** All customizations go in `.App.` files:

| Core File (DO NOT EDIT) | Extension File (EDIT THIS) |
|-------------------------|----------------------------|
| `DataAccess.cs` | `DataAccess.App.FreeManager.cs` |
| `DataController.cs` | `DataController.App.FreeManager.cs` |
| `EFDataModel.cs` | `EFDataModel.App.FreeManager.cs` |

This pattern enables:
- Upstream FreeCRM updates without merge conflicts
- Clear separation of framework vs. custom code
- Easy identification of FreeManager additions

---

## Common Commands

```bash
# Build solution
dotnet build

# Run the server (from CRM folder)
cd CRM
dotnet run

# Add EF migration (from CRM.EFModels folder)
cd CRM.EFModels
dotnet ef migrations add FM_InitialCreate --startup-project ../CRM

# Apply migrations
dotnet ef database update --startup-project ../CRM

# Run tests (when test project exists)
dotnet test
```

---

## Adding New Features

### 1. Start with the Data Model
- Add entity in `CRM.EFModels/EFModels/FM{Entity}.cs`
- Add DbSet in `EFDataModel.App.FreeManager.cs`
- Create migration

### 2. Create DTOs
- Add to `DataObjects.App.FreeManager.cs`
- Include `Info` (read), `Request` (write), `Response` types

### 3. Implement Business Logic
- Add methods to `DataAccess.App.FreeManager.cs`
- Follow `FM_` prefix convention
- Always filter by `CurrentUser.TenantId`

### 4. Expose via API
- Add endpoints to `DataController.App.FreeManager.cs`
- Use `[Authorize]` attribute
- Pass `CurrentUser` to DataAccess methods

### 5. Build UI (Phase 2+)
- Create `.App.razor` components
- Use existing FreeCRM patterns for consistency
- Follow style guide for Blazor conventions

---

## Security Checklist

- [ ] All endpoints have `[Authorize]` attribute
- [ ] All queries filter by `CurrentUser.TenantId`
- [ ] User input is validated before use
- [ ] No direct file system access from user input
- [ ] Sensitive data not logged or exposed in errors

---

## Reference: FreeCRM-main Folder

The `FreeCRM-main/` folder contains the **unmodified original FreeCRM framework**. Use it to:
- Understand how core features work
- Check if something is framework vs. custom
- Reference patterns for new features

**Do not modify files in `FreeCRM-main/`** - it's a reference copy.

---

## Related Documentation

- [001 Quickstart](001_index_quickstart-and-documentation-guide.md) - Project overview and onboarding
- [004 Status](004_status_implementation-progress-and-checklist.md) - Current progress
- [005 C# Style Guide](005_reference_csharp-style-guide.md) - Coding conventions (braces, naming, etc.)
