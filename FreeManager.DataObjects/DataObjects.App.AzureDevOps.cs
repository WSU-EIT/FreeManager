namespace FreeManager;

// App-specific Azure DevOps DTOs for discovery and file access.
// Designed to map closely to Azure DevOps concepts while remaining SDK-agnostic.

public partial class DataObjects
{
    // ---------- Connection & common ----------

    public class AdoConnection
    {
        /// <summary>Organization URL, e.g. https://dev.azure.com/contoso</summary>
        public string? OrgUrl { get; set; }

        /// <summary>Personal Access Token (if using PAT mode)</summary>
        public string? Pat { get; set; }

        /// <summary>Optional default Project Id or Name</summary>
        public string? Project { get; set; }

        /// <summary>Optional default Repo Id or Name</summary>
        public string? Repo { get; set; }

        /// <summary>Optional default Branch (e.g. refs/heads/main)</summary>
        public string? Branch { get; set; }

        /// <summary>True if credentials/config appear present</summary>
        public bool HasSecrets =>
            !string.IsNullOrWhiteSpace(OrgUrl) && !string.IsNullOrWhiteSpace(Pat);
    }

    public class AdoResponseBase
    {
        public bool Result { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        public string? CorrelationId { get; set; }
    }

    public class AdoListResponse<T> : AdoResponseBase
    {
        public List<T> Items { get; set; } = new List<T>();
    }

    // ---------- Domain entities (simplified) ----------

    public class AdoProject
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class AdoRepo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ProjectId { get; set; }
    }

    public class AdoBranch
    {
        /// <summary>Full ref name (e.g., refs/heads/main)</summary>
        public string Name { get; set; } = string.Empty;
        public string? ShortName { get; set; }
    }

    public class AdoFileItem
    {
        /// <summary>Path relative to the repo root (Unix-style)</summary>
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public bool IsAppFile { get; set; }
    }

    public class AdoFileContentResponse : AdoResponseBase
    {
        public string? Path { get; set; }
        public string? Content { get; set; }
        public string? Encoding { get; set; } = "utf-8";
    }

    // ---------- Requests ----------

    public class AdoProjectsRequest
    {
        public AdoConnection Connection { get; set; } = new AdoConnection();
    }

    public class AdoReposRequest
    {
        public AdoConnection Connection { get; set; } = new AdoConnection();
        public string ProjectIdOrName { get; set; } = string.Empty;
    }

    public class AdoBranchesRequest
    {
        public AdoConnection Connection { get; set; } = new AdoConnection();
        public string ProjectIdOrName { get; set; } = string.Empty;
        public string RepoIdOrName { get; set; } = string.Empty;
    }

    public class AdoAppFilesRequest
    {
        public AdoConnection Connection { get; set; } = new AdoConnection();
        public string ProjectIdOrName { get; set; } = string.Empty;
        public string RepoIdOrName { get; set; } = string.Empty;
        public string BranchName { get; set; } = "refs/heads/main";
        /// <summary>Optional directory to scope listing (relative to repo root)</summary>
        public string? PathPrefix { get; set; }
    }

    public class AdoFileContentRequest
    {
        public AdoConnection Connection { get; set; } = new AdoConnection();
        public string ProjectIdOrName { get; set; } = string.Empty;
        public string RepoIdOrName { get; set; } = string.Empty;
        public string BranchName { get; set; } = "refs/heads/main";
        public string Path { get; set; } = string.Empty; // relative path
    }
}
