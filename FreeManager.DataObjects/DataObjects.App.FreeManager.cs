namespace FreeManager;

// App-specific data objects for the FreeManager repo explorer workflow.
// Placed in a separate partial to keep merges clean.

public partial class DataObjects
{
    // ===== App Repo DTOs (for cloning/listing/reading .app files) =====

    public class AppRepoCloneRequest
    {
        public string? RepoUrl { get; set; }
    }

    public class AppRepoFileItem
    {
        public string RelativePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long Size { get; set; }
    }

    public class AppRepoCloneResponse
    {
        public bool Result { get; set; }
        public string? RepoId { get; set; }
        // LocalPath is included for transparency/debugging; client does not use it.
        public string? LocalPath { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        public List<AppRepoFileItem> Files { get; set; } = new List<AppRepoFileItem>();
    }

    public class AppRepoFileListResponse
    {
        public bool Result { get; set; }
        public string? RepoId { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        public List<AppRepoFileItem> Files { get; set; } = new List<AppRepoFileItem>();
    }

    public class AppRepoFileContentRequest
    {
        public string? RepoId { get; set; }
        public string? RelativePath { get; set; }
    }

    public class AppRepoFileContentResponse
    {
        public bool Result { get; set; }
        public string? RepoId { get; set; }
        public string? RelativePath { get; set; }
        public string? Content { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }
}
