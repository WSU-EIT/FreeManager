# FreeManager: Team Discussion - Phase 4 Planning

> Roleplay transcript planning default file templates and Phase 4 features.  
> Updated with real-world patterns from FreeCICD implementation.

**Date:** Current Session  
**Participants:** [Architect], [Blazor], [Data], [API], [Quality], [Sanity]

---

## Context

**[Architect]:** Team, we've completed Phases 1-3. The app is functional - users can create projects, edit files with Monaco, and export ZIP files. We've also just completed Task 4.1 (improved default templates). Now we have the FreeCICD project as a real-world reference showing how a complete application uses the `.App.` pattern.

**[Sanity]:** FreeCICD demonstrates the full pattern:
- `DataObjects.App.FreeCICD.cs` - DTOs for Azure DevOps integration
- `DataAccess.App.FreeCICD.cs` - Business logic for DevOps APIs  
- `DataController.App.FreeCICD.cs` - API endpoints
- `GlobalSettings.App.FreeCICD.cs` - App configuration
- `Index.App.FreeCICD.razor` - Feature-specific Blazor component
- `Helpers.App.cs` - Client-side utilities and menu items

---

## New File Type Discovery

**[Data]:** Looking at FreeCICD, we see additional file types we should support:

```
┌─────────────────────────────────────────────────────────────────┐
│                    FILE TYPE HIERARCHY                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐                    │
│  │  DataObjects    │    │   DataAccess    │                    │
│  │  .App.*.cs      │───▶│   .App.*.cs     │                    │
│  │  DTOs, Models   │    │  Business Logic │                    │
│  └────────┬────────┘    └────────┬────────┘                    │
│           │                      │                              │
│           │    ┌─────────────────┘                              │
│           │    │                                                │
│           ▼    ▼                                                │
│  ┌─────────────────┐    ┌─────────────────┐                    │
│  │   Controller    │    │   EF Entity     │                    │
│  │   .App.*.cs     │    │   .cs           │                    │
│  │  API Endpoints  │    │  Database Model │                    │
│  └─────────────────┘    └────────┬────────┘                    │
│                                  │                              │
│                                  ▼                              │
│                         ┌─────────────────┐                    │
│                         │  EFDataModel    │                    │
│                         │  .App.*.cs      │                    │
│                         │  DbSet Config   │                    │
│                         └─────────────────┘                    │
│                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐                    │
│  │ GlobalSettings  │    │    Helpers      │                    │
│  │  .App.*.cs      │    │   .App.cs       │                    │
│  │  App Config     │    │  Menu Items     │                    │
│  └─────────────────┘    └─────────────────┘                    │
│                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐                    │
│  │ RazorComponent  │    │   Stylesheet    │                    │
│  │  .App.razor     │    │   .css          │                    │
│  │  UI Component   │    │   CSS Styles    │                    │
│  └─────────────────┘    └─────────────────┘                    │
│                                                                 │
│  ┌─────────────────┐    ┌─────────────────┐                    │
│  │   RazorPage     │    │   Utilities     │                    │
│  │  .App.razor     │    │   .App.cs       │                    │
│  │  Routed Page    │    │  Helper Classes │                    │
│  └─────────────────┘    └─────────────────┘                    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Updated File Types

**[API]:** Based on FreeCICD patterns, here are all supported file types:

| File Type | Extension | Location | Purpose |
|-----------|-----------|----------|---------|
| **DataObjects** | `.App.*.cs` | `CRM.DataObjects/` | DTOs, endpoints, models |
| **DataAccess** | `.App.*.cs` | `CRM.DataAccess/` | Business logic, interface |
| **Controller** | `.App.*.cs` | `CRM/Controllers/` | API endpoints |
| **EFEntity** | `.cs` | `CRM.EFModels/EFModels/` | Database entity |
| **EFDataModel** | `.App.*.cs` | `CRM.EFModels/EFModels/` | DbSet configuration |
| **GlobalSettings** | `.App.*.cs` | `CRM.DataObjects/` | App configuration |
| **Helpers** | `.App.cs` | `CRM.Client/` | Menu items, utilities |
| **RazorComponent** | `.App.razor` | `CRM.Client/Shared/AppComponents/` | UI component |
| **RazorPage** | `.App.razor` | `CRM.Client/Pages/` | Routed page |
| **Stylesheet** | `.css` | `CRM.Client/wwwroot/css/` | CSS styles |
| **Utilities** | `.App.cs` | `CRM.DataAccess/` | Helper classes |

---

## New File Dialog Mockup

**[Blazor]:** Here's the updated "New File" dialog:

```
┌─────────────────────────────────────────────────────────────────┐
│  ╔═══════════════════════════════════════════════════════════╗  │
│  ║                   Create New File                          ║  │
│  ╚═══════════════════════════════════════════════════════════╝  │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  File Category                                          │    │
│  │  ┌─────────┐ ┌─────────┐ ┌─────────┐ ┌─────────┐       │    │
│  │  │ Backend │ │   UI    │ │ Database│ │  Config │       │    │
│  │  │   ▼     │ │         │ │         │ │         │       │    │
│  │  └─────────┘ └─────────┘ └─────────┘ └─────────┘       │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  File Type: [DataObjects ▼]                             │    │
│  │                                                         │    │
│  │    ○ DataObjects    - DTOs, Models, Endpoints           │    │
│  │    ○ DataAccess     - Business Logic Methods            │    │
│  │    ○ Controller     - API Endpoints                     │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  File Name: [________________________.App.cs]           │    │
│  │                                                         │    │
│  │  Preview: DataObjects.App.MyFeature.cs                  │    │
│  │  Location: CRM.DataObjects/                             │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Template Preview:                                      │    │
│  │  ┌───────────────────────────────────────────────────┐  │    │
│  │  │ namespace CRM;                                    │  │    │
│  │  │                                                   │  │    │
│  │  │ #region MyProject DTOs                            │  │    │
│  │  │ public partial class DataObjects                  │  │    │
│  │  │ {                                                 │  │    │
│  │  │     public class MyProjectItem { ... }            │  │    │
│  │  │ }                                                 │  │    │
│  │  │ #endregion                                        │  │    │
│  │  └───────────────────────────────────────────────────┘  │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│                            [Cancel]  [Create File]              │
└─────────────────────────────────────────────────────────────────┘
```

---

## Template: Helpers.App.cs (NEW)

**[Blazor]:** This is the client-side helpers file for menu items:

```csharp
namespace CRM.Client;

