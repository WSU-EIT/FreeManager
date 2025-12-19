using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CRM;

#region FreeManager Platform - DataAccess Methods
// ============================================================================
// FREEMANAGER PLATFORM EXTENSION
// Business logic for the FreeManager application builder platform.
// All methods follow the FM_ prefix convention for easy identification.
// ============================================================================

/// <summary>
/// FreeManager DataAccess interface - defines all business logic methods.
/// </summary>
public partial interface IDataAccess
{
    // Projects
    Task<List<DataObjects.FMProjectInfo>> FM_GetProjects(DataObjects.User CurrentUser);
    Task<DataObjects.FMProjectInfo?> FM_GetProject(Guid projectId, DataObjects.User CurrentUser);
    Task<DataObjects.FMProjectInfo> FM_CreateProject(DataObjects.FMCreateProjectRequest request, DataObjects.User CurrentUser);
    Task<DataObjects.BooleanResponse> FM_UpdateProject(DataObjects.FMUpdateProjectRequest request, DataObjects.User CurrentUser);
    Task<DataObjects.BooleanResponse> FM_DeleteProject(Guid projectId, DataObjects.User CurrentUser);
    Task<byte[]?> FM_ExportProjectAsZip(Guid projectId, DataObjects.User CurrentUser);

    // Files
    Task<List<DataObjects.FMAppFileInfo>> FM_GetAppFiles(Guid projectId, DataObjects.User CurrentUser);
    Task<DataObjects.FMAppFileContent?> FM_GetAppFile(Guid fileId, DataObjects.User CurrentUser);
    Task<DataObjects.FMSaveFileResponse> FM_SaveAppFile(DataObjects.FMSaveFileRequest request, DataObjects.User CurrentUser);
    Task<DataObjects.FMAppFileInfo?> FM_CreateAppFile(DataObjects.FMCreateFileRequest request, DataObjects.User CurrentUser);
    Task<DataObjects.BooleanResponse> FM_DeleteAppFile(Guid fileId, DataObjects.User CurrentUser);
    Task<List<DataObjects.FMFileVersionInfo>> FM_GetFileVersions(Guid fileId, DataObjects.User CurrentUser);
    Task<DataObjects.FMAppFileContent?> FM_GetFileVersion(Guid versionId, DataObjects.User CurrentUser);

    // Builds
    Task<DataObjects.FMBuildInfo> FM_StartBuild(DataObjects.FMStartBuildRequest request, DataObjects.User CurrentUser);
    Task<List<DataObjects.FMBuildInfo>> FM_GetBuilds(Guid projectId, DataObjects.User CurrentUser);
    Task<DataObjects.FMBuildDetailInfo?> FM_GetBuild(Guid buildId, DataObjects.User CurrentUser);
}

/// <summary>
/// FreeManager DataAccess implementation.
/// </summary>
public partial class DataAccess
{
    // ============================================================
    // PROJECT METHODS
    // ============================================================

    public async Task<List<DataObjects.FMProjectInfo>> FM_GetProjects(DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;

        var projects = await data.FMProjects
            .Where(p => p.TenantId == tenantId && !p.Deleted)
            .OrderByDescending(p => p.UpdatedAt)
            .Select(p => new DataObjects.FMProjectInfo
            {
                Id = p.FMProjectId,
                Name = p.Name,
                DisplayName = p.DisplayName,
                Description = p.Description,
                IncludedModules = string.IsNullOrEmpty(p.IncludedModules)
                    ? new List<string>()
                    : p.IncludedModules.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                FileCount = p.AppFiles.Count(f => !f.Deleted),
                BuildCount = p.Builds.Count()
            })
            .ToListAsync();

        // Load last build for each project
        foreach (var project in projects)
        {
            var lastBuild = await data.FMBuilds
                .Where(b => b.FMProjectId == project.Id)
                .OrderByDescending(b => b.BuildNumber)
                .Select(b => new DataObjects.FMBuildInfo
                {
                    Id = b.FMBuildId,
                    ProjectId = b.FMProjectId,
                    BuildNumber = b.BuildNumber,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    StartedAt = b.StartedAt,
                    CompletedAt = b.CompletedAt,
                    ArtifactSizeBytes = b.ArtifactSizeBytes,
                    ErrorMessage = b.ErrorMessage
                })
                .FirstOrDefaultAsync();

            project.LastBuild = lastBuild;
        }

        return projects;
    }

    public async Task<DataObjects.FMProjectInfo?> FM_GetProject(Guid projectId, DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;

        var project = await data.FMProjects
            .Where(p => p.FMProjectId == projectId && p.TenantId == tenantId && !p.Deleted)
            .Select(p => new DataObjects.FMProjectInfo
            {
                Id = p.FMProjectId,
                Name = p.Name,
                DisplayName = p.DisplayName,
                Description = p.Description,
                IncludedModules = string.IsNullOrEmpty(p.IncludedModules)
                    ? new List<string>()
                    : p.IncludedModules.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                Status = p.Status,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                FileCount = p.AppFiles.Count(f => !f.Deleted),
                BuildCount = p.Builds.Count()
            })
            .FirstOrDefaultAsync();

        if (project != null)
        {
            // Load last build
            project.LastBuild = await data.FMBuilds
                .Where(b => b.FMProjectId == projectId)
                .OrderByDescending(b => b.BuildNumber)
                .Select(b => new DataObjects.FMBuildInfo
                {
                    Id = b.FMBuildId,
                    ProjectId = b.FMProjectId,
                    BuildNumber = b.BuildNumber,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    StartedAt = b.StartedAt,
                    CompletedAt = b.CompletedAt,
                    ArtifactSizeBytes = b.ArtifactSizeBytes,
                    ErrorMessage = b.ErrorMessage
                })
                .FirstOrDefaultAsync();
        }

        return project;
    }

    public async Task<DataObjects.FMProjectInfo> FM_CreateProject(DataObjects.FMCreateProjectRequest request, DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;
        var userId = CurrentUser.UserId;

        // Validate project name (must be valid C# identifier)
        if (!Regex.IsMatch(request.Name, @"^[A-Za-z][A-Za-z0-9]*$"))
        {
            throw new ArgumentException("Project name must start with a letter and contain only letters and numbers.");
        }

        // Check for duplicate name in tenant
        var exists = await data.FMProjects
            .AnyAsync(p => p.TenantId == tenantId
                        && p.Name.ToLower() == request.Name.ToLower()
                        && !p.Deleted);

        if (exists)
        {
            throw new ArgumentException($"A project named '{request.Name}' already exists.");
        }

        // Create project
        var project = new EFModels.EFModels.FMProject
        {
            TenantId = tenantId,
            Name = request.Name,
            DisplayName = string.IsNullOrEmpty(request.DisplayName) ? request.Name : request.DisplayName,
            Description = request.Description ?? string.Empty,
            IncludedModules = string.Join(",", request.IncludedModules ?? new List<string>()),
            Status = "Active",
            CreatedBy = userId
        };

        data.FMProjects.Add(project);
        await data.SaveChangesAsync();

        // Create template files based on selected template
        await FM_CreateDefaultAppFiles(project, request.Template, CurrentUser);

        return await FM_GetProject(project.FMProjectId, CurrentUser)
               ?? throw new Exception("Failed to retrieve created project");
    }

