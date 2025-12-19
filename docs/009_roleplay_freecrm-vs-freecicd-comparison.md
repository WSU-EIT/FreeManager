# FreeManager: Team Analysis - FreeCRM vs FreeCICD .App. Patterns

> Roleplay transcript analyzing how FreeCICD extends FreeCRM using the `.App.` pattern.  
> This document catalogs every extension point for use in FreeManager templates.

**Date:** Current Session  
**Participants:** [Architect], [Blazor], [Data], [API], [Quality], [Sanity]

---

## Context

**[Architect]:** Team, we have two reference projects to analyze:
- **FreeCRM-main/** - The stock, unmodified FreeCRM framework
- **FreeCICD-main/** - A complete application built on FreeCRM using the `.App.` pattern

Let's systematically compare them to document every extension mechanism.

---

## Discovery: The Two-Tier .App. Pattern

**[Data]:** I noticed something important. FreeCICD uses a **two-tier** extension pattern:

```
┌─────────────────────────────────────────────────────────────────┐
│                    TWO-TIER .APP. PATTERN                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  TIER 1: Generic Extension Points (from FreeCRM)                │
│  ─────────────────────────────────────────────────              │
│  These files exist in stock FreeCRM with placeholder code:      │
│                                                                 │
│  • DataObjects.App.cs          → General app extensions         │
│  • DataAccess.App.cs           → General app methods            │
│  • DataController.App.cs       → General app endpoints          │
│  • GlobalSettings.App.cs       → General app config             │
│  • Helpers.App.cs              → General app utilities          │
│  • Program.App.cs              → Startup customization          │
│  • ConfigurationHelper.App.cs  → Config loader extension        │
│  • DataModel.App.cs            → Blazor model extension         │
│  • Utilities.App.cs            → Utility methods                │
│                                                                 │
│  TIER 2: Feature-Specific Extensions (FreeCICD adds these)      │
│  ────────────────────────────────────────────────────────       │
│  New files for specific features:                               │
│                                                                 │
│  • DataObjects.App.FreeCICD.cs    → Azure DevOps DTOs           │
│  • DataAccess.App.FreeCICD.cs     → Azure DevOps logic          │
│  • DataController.App.FreeCICD.cs → Azure DevOps endpoints      │
│  • GlobalSettings.App.FreeCICD.cs → Azure DevOps config         │
│  • Program.App.FreeCICD.cs        → Azure DevOps startup        │
│  • ConfigurationHelper.App.FreeCICD.cs → Azure DevOps config    │
│  • Index.App.FreeCICD.razor       → Azure DevOps UI             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**[Sanity]:** So Tier 1 is "where to put generic extensions" and Tier 2 is "add new files for specific features." This is brilliant for organization!

---

## Comparison: Stock FreeCRM vs FreeCICD

### File Count Comparison

| Project | Location | Stock FreeCRM | FreeCICD | Delta |
|---------|----------|---------------|----------|-------|
| Server | CRM/ | 1 .App.cs | 3 .App.cs | +2 |
| Controllers | CRM/Controllers/ | 2 .App.cs | 3 .App.cs | +1 |
| Classes | CRM/Classes/ | 0 | 2 .App.cs | +2 |
| Client | CRM.Client/ | 2 .App.cs | 2 .App.cs | 0 |
| DataAccess | CRM.DataAccess/ | 4 .App.cs | 5 .App.cs | +1 |
| DataObjects | CRM.DataObjects/ | 3 .App.cs | 5 .App.cs | +2 |
| Razor (.App.razor) | Shared/AppComponents/ | 9 | 11 | +2 |
| **Total** | | **21** | **31** | **+10** |

---

## Extension Point Catalog

### 1. Program.App.cs - Server Startup

**[Architect]:** This is the entry point for modifying ASP.NET Core startup.

**Stock FreeCRM:**
```csharp
public partial class Program
{
    public static WebApplicationBuilder AppModifyBuilderStart(WebApplicationBuilder builder) { }
    public static WebApplicationBuilder AppModifyBuilderEnd(WebApplicationBuilder builder) { }
    public static WebApplication AppModifyStart(WebApplication app) { }
    public static WebApplication AppModifyEnd(WebApplication app) { }
    public static List<string> AuthenticationPoliciesApp { get; }
    public static ConfigurationHelperLoader ConfigurationHelpersLoadApp(loader, builder) { }
}
```

**FreeCICD adds Program.App.FreeCICD.cs:**
```csharp
public partial class Program
{
    public static ConfigurationHelperLoader ConfigurationHelpersLoadFreeCICD(loader, builder)
    {
        // Load Azure DevOps PAT, ProjectId, RepoId, Branch, OrgName
        output.PAT = builder.Configuration.GetValue<string>("App:AzurePAT");
        // ... etc
    }
}
```

**Key Pattern:** The generic `.App.cs` calls the feature-specific `.App.FreeCICD.cs`.

---

### 2. ConfigurationHelper.App.cs - Configuration Properties

**[API]:** This extends the DI-injected configuration service.

**Stock FreeCRM:**
```csharp
public partial interface IConfigurationHelper { }
public partial class ConfigurationHelper : IConfigurationHelper { }
public partial class ConfigurationHelperLoader { }
public partial class ConfigurationHelperConnectionStrings { }
```

**FreeCICD adds ConfigurationHelper.App.FreeCICD.cs:**
```csharp
public partial interface IConfigurationHelper
{
    string? PAT { get; }
    string? ProjectId { get; }
    string? RepoId { get; }
    string? Branch { get; }
    string? OrgName { get; }
}

public partial class ConfigurationHelper : IConfigurationHelper
{
    public string? PAT => _loader.PAT;
    // ... etc
}

public partial class ConfigurationHelperLoader
{
    public string? PAT { get; set; }
    // ... etc
}
```

**Key Pattern:** Interface, implementation, and loader all use partial classes.

---

### 3. DataObjects.App.cs - DTOs and Models

**[Data]:** The data transfer objects layer.

**Stock FreeCRM (DataObjects.App.cs):**
```csharp
public partial class DataObjects
{
    public partial class User
    {
        public string? MyCustomUserProperty { get; set; }
    }

    public class YourClass { }
}
```

**FreeCICD adds DataObjects.App.FreeCICD.cs:**
```csharp
public partial class DataObjects
{
    public static partial class Endpoints
    {
        public static class DevOps
        {
            public const string GetDevOpsBranches = "api/Data/GetDevOpsBranches";
            public const string GetDevOpsFiles = "api/Data/GetDevOpsFiles";
            // ... 9 more endpoints
        }
    }

    public static class StepNameList { }  // Wizard step names
    public class EnvSetting { }           // Environment config
    public class Application { }          // IIS application info
    public class DevopsVariableGroup { }  // Azure DevOps variables
    public class BuildDefinition { }      // Pipeline definition
    // ... 20+ more DTOs
}
```

**Key Pattern:** Endpoints nested class for API route constants + feature DTOs.

---

### 4. DataAccess.App.cs - Business Logic

**[Data]:** Business logic methods with the interface + implementation pattern.

**Stock FreeCRM (DataAccess.App.cs):**
```csharp
public partial interface IDataAccess
{
    DataObjects.BooleanResponse YourMethod();
}

public partial class DataAccess
{
    private Dictionary<string, string> AppLanguage { get; }
    private void DataAccessAppInit() { }
    private async Task<DataObjects.BooleanResponse> DeleteAllPendingDeletedRecordsApp(...) { }
    private async Task<DataObjects.BooleanResponse> DeleteRecordImmediatelyApp(...) { }
    private async Task<DataObjects.BooleanResponse> DeleteRecordsApp(...) { }
}
```

**FreeCICD adds DataAccess.App.FreeCICD.cs:**
```csharp
public partial interface IDataAccess
{
    Task<DataObjects.DevopsGitRepoBranchInfo> GetDevOpsBranchAsync(...);
    Task<List<DataObjects.DevopsGitRepoBranchInfo>> GetDevOpsBranchesAsync(...);
    Task<List<DataObjects.DevopsFileItem>> GetDevOpsFilesAsync(...);
    // ... 15+ more methods
}

public partial class DataAccess
{
    private IMemoryCache? _cache;
    private VssConnection CreateConnection(string pat, string orgName) { }
    
    public async Task<DataObjects.DevopsVariableGroup> CreateVariableGroup(...) { }
    public async Task<List<DataObjects.DevopsProjectInfo>> GetDevOpsProjectsAsync(...) { }
    // ... implementations
}
```

**Key Pattern:** Interface defines contract, implementation in same partial file.

---

### 5. DataController.App.cs - API Endpoints

**[API]:** REST API endpoints.

**Stock FreeCRM (DataController.App.cs):**
```csharp
public partial class DataController
{
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/YourEndpoint/")]
    public ActionResult<DataObjects.BooleanResponse> YourEndpoint() { }
    
    private async Task<bool> SignalRUpdateApp(...) { }
}
```

**FreeCICD adds DataController.App.FreeCICD.cs:**
```csharp
public partial class DataController
{
    private (string orgName, string pat, ...) GetReleasePipelinesDevOpsConfig() { }

    [HttpGet($"~/{DataObjects.Endpoints.DevOps.GetDevOpsBranches}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DataObjects.DevopsGitRepoBranchInfo>>> GetDevOpsBranches(...) { }

    [HttpGet($"~/{DataObjects.Endpoints.DevOps.GetDevOpsFiles}")]
    [AllowAnonymous]
    public async Task<ActionResult<List<DataObjects.DevopsFileItem>>> GetDevOpsFiles(...) { }
    // ... 9 more endpoints
}
```

**Key Pattern:** Use `DataObjects.Endpoints.DevOps.*` constants for route strings.

---

### 6. GlobalSettings.App.cs - App Configuration

**[Architect]:** Application-wide settings and constants.

**Stock FreeCRM (GlobalSettings.App.cs):**
```csharp
public static partial class GlobalSettings
{
    // Add any app-specific global settings here.
}
```

**FreeCICD adds GlobalSettings.App.FreeCICD.cs:**
```csharp
public static partial class GlobalSettings
{
    public enum EnvironmentType { DEV, PROD, CMS }

    public class EnvironmentOptions
    {
        public string AgentPool { get; set; }
        public string Hostname { get; set; }
        public string IISJsonFilePath { get; set; }
    }

    public static class App
    {
        public static string Name { get; set; } = "FreeCICD";
        public static string Version { get; set; } = "1.0.1";
        public static string ReleaseDate { get; set; } = "8/29/2025";
        public static string CompanyName { get; set; }
        public static string CompanyUrl { get; set; }
        
        public static List<EnvironmentType> EnviormentTypeOrder { get; }
        public static Dictionary<EnvironmentType, EnvironmentOptions> EnvironmentOptions { get; }
        public static string BuildPipelineTemplate = @"..."; // YAML template
    }
}
```

**Key Pattern:** Nested `App` class for feature settings, enums, templates.

---

### 7. Helpers.App.cs - Client-Side Utilities

**[Blazor]:** Client-side helpers and menu items.

**Stock FreeCRM (Helpers.App.cs):**
```csharp
public static partial class Helpers
{
    public static Dictionary<string, List<string>> AppIcons { get; }
    public static bool AppMethod() { }
    public static List<DataObjects.Tag> AvailableTagListApp(...) { }
    private static List<string> GetDeletedRecordTypesApp() { }
    public static List<DataObjects.DeletedRecordItem>? GetDeletedRecordsForAppType(...) { }
    public static string GetDeletedRecordsLanguageTagForAppType(...) { }
    public static List<DataObjects.MenuItem> MenuItemsApp { get; }
    public static List<DataObjects.MenuItem> MenuItemsAdminApp { get; }
    public static async Task ProcessSignalRUpdateApp(...) { }
    public static async Task ProcessSignalRUpdateAppUndelete(...) { }
    private async static Task ReloadModelApp(...) { }
    private static void UpdateModelDeletedRecordCountsForAppItems(...) { }
}
```

**FreeCICD (same file, fills in the placeholders):**
- No additional file created - uses the generic extension points

**Key Pattern:** MenuItemsApp returns navigation menu items for the feature.

---

### 8. DataModel.App.cs - Blazor State

**[Blazor]:** Extends the Blazor data model for app-specific state.

**Stock FreeCRM = FreeCICD (DataModel.App.cs):**
```csharp
public partial class BlazorDataModel
{
    private List<string> _MyValues = new();
    
    private bool HaveDeletedRecordsApp { get; }
    public bool MyCustomDataModelMethod() { }
    public List<string> MyValues { get; set; }
    public DataObjects.SignalrClientRegistration SignalrClientRegistration { get; set; }
}
```

**Key Pattern:** Partial class extends the framework's BlazorDataModel.

---

### 9. Utilities.App.cs - Utility Methods

**[Data]:** Reusable utility/helper methods.

**Stock FreeCRM = FreeCICD (Utilities.App.cs):**
```csharp
public static partial class Utilities
{
    // Add your app-specific utility methods here.
}
```

**Key Pattern:** Static utility methods shared across layers.

---

### 10. Razor Components (.App.razor)

**[Blazor]:** UI components follow a delegation pattern.

**Stock FreeCRM (Index.App.razor):**
```razor
@* Standard home page content *@
```

**FreeCICD (Index.App.razor → Index.App.FreeCICD.razor):**
```razor
@* Index.App.razor - delegates to feature component *@
<Index_App_FreeCICD />
```

```razor
@* Index.App.FreeCICD.razor - actual feature implementation *@
@* ~700 lines of Pipeline Wizard UI *@
```

**Key Pattern:** Base `.App.razor` delegates to feature-specific `.App.FeatureName.razor`.

---

## Summary: Complete Extension Point List

**[Summary]:**

### Server Project (CRM/)

| File | Purpose | Extension Mechanism |
|------|---------|---------------------|
| `Program.App.cs` | Startup hooks | `AppModifyBuilderStart/End`, `ConfigurationHelpersLoadApp` |
| `Program.App.{Feature}.cs` | Feature startup | Called from `Program.App.cs` |
| `ConfigurationHelper.App.cs` | Config interface | Partial interface + class |
| `ConfigurationHelper.App.{Feature}.cs` | Feature config | Adds properties to partial |
| `DataController.App.cs` | API endpoints | Partial controller class |
| `DataController.App.{Feature}.cs` | Feature endpoints | Adds endpoints to partial |

### DataAccess Project (CRM.DataAccess/)

| File | Purpose | Extension Mechanism |
|------|---------|---------------------|
| `DataAccess.App.cs` | Business logic hooks | `DeleteRecordsApp`, `DataAccessAppInit` |
| `DataAccess.App.{Feature}.cs` | Feature logic | Interface + implementation |
| `Utilities.App.cs` | Utility methods | Static partial class |

### DataObjects Project (CRM.DataObjects/)

| File | Purpose | Extension Mechanism |
|------|---------|---------------------|
| `DataObjects.App.cs` | Generic DTOs | Partial class, extend `User` |
| `DataObjects.App.{Feature}.cs` | Feature DTOs | `Endpoints` class + new DTOs |
| `GlobalSettings.App.cs` | Generic config | Static partial class |
| `GlobalSettings.App.{Feature}.cs` | Feature config | `App` nested class, enums |

### Client Project (CRM.Client/)

| File | Purpose | Extension Mechanism |
|------|---------|---------------------|
| `Helpers.App.cs` | Menu items, utilities | `MenuItemsApp`, `MenuItemsAdminApp` |
| `DataModel.App.cs` | Blazor state | Partial BlazorDataModel class |
| `*.App.razor` | Base components | Delegate to feature components |
| `*.App.{Feature}.razor` | Feature UI | Full implementation |

---

## Implications for FreeManager

**[Architect]:** Based on this analysis, FreeManager's "New File" dialog should support these file types:

```
┌─────────────────────────────────────────────────────────────────┐
│              FREEMANAGER FILE TYPE TEMPLATES                    │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  BACKEND (Server + DataAccess)                                  │
│  ├── DataObjects.App.{Feature}.cs    DTOs, Endpoints class      │
│  ├── DataAccess.App.{Feature}.cs     Interface + implementation │
│  ├── DataController.App.{Feature}.cs API endpoints              │
│  ├── Utilities.App.cs                Static helpers             │
│  └── (EF Entity)                     Database model             │
│                                                                 │
│  CONFIG (DataObjects)                                           │
│  ├── GlobalSettings.App.{Feature}.cs App config, enums          │
│  ├── ConfigurationHelper.App.{Feature}.cs  Config loader        │
│  └── Program.App.{Feature}.cs        Startup customization      │
│                                                                 │
│  CLIENT (Blazor)                                                │
│  ├── Helpers.App.cs                  Menu items (modify)        │
│  ├── DataModel.App.cs                Blazor state (modify)      │
│  ├── {Feature}.App.razor             Base component (delegate)  │
│  ├── {Feature}.App.{SubFeature}.razor Feature UI                │
│  └── {feature}.css                   Styles                     │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

**[Sanity]:** This gives us 12+ distinct file types to template. That's comprehensive coverage of the extension pattern.

---

## Next Steps

1. **Update FMFileTemplates.App.cs** with all file types
2. **Add ConfigurationHelper.App template** - new type
3. **Add Program.App template** - new type  
4. **Document the two-tier pattern** in user guide
5. **Create CRUD scaffold** that generates the right combination

**[Quality]:** We now have a complete catalog of extension points. FreeManager users can build apps like FreeCICD did - entirely within `.App.` files.
