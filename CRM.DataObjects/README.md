# CRM.DataObjects

Data Transfer Objects (DTOs) and API contracts shared between all layers.

## Purpose

This project defines the **contract layer** - the shape of data as it flows between API, business logic, and UI. DTOs are intentionally separate from EF entities to:

1. Decouple API contracts from database schema
2. Control what data is exposed to clients
3. Support versioning without breaking changes
4. Enable validation and transformation

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        CRM.DataObjects                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐              │
│  │ DataObjects  │  │ GlobalSettings│  │   Caching    │              │
│  │    .cs       │  │     .cs      │  │     .cs      │              │
│  │              │  │              │  │              │              │
│  │ Core DTOs:   │  │ App-wide     │  │ Cache helper │              │
│  │ • User       │  │ constants:   │  │ utilities    │              │
│  │ • Tenant     │  │ • AppName    │  │              │              │
│  │ • BoolResp   │  │ • Version    │  │              │              │
│  │ • Settings   │  │ • Copyright  │  │              │              │
│  └──────────────┘  └──────────────┘  └──────────────┘              │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                   Module-Specific DTOs                       │   │
│  ├──────────────┬──────────────┬──────────────┬────────────────┤   │
│  │ Appointments │   Invoices   │   Payments   │     Tags       │   │
│  │ Departments  │  Locations   │   Services   │ EmailTemplates │   │
│  │  UserGroups  │   UDFLabels  │    Ajax      │    SignalR     │   │
│  └──────────────┴──────────────┴──────────────┴────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                   Extension Points                           │   │
│  ├──────────────────────┬──────────────────────────────────────┤   │
│  │  DataObjects.App.cs  │  DataObjects.App.FreeManager.cs      │   │
│  │  (User customization)│  (FreeManager platform DTOs)         │   │
│  └──────────────────────┴──────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

## Files

### Core Files

| File | Purpose |
|------|---------|
| `DataObjects.cs` | Core DTOs: User, Tenant, BooleanResponse, Settings, Auth |
| `GlobalSettings.cs` | Application-wide constants and configuration |
| `GlobalSettings.App.cs` | User customization point for settings |
| `Caching.cs` | Cache utility classes |

### Module DTOs

| File | Contents |
|------|----------|
| `DataObjects.Appointments.cs` | Appointment, AppointmentNote, AppointmentService DTOs |
| `DataObjects.Departments.cs` | Department, DepartmentGroup DTOs |
| `DataObjects.Invoices.cs` | Invoice, InvoiceLine DTOs |
| `DataObjects.Payments.cs` | Payment DTOs |
| `DataObjects.Locations.cs` | Location DTOs |
| `DataObjects.Services.cs` | Service DTOs |
| `DataObjects.Tags.cs` | Tag, TagItem DTOs |
| `DataObjects.EmailTemplates.cs` | EmailTemplate DTOs |
| `DataObjects.UserGroups.cs` | UserGroup, UserInGroup DTOs |
| `DataObjects.UDFLabels.cs` | User-defined field DTOs |
| `DataObjects.Ajax.cs` | AJAX response wrappers |
| `DataObjects.SignalR.cs` | Real-time update DTOs |
| `DataObjects.ActiveDirectory.cs` | AD/LDAP integration DTOs |

### Extension Files

| File | Purpose |
|------|---------|
| `DataObjects.App.cs` | User customization hook |
| `DataObjects.App.FreeManager.cs` | FreeManager platform DTOs |

## Key Classes

### Core Response Types

```csharp
// Standard boolean response with messages
public class BooleanResponse
{
    public bool Result { get; set; }
    public List<string> Messages { get; set; }
}

// Base class for action responses
public class ActionResponseObject
{
    public BooleanResponse ActionResponse { get; set; }
}
```

### User & Authentication

```csharp
public class User { /* Full user profile */ }
public class ActiveUser { /* Minimal user for UI state */ }
public class Authenticate { /* Login request */ }
public class AuthenticateResult { /* Login response with token */ }
```

### Blazor Data Model

```csharp
public class BlazorDataModelLoader
{
    public List<ActiveUser> ActiveUsers { get; set; }
    public List<Tenant> AllTenants { get; set; }
    public ApplicationSettings Settings { get; set; }
    // ... all data needed for UI initialization
}
```

## FreeManager DTOs

Located in `DataObjects.App.FreeManager.cs`:

```csharp
// Project management
public class FMProjectInfo { /* Project list/detail */ }
public class FMCreateProjectRequest { /* Create project */ }
public class FMUpdateProjectRequest { /* Update metadata */ }

// File management
public class FMAppFileInfo { /* File metadata */ }
public class FMAppFileContent { /* File with content */ }
public class FMSaveFileRequest { /* Save with concurrency */ }
public class FMSaveFileResponse { /* Save result */ }

// Build management
public class FMBuildInfo { /* Build status */ }
public class FMBuildDetailInfo { /* Build with logs */ }
public class FMStartBuildRequest { /* Trigger build */ }

// API endpoint constants
public static class Endpoints.FreeManager
{
    public const string GetProjects = "api/Data/FM_GetProjects";
    // ...
}
```

## Security

### Sensitive Data Attribute

```csharp
// Mark fields that should not be logged or serialized
public class SensitiveAttribute : System.Attribute { }

public class User
{
    [Sensitive]
    public string? Password { get; set; }
}
```

### Data Exposure Control

DTOs intentionally omit sensitive fields that exist in EF entities:
- Password hashes
- Security tokens
- Internal IDs
- Audit metadata (when not needed)

## Partial Class Pattern

All DataObjects classes are `partial` for extension:

```csharp
// Core file
public partial class DataObjects
{
    public partial class User
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}

// Extension file (DataObjects.App.cs)
public partial class DataObjects
{
    public partial class User
    {
        public string? MyCustomProperty { get; set; }
    }
}
```

## Enums

```csharp
public enum DeletePreference { Immediate, MarkAsDeleted }
public enum SettingType { Boolean, DateTime, Text, Object, ... }
public enum UserLookupType { Email, EmployeeId, Guid, Username }
```

## Dependencies

- No external dependencies (pure DTOs)
- Referenced by all other projects
- Shared between server and client (Blazor WebAssembly)
