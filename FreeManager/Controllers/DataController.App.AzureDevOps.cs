using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreeManager.Server.Controllers;

// App-specific Azure DevOps endpoints for discovery and file access.
// These are additive and sit alongside existing DataController partials.

public partial class DataController
{
    /// <summary>
    /// List Azure DevOps projects for a given connection (mock by default).
    /// POST body: DataObjects.AdoProjectsRequest
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("~/api/Ado/Projects")]
    public async Task<ActionResult<DataObjects.AdoListResponse<DataObjects.AdoProject>>> AdoProjects(
        [FromBody] DataObjects.AdoProjectsRequest req)
    {
        if (req == null) return BadRequest(new DataObjects.AdoListResponse<DataObjects.AdoProject> { Messages = { "Missing request." } });
        var result = await da.GetAdoProjects(req);
        return Ok(result);
    }

    /// <summary>
    /// List repositories for a given project.
    /// POST body: DataObjects.AdoReposRequest
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("~/api/Ado/Repos")]
    public async Task<ActionResult<DataObjects.AdoListResponse<DataObjects.AdoRepo>>> AdoRepos(
        [FromBody] DataObjects.AdoReposRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.ProjectIdOrName)) {
            return BadRequest(new DataObjects.AdoListResponse<DataObjects.AdoRepo> {
                Messages = { "ProjectIdOrName is required." }
            });
        }

        var result = await da.GetAdoRepos(req);
        return Ok(result);
    }

    /// <summary>
    /// List branches for a given repo.
    /// POST body: DataObjects.AdoBranchesRequest
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("~/api/Ado/Branches")]
    public async Task<ActionResult<DataObjects.AdoListResponse<DataObjects.AdoBranch>>> AdoBranches(
        [FromBody] DataObjects.AdoBranchesRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.ProjectIdOrName) || string.IsNullOrWhiteSpace(req.RepoIdOrName)) {
            return BadRequest(new DataObjects.AdoListResponse<DataObjects.AdoBranch> {
                Messages = { "ProjectIdOrName and RepoIdOrName are required." }
            });
        }

        var result = await da.GetAdoBranches(req);
        return Ok(result);
    }

    /// <summary>
    /// List *.app.* files in a repo/branch (optionally within a path prefix).
    /// POST body: DataObjects.AdoAppFilesRequest
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("~/api/Ado/AppFiles")]
    public async Task<ActionResult<DataObjects.AdoListResponse<DataObjects.AdoFileItem>>> AdoAppFiles(
        [FromBody] DataObjects.AdoAppFilesRequest req)
    {
        if (req == null || string.IsNullOrWhiteSpace(req.ProjectIdOrName) || string.IsNullOrWhiteSpace(req.RepoIdOrName)) {
            return BadRequest(new DataObjects.AdoListResponse<DataObjects.AdoFileItem> {
                Messages = { "ProjectIdOrName and RepoIdOrName are required." }
            });
        }

        var result = await da.GetAdoAppFiles(req);
        return Ok(result);
    }

    /// <summary>
    /// Get file content for a specific repo/branch/path.
    /// POST body: DataObjects.AdoFileContentRequest
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("~/api/Ado/FileContent")]
    public async Task<ActionResult<DataObjects.AdoFileContentResponse>> AdoFileContent(
        [FromBody] DataObjects.AdoFileContentRequest req)
    {
        if (req == null ||
            string.IsNullOrWhiteSpace(req.ProjectIdOrName) ||
            string.IsNullOrWhiteSpace(req.RepoIdOrName) ||
            string.IsNullOrWhiteSpace(req.Path)) {
            return BadRequest(new DataObjects.AdoFileContentResponse {
                Messages = { "ProjectIdOrName, RepoIdOrName, and Path are required." },
                Result = false
            });
        }

        var result = await da.GetAdoFileContent(req);
        return Ok(result);
    }
}