    public async Task<DataObjects.BooleanResponse> FM_UpdateProject(DataObjects.FMUpdateProjectRequest request, DataObjects.User CurrentUser)
    {
        var output = new DataObjects.BooleanResponse();
        var tenantId = CurrentUser.TenantId;

        var project = await data.FMProjects
            .FirstOrDefaultAsync(p => p.FMProjectId == request.Id
                                   && p.TenantId == tenantId
                                   && !p.Deleted);

        if (project == null)
        {
            output.Messages.Add("Project not found");
            return output;
        }

        project.DisplayName = request.DisplayName;
        project.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Status))
        {
            project.Status = request.Status;
        }
        project.UpdatedAt = DateTime.UtcNow;

        await data.SaveChangesAsync();

        output.Result = true;
        return output;
    }

    public async Task<DataObjects.BooleanResponse> FM_DeleteProject(Guid projectId, DataObjects.User CurrentUser)
    {
        var output = new DataObjects.BooleanResponse();
        var tenantId = CurrentUser.TenantId;

        var project = await data.FMProjects
            .FirstOrDefaultAsync(p => p.FMProjectId == projectId
                                   && p.TenantId == tenantId
                                   && !p.Deleted);

        if (project == null)
        {
            output.Messages.Add("Project not found");
            return output;
        }

        // Soft delete
        project.Deleted = true;
        project.DeletedAt = DateTime.UtcNow;
        project.UpdatedAt = DateTime.UtcNow;

        await data.SaveChangesAsync();

        output.Result = true;
        return output;
    }

    /// <summary>
    /// Creates default .App. files for a new project based on selected template.
    /// </summary>
    private async Task FM_CreateDefaultAppFiles(EFModels.EFModels.FMProject project, DataObjects.FMProjectTemplate template, DataObjects.User CurrentUser)
    {
        var userId = CurrentUser.UserId;

        // Get template files based on selected template
        var templateFiles = FM_GetProjectTemplateFiles(template, project.Name);

        foreach (var (fileName, fileType, content) in templateFiles) {
            var file = new EFModels.EFModels.FMAppFile {
                FMProjectId = project.FMProjectId,
                FilePath = fileName,
                FileType = fileType,
                CurrentVersion = 1
            };

            data.FMAppFiles.Add(file);
            await data.SaveChangesAsync();

            var version = new EFModels.EFModels.FMAppFileVersion {
                FMAppFileId = file.FMAppFileId,
                Version = 1,
                Content = content,
                ContentHash = FM_ComputeHash(content),
                CreatedBy = userId,
                Comment = "Initial file created from template"
            };

            data.FMAppFileVersions.Add(version);
        }

        await data.SaveChangesAsync();
    }

    /// <summary>
    /// Gets template files based on project template type.
    /// </summary>
    private static List<(string FileName, string FileType, string Content)> FM_GetProjectTemplateFiles(
        DataObjects.FMProjectTemplate template,
        string projectName)
    {
        List<(string, string, string)> files = new();

        switch (template) {
            case DataObjects.FMProjectTemplate.Empty:
                // No files
                break;

            case DataObjects.FMProjectTemplate.Skeleton:
                files.Add(($"DataObjects.App.{projectName}.cs", DataObjects.FMFileTypes.DataObjects, FM_GetSkeletonDataObjects(projectName)));
                files.Add(($"DataAccess.App.{projectName}.cs", DataObjects.FMFileTypes.DataAccess, FM_GetSkeletonDataAccess(projectName)));
                files.Add(($"DataController.App.{projectName}.cs", DataObjects.FMFileTypes.Controller, FM_GetSkeletonController(projectName)));
                files.Add(($"GlobalSettings.App.{projectName}.cs", DataObjects.FMFileTypes.GlobalSettings, FM_GetSkeletonGlobalSettings(projectName)));
                break;

            case DataObjects.FMProjectTemplate.Starter:
                files.Add(($"DataObjects.App.{projectName}.cs", DataObjects.FMFileTypes.DataObjects, FM_GetStarterDataObjects(projectName)));
                files.Add(($"DataAccess.App.{projectName}.cs", DataObjects.FMFileTypes.DataAccess, FM_GetStarterDataAccess(projectName)));
                files.Add(($"DataController.App.{projectName}.cs", DataObjects.FMFileTypes.Controller, FM_GetStarterController(projectName)));
                files.Add(($"GlobalSettings.App.{projectName}.cs", DataObjects.FMFileTypes.GlobalSettings, FM_GetStarterGlobalSettings(projectName)));
                files.Add(($"{projectName}.App.razor", DataObjects.FMFileTypes.RazorComponent, FM_GetStarterComponent(projectName)));
                files.Add(($"{projectName}Page.App.razor", DataObjects.FMFileTypes.RazorPage, FM_GetStarterPage(projectName)));
                break;

            case DataObjects.FMProjectTemplate.FullCrud:
                files.Add(($"DataObjects.App.{projectName}.cs", DataObjects.FMFileTypes.DataObjects, FM_GetFullCrudDataObjects(projectName)));
                files.Add(($"DataAccess.App.{projectName}.cs", DataObjects.FMFileTypes.DataAccess, FM_GetFullCrudDataAccess(projectName)));
                files.Add(($"DataController.App.{projectName}.cs", DataObjects.FMFileTypes.Controller, FM_GetFullCrudController(projectName)));
                files.Add(($"GlobalSettings.App.{projectName}.cs", DataObjects.FMFileTypes.GlobalSettings, FM_GetStarterGlobalSettings(projectName)));
                files.Add(($"{projectName}.App.razor", DataObjects.FMFileTypes.RazorComponent, FM_GetStarterComponent(projectName)));
                files.Add(($"{projectName}Page.App.razor", DataObjects.FMFileTypes.RazorPage, FM_GetStarterPage(projectName)));
                files.Add(($"{projectName}Item.cs", DataObjects.FMFileTypes.EFModel, FM_GetFullCrudEntity(projectName)));
                files.Add(($"EFDataModel.App.{projectName}.cs", DataObjects.FMFileTypes.EFDataModel, FM_GetFullCrudDbContext(projectName)));
                break;
        }

        return files;
    }

    // ============================================================
    // FILE METHODS
    // ============================================================

    public async Task<List<DataObjects.FMAppFileInfo>> FM_GetAppFiles(Guid projectId, DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;

        // Verify project belongs to tenant
        var projectExists = await data.FMProjects
            .AnyAsync(p => p.FMProjectId == projectId && p.TenantId == tenantId && !p.Deleted);

        if (!projectExists) return new List<DataObjects.FMAppFileInfo>();

        var files = await data.FMAppFiles
            .Where(f => f.FMProjectId == projectId && !f.Deleted)
            .OrderBy(f => f.FileType)
            .ThenBy(f => f.FilePath)
            .Select(f => new DataObjects.FMAppFileInfo
            {
                Id = f.FMAppFileId,
                ProjectId = f.FMProjectId,
                FilePath = f.FilePath,
                FileType = f.FileType,
                CurrentVersion = f.CurrentVersion,
                UpdatedAt = f.UpdatedAt
            })
            .ToListAsync();

        return files;
    }

    public async Task<DataObjects.FMAppFileContent?> FM_GetAppFile(Guid fileId, DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;

        var file = await data.FMAppFiles
            .Include(f => f.Project)
            .Where(f => f.FMAppFileId == fileId
                     && f.Project != null
                     && f.Project.TenantId == tenantId
                     && !f.Deleted)
            .FirstOrDefaultAsync();

        if (file == null) return null;

        // Get latest version content
        var latestVersion = await data.FMAppFileVersions
            .Where(v => v.FMAppFileId == fileId)
            .OrderByDescending(v => v.Version)
            .FirstOrDefaultAsync();

        return new DataObjects.FMAppFileContent
        {
            Id = file.FMAppFileId,
            ProjectId = file.FMProjectId,
            FilePath = file.FilePath,
            FileType = file.FileType,
            Content = latestVersion?.Content ?? string.Empty,
            Version = file.CurrentVersion,
            UpdatedAt = file.UpdatedAt
        };
    }

    public async Task<DataObjects.FMSaveFileResponse> FM_SaveAppFile(DataObjects.FMSaveFileRequest request, DataObjects.User CurrentUser)
    {
        var response = new DataObjects.FMSaveFileResponse();
        var tenantId = CurrentUser.TenantId;
        var userId = CurrentUser.UserId;

        var file = await data.FMAppFiles
            .Include(f => f.Project)
            .FirstOrDefaultAsync(f => f.FMAppFileId == request.FileId
                                   && f.Project != null
                                   && f.Project.TenantId == tenantId
                                   && !f.Deleted);

        if (file == null)
        {
            response.Message = "File not found";
            return response;
        }

        // Optimistic concurrency check
        if (file.CurrentVersion != request.ExpectedVersion)
        {
            response.Message = $"Version conflict. Expected v{request.ExpectedVersion}, but file is at v{file.CurrentVersion}. Please refresh and try again.";
            return response;
        }

        // Check if content actually changed
        var contentHash = FM_ComputeHash(request.Content);
        var lastVersion = await data.FMAppFileVersions
            .Where(v => v.FMAppFileId == file.FMAppFileId)
            .OrderByDescending(v => v.Version)
            .FirstOrDefaultAsync();

        if (lastVersion != null && lastVersion.ContentHash == contentHash)
        {
            response.Success = true;
            response.NewVersion = file.CurrentVersion;
            response.Message = "No changes detected";
            return response;
        }

        // Create new version
        var newVersion = file.CurrentVersion + 1;

        var version = new EFModels.EFModels.FMAppFileVersion
        {
            FMAppFileId = file.FMAppFileId,
            Version = newVersion,
            Content = request.Content,
            ContentHash = contentHash,
            CreatedBy = userId,
            Comment = request.Comment ?? string.Empty
        };

        data.FMAppFileVersions.Add(version);

        // Update file metadata
        file.CurrentVersion = newVersion;
        file.UpdatedAt = DateTime.UtcNow;

        // Update project timestamp
        if (file.Project != null)
        {
            file.Project.UpdatedAt = DateTime.UtcNow;
        }

        await data.SaveChangesAsync();

        response.Success = true;
        response.NewVersion = newVersion;
        response.Message = $"Saved as version {newVersion}";

        return response;
    }

    public async Task<DataObjects.FMAppFileInfo?> FM_CreateAppFile(DataObjects.FMCreateFileRequest request, DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;
        var userId = CurrentUser.UserId;

        // Verify project belongs to tenant
        var project = await data.FMProjects
            .FirstOrDefaultAsync(p => p.FMProjectId == request.ProjectId
                                   && p.TenantId == tenantId
                                   && !p.Deleted);

        if (project == null) return null;

        // Check for duplicate file path
        var exists = await data.FMAppFiles
            .AnyAsync(f => f.FMProjectId == request.ProjectId
                        && f.FilePath.ToLower() == request.FilePath.ToLower()
                        && !f.Deleted);

        if (exists)
        {
            throw new ArgumentException($"A file named '{request.FilePath}' already exists in this project.");
        }

        // Create file
        var file = new EFModels.EFModels.FMAppFile
        {
            FMProjectId = request.ProjectId,
            FilePath = request.FilePath,
            FileType = request.FileType,
            CurrentVersion = 1
        };

        data.FMAppFiles.Add(file);
        await data.SaveChangesAsync();

        // Create initial version
        var version = new EFModels.EFModels.FMAppFileVersion
        {
            FMAppFileId = file.FMAppFileId,
            Version = 1,
            Content = request.Content ?? string.Empty,
            ContentHash = FM_ComputeHash(request.Content ?? string.Empty),
            CreatedBy = userId,
            Comment = "File created"
        };

        data.FMAppFileVersions.Add(version);

        // Update project timestamp
        project.UpdatedAt = DateTime.UtcNow;

        await data.SaveChangesAsync();

        return new DataObjects.FMAppFileInfo
        {
            Id = file.FMAppFileId,
            ProjectId = file.FMProjectId,
            FilePath = file.FilePath,
            FileType = file.FileType,
            CurrentVersion = file.CurrentVersion,
            UpdatedAt = file.UpdatedAt
        };
    }

    public async Task<DataObjects.BooleanResponse> FM_DeleteAppFile(Guid fileId, DataObjects.User CurrentUser)
    {
        var output = new DataObjects.BooleanResponse();
        var tenantId = CurrentUser.TenantId;

        var file = await data.FMAppFiles
            .Include(f => f.Project)
            .FirstOrDefaultAsync(f => f.FMAppFileId == fileId
                                   && f.Project != null
                                   && f.Project.TenantId == tenantId
                                   && !f.Deleted);

        if (file == null)
        {
            output.Messages.Add("File not found");
            return output;
        }

        // Soft delete
        file.Deleted = true;
        file.DeletedAt = DateTime.UtcNow;
        file.UpdatedAt = DateTime.UtcNow;

        // Update project timestamp
        if (file.Project != null)
        {
            file.Project.UpdatedAt = DateTime.UtcNow;
        }

        await data.SaveChangesAsync();

        output.Result = true;
        return output;
    }

    public async Task<List<DataObjects.FMFileVersionInfo>> FM_GetFileVersions(Guid fileId, DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;

        // Verify file belongs to tenant
        var fileExists = await data.FMAppFiles
            .Include(f => f.Project)
            .AnyAsync(f => f.FMAppFileId == fileId
                        && f.Project != null
                        && f.Project.TenantId == tenantId
                        && !f.Deleted);

        if (!fileExists) return new List<DataObjects.FMFileVersionInfo>();

        var versions = await data.FMAppFileVersions
            .Where(v => v.FMAppFileId == fileId)
            .OrderByDescending(v => v.Version)
            .Select(v => new DataObjects.FMFileVersionInfo
            {
                Id = v.FMAppFileVersionId,
                Version = v.Version,
                CreatedAt = v.CreatedAt,
                Comment = v.Comment,
                CreatedByName = string.Empty // TODO: Join with Users table
            })
            .ToListAsync();

        return versions;
    }

    public async Task<DataObjects.FMAppFileContent?> FM_GetFileVersion(Guid versionId, DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;

        var version = await data.FMAppFileVersions
            .Include(v => v.AppFile)
            .ThenInclude(f => f!.Project)
            .Where(v => v.FMAppFileVersionId == versionId
                     && v.AppFile != null
                     && v.AppFile.Project != null
                     && v.AppFile.Project.TenantId == tenantId)
            .FirstOrDefaultAsync();

        if (version?.AppFile == null) return null;

        return new DataObjects.FMAppFileContent
        {
            Id = version.AppFile.FMAppFileId,
            ProjectId = version.AppFile.FMProjectId,
            FilePath = version.AppFile.FilePath,
            FileType = version.AppFile.FileType,
            Content = version.Content,
            Version = version.Version,
            UpdatedAt = version.CreatedAt
        };
    }

    // ============================================================
    // BUILD METHODS
    // ============================================================

    public async Task<DataObjects.FMBuildInfo> FM_StartBuild(DataObjects.FMStartBuildRequest request, DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;
        var userId = CurrentUser.UserId;

        var project = await data.FMProjects
            .Include(p => p.Builds)
            .FirstOrDefaultAsync(p => p.FMProjectId == request.ProjectId
                                   && p.TenantId == tenantId
                                   && !p.Deleted);

        if (project == null)
        {
            throw new ArgumentException("Project not found");
        }

        var buildNumber = (project.Builds.Any() ? project.Builds.Max(b => b.BuildNumber) : 0) + 1;

        var build = new EFModels.EFModels.FMBuild
        {
            FMProjectId = project.FMProjectId,
            BuildNumber = buildNumber,
            Status = "Queued",
            CreatedBy = userId
        };

        data.FMBuilds.Add(build);
        await data.SaveChangesAsync();

        // TODO: Queue actual build job via background service
        // For now, the build remains in Queued status

        return new DataObjects.FMBuildInfo
        {
            Id = build.FMBuildId,
            ProjectId = build.FMProjectId,
            BuildNumber = build.BuildNumber,
            Status = build.Status,
            CreatedAt = build.CreatedAt
        };
    }

    public async Task<List<DataObjects.FMBuildInfo>> FM_GetBuilds(Guid projectId, DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;

        // Verify project belongs to tenant
        var projectExists = await data.FMProjects
            .AnyAsync(p => p.FMProjectId == projectId && p.TenantId == tenantId && !p.Deleted);

        if (!projectExists) return new List<DataObjects.FMBuildInfo>();

        var builds = await data.FMBuilds
            .Where(b => b.FMProjectId == projectId)
            .OrderByDescending(b => b.BuildNumber)
            .Select(b => new DataObjects.FMBuildInfo
            {
                Id = b.FMBuildId,
                ProjectId = b.FMProjectId,
                BuildNumber = b.BuildNumber,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                StartedAt = b.StartedAt,
                CompletedAt = b.CompletedAt,
                ArtifactSizeBytes = b.ArtifactSizeBytes,
                ErrorMessage = b.ErrorMessage
            })
            .ToListAsync();

        return builds;
    }

    public async Task<DataObjects.FMBuildDetailInfo?> FM_GetBuild(Guid buildId, DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;

        var build = await data.FMBuilds
            .Include(b => b.Project)
            .Where(b => b.FMBuildId == buildId
                     && b.Project != null
                     && b.Project.TenantId == tenantId)
            .Select(b => new DataObjects.FMBuildDetailInfo
            {
                Id = b.FMBuildId,
                ProjectId = b.FMProjectId,
                BuildNumber = b.BuildNumber,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                StartedAt = b.StartedAt,
                CompletedAt = b.CompletedAt,
                ArtifactSizeBytes = b.ArtifactSizeBytes,
                ErrorMessage = b.ErrorMessage,
                LogOutput = b.LogOutput
            })
            .FirstOrDefaultAsync();

        return build;
    }

    // ============================================================
    // EXPORT METHODS
    // ============================================================

    /// <summary>
    /// Exports all project files as a ZIP with correct folder structure.
    /// User can extract this on top of a fresh FreeCRM clone.
    /// </summary>
    public async Task<byte[]?> FM_ExportProjectAsZip(Guid projectId, DataObjects.User CurrentUser)
    {
        var tenantId = CurrentUser.TenantId;

        // Get project with files
        var project = await data.FMProjects
            .Include(p => p.AppFiles.Where(f => !f.Deleted))
            .FirstOrDefaultAsync(p => p.FMProjectId == projectId
                                   && p.TenantId == tenantId
                                   && !p.Deleted);

        if (project == null) return null;

        // Get latest version content for each file
        List<(string FilePath, string Content)> files = new();
        
        foreach (var file in project.AppFiles) {
            var latestVersion = await data.FMAppFileVersions
                .Where(v => v.FMAppFileId == file.FMAppFileId)
                .OrderByDescending(v => v.Version)
                .FirstOrDefaultAsync();

            if (latestVersion != null) {
                string path = FM_GetExportPath(file.FileType, file.FilePath, project.Name);
                files.Add((path, latestVersion.Content));
            }
        }

        if (files.Count == 0) return null;

        // Create ZIP in memory
        using var memoryStream = new MemoryStream();
        using (var archive = new System.IO.Compression.ZipArchive(memoryStream, System.IO.Compression.ZipArchiveMode.Create, true)) {
            foreach (var (filePath, content) in files) {
                var entry = archive.CreateEntry(filePath, System.IO.Compression.CompressionLevel.Optimal);
                using var entryStream = entry.Open();
                using var writer = new StreamWriter(entryStream, Encoding.UTF8);
                await writer.WriteAsync(content);
            }

            // Add a README
            var readmeEntry = archive.CreateEntry("README.txt", System.IO.Compression.CompressionLevel.Optimal);
            using var readmeStream = readmeEntry.Open();
            using var readmeWriter = new StreamWriter(readmeStream, Encoding.UTF8);
            await readmeWriter.WriteAsync(FM_GetExportReadme(project.Name, project.DisplayName, files.Count));
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Maps file type to export path in FreeCRM folder structure.
    /// </summary>
    private static string FM_GetExportPath(string fileType, string fileName, string projectName)
    {
        // Ensure .App. naming convention for most file types
        string baseName = fileName;
        if (!baseName.Contains(".App.") && !fileType.Equals("EFModel") && !fileType.Equals("Stylesheet")) {
            int extIndex = baseName.LastIndexOf('.');
            if (extIndex > 0) {
                baseName = baseName.Substring(0, extIndex) + ".App" + baseName.Substring(extIndex);
            }
        }

        return fileType switch {
            DataObjects.FMFileTypes.DataObjects => $"CRM.DataObjects/{baseName}",
            DataObjects.FMFileTypes.DataAccess => $"CRM.DataAccess/{baseName}",
            DataObjects.FMFileTypes.Controller => $"CRM/Controllers/{baseName}",
            DataObjects.FMFileTypes.RazorComponent => $"CRM.Client/Shared/AppComponents/{baseName}",
            DataObjects.FMFileTypes.RazorPage => $"CRM.Client/Pages/{baseName}",
            DataObjects.FMFileTypes.Stylesheet => $"CRM.Client/wwwroot/css/{baseName}",
            DataObjects.FMFileTypes.GlobalSettings => $"CRM.DataObjects/{baseName}",
            DataObjects.FMFileTypes.HelpersApp => $"CRM.Client/{baseName}",
            DataObjects.FMFileTypes.EFModel => $"CRM.EFModels/EFModels/{baseName}",
            DataObjects.FMFileTypes.EFDataModel => $"CRM.EFModels/EFModels/{baseName}",
            DataObjects.FMFileTypes.Utilities => $"CRM.DataAccess/{baseName}",
            _ => $"CRM/{baseName}"
        };
    }

    /// <summary>
    /// Generates README content for the export ZIP.
    /// </summary>
    private static string FM_GetExportReadme(string projectName, string displayName, int fileCount) => $@"
================================================================================
{displayName} ({projectName})
Exported from FreeManager
================================================================================

This ZIP contains {fileCount} .App. extension file(s) for your FreeCRM project.

INSTALLATION:
1. Clone or download the latest FreeCRM from GitHub
2. Extract this ZIP on top of the FreeCRM folder
3. Files will be placed in their correct locations:
   - CRM.DataObjects/     -> DataObjects extensions
   - CRM.DataAccess/      -> DataAccess extensions  
   - CRM/Controllers/     -> API controller extensions
   - CRM.Client/          -> Blazor components and styles
   - CRM.EFModels/        -> Entity Framework models

4. Run: dotnet build
5. Run: dotnet ef database update (if you added entities)
6. Run: dotnet run

NOTES:
- All files use the .App. naming convention to avoid conflicts
- Never modify core FreeCRM files - only edit .App. files
- See FreeCRM documentation for more details

Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
================================================================================
";

    // ============================================================
    // HELPER METHODS
    // ============================================================

    /// <summary>
    /// Computes SHA256 hash of content for change detection.
    /// </summary>
    private static string FM_ComputeHash(string content)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }

    // ============================================================
    // SKELETON TEMPLATES
    // ============================================================

    private static string FM_GetSkeletonDataObjects(string name) => $@"namespace CRM;

#region {name} DataObjects
// ============================================================================
// {name.ToUpper()} PROJECT
// Add your DTOs and models here.
// ============================================================================

public partial class DataObjects
{{
    public static partial class Endpoints
    {{
        public static class {name}
        {{
            // Define your API endpoints here
            // public const string GetItems = ""api/Data/{name}_GetItems"";
        }}
    }}

    // Add your DTOs here
    // public class {name}Item {{ }}
}}

#endregion
";

    private static string FM_GetSkeletonDataAccess(string name) => $@"namespace CRM;

#region {name} DataAccess
// ============================================================================
// {name.ToUpper()} PROJECT
// Add your business logic methods here.
// ============================================================================

public partial interface IDataAccess
{{
    // Define your method signatures here
    // Task<List<DataObjects.{name}Item>> {name}_GetItems(DataObjects.User CurrentUser);
}}

public partial class DataAccess
{{
    // Implement your methods here
}}

#endregion
";

    private static string FM_GetSkeletonController(string name) => $@"using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

#region {name} API Endpoints
// ============================================================================
// {name.ToUpper()} PROJECT
// Add your API endpoints here.
// ============================================================================

public partial class DataController
{{
    // Add your endpoints here
    // [HttpGet]
    // [Authorize]
    // [Route(""~/api/Data/{name}_GetItems"")]
    // public async Task<ActionResult<List<DataObjects.{name}Item>>> {name}_GetItems() {{ }}
}}

#endregion
";

    private static string FM_GetSkeletonGlobalSettings(string name) => $@"namespace CRM;

#region {name} Settings
// ============================================================================
// {name.ToUpper()} PROJECT
// Add your app configuration here.
// ============================================================================

public static partial class GlobalSettings
{{
    public static class {name}
    {{
        public static string AppName {{ get; set; }} = ""{name}"";
        public static string Version {{ get; set; }} = ""1.0.0"";
    }}
}}

#endregion
";

    // ============================================================
    // STARTER TEMPLATES (Working example with Settings storage)
    // ============================================================

    private static string FM_GetStarterDataObjects(string name) => $@"using System.Text.Json.Serialization;

namespace CRM;

#region {name} DataObjects
// ============================================================================
// {name.ToUpper()} PROJECT - STARTER TEMPLATE
// This template provides a working Items list stored in the Settings table.
// No database migration required!
// ============================================================================

public partial class DataObjects
{{
    public static partial class Endpoints
    {{
        public static class {name}
        {{
            public const string GetItems = ""api/Data/{name}_GetItems"";
            public const string SaveItem = ""api/Data/{name}_SaveItem"";
            public const string DeleteItem = ""api/Data/{name}_DeleteItem"";
        }}
    }}

    /// <summary>
    /// {name} item - stored as JSON in Settings table.
    /// </summary>
    public class {name}Item
    {{
        public Guid Id {{ get; set; }} = Guid.NewGuid();
        public string Name {{ get; set; }} = string.Empty;
        public string Description {{ get; set; }} = string.Empty;
        public bool IsComplete {{ get; set; }} = false;
        public DateTime CreatedAt {{ get; set; }} = DateTime.UtcNow;
        public DateTime? CompletedAt {{ get; set; }}
    }}

    /// <summary>
    /// Request to save an item.
    /// </summary>
    public class {name}SaveRequest
    {{
        public Guid? Id {{ get; set; }}
        public string Name {{ get; set; }} = string.Empty;
        public string Description {{ get; set; }} = string.Empty;
        public bool IsComplete {{ get; set; }} = false;
    }}
}}

#endregion
";

    private static string FM_GetStarterDataAccess(string name) => $@"using System.Text.Json;

namespace CRM;

#region {name} DataAccess
// ============================================================================
// {name.ToUpper()} PROJECT - STARTER TEMPLATE
// Business logic using Settings table for JSON storage.
// ============================================================================

public partial interface IDataAccess
{{
    Task<List<DataObjects.{name}Item>> {name}_GetItems(DataObjects.User CurrentUser);
    Task<DataObjects.{name}Item?> {name}_SaveItem(DataObjects.{name}SaveRequest request, DataObjects.User CurrentUser);
    Task<DataObjects.BooleanResponse> {name}_DeleteItem(Guid itemId, DataObjects.User CurrentUser);
}}

public partial class DataAccess
{{
    private const string {name}SettingsKey = ""{name}_Items"";

    public async Task<List<DataObjects.{name}Item>> {name}_GetItems(DataObjects.User CurrentUser)
    {{
        var items = await {name}_LoadItems(CurrentUser.TenantId);
        return items.OrderByDescending(x => x.CreatedAt).ToList();
    }}

    public async Task<DataObjects.{name}Item?> {name}_SaveItem(DataObjects.{name}SaveRequest request, DataObjects.User CurrentUser)
    {{
        List<DataObjects.{name}Item> items = await {name}_LoadItems(CurrentUser.TenantId);
        DataObjects.{name}Item item;

        if (request.Id.HasValue && request.Id != Guid.Empty) {{
            // Update existing
            item = items.FirstOrDefault(x => x.Id == request.Id.Value) ?? new DataObjects.{name}Item();
            item.Name = request.Name;
            item.Description = request.Description;
            
            if (request.IsComplete && !item.IsComplete) {{
                item.CompletedAt = DateTime.UtcNow;
            }} else if (!request.IsComplete) {{
                item.CompletedAt = null;
            }}
            item.IsComplete = request.IsComplete;

            if (!items.Any(x => x.Id == item.Id)) {{
                items.Add(item);
            }}
        }} else {{
            // Create new
            item = new DataObjects.{name}Item {{
                Id = Guid.NewGuid(),
                Name = request.Name,
                Description = request.Description,
                IsComplete = request.IsComplete,
                CreatedAt = DateTime.UtcNow
            }};
            items.Add(item);
        }}

        await {name}_SaveItems(items, CurrentUser.TenantId);
        return item;
    }}

    public async Task<DataObjects.BooleanResponse> {name}_DeleteItem(Guid itemId, DataObjects.User CurrentUser)
    {{
        DataObjects.BooleanResponse output = new();
        List<DataObjects.{name}Item> items = await {name}_LoadItems(CurrentUser.TenantId);
        
        int removed = items.RemoveAll(x => x.Id == itemId);
        if (removed > 0) {{
            await {name}_SaveItems(items, CurrentUser.TenantId);
            output.Result = true;
        }} else {{
            output.Messages.Add(""Item not found"");
        }}

        return output;
    }}

    private async Task<List<DataObjects.{name}Item>> {name}_LoadItems(Guid tenantId)
    {{
        DataObjects.Setting? setting = await GetSetting({name}SettingsKey, tenantId);
        if (setting == null || string.IsNullOrEmpty(setting.Value)) {{
            return new List<DataObjects.{name}Item>();
        }}
        return JsonSerializer.Deserialize<List<DataObjects.{name}Item>>(setting.Value) ?? new();
    }}

    private async Task {name}_SaveItems(List<DataObjects.{name}Item> items, Guid tenantId)
    {{
        string json = JsonSerializer.Serialize(items);
        await SaveSetting({name}SettingsKey, json, tenantId);
    }}
}}

#endregion
";

    private static string FM_GetStarterController(string name) => $@"using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

#region {name} API Endpoints
// ============================================================================
// {name.ToUpper()} PROJECT - STARTER TEMPLATE
// REST API endpoints for {name} items.
// ============================================================================

public partial class DataController
{{
    [HttpGet]
    [Authorize]
    [Route($""~/{{DataObjects.Endpoints.{name}.GetItems}}"")]
    public async Task<ActionResult<List<DataObjects.{name}Item>>> {name}_GetItems()
    {{
        return await da.{name}_GetItems(CurrentUser);
    }}

    [HttpPost]
    [Authorize]
    [Route($""~/{{DataObjects.Endpoints.{name}.SaveItem}}"")]
    public async Task<ActionResult<DataObjects.{name}Item?>> {name}_SaveItem([FromBody] DataObjects.{name}SaveRequest request)
    {{
        return await da.{name}_SaveItem(request, CurrentUser);
    }}

    [HttpDelete]
    [Authorize]
    [Route($""~/{{DataObjects.Endpoints.{name}.DeleteItem}}"")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> {name}_DeleteItem([FromQuery] Guid itemId)
    {{
        return await da.{name}_DeleteItem(itemId, CurrentUser);
    }}
}}

#endregion
";

    private static string FM_GetStarterGlobalSettings(string name) => $@"namespace CRM;

#region {name} Settings
// ============================================================================
// {name.ToUpper()} PROJECT - STARTER TEMPLATE
// App configuration and constants.
// ============================================================================

public static partial class GlobalSettings
{{
    public static class {name}
    {{
        public static string AppName {{ get; set; }} = ""{name}"";
        public static string Version {{ get; set; }} = ""1.0.0"";
        public static string Description {{ get; set; }} = ""A {name} application built with FreeManager"";
    }}
}}

#endregion
";

    private static string FM_GetStarterComponent(string name) => $@"@implements IDisposable
@inject BlazorDataModel Model
@inject HttpClient Http

@* ============================================================================
   {name} Component - STARTER TEMPLATE
   Card-based list view with add/edit/delete functionality.
   ============================================================================ *@

@if (Model.Loaded && Model.LoggedIn) {{
    <div class=""container-fluid"">
        <h1 class=""page-title"">
            <i class=""fa-solid fa-list-check me-2""></i>
            {name} Items
        </h1>

        <div class=""mb-3"">
            <button class=""btn btn-success"" @onclick=""ShowAddModal"">
                <i class=""fa-solid fa-plus me-1""></i>
                Add Item
            </button>
        </div>

        @if (_loading) {{
            <LoadingMessage />
        }} else {{
            @if (_items.Count == 0) {{
                <div class=""alert alert-info"">
                    <i class=""fa-solid fa-info-circle me-2""></i>
                    No items yet. Click ""Add Item"" to create your first one!
                </div>
            }} else {{
                <div class=""row"">
                    @foreach (var item in _items) {{
                        <div class=""col-md-4 mb-3"">
                            <div class=""card @(item.IsComplete ? ""border-success"" : """")"">
                                <div class=""card-body"">
                                    <h5 class=""card-title"">
                                        @if (item.IsComplete) {{
                                            <i class=""fa-solid fa-check-circle text-success me-1""></i>
                                        }}
                                        @item.Name
                                    </h5>
                                    <p class=""card-text text-muted"">@item.Description</p>
                                    <div class=""d-flex justify-content-between align-items-center"">
                                        <small class=""text-muted"">@item.CreatedAt.ToString(""MMM dd, yyyy"")</small>
                                        <div class=""btn-group btn-group-sm"">
                                            <button class=""btn btn-outline-primary"" @onclick=""() => ShowEditModal(item)"">
                                                <i class=""fa-solid fa-edit""></i>
                                            </button>
                                            <button class=""btn btn-outline-danger"" @onclick=""() => DeleteItem(item)"">
                                                <i class=""fa-solid fa-trash""></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    }}
                </div>
            }}
        }}
    </div>

    @* Add/Edit Modal *@
    @if (_showModal) {{
        <div class=""modal fade show d-block"" style=""background-color: rgba(0,0,0,0.5);"">
            <div class=""modal-dialog"">
                <div class=""modal-content"">
                    <div class=""modal-header"">
                        <h5 class=""modal-title"">@(_editingItem.Id == null ? ""Add"" : ""Edit"") Item</h5>
                        <button type=""button"" class=""btn-close"" @onclick=""CloseModal""></button>
                    </div>
                    <div class=""modal-body"">
                        <div class=""mb-3"">
                            <label class=""form-label"">Name</label>
                            <input type=""text"" class=""form-control"" @bind=""_editingItem.Name"" />
                        </div>
                        <div class=""mb-3"">
                            <label class=""form-label"">Description</label>
                            <textarea class=""form-control"" rows=""3"" @bind=""_editingItem.Description""></textarea>
                        </div>
                        <div class=""form-check"">
                            <input type=""checkbox"" class=""form-check-input"" id=""isComplete"" @bind=""_editingItem.IsComplete"" />
                            <label class=""form-check-label"" for=""isComplete"">Mark as complete</label>
                        </div>
                    </div>
                    <div class=""modal-footer"">
                        <button class=""btn btn-secondary"" @onclick=""CloseModal"">Cancel</button>
                        <button class=""btn btn-primary"" @onclick=""SaveItem"" disabled=""@_saving"">
                            @if (_saving) {{
                                <i class=""fa-solid fa-spinner fa-spin me-1""></i>
                            }}
                            Save
                        </button>
                    </div>
                </div>
            </div>
        </div>
    }}
}}

@code {{
    private bool _loading = true;
    private bool _loadedData = false;
    private bool _saving = false;
    private bool _showModal = false;
    private List<DataObjects.{name}Item> _items = new();
    private DataObjects.{name}SaveRequest _editingItem = new();

    public void Dispose()
    {{
        Model.OnChange -= StateHasChanged;
    }}

    protected override void OnInitialized()
    {{
        Model.OnChange += StateHasChanged;
    }}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {{
        if (Model.Loaded && Model.LoggedIn && !_loadedData) {{
            _loadedData = true;
            await LoadData();
        }}
    }}

    private async Task LoadData()
    {{
        try {{
            _items = await Http.GetFromJsonAsync<List<DataObjects.{name}Item>>(
                DataObjects.Endpoints.{name}.GetItems) ?? new();
        }} catch (Exception ex) {{
            await Helpers.ConsoleLog($""Error loading items: {{ex.Message}}"");
        }}
        _loading = false;
        StateHasChanged();
    }}

    private void ShowAddModal()
    {{
        _editingItem = new DataObjects.{name}SaveRequest();
        _showModal = true;
    }}

    private void ShowEditModal(DataObjects.{name}Item item)
    {{
        _editingItem = new DataObjects.{name}SaveRequest {{
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            IsComplete = item.IsComplete
        }};
        _showModal = true;
    }}

    private void CloseModal()
    {{
        _showModal = false;
        _editingItem = new();
    }}

    private async Task SaveItem()
    {{
        if (string.IsNullOrWhiteSpace(_editingItem.Name)) return;

        _saving = true;
        StateHasChanged();

        try {{
            await Helpers.GetOrPost<DataObjects.{name}Item>(
                DataObjects.Endpoints.{name}.SaveItem, _editingItem);
            await LoadData();
            CloseModal();
        }} catch (Exception ex) {{
            await Helpers.ConsoleLog($""Error saving item: {{ex.Message}}"");
        }}

        _saving = false;
        StateHasChanged();
    }}

    private async Task DeleteItem(DataObjects.{name}Item item)
    {{
        try {{
            await Http.DeleteAsync($""{{DataObjects.Endpoints.{name}.DeleteItem}}?itemId={{item.Id}}"");
            await LoadData();
        }} catch (Exception ex) {{
            await Helpers.ConsoleLog($""Error deleting item: {{ex.Message}}"");
        }}
    }}
}}
";

    private static string FM_GetStarterPage(string name) => $@"@page ""/{name}""
@page ""/{{TenantCode}}/{name}""
@inject BlazorDataModel Model
@implements IDisposable

@* ============================================================================
   {name} Page - STARTER TEMPLATE
   Routed page that hosts the {name} component.
   ============================================================================ *@

@if (Model.Loaded && Model.LoggedIn && Model.View == _pageName) {{
    <{name}_App />
}}

@code {{
    [Parameter] public string? TenantCode {{ get; set; }}

    protected bool _loadedData = false;
    protected string _pageName = ""{name.ToLower()}"";

    public void Dispose()
    {{
        Model.OnChange -= OnDataModelUpdated;
    }}

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {{
        if (firstRender) {{
            Model.TenantCodeFromUrl = TenantCode;
        }}

        if (Model.Loaded) {{
            if (Model.LoggedIn) {{
                if (!_loadedData) {{
                    _loadedData = true;
                    await Helpers.ValidateUrl(TenantCode, true);
                }}
            }} else {{
                Helpers.NavigateToLogin();
            }}
        }}
    }}

    protected void OnDataModelUpdated()
    {{
        if (Model.View == _pageName) {{
            StateHasChanged();
        }}
    }}

    protected override void OnInitialized()
    {{
        Model.View = _pageName;
        Model.OnChange += StateHasChanged;
    }}
}}
";

    // ============================================================
    // FULL CRUD TEMPLATES (EF Entity based)
    // ============================================================

    private static string FM_GetFullCrudDataObjects(string name) => $@"namespace CRM;

#region {name} DataObjects
// ============================================================================
// {name.ToUpper()} PROJECT - FULL CRUD TEMPLATE
// DTOs for database-backed CRUD operations.
// ============================================================================

public partial class DataObjects
{{
    public static partial class Endpoints
    {{
        public static class {name}
        {{
            public const string GetItems = ""api/Data/{name}_GetItems"";
            public const string GetItem = ""api/Data/{name}_GetItem"";
            public const string SaveItem = ""api/Data/{name}_SaveItem"";
            public const string DeleteItem = ""api/Data/{name}_DeleteItem"";
        }}
    }}

    /// <summary>
    /// {name} item DTO for API responses.
    /// </summary>
    public class {name}Item
    {{
        public Guid Id {{ get; set; }}
        public string Name {{ get; set; }} = string.Empty;
        public string Description {{ get; set; }} = string.Empty;
        public bool IsComplete {{ get; set; }}
        public DateTime CreatedAt {{ get; set; }}
        public DateTime UpdatedAt {{ get; set; }}
        public DateTime? CompletedAt {{ get; set; }}
    }}

    /// <summary>
    /// Request to save an item.
    /// </summary>
    public class {name}SaveRequest
    {{
        public Guid? Id {{ get; set; }}
        public string Name {{ get; set; }} = string.Empty;
        public string Description {{ get; set; }} = string.Empty;
        public bool IsComplete {{ get; set; }}
    }}
}}

#endregion
";

    private static string FM_GetFullCrudDataAccess(string name) => $@"using Microsoft.EntityFrameworkCore;

namespace CRM;

#region {name} DataAccess
// ============================================================================
// {name.ToUpper()} PROJECT - FULL CRUD TEMPLATE
// Business logic with EF Core database operations.
// ============================================================================

public partial interface IDataAccess
{{
    Task<List<DataObjects.{name}Item>> {name}_GetItems(DataObjects.User CurrentUser);
    Task<DataObjects.{name}Item?> {name}_GetItem(Guid itemId, DataObjects.User CurrentUser);
    Task<DataObjects.{name}Item?> {name}_SaveItem(DataObjects.{name}SaveRequest request, DataObjects.User CurrentUser);
    Task<DataObjects.BooleanResponse> {name}_DeleteItem(Guid itemId, DataObjects.User CurrentUser);
}}

public partial class DataAccess
{{
    public async Task<List<DataObjects.{name}Item>> {name}_GetItems(DataObjects.User CurrentUser)
    {{
        Guid tenantId = CurrentUser.TenantId;

        List<DataObjects.{name}Item> output = await data.{name}Items
            .Where(x => x.TenantId == tenantId && !x.Deleted)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new DataObjects.{name}Item {{
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                IsComplete = x.IsComplete,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                CompletedAt = x.CompletedAt
            }})
            .ToListAsync();

        return output;
    }}

    public async Task<DataObjects.{name}Item?> {name}_GetItem(Guid itemId, DataObjects.User CurrentUser)
    {{
        Guid tenantId = CurrentUser.TenantId;

        EFModels.EFModels.{name}Item? entity = await data.{name}Items
            .FirstOrDefaultAsync(x => x.Id == itemId && x.TenantId == tenantId && !x.Deleted);

        if (entity == null) return null;

        return new DataObjects.{name}Item {{
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsComplete = entity.IsComplete,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CompletedAt = entity.CompletedAt
        }};
    }}

    public async Task<DataObjects.{name}Item?> {name}_SaveItem(DataObjects.{name}SaveRequest request, DataObjects.User CurrentUser)
    {{
        Guid tenantId = CurrentUser.TenantId;
        EFModels.EFModels.{name}Item entity;

        if (request.Id.HasValue && request.Id != Guid.Empty) {{
            // Update existing
            entity = await data.{name}Items
                .FirstOrDefaultAsync(x => x.Id == request.Id.Value && x.TenantId == tenantId && !x.Deleted)
                ?? new EFModels.EFModels.{name}Item {{ TenantId = tenantId }};
            
            entity.Name = request.Name;
            entity.Description = request.Description;
            entity.UpdatedAt = DateTime.UtcNow;
            
            if (request.IsComplete && !entity.IsComplete) {{
                entity.CompletedAt = DateTime.UtcNow;
            }} else if (!request.IsComplete) {{
                entity.CompletedAt = null;
            }}
            entity.IsComplete = request.IsComplete;

            if (entity.Id == Guid.Empty) {{
                entity.Id = Guid.NewGuid();
                entity.CreatedAt = DateTime.UtcNow;
                entity.CreatedBy = CurrentUser.UserId;
                data.{name}Items.Add(entity);
            }}
        }} else {{
            // Create new
            entity = new EFModels.EFModels.{name}Item {{
                Id = Guid.NewGuid(),
                TenantId = tenantId,
                Name = request.Name,
                Description = request.Description,
                IsComplete = request.IsComplete,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = CurrentUser.UserId
            }};
            data.{name}Items.Add(entity);
        }}

        await data.SaveChangesAsync();

        return new DataObjects.{name}Item {{
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            IsComplete = entity.IsComplete,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            CompletedAt = entity.CompletedAt
        }};
    }}

    public async Task<DataObjects.BooleanResponse> {name}_DeleteItem(Guid itemId, DataObjects.User CurrentUser)
    {{
        DataObjects.BooleanResponse output = new();
        Guid tenantId = CurrentUser.TenantId;

        EFModels.EFModels.{name}Item? entity = await data.{name}Items
            .FirstOrDefaultAsync(x => x.Id == itemId && x.TenantId == tenantId && !x.Deleted);

        if (entity != null) {{
            entity.Deleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            entity.UpdatedAt = DateTime.UtcNow;
            await data.SaveChangesAsync();
            output.Result = true;
        }} else {{
            output.Messages.Add(""Item not found"");
        }}

        return output;
    }}
}}

#endregion
";

    private static string FM_GetFullCrudController(string name) => $@"using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

#region {name} API Endpoints
// ============================================================================
// {name.ToUpper()} PROJECT - FULL CRUD TEMPLATE
// REST API endpoints with full CRUD operations.
// ============================================================================

public partial class DataController
{{
    [HttpGet]
    [Authorize]
    [Route($""~/{{DataObjects.Endpoints.{name}.GetItems}}"")]
    public async Task<ActionResult<List<DataObjects.{name}Item>>> {name}_GetItems()
    {{
        return await da.{name}_GetItems(CurrentUser);
    }}

    [HttpGet]
    [Authorize]
    [Route($""~/{{DataObjects.Endpoints.{name}.GetItem}}"")]
    public async Task<ActionResult<DataObjects.{name}Item?>> {name}_GetItem([FromQuery] Guid itemId)
    {{
        return await da.{name}_GetItem(itemId, CurrentUser);
    }}

    [HttpPost]
    [Authorize]
    [Route($""~/{{DataObjects.Endpoints.{name}.SaveItem}}"")]
    public async Task<ActionResult<DataObjects.{name}Item?>> {name}_SaveItem([FromBody] DataObjects.{name}SaveRequest request)
    {{
        return await da.{name}_SaveItem(request, CurrentUser);
    }}

    [HttpDelete]
    [Authorize]
    [Route($""~/{{DataObjects.Endpoints.{name}.DeleteItem}}"")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> {name}_DeleteItem([FromQuery] Guid itemId)
    {{
        return await da.{name}_DeleteItem(itemId, CurrentUser);
    }}
}}

#endregion
";

    private static string FM_GetFullCrudEntity(string name) => $@"using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CRM.EFModels.EFModels;

#region {name} Entity
// ============================================================================
// {name.ToUpper()} PROJECT - FULL CRUD TEMPLATE
// EF Core entity for database storage.
//
// AFTER EXPORT, run these commands:
// 1. dotnet ef migrations add {name}_Initial --startup-project ../CRM
// 2. dotnet ef database update --startup-project ../CRM
// ============================================================================

[Table(""{name}Items"")]
public class {name}Item
{{
    [Key]
    public Guid Id {{ get; set; }} = Guid.NewGuid();

    public Guid TenantId {{ get; set; }}

    [Required]
    [MaxLength(200)]
    public string Name {{ get; set; }} = string.Empty;

    [MaxLength(1000)]
    public string Description {{ get; set; }} = string.Empty;

    public bool IsComplete {{ get; set; }} = false;

    public DateTime CreatedAt {{ get; set; }} = DateTime.UtcNow;
    public DateTime UpdatedAt {{ get; set; }} = DateTime.UtcNow;
    public DateTime? CompletedAt {{ get; set; }}
    public Guid? CreatedBy {{ get; set; }}

    public bool Deleted {{ get; set; }} = false;
    public DateTime? DeletedAt {{ get; set; }}

    // Navigation
    public virtual Tenant? Tenant {{ get; set; }}
}}

#endregion
";

    private static string FM_GetFullCrudDbContext(string name) => $@"using Microsoft.EntityFrameworkCore;

namespace CRM.EFModels.EFModels;

#region {name} DbContext Extension
// ============================================================================
// {name.ToUpper()} PROJECT - FULL CRUD TEMPLATE
// DbSet registration for EF Core.
// ============================================================================

public partial class EFDataModel
{{
    public virtual DbSet<{name}Item> {name}Items {{ get; set; }} = null!;
}}

#endregion
";
}

#endregion
