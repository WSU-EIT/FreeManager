# CRM.Client

Blazor WebAssembly front-end application - runs entirely in the browser.

## Purpose

This project contains the **user interface** - all Razor components, pages, layouts, and client-side logic. It's a Blazor WebAssembly app that:
- Runs in the browser via WebAssembly
- Communicates with the server via REST API
- Receives real-time updates via SignalR
- Supports offline-capable scenarios

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CRM.Client                                  │
│                    (Blazor WebAssembly)                             │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                      Layout                                  │   │
│  │  ┌─────────────────────────────────────────────────────┐    │   │
│  │  │              MainLayout.razor                        │    │   │
│  │  │  ┌──────────┐ ┌────────────────────────────────┐    │    │   │
│  │  │  │ NavMenu  │ │         @Body                  │    │    │   │
│  │  │  │          │ │   (Page Content)               │    │    │   │
│  │  │  │ • Home   │ │                                │    │    │   │
│  │  │  │ • Schedule│ │                               │    │    │   │
│  │  │  │ • Invoices│ │                               │    │    │   │
│  │  │  │ • Settings│ │                               │    │    │   │
│  │  │  └──────────┘ └────────────────────────────────┘    │    │   │
│  │  └─────────────────────────────────────────────────────┘    │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                         Pages                                 │  │
│  ├──────────────┬──────────────┬──────────────┬─────────────────┤  │
│  │   Scheduling │   Invoices   │   Settings   │  Authorization  │  │
│  │              │              │              │                 │  │
│  │ • Schedule   │ • Invoices   │ • Users      │ • Login         │  │
│  │ • EditAppt   │ • EditInvoice│ • UserGroups │ • Logout        │  │
│  │              │ • ViewInvoice│ • Departments│ • ProcessLogin  │  │
│  │              │              │ • Tenants    │ • AccessDenied  │  │
│  │              │              │ • Tags       │                 │  │
│  │              │              │ • Locations  │                 │  │
│  │              │              │ • Services   │                 │  │
│  └──────────────┴──────────────┴──────────────┴─────────────────┘  │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                    Shared Components                          │  │
│  │                                                               │  │
│  │  LoadingMessage │ ModalMessage │ Icon │ Tooltip │ TagSelector │  │
│  │  MonacoEditor   │ UploadFile   │ PDF_Viewer │ Highcharts     │  │
│  │  UserDefinedFields │ NavigationMenu │ RequiredIndicator      │  │
│  └──────────────────────────────────────────────────────────────┘  │
│                                                                     │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │                 App Extension Components                      │  │
│  │                 (Shared/AppComponents/)                       │  │
│  │                                                               │  │
│  │  Index.App.razor │ About.App.razor │ Settings.App.razor       │  │
│  │  EditUser.App.razor │ EditAppointment.App.razor │ ...         │  │
│  └──────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

## Directory Structure

```
CRM.Client/
├── Layout/
│   └── MainLayout.razor          # Main app shell with navigation
├── Pages/
│   ├── Index.razor               # Home/dashboard
│   ├── Profile.razor             # User profile
│   ├── About.razor               # About page
│   ├── ChangePassword.razor      # Password change
│   ├── Authorization/
│   │   ├── Login.razor           # Login form
│   │   ├── Logout.razor          # Logout handler
│   │   ├── ProcessLogin.razor    # OAuth callback
│   │   ├── AccessDenied.razor    # Permission denied
│   │   ├── InvalidUser.razor     # Invalid user
│   │   └── NoLocalAccount.razor  # No local account
│   ├── Scheduling/
│   │   ├── Schedule.razor        # Calendar view
│   │   └── EditAppointment.razor # Appointment editor
│   ├── Invoices/
│   │   ├── Invoices.razor        # Invoice list
│   │   ├── EditInvoice.razor     # Invoice editor
│   │   ├── ViewInvoice.razor     # Invoice preview
│   │   └── AppointmentInvoices.razor
│   ├── Payments/
│   │   └── Payments.razor        # Payment list
│   ├── Settings/
│   │   ├── Users/                # User management
│   │   ├── Departments/          # Department management
│   │   ├── Tenants/              # Tenant management
│   │   ├── Locations/            # Location management
│   │   ├── Services/             # Service catalog
│   │   ├── Tags/                 # Tag management
│   │   ├── Email/                # Email templates
│   │   ├── Files/                # File management
│   │   └── Misc/                 # App settings, UDF, etc.
│   └── TestPages/                # Development/test pages
│       ├── Monaco.razor          # Monaco editor test
│       └── Test.razor            # General testing
├── Shared/
│   ├── NavigationMenu.razor      # Side navigation
│   ├── LoadingMessage.razor      # Loading spinner
│   ├── ModalMessage.razor        # Modal dialogs
│   ├── MonacoEditor.razor        # Code editor component
│   ├── UploadFile.razor          # File upload
│   ├── PDF_Viewer.razor          # PDF display
│   ├── Highcharts.razor          # Charts
│   ├── TagSelector.razor         # Tag picker
│   ├── Icon.razor                # Icon component
│   ├── Tooltip.razor             # Tooltip wrapper
│   └── AppComponents/            # Extension components
│       ├── Index.App.razor
│       ├── About.App.razor
│       ├── EditUser.App.razor
│       └── ...
├── Routes.razor                  # Router configuration
├── _Imports.razor                # Global using statements
└── wwwroot/
    ├── css/                      # Stylesheets
    └── js/                       # JavaScript interop
```

