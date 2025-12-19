# CRM (Server)

ASP.NET Core server application - hosts the REST API and Blazor WebAssembly client.

## Purpose

This is the **main entry point** for the application. It:
- Hosts the ASP.NET Core web server
- Serves the Blazor WebAssembly client
- Exposes REST API endpoints via `DataController`
- Manages authentication (OAuth, OpenID, local)
- Provides SignalR real-time communication hub

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                          CRM (Server)                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                      Program.cs                              │   │
│  │                  (Application Entry)                         │   │
│  │                                                              │   │
│  │  • WebApplication builder                                    │   │
│  │  • DI container setup                                        │   │
│  │  • Authentication configuration                              │   │
│  │  • SignalR hub registration                                  │   │
│  │  • Plugin loading                                            │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                              │                                      │
│         ┌────────────────────┼────────────────────┐                │
│         ▼                    ▼                    ▼                │
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────┐          │
│  │ Controllers │     │    Hubs     │     │   Blazor    │          │
│  │             │     │             │     │   Hosting   │          │
│  │ DataController    │ crmHub      │     │             │          │
│  │ AuthController    │ (SignalR)   │     │ Serves WASM │          │
│  │ SetupController   │             │     │ client      │          │
│  └─────────────┘     └─────────────┘     └─────────────┘          │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

## Files

### Core Files

| File | Purpose |
|------|---------|
| `Program.cs` | Main entry point, DI setup, middleware configuration |
| `Program.App.cs` | User customization hooks for startup |
| `PluginsInterfaces.cs` | Plugin system interface definitions |

### Controllers

All API endpoints are in `Controllers/`:

| File | Endpoints |
|------|-----------|
| `DataController.cs` | Base controller with DI and user context |
| `DataController.Users.cs` | User CRUD endpoints |
| `DataController.Tenants.cs` | Tenant management |
| `DataController.Appointments.cs` | Scheduling endpoints |
| `DataController.Invoices.cs` | Billing endpoints |
| `DataController.Payments.cs` | Payment processing |
| `DataController.Departments.cs` | Organization structure |
| `DataController.UserGroups.cs` | Permission groups |
| `DataController.Locations.cs` | Location management |
| `DataController.Services.cs` | Service catalog |
| `DataController.Tags.cs` | Tagging system |
| `DataController.EmailTemplates.cs` | Email templates |
| `DataController.FileStorage.cs` | File upload/download |
| `DataController.ApplicationSettings.cs` | App configuration |
| `DataController.Language.cs` | Localization |
| `DataController.UDF.cs` | User-defined fields |
| `DataController.Plugins.cs` | Plugin management |
| `DataController.Utilities.cs` | Utility endpoints |
| `DataController.Ajax.cs` | AJAX helpers |
| `DataController.Encryption.cs` | Encryption services |
| `DataController.Authenticate.cs` | Login/logout |
| `DataController.App.cs` | User customization |
| `DataController.App.FreeManager.cs` | **FreeManager API endpoints** |
| `AuthorizationController.cs` | OAuth callback handling |
| `SetupController.cs` | Initial setup wizard |

## API Endpoints

### Authentication

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| POST | `/api/Data/Authenticate` | Local login | No |
| POST | `/api/Data/Logout` | End session | Yes |
| GET | `/api/Data/ValidateToken` | Check token validity | Yes |

### Users

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/Data/GetUsers` | List users | Yes |
| GET | `/api/Data/GetUser` | Get single user | Yes |
| POST | `/api/Data/SaveUser` | Create/update user | Yes |
| DELETE | `/api/Data/DeleteUser` | Soft delete user | Yes |

### Appointments (Module)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/Data/GetAppointments` | List appointments | Yes |
| GET | `/api/Data/GetAppointment` | Get appointment detail | Yes |
| POST | `/api/Data/SaveAppointment` | Create/update | Yes |
| DELETE | `/api/Data/DeleteAppointment` | Delete appointment | Yes |