#region {ProjectName} Client Helpers
// ============================================================================
// {PROJECTNAME} PROJECT EXTENSION
// Add menu items and client-side utilities here.
// ============================================================================

public static partial class Helpers
{
    /// <summary>
    /// App-specific menu items. Add your pages here.
    /// </summary>
    public static List<DataObjects.MenuItem> MenuItemsApp {
        get {
            List<DataObjects.MenuItem> output = new();

            // Example: Add a menu item for your feature
            // Requires Model.User.Admin or other permission check
            if (Model.User.Admin) {
                output.Add(new DataObjects.MenuItem {
                    Title = "{ProjectName}",
                    Icon = "fa-solid fa-cube",
                    PageNames = new List<string> { "{projectname}" },
                    SortOrder = 100,
                    url = Helpers.BuildUrl("{ProjectName}"),
                    AppAdminOnly = false,
                });
            }

            return output;
        }
    }

    /// <summary>
    /// Admin-only menu items.
    /// </summary>
    public static List<DataObjects.MenuItem> MenuItemsAdminApp {
        get {
            List<DataObjects.MenuItem> output = new();
            
            // Example: Admin settings page
            // output.Add(new DataObjects.MenuItem {
            //     Title = "{ProjectName} Settings",
            //     Icon = "fa-solid fa-cog",
            //     PageNames = new List<string> { "{projectname}-settings" },
            //     SortOrder = 200,
            //     url = Helpers.BuildUrl("{ProjectName}/Settings"),
            // });

            return output;
        }
    }
}

#endregion
```

---

## Template: RazorPage (NEW)

**[Blazor]:** A routed page component:

```razor
@page "/{ProjectName}"
@page "/{{TenantCode}}/{ProjectName}"
@using CRM.Client.Shared.AppComponents
@inject BlazorDataModel Model
@implements IDisposable

@* ============================================================================
   {ProjectName} Page
   Routes: /{ProjectName}, /{{TenantCode}}/{ProjectName}
   ============================================================================ *@