## Key Components

### Layout Components

| Component | Purpose |
|-----------|---------|
| `MainLayout.razor` | App shell with header, nav, and content area |
| `NavigationMenu.razor` | Side navigation with module-aware menu items |

### Shared Components

| Component | Purpose |
|-----------|---------|
| `LoadingMessage.razor` | Spinning loader with message |
| `ModalMessage.razor` | Confirmation/alert dialogs |
| `MonacoEditor.razor` | VS Code-style code editor |
| `UploadFile.razor` | Drag-and-drop file upload |
| `PDF_Viewer.razor` | PDF document display |
| `Highcharts.razor` | Interactive charts |
| `TagSelector.razor` | Multi-select tag picker |
| `Icon.razor` | Icon rendering (Bootstrap Icons) |
| `Tooltip.razor` | Hover tooltips |
| `UserDefinedFields.razor` | Dynamic UDF rendering |
| `RequiredIndicator.razor` | Required field marker |

### App Components (Extensions)

Located in `Shared/AppComponents/`:

| Component | Purpose |
|-----------|---------|
| `Index.App.razor` | Dashboard customization |
| `About.App.razor` | About page customization |
| `Settings.App.razor` | Settings page extension |
| `EditUser.App.razor` | User form extension |
| `EditAppointment.App.razor` | Appointment form extension |
| `EditDepartment.App.razor` | Department form extension |
| `EditTenant.App.razor` | Tenant form extension |
| `EditTag.App.razor` | Tag form extension |

## State Management

### BlazorDataModel

Central state container injected into components:

```csharp
public class BlazorDataModel
{
    public DataObjects.User CurrentUser { get; set; }
    public DataObjects.Tenant CurrentTenant { get; set; }
    public List<DataObjects.Department> Departments { get; set; }
    public List<DataObjects.UserGroup> UserGroups { get; set; }
    public DataObjects.ApplicationSettings Settings { get; set; }
    // ... all shared state
}
```

### SignalR Integration

Real-time updates via hub connection:

```csharp
hubConnection = new HubConnectionBuilder()
    .WithUrl(NavigationManager.ToAbsoluteUri("/crmHub"))
    .WithAutomaticReconnect()
    .Build();

hubConnection.On<DataObjects.SignalRUpdate>("ReceiveMessage", update => {
    // Handle real-time update
    StateHasChanged();
});
```

## API Communication

All API calls go through a service layer:

```csharp
// Example: Get users
var response = await Http.GetFromJsonAsync<List<DataObjects.User>>(
    DataObjects.Endpoints.GetUsers);

// Example: Save user
var result = await Http.PostAsJsonAsync<DataObjects.BooleanResponse>(
    DataObjects.Endpoints.SaveUser, user);
```

## Routing

Defined in `Routes.razor`:

```razor
<Router AppAssembly="@typeof(App).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MainLayout)" />
    </Found>
    <NotFound>
        <PageTitle>Not found</PageTitle>
        <LayoutView Layout="@typeof(MainLayout)">
            <NotFound />
        </LayoutView>
    </NotFound>
</Router>
```

## Page Routes

| Route | Page | Description |
|-------|------|-------------|
| `/` | Index.razor | Home/dashboard |
| `/profile` | Profile.razor | User profile |
| `/about` | About.razor | About info |
| `/schedule` | Schedule.razor | Calendar |
| `/schedule/edit/{id}` | EditAppointment.razor | Edit appointment |
| `/invoices` | Invoices.razor | Invoice list |
| `/invoices/edit/{id}` | EditInvoice.razor | Edit invoice |
| `/payments` | Payments.razor | Payment list |
| `/settings/users` | Users.razor | User management |
| `/settings/departments` | Departments.razor | Departments |
| `/settings/tags` | Tags.razor | Tag management |
| `/login` | Login.razor | Login form |
| `/logout` | Logout.razor | Logout |

## Module-Aware UI

Navigation items are shown/hidden based on enabled modules:

```razor
@if (DataModel.EnabledModules.Contains("appointments"))
{
    <NavLink href="schedule">
        <Icon Name="calendar" /> Schedule
    </NavLink>
}

@if (DataModel.EnabledModules.Contains("invoices"))
{
    <NavLink href="invoices">
        <Icon Name="receipt" /> Invoices
    </NavLink>
}
```

## Extension Pattern

Pages can be extended via App components:

```razor
<!-- In EditUser.razor -->
<div class="form-group">
    <label>First Name</label>
    <input @bind="User.FirstName" />
</div>

<!-- Extension point -->
<EditUser_App User="@User" />

<div class="form-group">
    <label>Last Name</label>
    <input @bind="User.LastName" />
</div>
```

## JavaScript Interop

For functionality requiring JS:

```csharp
// MonacoEditor.razor
await JSRuntime.InvokeVoidAsync("monacoInterop.createEditor", elementId, options);
var content = await JSRuntime.InvokeAsync<string>("monacoInterop.getValue", elementId);
```

## CSS

- Bootstrap 5 for layout and components
- Custom styles in `wwwroot/css/`
- Component-scoped CSS via `.razor.css` files
