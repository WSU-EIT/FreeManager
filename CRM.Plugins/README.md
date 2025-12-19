# CRM.Plugins

Plugin system for dynamic code execution and extensibility.

## Purpose

This project provides a **plugin architecture** that allows:
- Loading external DLL plugins at runtime
- Executing dynamic C# code
- Extending application functionality without recompilation
- Secure code execution with assembly sandboxing

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CRM.Plugins                                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                     IPlugins Interface                       │   │
│  │                                                              │   │
│  │  • Load(path) - Load plugins from folder                    │   │
│  │  • ExecuteDynamicCSharpCode<T>() - Run dynamic code         │   │
│  │  • AllPlugins - Get loaded plugins                          │   │
│  │  • ServerReferences - Assemblies for dynamic code           │   │
│  │  • UsingStatements - Auto-added using statements            │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                              │                                      │
│                              ▼                                      │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                     Plugins Class                            │   │
│  │                                                              │   │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐         │   │
│  │  │   Plugin    │  │   Plugin    │  │   Plugin    │         │   │
│  │  │   Loader    │  │   Cache     │  │   Runtime   │         │   │
│  │  │             │  │             │  │             │         │   │
│  │  │ • DLL scan  │  │ • Metadata  │  │ • Roslyn    │         │   │
│  │  │ • Assembly  │  │ • Version   │  │ • Compile   │         │   │
│  │  │   loading   │  │ • Author    │  │ • Execute   │         │   │
│  │  └─────────────┘  └─────────────┘  └─────────────┘         │   │
│  └─────────────────────────────────────────────────────────────┘   │
│                                                                     │
│  ┌─────────────────────────────────────────────────────────────┐   │
│  │                    Encryption Helper                         │   │
│  │                                                              │   │
│  │  • AES encryption/decryption for secure plugin storage      │   │
│  └─────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────┘
```

## Files

| File | Purpose |
|------|---------|
| `Plugins.cs` | Main plugin system with loader, cache, and dynamic execution |
| `Encryption.cs` | AES encryption utilities for plugin content |

## Interfaces

### IPlugins

```csharp
public interface IPlugins
{
    /// <summary>
    /// Gets all loaded plugins.
    /// </summary>
    List<Plugin> AllPlugins { get; }

    /// <summary>
    /// Gets plugins formatted for database cache.
    /// </summary>
    List<Plugin> AllPluginsForCache { get; }

    /// <summary>
    /// Executes dynamic C# code and returns result.
    /// </summary>
    T? ExecuteDynamicCSharpCode<T>(
        string code,
        IEnumerable<object>? objects,
        List<string>? additionalAssemblies,
        string Namespace,
        string Classname,
        string invokerFunction);

    /// <summary>
    /// Loads plugins from a folder path.
    /// </summary>
    List<Plugin> Load(string path);

    /// <summary>
    /// Path to the plugins folder.
    /// </summary>
    string PluginFolder { get; }

    /// <summary>
    /// Assembly references for dynamic code execution.
    /// </summary>
    List<string> ServerReferences { get; set; }

    /// <summary>
    /// Using statements auto-added to dynamic code.
    /// </summary>
    List<string> UsingStatements { get; set; }
}
```

### Plugin Model

```csharp
public class Plugin
{
    public string Name { get; set; }
    public string Version { get; set; }
    public string Author { get; set; }
    public string Namespace { get; set; }
    public string ClassName { get; set; }
    public string Type { get; set; }  // DLL or CSharp
    public string Code { get; set; }  // For dynamic plugins
    // ... additional metadata
}
```

## Usage

### Loading Plugins at Startup

```csharp
// In Program.cs
var plugins = new Plugins.Plugins();

// Set assembly references for dynamic code
plugins.ServerReferences = new List<string> {
    typeof(DataAccess).Assembly.Location,
    typeof(DataObjects.BooleanResponse).Assembly.Location,
    typeof(EFModels.User).Assembly.Location,
    // ... other assemblies
};

