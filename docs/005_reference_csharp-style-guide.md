# FreeManager: C# Style Guide

> Comprehensive coding conventions for the FreeManager project.

This guide is consolidated from explicit style preferences observed in the FreeCRM project. Follow these conventions for all new code.

---

## Quick Reference (Most Common)

| Topic | Rule |
|-------|------|
| Opening braces (classes/methods) | New line |
| Opening braces (if/for/while) | Same line |
| Variable declarations | Explicit type + `new()` |
| Private fields | `_camelCase` prefix |
| Local variables | `camelCase` (no prefix) |
| String building | Interpolation `$""` |
| Null checks | Explicit `if (x == null)` |
| LINQ | Fluent method syntax |
| Async methods | No `Async` suffix |
| File size | 0-300 ideal, 600 max |

---

## Braces

### Opening Brace Placement

**Classes, methods, namespaces:** new line
```csharp
public class MyService
{
    public void DoSomething()
    {
    }
}
```

**Control statements (if, for, while, foreach, switch, try):** same line
```csharp
if (condition) {
    Execute();
}

for (int i = 0; i < 10; i++) {
    Process(i);
}

foreach (var item in items) {
    Process(item);
}
```

### Single-Statement Braces

Always use braces, except for early guard clauses:
```csharp
public void Process(User user)
{
    // Guard clauses: single-line, no braces
    if (user == null) throw new ArgumentNullException(nameof(user));
    if (!user.IsAuthorized) return;

    // Regular control flow: always braces
    if (condition) {
        DoSomething();
    }
}
```

### Return Statements

Prefer single return at end. Early returns only for guard clauses:
```csharp
// GOOD
public string GetDisplayName(User user)
{
    if (user == null) throw new ArgumentNullException(nameof(user));
    
    string result = String.Empty;
    
    if (user.HasNickname) {
        result = user.Nickname;
    } else {
        result = user.FullName;
    }
    
    return result;
}
```

---

## Type Declarations

### var vs Explicit Types

Prefer explicit types with target-typed `new()`:
```csharp
// GOOD
List<string> names = new();
Dictionary<int, User> userCache = new();
StringBuilder sb = new();
string name = GetName();
int count = items.Count;

// AVOID
var names = new List<string>();
var user = GetUser();
```

`var` acceptable for iteration:
```csharp
foreach (var item in items) {
    Process(item);
}
```

---

## Naming Conventions

### Private Fields

Underscore prefix for injected services and long-lived state:
```csharp
public class OrderService
{
    private readonly IDbConnection _dbConnection;
    private readonly ILogger _logger;
    private int _orderCount;
}
```

### Protected Fields

Underscore prefix:
```csharp
public class BaseComponent
{
    protected bool _loadedData = false;
    protected string _pageName = "home";
}
```

### Local Variables

camelCase, no prefix:
```csharp
public void ProcessOrder(Order order)
{
    OrderValidator validator = new();
    DateTime now = DateTime.UtcNow;
    bool newRecord = false;
}
```

---

## Properties

Auto-properties for simple get/set:
```csharp
public string Name { get; set; }
public int Count { get; private set; }
```

Expression-bodied for computed:
```csharp
public string FullName => $"{FirstName} {LastName}";
public bool IsValid => Items.Count > 0;
```

---

## Methods

### Expression-Bodied vs Block Body

Expression-bodied if it fits one line:
```csharp
public string GetName() => _name;
public bool IsValid() => _count > 0;
public void Notify() => _eventBus.Publish(this);
```

Block body if it would wrap:
```csharp
public string GetFullDisplayName()
{
    return $"{FirstName} {MiddleName} {LastName}".Trim();
}
```

---

## Strings

Use interpolation as default:
```csharp
string message = $"Hello, {name}! You have {count} items.";
string path = $"{baseDir}/{folder}/{filename}";
```

---

## LINQ

Fluent method syntax, one operation per line:
```csharp
List<string> names = users
    .Where(u => u.IsActive)
    .OrderBy(u => u.LastName)
    .Select(u => u.FullName)
    .ToList();
```

Avoid query syntax.

---

## Null Handling

Prefer explicit null checks:
```csharp
// GOOD
if (user != null) {
    string name = user.Name;
    Process(user);
}

if (user == null) throw new ArgumentNullException(nameof(user));
if (user == null) return;
```

Null-coalescing assignment for initialization:
```csharp
_cache ??= new Dictionary<int, User>();
name ??= "Unknown";
```

---

## Exception Handling

Catch general Exception, type-check for specific handling:
```csharp
try {
    await SaveAsync();
} catch (Exception ex) {
    if (ex is DbUpdateException) {
        _logger.LogError(ex, "Database error");
        throw;
    }
    if (ex is ValidationException validationEx) {
        return BadRequest(validationEx.Message);
    }
    
    _logger.LogError(ex, "Unexpected error");
    throw;
}
```

