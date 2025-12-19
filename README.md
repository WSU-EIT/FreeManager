# FreeManager

An open-source CRM solution built in **C# Blazor WebAssembly** using **.NET 10**. Based on the [FreeCRM](https://github.com/WSU-EIT/FreeCRM) framework.

You can use this project as-is, customize it to fit your needs, or grab the code you need for your own project.

## Technology Stack

### Backend
| Technology | Purpose |
|------------|---------|
| **.NET 10** | Runtime and SDK |
| **ASP.NET Core 10** | Web framework and API hosting |
| **Entity Framework Core 10** | ORM with multi-database support |
| **SignalR** | Real-time communication |
| **Azure SignalR Service** | Scalable SignalR hosting |
| **QuestPDF** | PDF document generation |

### Frontend
| Technology | Purpose |
|------------|---------|
| **Blazor WebAssembly** | Client-side SPA framework |
| **MudBlazor** | Material Design components |
| **Radzen.Blazor** | Additional UI components |
| **Blazor.Bootstrap** | Bootstrap components |
| **BlazorMonaco** | Code editor component |
| **Blazored.LocalStorage** | Client-side storage |

### Database Support
| Database | Package |
|----------|---------|
| **SQL Server** | Microsoft.EntityFrameworkCore.SqlServer |
| **PostgreSQL** | Npgsql.EntityFrameworkCore.PostgreSQL |
| **MySQL** | MySql.EntityFrameworkCore |
| **SQLite** | Microsoft.EntityFrameworkCore.Sqlite |
| **In-Memory** | Microsoft.EntityFrameworkCore.InMemory |

### Authentication Providers
- Google
- Facebook
- Microsoft Account
- Apple
- OpenID Connect
- Active Directory / LDAP

## Modular Architecture