// Set using statements from config
plugins.UsingStatements = new List<string> {
    "using System;",
    "using System.Linq;",
    "using CRM;",
    // ... other using statements
};

// Load plugins from folder
string pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
plugins.Load(pluginsPath);

// Register with DI
builder.Services.AddTransient<Plugins.IPlugins>(x => plugins);
```

### Executing Dynamic Code

```csharp
// Execute C# code at runtime
var result = plugins.ExecuteDynamicCSharpCode<DataObjects.BooleanResponse>(
    code: @"
        namespace MyPlugin {
            public class MyClass {
                public DataObjects.BooleanResponse Execute(IDataAccess da) {
                    // Your dynamic code here
                    return new DataObjects.BooleanResponse { Result = true };
                }
            }
        }",
    objects: new object[] { dataAccess },
    additionalAssemblies: null,
    Namespace: "MyPlugin",
    Classname: "MyClass",
    invokerFunction: "Execute"
);
```

## Plugin Types

### 1. DLL Plugins

Pre-compiled .NET assemblies placed in the `Plugins` folder:

```
/Plugins/
  ├── MyPlugin.dll
  ├── AnotherPlugin.dll
  └── CustomIntegration.dll
```

### 2. Dynamic C# Plugins

C# code stored in the database and compiled at runtime:

```csharp
// Stored in PluginCache table
{
    "Name": "CustomCalculation",
    "Type": "CSharp",
    "Code": "namespace Custom { public class Calc { ... } }"
}
```

## Dynamic Code Execution Flow

```
┌───────────────┐     ┌───────────────┐     ┌───────────────┐
│  Source Code  │────▶│    Roslyn     │────▶│   Assembly    │
│   (string)    │     │   Compiler    │     │  (in-memory)  │
└───────────────┘     └───────────────┘     └───────────────┘
                                                    │
                                                    ▼
┌───────────────┐     ┌───────────────┐     ┌───────────────┐
│    Result     │◀────│   Invoke      │◀────│  Reflection   │
│   (object)    │     │   Method      │     │  Load Type    │
└───────────────┘     └───────────────┘     └───────────────┘
```

## Security Considerations

### Assembly Sandboxing

- Dynamic code runs with limited permissions
- No file system access by default
- Network access controlled
- Assembly references explicitly allowed

### Encryption

Plugin content can be encrypted for storage:

```csharp
// Encrypt plugin code
var encrypted = Encryption.Encrypt(pluginCode);

// Decrypt for execution
var decrypted = Encryption.Decrypt(encrypted);
```

### Validation

- Plugins are validated before loading
- Code is scanned for dangerous patterns
- Assembly signatures verified (if configured)

## Plugin Discovery

The loader scans for:

1. **DLL files** implementing plugin interfaces
2. **Database records** in PluginCache table
3. **JSON manifests** describing plugin metadata

## Dependencies

- `Microsoft.CodeAnalysis.CSharp` - Roslyn compiler for dynamic code
- `System.Reflection` - Assembly loading and type discovery

## Configuration

In `appsettings.json`:

```json
{
  "PluginUsingStatements": [
    "using System;",
    "using System.Linq;",
    "using System.Collections.Generic;",
    "using CRM;",
    "using CRM.DataObjects;"
  ],
  "PluginSettings": {
    "AllowDynamicCode": true,
    "MaxExecutionTimeMs": 5000,
    "EnableSandbox": true
  }
}
```

## Integration Points

### DataAccess

```csharp
// Plugins can access DataAccess methods
public DataObjects.BooleanResponse Execute(IDataAccess da)
{
    var users = da.GetUsers();
    // Process users...
}
```

### HttpContext

```csharp
// Plugins can access request context
public DataObjects.BooleanResponse Execute(HttpContext context)
{
    var userId = context.User.FindFirst("sub")?.Value;
    // Use user info...
}
```

### SignalR

```csharp
// Plugins can send real-time updates
public async Task Execute(IHubContext<crmHub> signalR)
{
    await signalR.Clients.All.SendAsync("ReceiveMessage", update);
}
```