@if (Model.Loaded && Model.LoggedIn && Model.View == _pageName) {{
    <{ProjectName}_App />
}}

@code {{
    [Parameter] public string? TenantCode {{ get; set; }}

    protected bool _loadedData = false;
    protected string _pageName = "{projectname}";

    public void Dispose()
    {{
        Model.OnChange -= OnDataModelUpdated;
    }}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {{
        if (firstRender) {{
            Model.TenantCodeFromUrl = TenantCode;
        }}

        if (Model.Loaded) {{
            if (Model.LoggedIn) {{
                if (!_loadedData) {{
                    _loadedData = true;
                    await Helpers.ValidateUrl(TenantCode, true);
                }}
            }} else {{
                Helpers.NavigateToLogin();
            }}
        }}
    }}

    protected void OnDataModelUpdated()
    {{
        if (Model.View == _pageName) {{
            StateHasChanged();
        }}
    }}

    protected override void OnInitialized()
    {{
        Model.View = _pageName;
        Model.OnChange += StateHasChanged;
    }}
}}
```

---

## Template: EFDataModel.App (NEW)

**[Data]:** DbContext extension for adding DbSets:

```csharp
using Microsoft.EntityFrameworkCore;

namespace CRM.EFModels.EFModels;

#region {ProjectName} DbContext Extension
// ============================================================================
// {PROJECTNAME} PROJECT EXTENSION
// Add your DbSets here for EF Core entities.
// ============================================================================

public partial class EFDataModel
{
    /// <summary>
    /// {ProjectName} items table.
    /// </summary>
    public virtual DbSet<{ProjectName}Item> {ProjectName}Items { get; set; } = null!;
    
    // Add more DbSets as needed:
    // public virtual DbSet<{ProjectName}Category> {ProjectName}Categories { get; set; } = null!;
}

#endregion

// ============================================================================
// NEXT STEPS:
// 1. Create the entity class: {ProjectName}Item.cs in CRM.EFModels/EFModels/
// 2. Run: dotnet ef migrations add {ProjectName}_Initial
// 3. Run: dotnet ef database update
// ============================================================================
```

---

## Template: Utilities.App.cs (NEW)

**[Data]:** Reusable utility classes:

```csharp
namespace CRM;

#region {ProjectName} Utilities
// ============================================================================
// {PROJECTNAME} PROJECT EXTENSION
// Add utility/helper classes here.
// ============================================================================

/// <summary>
/// {ProjectName} utility methods.
/// </summary>
public static class {ProjectName}Utilities
{
    /// <summary>
    /// Example: Validate input string.
    /// </summary>
    public static bool IsValidInput(string? input)
    {
        if (String.IsNullOrWhiteSpace(input)) {
            return false;
        }
        return input.Length >= 3 && input.Length <= 100;
    }

    /// <summary>
    /// Example: Format display text.
    /// </summary>
    public static string FormatDisplayText(string name, DateTime createdAt)
    {
        return $"{name} (created {createdAt:MMM dd, yyyy})";
    }

    /// <summary>
    /// Example: Generate unique code.
    /// </summary>
    public static string GenerateCode(string prefix = "")
    {
        string timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        string random = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
        return $"{prefix}{timestamp}-{random}";
    }
}