FreeCRM is designed as a **modular, extensible framework**. You can build completely different applications on top of it (like [FreeCICD](https://github.com/WSU-EIT/FreeCICD) for CI/CD pipelines) while maintaining the ability to receive upstream updates.

### The `.App.` File Convention

The framework uses **partial classes** and a **file naming convention** to separate:

| File Pattern | Purpose | Should You Modify? |
|--------------|---------|-------------------|
| `*.cs` / `*.razor` | Core framework code | **Never** - updates will overwrite |
| `*.App.cs` / `*.App.razor` | Customization hooks | **Extend** - add code, don't remove |
| `*.App.{ProjectName}.cs` | Your project code | **Yes** - this is where your code lives |

### How It Works

```
┌─────────────────────────────────────────────────────────────────┐
│                     Your Project Code                            │
│              *.App.{ProjectName}.cs files                       │
│         (Add new methods, DTOs, endpoints, UI)                  │
├─────────────────────────────────────────────────────────────────┤
│                   Customization Hooks                            │
│                    *.App.cs files                               │
│    (Template methods called by core, language strings)          │
├─────────────────────────────────────────────────────────────────┤
│                    Core Framework                                │
│                     *.cs files                                  │
│         (Don't modify - receives upstream updates)              │
└─────────────────────────────────────────────────────────────────┘
```

### Extension Points by Layer

#### DataObjects (`CRM.DataObjects/`)
```
DataObjects.cs              # Core DTOs (don't modify)
DataObjects.App.cs          # Add custom DTOs and extend existing ones
DataObjects.App.MyProject.cs # Your project-specific DTOs
GlobalSettings.App.cs       # App name, version, configuration
```

**Example - Extending a DTO:**
```csharp
// In DataObjects.App.MyProject.cs
public partial class DataObjects
{
    // Extend existing User class
    public partial class User
    {
        public string? MyCustomProperty { get; set; }
    }

    // Add new DTOs
    public class MyNewFeature
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
```

#### DataAccess (`CRM.DataAccess/`)
```
DataAccess.cs               # Core data methods (don't modify)
DataAccess.App.cs           # Hooks: DataAccessAppInit(), GetBlazorDataModelApp(), etc.
DataAccess.App.MyProject.cs # Your business logic and data methods
```

**Available Hooks in `DataAccess.App.cs`:**
| Hook Method | Purpose |
|-------------|---------|
| `DataAccessAppInit()` | Initialize app-specific services |
| `GetBlazorDataModelApp()` | Load custom data into Blazor model |
| `DeleteAllPendingDeletedRecordsApp()` | Custom soft-delete cleanup |
| `GetFilterColumnsApp()` | Add custom filter columns |
| `AppLanguage` dictionary | Custom language/localization strings |

#### Controllers (`CRM/Controllers/`)
```
DataController.cs           # Core API endpoints (don't modify)
DataController.App.cs       # Hook for custom endpoints
DataController.App.MyProject.cs # Your API endpoints
```

**Example - Adding API Endpoints:**
```csharp
// In DataController.App.MyProject.cs
public partial class DataController
{
    [HttpGet]
    [Route("api/Data/GetMyFeatures")]
    public async Task<ActionResult<List<DataObjects.MyFeature>>> GetMyFeatures()
    {
        return await _da.GetMyFeatures();
    }
}
```

#### UI Components (`CRM.Client/Shared/AppComponents/`)
```
Index.App.razor             # Custom home page content
About.App.razor             # Custom about page
Settings.App.razor          # App-specific settings
EditUser.App.razor          # User form extensions
```

#### Styling (`CRM.Client/wwwroot/css/`)
```
site.css                    # Core styles (don't modify)
site.App.css                # Your custom styles
```

### Creating a New Project

```bash
# 1. Clone FreeCRM
git clone https://github.com/WSU-EIT/FreeCRM.git MyProject
cd MyProject

# 2. Rename everything to your project name
"Rename FreeCRM.exe" MyProject

# 3. Remove modules you don't need
"Remove Modules from FreeCRM.exe" remove:Invoices,Payments,Appointments

# 4. Create your project-specific files:
#    - MyProject.DataObjects/DataObjects.App.MyProject.cs
#    - MyProject.DataAccess/DataAccess.App.MyProject.cs
#    - MyProject/Controllers/DataController.App.MyProject.cs

# 5. Update GlobalSettings.App.cs with your app info

# 6. Customize UI in *.App.razor files
```

### Real-World Example: FreeCICD

[FreeCICD](https://github.com/WSU-EIT/FreeCICD) is a CI/CD pipeline management tool built on FreeCRM:

| What They Did | How |
|---------------|-----|
| Renamed project | `Rename FreeCRM.exe FreeCICD` |
| Removed CRM modules | `Remove Modules... remove:Invoices,Payments,...` |
| Added Azure DevOps integration | `DataAccess.App.FreeCICD.cs` |
| Added DevOps DTOs | `DataObjects.App.FreeCICD.cs` |
| Added pipeline endpoints | `DataController.App.FreeCICD.cs` |
| Custom configuration | `GlobalSettings.App.cs` |

The result is a completely different application that shares the same authentication, user management, and infrastructure code.

## Project Structure

```
FreeManager/
├── CRM/                      # ASP.NET Core server application
│   ├── Controllers/          # API controllers
│   │   ├── DataController.cs           # Core endpoints
│   │   ├── DataController.App.cs       # Extension hooks
│   │   └── DataController.App.*.cs     # Project-specific
│   ├── Plugins/              # Server-side plugins
│   ├── wwwroot/              # Static files
│   └── Program.cs            # Application entry point
│
├── CRM.Client/               # Blazor WebAssembly client
│   ├── Pages/                # Routable Razor pages
│   ├── Shared/
│   │   └── AppComponents/    # *.App.razor customization files
│   └── wwwroot/
│       └── css/
│           ├── site.css      # Core styles
│           └── site.App.css  # Custom styles
│
├── CRM.DataAccess/           # Data access layer
│   ├── DataAccess.cs         # Core methods
│   ├── DataAccess.App.cs     # Hooks
│   └── DataAccess.App.*.cs   # Project-specific
│
├── CRM.DataObjects/          # Data transfer objects
│   ├── DataObjects.cs        # Core DTOs
│   ├── DataObjects.App.cs    # Extension point
│   └── GlobalSettings.App.cs # App configuration
│
├── CRM.EFModels/             # Entity Framework models
│   └── EFModels/             # Database entities
│
├── CRM.Plugins/              # Plugin system
│
├── docs/                     # Documentation
│   ├── meeting-notes/        # Team discussion notes
│   └── architecture/         # ADRs
│
├── CRM.slnx                  # Solution file
├── Rename FreeCRM.exe        # Project rename utility
└── Remove Modules from FreeCRM.exe  # Module removal utility
```

## Core Modules

| Module | Description | Optional |
|--------|-------------|----------|
| **Contacts** | Contact and customer management | No |
| **Departments** | Department organization | No |
| **User Groups** | User permissions and grouping | No |
| **Appointments** | Calendar and scheduling | Yes |
| **Email Templates** | Email template management | Yes |
| **Invoices** | Invoice generation and tracking | Yes |
| **Locations** | Location/address management | Yes |
| **Payments** | Payment processing | Yes |
| **Services** | Service catalog | Yes |
| **Tags** | Tagging and categorization | Yes |

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Visual Studio 2022 17.12+](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# Dev Kit
- Database server (SQL Server, PostgreSQL, MySQL, or SQLite)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/DanielPepka/FreeManager.git
   cd FreeManager
   ```

2. **Configure the database**

   Update connection string in `CRM/appsettings.json`

3. **Run the application**
   ```bash
   dotnet run --project CRM
   ```

4. **Access the application**

   Navigate to `https://localhost:5001` in your browser.

## Customization Tools

### Rename Project

Use the rename utility to customize the project name, namespaces, and GUIDs:

```bash
"Rename FreeCRM.exe" MyProjectName
```

This will:
- Rename all project files and folders
- Generate new GUIDs for each project
- Update namespaces throughout the codebase
- Update solution file references

### Remove Optional Modules

Remove modules you don't need using the removal utility:

```bash
# Remove specific modules
"Remove Modules from FreeCRM.exe" remove:Invoices,Payments

# Keep only specific modules
"Remove Modules from FreeCRM.exe" keep:Tags,Appointments

# Remove all optional modules
"Remove Modules from FreeCRM.exe" remove:all
```

**Available optional modules:**
- Appointments
- EmailTemplates
- Invoices
- Locations
- Payments
- Services
- Tags

> **Note:** The removal tool may leave some remnants. If you find any, please open an issue on GitHub with the file name and line number.

> **Note:** These utilities are Windows-only (.exe). Cross-platform alternatives are planned.

## Development

### Running Tests

```bash
dotnet test
```

### Building for Production

```bash
dotnet publish -c Release
```

### Database Migrations

```bash
# Add migration
dotnet ef migrations add MigrationName --project CRM.EFModels --startup-project CRM

# Apply migrations
dotnet ef database update --project CRM.EFModels --startup-project CRM
```

## CI/CD Pipeline

GitHub Actions workflow runs on push to `main`, `master`, and `claude/**` branches:

| Job | Purpose |
|-----|---------|
| **build** | Restore, build, and run tests |
| **publish** | Create deployment artifacts |

Both jobs run in parallel on `ubuntu-latest`.

## Plugin System

FreeCRM includes a plugin system for extending functionality. See `CRM/Plugins/Plugins.md` for documentation.

Example plugins included:
- `HelloWorld` - Basic plugin example
- `Example1`, `Example2`, `Example3` - Feature examples
- `LoginWithPrompts` - Custom login flow
- `UserUpdate` - User lifecycle hooks

## Configuration

Key settings in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=FreeCRM;..."
  },
  "Authentication": {
    "Google": { "ClientId": "", "ClientSecret": "" },
    "Facebook": { "AppId": "", "AppSecret": "" },
    "Microsoft": { "ClientId": "", "ClientSecret": "" }
  }
}
```

App-specific settings in `GlobalSettings.App.cs`:

```csharp
public static class App
{
    public static string Name { get; set; } = "FreeManager";
    public static string Version { get; set; } = "1.0.0";
    public static string CompanyName { get; set; } = "Your Company";
    // Add your app-specific settings here
}
```

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Add code to `.App.` files (never modify core files)
4. Commit your changes (`git commit -m 'Add amazing feature'`)
5. Push to the branch (`git push origin feature/amazing-feature`)
6. Open a Pull Request

## Documentation

Additional documentation is available in the `docs/` folder:

- [Meeting Notes](docs/meeting-notes/) - Team discussion records
- [Architecture Decisions](docs/architecture/) - ADRs

## License

See [LICENSE](LICENSE) file for details.

## Resources

- [FreeCRM (Original)](https://github.com/WSU-EIT/FreeCRM)
- [FreeCICD (Example Derivative)](https://github.com/WSU-EIT/FreeCICD)
- [Blazor Documentation](https://learn.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core](https://learn.microsoft.com/en-us/ef/core/)
- [MudBlazor](https://mudblazor.com/)
- [Radzen Blazor](https://blazor.radzen.com/)
