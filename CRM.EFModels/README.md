# CRM.EFModels

Entity Framework Core data model layer containing all database entities and the DbContext.

## Purpose

This project defines the **persistence layer** - the shape of data as it exists in the database. All EF Core entities, relationships, constraints, and the `DbContext` live here.

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CRM.EFModels                                │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                     EFDataModel.cs                          │   │
│  │                 (DbContext - Main Entry)                    │   │
│  │                                                             │   │
│  │  • DbSet<T> for each entity                                │   │
│  │  • OnModelCreating() for Fluent API config                 │   │
│  │  • Partial class pattern for extensions                    │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                              │                                      │
│              ┌───────────────┼───────────────┐                     │
│              ▼               ▼               ▼                     │
│  ┌───────────────┐ ┌───────────────┐ ┌───────────────┐            │
│  │  Core CRM     │ │  Optional     │ │  FreeManager  │            │
│  │  Entities     │ │  Modules      │ │  Entities     │            │
│  │               │ │               │ │               │            │
│  │  • User       │ │  • Appointment│ │  • FMProject  │            │
│  │  • Tenant     │ │  • Invoice    │ │  • FMAppFile  │            │
│  │  • Department │ │  • Payment    │ │  • FMAppFile  │            │
│  │  • UserGroup  │ │  • Location   │ │    Version    │            │
│  │  • Setting    │ │  • Service    │ │  • FMBuild    │            │
│  │  • FileStorage│ │  • Tag        │ │               │            │
│  └───────────────┘ │  • Email      │ └───────────────┘            │
│                    │    Template   │                               │
│                    └───────────────┘                               │
└─────────────────────────────────────────────────────────────────────┘
```

## Files

### Core Files

| File | Purpose |
|------|---------|
| `EFDataModel.cs` | Main DbContext with all DbSets and Fluent API configuration |
| `EFModelOverrides.cs` | Entity-level customizations and validation |

### Core Entities (Always Included)

| Entity | Table | Description |
|--------|-------|-------------|
| `User.cs` | Users | User accounts with profile, auth, and tenant association |
| `Tenant.cs` | Tenants | Multi-tenant isolation container |
| `Department.cs` | Departments | Organizational structure |
| `DepartmentGroup.cs` | DepartmentGroups | Grouping of departments |
| `UserGroup.cs` | UserGroups | Role-based permission groups |
| `UserInGroup.cs` | UserInGroups | Many-to-many user/group junction |
| `Setting.cs` | Settings | Key-value application settings |
| `FileStorage.cs` | FileStorage | Binary file attachments |
| `UDFLabel.cs` | UDFLabels | User-defined field labels |
| `PluginCache.cs` | PluginCache | Cached plugin metadata |

### Optional Module Entities

These entities are wrapped with `{{ModuleItemStart:X}}` / `{{ModuleItemEnd:X}}` markers for conditional removal:

| Entity | Module | Description |
|--------|--------|-------------|
| `Appointment.cs` | Appointments | Scheduled appointments |
| `AppointmentNote.cs` | Appointments | Notes attached to appointments |
| `AppointmentService.cs` | Appointments | Services linked to appointments |
| `AppointmentUser.cs` | Appointments | Attendees for appointments |
| `Invoice.cs` | Invoices | Billing invoices |
| `Payment.cs` | Payments | Payment records |
| `Location.cs` | Locations | Physical locations |
| `Service.cs` | Services | Billable services |
| `Tag.cs` | Tags | Tagging system |
| `TagItem.cs` | Tags | Tag-to-entity junction |
| `EmailTemplate.cs` | EmailTemplates | Email template storage |

### FreeManager Extension Entities

| Entity | Table | Description |
|--------|-------|-------------|
| `FMProject.cs` | FMProjects | User's custom application project |
| `FMAppFile.cs` | FMAppFiles | .App. file metadata |
| `FMAppFileVersion.cs` | FMAppFileVersions | File content versioning |
| `FMBuild.cs` | FMBuilds | Build job tracking |
| `EFDataModel.App.FreeManager.cs` | - | Partial class adding FM DbSets |

## Extension Pattern

The project uses **partial classes** for extension without modification:

```csharp
// Core file (never modify)
public partial class EFDataModel : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    // ...
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}

// Extension file (safe to modify)
public partial class EFDataModel
{
    public virtual DbSet<FMProject> FMProjects { get; set; }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        // Custom configuration here
    }
}
```

## Database Support

The DbContext supports multiple database providers:

| Provider | Configuration |
|----------|---------------|
| SQL Server | `UseSqlServer()` with retry |
| PostgreSQL | `UseNpgsql()` with retry |
| MySQL | `UseMySQL()` with retry |
| SQLite | `UseSqlite()` |
| In-Memory | `UseInMemoryDatabase()` for testing |

## Key Conventions

1. **Primary Keys**: All entities use `Guid` primary keys with `ValueGeneratedNever()`
2. **Soft Delete**: Entities have `Deleted` and `DeletedAt` columns
3. **Audit Trail**: `Added`, `AddedBy`, `LastModified`, `LastModifiedBy` columns
4. **Multi-Tenancy**: Most entities have `TenantId` foreign key

## Relationships

```
Tenant ─────┬──── Users
            ├──── Departments
            ├──── UserGroups
            ├──── Appointments
            ├──── Invoices
            └──── FMProjects

User ───────┬──── FileStorages
            ├──── UserInGroups ──── UserGroups
            └──── AppointmentUsers ──── Appointments

FMProject ──┬──── FMAppFiles ──── FMAppFileVersions
            └──── FMBuilds
```

## Security Considerations

- Entities do NOT enforce security - that's the DataAccess layer's job
- Navigation properties can leak cross-tenant data if not careful
- Sensitive fields should be marked with `[Sensitive]` attribute in DataObjects
