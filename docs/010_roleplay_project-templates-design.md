# FreeManager: Team Discussion - Project Templates

> Roleplay transcript designing project templates for the New Project wizard.

**Date:** Current Session  
**Participants:** [Architect], [Blazor], [Data], [API], [Quality], [Sanity]

**Status:** ✅ IMPLEMENTED

---

## Context

**[Architect]:** Team, we need to add project templates to the New Project wizard. Instead of creating an empty project, users should be able to choose from pre-built templates that include starter files.

**[Sanity]:** What templates make sense? Based on our FreeCICD analysis (doc 009), we know the full scope of `.App.` files. Let's design templates for different skill levels.

---

## Template Categories - IMPLEMENTED ✅

**[Blazor]:** We implemented four template levels:

```
┌─────────────────────────────────────────────────────────────────┐
│                   PROJECT TEMPLATES                              │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  🆕 EMPTY PROJECT                                       │    │
│  │  ─────────────────                                      │    │
│  │  No starter files. You create everything from scratch.  │    │
│  │                                                         │    │
│  │  Files: 0                                               │    │
│  │  Best for: Experienced developers                       │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  📋 SKELETON PROJECT                                    │    │
│  │  ──────────────────                                     │    │
│  │  Basic structure with placeholder comments.             │    │
│  │  Shows where to add code but doesn't do anything.       │    │
│  │                                                         │    │
│  │  Files: 4                                               │    │
│  │  • DataObjects.App.{Name}.cs   (empty DTOs)            │    │
│  │  • DataAccess.App.{Name}.cs    (empty methods)         │    │
│  │  • DataController.App.{Name}.cs (empty endpoints)      │    │
│  │  • GlobalSettings.App.{Name}.cs (empty config)         │    │
│  │                                                         │    │
│  │  Best for: Learning the .App. pattern                   │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  ⭐ STARTER PROJECT (Recommended)                       │    │
│  │  ────────────────────────────────                       │    │
│  │  Working example with Items list. Has UI, API, and      │    │
│  │  data layer. Uses Settings table for storage.           │    │
│  │                                                         │    │
│  │  Files: 6                                               │    │
│  │  • DataObjects.App.{Name}.cs   (Item DTO, endpoints)   │    │
│  │  • DataAccess.App.{Name}.cs    (CRUD methods)          │    │
│  │  • DataController.App.{Name}.cs (REST API)             │    │
│  │  • GlobalSettings.App.{Name}.cs (app config)           │    │
│  │  • {Name}.App.razor            (list page component)   │    │
│  │  • {Name}Page.App.razor        (routed page)           │    │
│  │                                                         │    │
│  │  Best for: Getting started quickly                      │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  🔧 FULL CRUD PROJECT                                   │    │
│  │  ────────────────────                                   │    │
│  │  Complete CRUD with EF Entity, edit form, validation.   │    │
│  │  Requires database migration after export.              │    │
│  │                                                         │    │
│  │  Files: 8                                               │    │
│  │  • All Starter files PLUS:                             │    │
│  │  • {Name}Item.cs               (EF Entity)             │    │
│  │  • EFDataModel.App.{Name}.cs   (DbSet)                 │    │
│  │                                                         │    │
│  │  Best for: Real database-backed features                │    │
│  └─────────────────────────────────────────────────────────┘    │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Implementation Summary

### Files Created/Modified:

| File | Purpose |
|------|---------|
| `CRM.DataObjects/DataObjects.App.FreeManager.cs` | Added `FMProjectTemplate` enum, `FMProjectTemplateInfo` class |
| `CRM.Client/FMProjectTemplates.App.cs` | Template definitions for UI display |
| `CRM.DataAccess/DataAccess.App.FreeManager.cs` | Template file generation methods |
| `CRM.Client/Shared/AppComponents/FMNewProject.App.razor` | Updated UI with template selection |

### Key Features:

1. **Template Selection UI** - Radio button cards with icons and descriptions
2. **File Preview Panel** - Shows files that will be created based on selection
3. **Starter Template** - Uses Settings table for JSON storage (no migration needed!)
4. **Full CRUD Template** - Includes EF Entity for database-backed storage

### Starter Template Storage Strategy:

The Starter template cleverly uses the existing Settings table to store items as JSON:

```csharp
// Store items in Settings table
// Key: "{Name}_Items"
// Value: JSON array of items
```

This means:
- No database migration required
- Works immediately after project creation
- Easy to upgrade to EF Entity later if needed

---

## Next Steps

- [ ] Add template preview (show sample code for each template)
- [ ] Add more template types (Dashboard, Report, etc.)
- [ ] Create CRUD scaffold wizard for custom entities
