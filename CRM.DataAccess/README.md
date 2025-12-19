# CRM.DataAccess

Business logic and data access layer - the heart of the application.

## Purpose

This project contains **all business logic** including:
- CRUD operations for all entities
- Data validation and transformation
- Security enforcement (tenant isolation, permissions)
- External integrations (Active Directory, Email, etc.)
- Utility functions (encryption, PDF generation, etc.)

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        CRM.DataAccess                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    DataAccess.cs                             │   │
│  │                (Main Entry Point)                            │   │
│  │                                                              │   │
│  │  • Constructor with DI                                       │   │
│  │  • Database connection setup                                 │   │
│  │  • Multi-provider support (SQL, PostgreSQL, MySQL, SQLite)   │   │
│  │  • User context management                                   │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                              │                                      │
│         ┌────────────────────┼────────────────────┐                │
│         ▼                    ▼                    ▼                │
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────┐          │
│  │   Entity    │     │   Utility   │     │  External   │          │
│  │   CRUD      │     │  Services   │     │ Integrations│          │
│  │             │     │             │     │             │          │
│  │ Users       │     │ Encryption  │     │ ActiveDir   │          │
│  │ Tenants     │     │ JWT/Auth    │     │ GraphAPI    │          │
│  │ Appointments│     │ PDF Gen     │     │ SignalR     │          │
│  │ Invoices    │     │ Language    │     │ Plugins     │          │
│  │ Payments    │     │ Settings    │     │             │          │
│  │ ...         │     │ FileStorage │     │             │          │
│  └─────────────┘     └─────────────┘     └─────────────┘          │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                   Extension Points                           │   │
│  ├──────────────────────┬──────────────────────────────────────┤   │
│  │  DataAccess.App.cs   │  DataAccess.App.FreeManager.cs       │   │
│  │  (User hooks)        │  (FreeManager business logic)        │   │
│  └──────────────────────┴──────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                   Database Migrations                        │   │
│  ├────────────────┬───────────────┬───────────────┬────────────┤   │
│  │  SQLServer     │  PostgreSQL   │    MySQL      │   SQLite   │   │
│  └────────────────┴───────────────┴───────────────┴────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

## Files

### Core Files

| File | Purpose |
|------|---------|
| `DataAccess.cs` | Main class: constructor, DB setup, partial class base |
| `DataAccess.Disposable.cs` | IDisposable implementation |
| `GlobalUsings.cs` | Global using statements |

### Entity CRUD Operations

| File | Operations |
|------|------------|
| `DataAccess.Users.cs` | User CRUD, search, filtering, password management |
| `DataAccess.Tenants.cs` | Tenant CRUD, tenant switching |
| `DataAccess.Departments.cs` | Department and DepartmentGroup CRUD |
| `DataAccess.UserGroups.cs` | UserGroup CRUD, membership management |
| `DataAccess.Appointments.cs` | Appointment scheduling, attendees, notes |
| `DataAccess.Invoices.cs` | Invoice generation, line items |
| `DataAccess.Payments.cs` | Payment processing, refunds |
| `DataAccess.Locations.cs` | Location management |
| `DataAccess.Services.cs` | Service catalog management |
| `DataAccess.Tags.cs` | Tagging system |
| `DataAccess.EmailTemplates.cs` | Email template management |
| `DataAccess.UDFLabels.cs` | User-defined field configuration |
| `DataAccess.Settings.cs` | Application settings CRUD |
| `DataAccess.FileStorage.cs` | Binary file upload/download |

### Utility Services

| File | Purpose |
|------|---------|
| `DataAccess.Authenticate.cs` | Login, logout, token validation |
| `DataAccess.JWT.cs` | JWT token generation and validation |
| `DataAccess.Encryption.cs` | AES encryption/decryption |
| `DataAccess.PDF.cs` | PDF generation (QuestPDF) |
| `DataAccess.Language.cs` | Localization and language strings |
| `DataAccess.Utilities.cs` | Helper functions |
| `DataAccess.ApplicationSettings.cs` | Global app config |
| `DataAccess.Ajax.cs` | AJAX response helpers |
| `DataAccess.CSharpCode.cs` | Dynamic C# compilation |
| `DataAccess.SeedTestData.cs` | Test data generation |

### External Integrations

| File | Purpose |
|------|---------|
| `DataAccess.ActiveDirectory.cs` | LDAP/AD authentication |
| `DataAccess.SignalR.cs` | Real-time updates |
| `DataAccess.Plugins.cs` | Plugin system integration |
| `GraphAPI.cs` | Microsoft Graph API integration |
| `GraphAPI.App.cs` | Graph API customization hook |

### Database Migrations

| File | Purpose |
|------|---------|
| `DataAccess.Migrations.cs` | Migration orchestration |
| `DataMigrations.SQLServer.cs` | SQL Server specific migrations |
| `DataMigrations.PostgreSQL.cs` | PostgreSQL specific migrations |
| `DataMigrations.MySQL.cs` | MySQL specific migrations |
| `DataMigrations.SQLite.cs` | SQLite specific migrations |