---

## Async/Await

No `Async` suffix on method names. Always await:
```csharp
// GOOD
public async Task<User> GetUser(int id)
{
    User user = await _repository.FindById(id);
    return user;
}

// AVOID
public Task<User> GetUserAsync(int id)
{
    return _repository.FindById(id);
}
```

---

## File Organization

### Size Limits

| Category | Lines | Action |
|----------|-------|--------|
| Ideal | 0-300 | Target |
| Large | 300-500 | Consider splitting |
| Maximum | 500-600 | Hard limit |
| Override | 600+ | Requires justification |

### Splitting with Partial Classes

```csharp
// DataAccess.cs - Core
public partial class DataAccess : IDisposable, IDataAccess
{
    private readonly ILogger _logger;
}

// DataAccess.Users.cs - User methods
public partial interface IDataAccess
{
    Task<User> GetUser(Guid UserId);
}

public partial class DataAccess
{
    public async Task<User> GetUser(Guid UserId)
    {
        // Implementation
    }
}
```

---

## Class Structure

### Member Ordering

Fields, Properties, Constructors, Methods. Use regions:
```csharp
public class UserService
{
    #region Fields
    private readonly ILogger _logger;
    #endregion

    #region Properties
    public int Count { get; private set; }
    #endregion

    #region Constructors
    public UserService(ILogger logger)
    {
        _logger = logger;
    }
    #endregion

    #region Public Methods
    public void Process() { }
    #endregion

    #region Private Methods
    private void Validate() { }
    #endregion
}
```

### Access Modifiers

Always explicit, never implicit:
```csharp
public class UserService
{
    private readonly ILogger _logger;
    public int Count => _count;
    protected virtual void OnChanged() { }
    internal void InternalOp() { }
    private void Validate() { }
}
```

---

## Comments

Minimal comments. Own line only, never inline:
```csharp
// GOOD
public async Task<User> GetUser(int id)
{
    // Check cache first
    if (_cache.TryGetValue(id, out User user)) {
        return user;
    }
    
    // Fall back to database
    return await _repository.FindById(id);
}

// AVOID
if (_cache.TryGetValue(id, out User user)) { // check cache
    return user;
}
```

---

## Switch

Switch expressions for value mappings:
```csharp
string text = status switch
{
    Status.Active => "Active",
    Status.Pending => "Pending",
    _ => "Unknown"
};
```

Traditional switch for complex logic with side effects.

---

## Namespaces

File-scoped:
```csharp
namespace MyApp.Services;

public class UserService
{
}
```

---

## Blazor Components

### Code Organization

`@code` block at bottom for short components. Code-behind (`.razor.cs`) when approaching size limits.

### Size Limits

| Category | Lines | Action |
|----------|-------|--------|
| Ideal | 0-450 | Good |
| Soft cap | 500 | Consider refactoring |
| Hard cap | 600 | Requires justification |

### Loading Pattern

Pages start hidden with spinner:
```razor
@if (!loaded) {
    <div class="d-flex justify-content-center p-5">
        <div class="spinner-border text-primary"></div>
    </div>
} else {
    <div class="container">
        @* Content *@
    </div>
}

@code {
    private bool loaded;
    
    protected override async Task OnInitializedAsync()
    {
        // Load data
        loaded = true;
    }
}
```

### Event Handlers

Named methods preferred. Lambdas only for parameters:
```razor
<button @onclick="Toggle">Toggle</button>
<button @onclick="Save">Save</button>
<button @onclick="() => Delete(item.Id)">Delete</button>
```

---

## Bootstrap & Styling

All classes on single line:
```razor
<div class="container mt-4 p-3 bg-light rounded shadow-sm d-flex justify-content-between">
    <span>Content</span>
</div>
```

Use Bootstrap exclusively. Custom CSS only when Bootstrap cannot achieve result.

### Font Awesome

Inline `<i>` tags:
```razor
<button class="btn btn-primary">
    <i class="fa-solid fa-save me-2"></i>Save
</button>
```

---

## Dependency Injection

### In Classes

Readonly fields with underscore:
```csharp
public class DataAccess
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public DataAccess(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
}
```

### In Blazor

Property injection:
```razor
@inject BlazorDataModel Model
@inject NavigationManager Navigation
```

---

## Enums

Place as standalone types within partial wrapper class:
```csharp
public partial class DataObjects
{
    public enum DeletePreference
    {
        Immediate,
        MarkAsDeleted,
    }
}
```

Usage: `DataObjects.DeletePreference.Immediate`

---

## Interfaces

Separate interface at top of file:
```csharp
public partial interface IDataAccess
{
    Task<User> GetUser(Guid UserId);
}

public partial class DataAccess
{
    public async Task<User> GetUser(Guid UserId)
    {
        // Implementation
    }
}
```
