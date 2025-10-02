using System.Text.RegularExpressions;

namespace FreeManager;

// App-specific Azure DevOps data access facade.
// This file wires controller calls to an internal provider (mock by default) so we can
// swap in a real Azure DevOps SDK provider later with no API surface changes.

public partial interface IDataAccess
{
    Task<DataObjects.AdoListResponse<DataObjects.AdoProject>> GetAdoProjects(DataObjects.AdoProjectsRequest req);
    Task<DataObjects.AdoListResponse<DataObjects.AdoRepo>> GetAdoRepos(DataObjects.AdoReposRequest req);
    Task<DataObjects.AdoListResponse<DataObjects.AdoBranch>> GetAdoBranches(DataObjects.AdoBranchesRequest req);
    Task<DataObjects.AdoListResponse<DataObjects.AdoFileItem>> GetAdoAppFiles(DataObjects.AdoAppFilesRequest req);
    Task<DataObjects.AdoFileContentResponse> GetAdoFileContent(DataObjects.AdoFileContentRequest req);
}

public partial class DataAccess
{
    // ---------- Public facade (called by controllers) ----------

    public async Task<DataObjects.AdoListResponse<DataObjects.AdoProject>> GetAdoProjects(DataObjects.AdoProjectsRequest req)
        => await ResolveProvider(req?.Connection).GetProjects(req!);

    public async Task<DataObjects.AdoListResponse<DataObjects.AdoRepo>> GetAdoRepos(DataObjects.AdoReposRequest req)
        => await ResolveProvider(req?.Connection).GetRepos(req!);

    public async Task<DataObjects.AdoListResponse<DataObjects.AdoBranch>> GetAdoBranches(DataObjects.AdoBranchesRequest req)
        => await ResolveProvider(req?.Connection).GetBranches(req!);

    public async Task<DataObjects.AdoListResponse<DataObjects.AdoFileItem>> GetAdoAppFiles(DataObjects.AdoAppFilesRequest req)
        => await ResolveProvider(req?.Connection).GetAppFiles(req!);

    public async Task<DataObjects.AdoFileContentResponse> GetAdoFileContent(DataObjects.AdoFileContentRequest req)
        => await ResolveProvider(req?.Connection).GetFileContent(req!);

    // ---------- Provider resolution ----------

    private IAdoProvider ResolveProvider(DataObjects.AdoConnection? connection)
    {
        // Seam for future: if connection.HasSecrets, return SdkAdoProvider using Azure DevOps .NET SDK.
        // For now, always return the mock provider to keep this additive and compile-safe.
        return new MockAdoProvider();
    }

    // ---------- Provider contracts ----------

    private interface IAdoProvider
    {
        Task<DataObjects.AdoListResponse<DataObjects.AdoProject>> GetProjects(DataObjects.AdoProjectsRequest req);
        Task<DataObjects.AdoListResponse<DataObjects.AdoRepo>> GetRepos(DataObjects.AdoReposRequest req);
        Task<DataObjects.AdoListResponse<DataObjects.AdoBranch>> GetBranches(DataObjects.AdoBranchesRequest req);
        Task<DataObjects.AdoListResponse<DataObjects.AdoFileItem>> GetAppFiles(DataObjects.AdoAppFilesRequest req);
        Task<DataObjects.AdoFileContentResponse> GetFileContent(DataObjects.AdoFileContentRequest req);
    }

    // ---------- Mock provider (deterministic, .app filtered) ----------

