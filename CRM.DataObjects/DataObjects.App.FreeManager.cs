namespace CRM;

#region FreeManager Platform - Custom DataObjects
// ============================================================================
// FREEMANAGER PLATFORM EXTENSION
// This file contains DTOs for the FreeManager application builder platform.
// FreeManager allows users to create custom FreeCRM-based applications by
// editing only .App. files, with all content stored in the database.
//
// These are NOT part of the stock FreeCRM framework.
// ============================================================================

/// <summary>
/// FreeManager DTOs - all data transfer objects for the platform
/// </summary>
public partial class DataObjects
{
    // ============================================================
    // API ENDPOINT CONSTANTS
    // ============================================================

    public static partial class Endpoints
    {
        public static class FreeManager
        {
            // Projects
            public const string GetProjects = "api/Data/FM_GetProjects";
            public const string GetProject = "api/Data/FM_GetProject";
            public const string CreateProject = "api/Data/FM_CreateProject";
            public const string UpdateProject = "api/Data/FM_UpdateProject";
            public const string DeleteProject = "api/Data/FM_DeleteProject";
            public const string ExportProject = "api/Data/FM_ExportProject";

            // Files
            public const string GetAppFiles = "api/Data/FM_GetAppFiles";
            public const string GetAppFile = "api/Data/FM_GetAppFile";
            public const string SaveAppFile = "api/Data/FM_SaveAppFile";
            public const string CreateAppFile = "api/Data/FM_CreateAppFile";
            public const string DeleteAppFile = "api/Data/FM_DeleteAppFile";
            public const string GetFileVersions = "api/Data/FM_GetFileVersions";
            public const string GetFileVersion = "api/Data/FM_GetFileVersion";

            // Builds (for future use)
            public const string StartBuild = "api/Data/FM_StartBuild";
            public const string GetBuilds = "api/Data/FM_GetBuilds";
            public const string GetBuild = "api/Data/FM_GetBuild";
            public const string DownloadArtifact = "api/Data/FM_DownloadArtifact";
        }
    }

    // ============================================================
    // PROJECT TEMPLATES
    // ============================================================

    /// <summary>
    /// Available project templates.
    /// </summary>
    public enum FMProjectTemplate
    {
        /// <summary>No starter files - create everything from scratch.</summary>
        Empty = 0,
        
        /// <summary>Basic structure with placeholder comments.</summary>
        Skeleton = 1,
        
        /// <summary>Working example with Items list using Settings storage.</summary>
        Starter = 2,
        
        /// <summary>Complete CRUD with EF Entity (requires migration).</summary>
        FullCrud = 3
    }

    /// <summary>
    /// Project template information for UI display.
    /// </summary>
    public class FMProjectTemplateInfo
    {
        public FMProjectTemplate Template { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int FileCount { get; set; }
        public List<string> IncludedFiles { get; set; } = new();
        public bool IsRecommended { get; set; }
    }

    // ============================================================
    // PROJECT DTOs
    // ============================================================

    /// <summary>
    /// Project information for list views and detail views.
    /// </summary>
    public class FMProjectInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> IncludedModules { get; set; } = new();
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int FileCount { get; set; }
        public int BuildCount { get; set; }
        public FMBuildInfo? LastBuild { get; set; }
    }

    /// <summary>
    /// Request to create a new project.
    /// </summary>
    public class FMCreateProjectRequest
    {
        /// <summary>
        /// Project template to use.
        /// </summary>
        public FMProjectTemplate Template { get; set; } = FMProjectTemplate.Starter;

        /// <summary>
        /// Project name - must be valid C# identifier (letters and numbers, starts with letter).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Human-friendly display name.
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// Project description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// List of optional modules to include (e.g., ["Tags", "Appointments"]).
        /// </summary>
        public List<string> IncludedModules { get; set; } = new();
    }

    /// <summary>
    /// Request to update project metadata.
    /// </summary>
    public class FMUpdateProjectRequest
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    // ============================================================
    // FILE DTOs
    // ============================================================

    /// <summary>
    /// File metadata for list views.
    /// </summary>
    public class FMAppFileInfo
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public int CurrentVersion { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// File content for editing.
    /// </summary>
    public class FMAppFileContent
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Version { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    /// <summary>
    /// Request to save file content. Uses optimistic concurrency.
    /// </summary>
    public class FMSaveFileRequest
    {
        public Guid FileId { get; set; }
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Expected version number - save fails if file was modified by another user.
        /// </summary>
        public int ExpectedVersion { get; set; }

        /// <summary>
        /// Optional commit message describing the change.
        /// </summary>
        public string Comment { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response from saving a file.
    /// </summary>
    public class FMSaveFileResponse
    {
        public bool Success { get; set; }
        public int NewVersion { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to create a new file.
    /// </summary>
    public class FMCreateFileRequest
    {
        public Guid ProjectId { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    /// <summary>
    /// Version history entry.
    /// </summary>
    public class FMFileVersionInfo
    {
        public Guid Id { get; set; }
        public int Version { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
    }

    // ============================================================
    // BUILD DTOs
    // ============================================================

    /// <summary>
    /// Build information for list views.
    /// </summary>
    public class FMBuildInfo
    {
        public Guid Id { get; set; }
        public Guid ProjectId { get; set; }
        public int BuildNumber { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public long? ArtifactSizeBytes { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    /// <summary>
    /// Detailed build information including log output.
    /// </summary>
    public class FMBuildDetailInfo : FMBuildInfo
    {
        public string LogOutput { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request to start a new build.
    /// </summary>
    public class FMStartBuildRequest
    {
        public Guid ProjectId { get; set; }
    }

    // ============================================================
    // AVAILABLE MODULES
    // ============================================================

    /// <summary>
    /// Lists available FreeCRM modules for project configuration.
    /// </summary>
    public static class FMModules
    {
        /// <summary>
        /// Optional modules that can be included or excluded.
        /// </summary>
        public static readonly List<string> Optional = new()
        {
            "Appointments",
            "EmailTemplates",
            "Invoices",
            "Locations",
            "Payments",
            "Services",
            "Tags"
        };

        /// <summary>
        /// Required modules that are always included.
        /// </summary>
        public static readonly List<string> Required = new()
        {
            "Contacts",
            "Departments",
            "UserGroups"
        };
    }

    // ============================================================
    // FILE TYPE CONSTANTS
    // ============================================================

    /// <summary>
    /// File type classifications for .App. files.
    /// </summary>
    public static class FMFileTypes
    {
        public const string DataObjects = "DataObjects";
        public const string DataAccess = "DataAccess";
        public const string Controller = "Controller";
        public const string RazorComponent = "RazorComponent";
        public const string RazorPage = "RazorPage";
        public const string Stylesheet = "Stylesheet";
        public const string GlobalSettings = "GlobalSettings";
        public const string EFModel = "EFModel";
        public const string EFDataModel = "EFDataModel";
        public const string HelpersApp = "HelpersApp";
        public const string Utilities = "Utilities";
        public const string Other = "Other";
    }
}

#endregion
