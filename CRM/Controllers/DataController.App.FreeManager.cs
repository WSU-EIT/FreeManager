using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.Server.Controllers;

#region FreeManager Platform - API Endpoints
// ============================================================================
// FREEMANAGER PLATFORM EXTENSION
// REST API endpoints for the FreeManager application builder platform.
// All endpoints require authentication and are tenant-scoped.
// ============================================================================

public partial class DataController
{
    // ============================================================
    // PROJECT ENDPOINTS
    // ============================================================

    /// <summary>
    /// Gets all projects for the current tenant.
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/FM_GetProjects")]
    public async Task<ActionResult<List<DataObjects.FMProjectInfo>>> FM_GetProjects()
    {
        return await da.FM_GetProjects(CurrentUser);
    }

    /// <summary>
    /// Gets a specific project by ID.
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/FM_GetProject")]
    public async Task<ActionResult<DataObjects.FMProjectInfo>> FM_GetProject(Guid projectId)
    {
        var project = await da.FM_GetProject(projectId, CurrentUser);
        if (project == null) return NotFound();
        return project;
    }

    /// <summary>
    /// Creates a new project.
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("~/api/Data/FM_CreateProject")]
    public async Task<ActionResult<DataObjects.FMProjectInfo>> FM_CreateProject(
        [FromBody] DataObjects.FMCreateProjectRequest request)
    {
        try
        {
            return await da.FM_CreateProject(request, CurrentUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Updates project metadata.
    /// </summary>
    [HttpPut]
    [Authorize]
    [Route("~/api/Data/FM_UpdateProject")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> FM_UpdateProject(
        [FromBody] DataObjects.FMUpdateProjectRequest request)
    {
        return await da.FM_UpdateProject(request, CurrentUser);
    }

    /// <summary>
    /// Soft-deletes a project.
    /// </summary>
    [HttpDelete]
    [Authorize]
    [Route("~/api/Data/FM_DeleteProject")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> FM_DeleteProject(Guid projectId)
    {
        return await da.FM_DeleteProject(projectId, CurrentUser);
    }

    // ============================================================
    // FILE ENDPOINTS
    // ============================================================

    /// <summary>
    /// Gets all files for a project.
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/FM_GetAppFiles")]
    public async Task<ActionResult<List<DataObjects.FMAppFileInfo>>> FM_GetAppFiles(Guid projectId)
    {
        return await da.FM_GetAppFiles(projectId, CurrentUser);
    }

    /// <summary>
    /// Gets a file with its content.
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/FM_GetAppFile")]
    public async Task<ActionResult<DataObjects.FMAppFileContent>> FM_GetAppFile(Guid fileId)
    {
        var file = await da.FM_GetAppFile(fileId, CurrentUser);
        if (file == null) return NotFound();
        return file;
    }

    /// <summary>
    /// Saves file content with optimistic concurrency.
    /// </summary>
    [HttpPut]
    [Authorize]
    [Route("~/api/Data/FM_SaveAppFile")]
    public async Task<ActionResult<DataObjects.FMSaveFileResponse>> FM_SaveAppFile(
        [FromBody] DataObjects.FMSaveFileRequest request)
    {
        return await da.FM_SaveAppFile(request, CurrentUser);
    }

    /// <summary>
    /// Creates a new file in a project.
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("~/api/Data/FM_CreateAppFile")]
    public async Task<ActionResult<DataObjects.FMAppFileInfo>> FM_CreateAppFile(
        [FromBody] DataObjects.FMCreateFileRequest request)
    {
        try
        {
            var file = await da.FM_CreateAppFile(request, CurrentUser);
            if (file == null) return NotFound("Project not found");
            return file;
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Soft-deletes a file.
    /// </summary>
    [HttpDelete]
    [Authorize]
    [Route("~/api/Data/FM_DeleteAppFile")]
    public async Task<ActionResult<DataObjects.BooleanResponse>> FM_DeleteAppFile(Guid fileId)
    {
        return await da.FM_DeleteAppFile(fileId, CurrentUser);
    }

    /// <summary>
    /// Gets version history for a file.
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/FM_GetFileVersions")]
    public async Task<ActionResult<List<DataObjects.FMFileVersionInfo>>> FM_GetFileVersions(Guid fileId)
    {
        return await da.FM_GetFileVersions(fileId, CurrentUser);
    }

    /// <summary>
    /// Gets a specific version of a file.
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/FM_GetFileVersion")]
    public async Task<ActionResult<DataObjects.FMAppFileContent>> FM_GetFileVersion(Guid versionId)
    {
        var version = await da.FM_GetFileVersion(versionId, CurrentUser);
        if (version == null) return NotFound();
        return version;
    }

    // ============================================================
    // BUILD ENDPOINTS
    // ============================================================

    /// <summary>
    /// Starts a new build for a project.
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("~/api/Data/FM_StartBuild")]
    public async Task<ActionResult<DataObjects.FMBuildInfo>> FM_StartBuild(
        [FromBody] DataObjects.FMStartBuildRequest request)
    {
        try
        {
            return await da.FM_StartBuild(request, CurrentUser);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Gets build history for a project.
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/FM_GetBuilds")]
    public async Task<ActionResult<List<DataObjects.FMBuildInfo>>> FM_GetBuilds(Guid projectId)
    {
        return await da.FM_GetBuilds(projectId, CurrentUser);
    }

    /// <summary>
    /// Gets build details including log output.
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/FM_GetBuild")]
    public async Task<ActionResult<DataObjects.FMBuildDetailInfo>> FM_GetBuild(Guid buildId)
    {
        var build = await da.FM_GetBuild(buildId, CurrentUser);
        if (build == null) return NotFound();
        return build;
    }

    /// <summary>
    /// Downloads the build artifact ZIP file.
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/FM_DownloadArtifact")]
    public async Task<IActionResult> FM_DownloadArtifact(Guid buildId)
    {
        var build = await da.FM_GetBuild(buildId, CurrentUser);
        if (build == null) return NotFound();

        if (build.Status != "Succeeded" || string.IsNullOrEmpty(build.LogOutput)) {
            return BadRequest("Build artifact not available");
        }

        // TODO: Implement actual artifact download from storage
        return Ok(new { Message = "Artifact download not yet implemented", BuildId = buildId });
    }

    /// <summary>
    /// Exports all project files as a ZIP in the correct folder structure.
    /// User can extract this on top of a fresh FreeCRM clone.
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("~/api/Data/FM_ExportProject")]
    public async Task<IActionResult> FM_ExportProject(Guid projectId)
    {
        byte[]? zipBytes = await da.FM_ExportProjectAsZip(projectId, CurrentUser);
        
        if (zipBytes == null) {
            return NotFound("Project not found or no files to export");
        }

        DataObjects.FMProjectInfo? project = await da.FM_GetProject(projectId, CurrentUser);
        string fileName = $"{project?.Name ?? "project"}_export.zip";

        return File(zipBytes, "application/zip", fileName);
    }
}

#endregion