    private sealed class MockAdoProvider : IAdoProvider
    {
        private static readonly Regex AppFileRegex = new Regex(@"\.app\.[^/\\]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public Task<DataObjects.AdoListResponse<DataObjects.AdoProject>> GetProjects(DataObjects.AdoProjectsRequest req)
        {
            var items = new List<DataObjects.AdoProject>
            {
                new DataObjects.AdoProject { Id = "p-001", Name = "Contoso.Web" },
                new DataObjects.AdoProject { Id = "p-002", Name = "Fabrikam.Platform" }
            };

            return Task.FromResult(Success(items));
        }

        public Task<DataObjects.AdoListResponse<DataObjects.AdoRepo>> GetRepos(DataObjects.AdoReposRequest req)
        {
            var items = new List<DataObjects.AdoRepo>
            {
                new DataObjects.AdoRepo { Id = "r-001", Name = "WebApp", ProjectId = NormalizeProject(req.ProjectIdOrName) },
                new DataObjects.AdoRepo { Id = "r-002", Name = "OpsScripts", ProjectId = NormalizeProject(req.ProjectIdOrName) },
            };

            return Task.FromResult(Success(items));
        }

        public Task<DataObjects.AdoListResponse<DataObjects.AdoBranch>> GetBranches(DataObjects.AdoBranchesRequest req)
        {
            var items = new List<DataObjects.AdoBranch>
            {
                new DataObjects.AdoBranch { Name = "refs/heads/main", ShortName = "main" },
                new DataObjects.AdoBranch { Name = "refs/heads/develop", ShortName = "develop" },
                new DataObjects.AdoBranch { Name = "refs/heads/release", ShortName = "release" },
            };

            return Task.FromResult(Success(items));
        }

        public Task<DataObjects.AdoListResponse<DataObjects.AdoFileItem>> GetAppFiles(DataObjects.AdoAppFilesRequest req)
        {
            // Simulated tree with mixed files; only *.app.* are flagged IsAppFile = true
            var all = new List<DataObjects.AdoFileItem>
            {
                new DataObjects.AdoFileItem { Path = "FreeManager/Components/Modules.App.razor", Size = 1800, IsAppFile = true },
                new DataObjects.AdoFileItem { Path = "FreeManager/Components/App.razor", Size = 16000, IsAppFile = false },
                new DataObjects.AdoFileItem { Path = "FreeManager.Client/Shared/AppComponents/Index.App.razor", Size = 12500, IsAppFile = true },
                new DataObjects.AdoFileItem { Path = "README.md", Size = 9000, IsAppFile = false },
                new DataObjects.AdoFileItem { Path = "FreeManager.DataAccess/DataAccess.App.cs", Size = 8200, IsAppFile = true },
                new DataObjects.AdoFileItem { Path = "FreeManager.DataObjects/DataObjects.App.cs", Size = 300, IsAppFile = true },
                new DataObjects.AdoFileItem { Path = "FreeManager/Controllers/DataController.App.cs", Size = 900, IsAppFile = true },
            };

            // If a PathPrefix was supplied, scope to that "folder"
            if (!string.IsNullOrWhiteSpace(req.PathPrefix)) {
                var prefix = req.PathPrefix!.Replace('\\', '/').TrimStart('/');
                all = all.Where(f => f.Path.Replace('\\', '/').StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Ensure IsAppFile is accurately set by regex
            foreach (var f in all) {
                f.IsAppFile = AppFileRegex.IsMatch(f.Path);
            }

            // Only return app files for this endpoint
            var appOnly = all.Where(x => x.IsAppFile).OrderBy(x => x.Path, StringComparer.OrdinalIgnoreCase).ToList();
            return Task.FromResult(Success(appOnly));
        }

        public Task<DataObjects.AdoFileContentResponse> GetFileContent(DataObjects.AdoFileContentRequest req)
        {
            // Return a deterministic mock; real impl would stream from ADO Git.
            var path = (req.Path ?? "").Replace('\\', '/');

            string content = path.ToLowerInvariant() switch {
                "freemanager/components/modules.app.razor" =>
                    "// Modules.App.razor (mock)\n@* App-specific additions *@\n",

                "freemanager.client/shared/appcomponents/index.app.razor" =>
                    "// Index.App.razor (mock)\n@* Home page additions *@\n",

                "freemanager.dataaccess/dataaccess.app.cs" =>
                    "// DataAccess.App.cs (mock)\nnamespace FreeManager { public partial class DataAccess { } }",

                "freemanager.dataobjects/dataobjects.app.cs" =>
                    "// DataObjects.App.cs (mock)\nnamespace FreeManager { public partial class DataObjects { } }",

                "freemanager/controllers/datacontroller.app.cs" =>
                    "// DataController.App.cs (mock)\nnamespace FreeManager.Server.Controllers { public partial class DataController { } }",

                _ =>
@"// Mock content for " + path + @"
This is example content returned by the mock Azure DevOps provider.
Enable a real provider to stream actual file content from ADO Git."
            };

            var resp = new DataObjects.AdoFileContentResponse {
                Result = true,
                Path = path,
                Content = content,
                Encoding = "utf-8",
                CorrelationId = Guid.NewGuid().ToString("N")
            };
            return Task.FromResult(resp);
        }

        // ---------- helpers ----------

        private static DataObjects.AdoListResponse<T> Success<T>(List<T> items)
            => new DataObjects.AdoListResponse<T> { Result = true, Items = items, CorrelationId = Guid.NewGuid().ToString("N") };

        private static string NormalizeProject(string input)
            => string.IsNullOrWhiteSpace(input) ? "p-unknown" : input.Trim();
    }
}