#endregion
```

---

## CRUD Scaffold Wizard Mockup

**[Blazor]:** When user wants to scaffold a complete CRUD feature:

```
┌─────────────────────────────────────────────────────────────────┐
│  ╔═══════════════════════════════════════════════════════════╗  │
│  ║              Scaffold CRUD Entity                          ║  │
│  ╠═══════════════════════════════════════════════════════════╣  │
│  ║  Generate all files for a new entity with one click        ║  │
│  ╚═══════════════════════════════════════════════════════════╝  │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Entity Name: [Product                              ]   │    │
│  │                                                         │    │
│  │  Prefix:      [_____] (optional, e.g., "FM")           │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Files to Generate:                            Count: 6 │    │
│  │  ┌─────────────────────────────────────────────────────┐│    │
│  │  │ ☑ DataObjects.App.Product.cs     CRM.DataObjects/  ││    │
│  │  │ ☑ DataAccess.App.Product.cs      CRM.DataAccess/   ││    │
│  │  │ ☑ DataController.App.Product.cs  CRM/Controllers/  ││    │
│  │  │ ☑ Product.cs (Entity)            CRM.EFModels/     ││    │
│  │  │ ☑ EFDataModel.App.Product.cs     CRM.EFModels/     ││    │
│  │  │ ☑ Product.App.razor              CRM.Client/       ││    │
│  │  └─────────────────────────────────────────────────────┘│    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Entity Properties:                                     │    │
│  │  ┌─────────────┬────────────┬──────────┬─────────────┐ │    │
│  │  │ Name        │ Type       │ Required │ Max Length  │ │    │
│  │  ├─────────────┼────────────┼──────────┼─────────────┤ │    │
│  │  │ Name        │ string     │ ☑        │ 200         │ │    │
│  │  │ Description │ string     │ ☐        │ 1000        │ │    │
│  │  │ Price       │ decimal    │ ☑        │ -           │ │    │
│  │  │ IsActive    │ bool       │ ☐        │ -           │ │    │
│  │  │ [+ Add]     │            │          │             │ │    │
│  │  └─────────────┴────────────┴──────────┴─────────────┘ │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Options:                                               │    │
│  │  ☑ Include soft delete (Deleted, DeletedAt)            │    │
│  │  ☑ Include audit fields (CreatedAt, UpdatedAt, By)     │    │
│  │  ☑ Include tenant isolation (TenantId)                 │    │
│  │  ☑ Generate Razor component                             │    │
│  │  ☐ Generate API tests                                   │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│                     [Cancel]  [Preview]  [Generate All]         │
└─────────────────────────────────────────────────────────────────┘
```

---

## Export ZIP Structure

**[Architect]:** Updated export folder structure:

```
{ProjectName}_export.zip
│
├── README.txt
│
├── CRM/
│   └── Controllers/
│       └── DataController.App.{ProjectName}.cs
│
├── CRM.Client/
│   ├── Helpers.App.cs
│   ├── Pages/
│   │   └── {ProjectName}.App.razor
│   ├── Shared/
│   │   └── AppComponents/
│   │       └── {ProjectName}_App.App.razor
│   └── wwwroot/
│       └── css/
│           └── {projectname}.css
│
├── CRM.DataAccess/
│   ├── DataAccess.App.{ProjectName}.cs
│   └── {ProjectName}Utilities.App.cs
│
├── CRM.DataObjects/
│   ├── DataObjects.App.{ProjectName}.cs
│   └── GlobalSettings.App.{ProjectName}.cs
│
└── CRM.EFModels/
    └── EFModels/
        ├── {ProjectName}Item.cs
        └── EFDataModel.App.{ProjectName}.cs
```

---

## Phase 4 Task List - Updated

**[Summary]:**

### 4.1 Improved Default Templates - COMPLETE ✓
- Rich templates for all file types
- Moved to `FMFileTemplates.App.cs`

### 4.2 Add New File Types - IN PROGRESS
- [ ] **4.2.1** Add Helpers.App template
- [ ] **4.2.2** Add RazorPage template
- [ ] **4.2.3** Add EFDataModel.App template
- [ ] **4.2.4** Add Utilities.App template
- [ ] **4.2.5** Update export folder mapping

### 4.3 Enhanced New File Dialog
- [ ] **4.3.1** Add file category tabs (Backend, UI, Database, Config)
- [ ] **4.3.2** Show template preview
- [ ] **4.3.3** Auto-suggest file location

### 4.4 CRUD Scaffold Wizard (STRETCH)
- [ ] **4.4.1** Create scaffold dialog UI
- [ ] **4.4.2** Entity property editor
- [ ] **4.4.3** Multi-file generation
- [ ] **4.4.4** Preview before create

### 4.5 Version Diff Viewer (OPTIONAL)
- [ ] **4.5.1** Monaco diff editor integration
- [ ] **4.5.2** Version comparison UI

---

## Priority Order

1. **4.2** - Add new file types (quick win, high value)
2. **4.3** - Enhanced new file dialog (better UX)
3. **4.4** - CRUD scaffold (big feature, can defer)
4. **4.5** - Diff viewer (nice to have)

**[Sanity]:** With the FreeCICD patterns as reference, we have proven templates. Start with 4.2 to complete the file type coverage, then evaluate if 4.4 is worth the complexity.