### Invoices & Payments (Module)

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/Data/GetInvoices` | List invoices | Yes |
| POST | `/api/Data/SaveInvoice` | Create/update invoice | Yes |
| POST | `/api/Data/SavePayment` | Record payment | Yes |

### FreeManager Endpoints

| Method | Route | Description | Auth |
|--------|-------|-------------|------|
| GET | `/api/Data/FM_GetProjects` | List projects | Yes |
| GET | `/api/Data/FM_GetProject` | Get project | Yes |
| POST | `/api/Data/FM_CreateProject` | Create project | Yes |
| PUT | `/api/Data/FM_UpdateProject` | Update project | Yes |
| DELETE | `/api/Data/FM_DeleteProject` | Delete project | Yes |
| GET | `/api/Data/FM_GetAppFiles` | List project files | Yes |
| GET | `/api/Data/FM_GetAppFile` | Get file content | Yes |
| PUT | `/api/Data/FM_SaveAppFile` | Save file | Yes |
| POST | `/api/Data/FM_CreateAppFile` | Create new file | Yes |
| DELETE | `/api/Data/FM_DeleteAppFile` | Delete file | Yes |
| GET | `/api/Data/FM_GetFileVersions` | File history | Yes |
| GET | `/api/Data/FM_GetFileVersion` | Get specific version | Yes |
| POST | `/api/Data/FM_StartBuild` | Queue build | Yes |
| GET | `/api/Data/FM_GetBuilds` | Build history | Yes |
| GET | `/api/Data/FM_GetBuild` | Build detail | Yes |
| GET | `/api/Data/FM_DownloadArtifact` | Download ZIP | Yes |

## Security

### Authentication Flow

```
┌────────────────┐     ┌────────────────┐     ┌────────────────┐
│                │     │                │     │                │
│    Client      │────▶│   /Authenticate│────▶│   DataAccess   │
│                │     │                │     │  .Authenticate │
│                │◀────│   JWT Token    │◀────│                │
│                │     │                │     │                │
└────────────────┘     └────────────────┘     └────────────────┘
         │
         │ All subsequent requests include:
         │ Header: Authorization: Bearer <token>
         │ OR
         │ Header: Token: <token>
         ▼
┌────────────────┐
│  [Authorize]   │
│   Endpoints    │
└────────────────┘
```

### Supported Auth Providers

- **Local** - Username/password with JWT
- **OpenID Connect** - Azure AD, Okta, etc.
- **Google** - OAuth 2.0
- **Microsoft Account** - OAuth 2.0
- **Facebook** - OAuth 2.0
- **Apple** - Sign in with Apple

### Authorization Policies

```csharp
var policies = new List<string> {
    "AppAdmin",           // Super admin
    "Admin",              // Tenant admin
    "CanBeScheduled",     // Can appear in scheduling
    "ManageAppointments", // Can manage appointments
    "ManageFiles",        // Can manage files
    "PreventPasswordChange", // Cannot change password
};
```

## SignalR Hub

Real-time communication via `/crmHub`:

```csharp
public interface IsrHub
{
    Task ReceiveMessage(DataObjects.SignalRUpdate update);
}

public class crmHub : Hub<IsrHub>
{
    // Clients join tenant-specific groups
    // Updates are broadcast to tenant members
}
```

## Dependency Injection

```csharp
// DataAccess - scoped per request
builder.Services.AddTransient<IDataAccess>(x =>
    ActivatorUtilities.CreateInstance<DataAccess>(x,
        connectionString, databaseType, localModeUrl, serviceProvider, cookiePrefix));

// Plugins - loaded at startup
builder.Services.AddTransient<Plugins.IPlugins>(x => plugins);

// Configuration helper
builder.Services.AddTransient<IConfigurationHelper>(x =>
    ActivatorUtilities.CreateInstance<ConfigurationHelper>(x, configurationHelperLoader));
```

## Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "AppData": "Server=...;Database=CRM;..."
  },
  "DatabaseType": "sqlserver",
  "AuthenticationProviders": {
    "OpenId": {
      "Authority": "https://...",
      "ClientId": "...",
      "ForceHttps": true
    }
  },
  "GloballyDisabledModules": ["Invoices"],
  "GloballyEnabledModules": ["Tags"],
  "AzureSignalRurl": "",
  "AllowApplicationEmbedding": false
}
```

## Extension Points

### Program.App.cs

```csharp
// Called at builder start
partial WebApplicationBuilder AppModifyBuilderStart(WebApplicationBuilder builder);

// Called at builder end
partial WebApplicationBuilder AppModifyBuilderEnd(WebApplicationBuilder builder);

// Called at app start
partial WebApplication AppModifyStart(WebApplication app);

// Called at app end
partial WebApplication AppModifyEnd(WebApplication app);

// Custom auth policies
static partial List<string> AuthenticationPoliciesApp { get; }
```

### DataController.App.cs

```csharp
// Custom SignalR handling
private async Task<bool> SignalRUpdateApp(DataObjects.SignalRUpdate update);

// Custom endpoints
[HttpGet]
[Authorize]
[Route("~/api/Data/YourEndpoint/")]
public ActionResult<DataObjects.BooleanResponse> YourEndpoint();
```