### Utility Classes

| File | Purpose |
|------|---------|
| `Utilities.cs` | Core utility functions |
| `Utilities.App.cs` | User-extensible utilities |
| `RandomPasswordGenerator.cs` | Secure password generation |
| `RandomPasswordGenerator.App.cs` | Custom password rules |

### Extension Points

| File | Purpose |
|------|---------|
| `DataAccess.App.cs` | **Main customization hook** with callback methods |
| `DataAccess.App.FreeManager.cs` | FreeManager platform business logic |

## Interface

The `IDataAccess` interface defines all public methods:

```csharp
public partial interface IDataAccess
{
    // User operations
    Task<DataObjects.User?> GetUser(Guid userId);
    Task<DataObjects.BooleanResponse> SaveUser(DataObjects.User user);

    // Authentication
    Task<DataObjects.AuthenticateResult> Authenticate(DataObjects.Authenticate auth);

    // FreeManager (from extension)
    Task<List<DataObjects.FMProjectInfo>> FM_GetProjects();
    Task<DataObjects.FMProjectInfo> FM_CreateProject(DataObjects.FMCreateProjectRequest request);
    // ...
}
```

## Database Support

```csharp
switch (_databaseType.ToLower())
{
    case "sqlserver":
        optionsBuilder.UseSqlServer(_connectionString);
        break;
    case "postgresql":
        optionsBuilder.UseNpgsql(_connectionString);
        break;
    case "mysql":
        optionsBuilder.UseMySQL(_connectionString);
        break;
    case "sqlite":
        optionsBuilder.UseSqlite(_connectionString);
        break;
    case "inmemory":
        optionsBuilder.UseInMemoryDatabase("InMemory");
        break;
}
```

## Security Patterns

### Tenant Isolation

All queries are filtered by tenant:

```csharp
public async Task<List<DataObjects.User>> GetUsers()
{
    var tenantId = CurrentTenantId();
    return await data.Users
        .Where(u => u.TenantId == tenantId && !u.Deleted)
        .ToListAsync();
}
```

### User Context

Current user is loaded from JWT token or session:

```csharp
private DataObjects.User? _loadedUser;

public void SetCurrentUser(DataObjects.User user)
{
    _loadedUser = user;
}

private Guid CurrentTenantId() => _loadedUser?.TenantId ?? Guid.Empty;
private Guid CurrentUserId() => _loadedUser?.UserId ?? Guid.Empty;
```

## Extension Pattern (DataAccess.App.cs)

The App file provides hook methods called at key points:

```csharp
// Called during initialization
private void DataAccessAppInit() { }

// Called when mapping EF entity to DTO
private void GetDataApp(object Rec, object DataObject, DataObjects.User? CurrentUser) { }

// Called when saving DTO back to EF entity
private void SaveDataApp(object Rec, object DataObject, DataObjects.User? CurrentUser) { }

// Called before deleting records
private async Task<DataObjects.BooleanResponse> DeleteRecordsApp(object Rec) { }

// Add custom language strings
private Dictionary<string, string> AppLanguage { get { ... } }
```

## FreeManager Methods

Located in `DataAccess.App.FreeManager.cs`:

```csharp
// Project management
Task<List<DataObjects.FMProjectInfo>> FM_GetProjects()
Task<DataObjects.FMProjectInfo?> FM_GetProject(Guid projectId)
Task<DataObjects.FMProjectInfo> FM_CreateProject(FMCreateProjectRequest request)
Task<DataObjects.BooleanResponse> FM_UpdateProject(FMUpdateProjectRequest request)
Task<DataObjects.BooleanResponse> FM_DeleteProject(Guid projectId)

// File management
Task<List<DataObjects.FMAppFileInfo>> FM_GetAppFiles(Guid projectId)
Task<DataObjects.FMAppFileContent?> FM_GetAppFile(Guid fileId)
Task<DataObjects.FMSaveFileResponse> FM_SaveAppFile(FMSaveFileRequest request)
Task<DataObjects.FMAppFileInfo?> FM_CreateAppFile(FMCreateFileRequest request)

// Build management
Task<DataObjects.FMBuildInfo> FM_StartBuild(FMStartBuildRequest request)
Task<List<DataObjects.FMBuildInfo>> FM_GetBuilds(Guid projectId)
Task<DataObjects.FMBuildDetailInfo?> FM_GetBuild(Guid buildId)
```

## Dependencies

- `CRM.EFModels` - Entity Framework entities
- `CRM.DataObjects` - DTOs and contracts
- `CRM.Plugins` - Plugin interfaces
- `QuestPDF` - PDF generation
- `JWT` - JSON Web Token handling
- `Microsoft.EntityFrameworkCore` - ORM
