using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace FreeManager.Server.Controllers;

// App-specific endpoints for the FreeManager repo explorer workflow.
// Placed in a separate partial to keep merges clean.

public partial class DataController
{
    // Track ephemeral clone folders by a short-lived repoId token.
    // NOTE: This is in-memory for simplicity; restarts clear the map.
    private static readonly ConcurrentDictionary<string, string> _appRepoMap = new();

    // Safe exclusions when enumerating files.
    private static readonly string[] _excludeDirs = new[] { ".git", "bin", "obj", "node_modules", "wwwroot" };

    // Recognize *.app.* files (case-insensitive).
    private static readonly Regex _appFileRegex = new Regex(@".*\.app\.[^\\/]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Clone a public repo anonymously (shallow) and return its *.app.* files.
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("~/api/AppRepo/Clone")]
    public async Task<ActionResult<DataObjects.AppRepoCloneResponse>> AppRepoClone([FromBody] DataObjects.AppRepoCloneRequest req)
    {
        var resp = new DataObjects.AppRepoCloneResponse();

        try {
            if (req == null || string.IsNullOrWhiteSpace(req.RepoUrl)) {
                resp.Messages.Add("RepoUrl is required.");
                return BadRequest(resp);
            }

            var url = req.RepoUrl.Trim();
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
                resp.Messages.Add("RepoUrl must start with http:// or https://");
                return BadRequest(resp);
            }

            var repoId = Guid.NewGuid().ToString("N");
            var root = GetAppRepoRoot();
            Directory.CreateDirectory(root);
            var target = Path.Combine(root, repoId);

            // Shallow clone via git CLI.
            var cloneOk = await RunGitClone(url, target, resp.Messages);
            if (!cloneOk) {
                resp.Messages.Add("Failed to clone repository. Ensure the URL is public and the server has 'git' installed.");
                resp.Result = false;
                return Ok(resp);
            }

            _appRepoMap[repoId] = target;

            resp.Files = EnumerateAppFiles(target);
            resp.RepoId = repoId;
            resp.LocalPath = target;
            resp.Result = true;
        } catch (Exception ex) {
            resp.Messages.Add("An unexpected error occurred while cloning.");
            resp.Messages.Add(ex.Message);
        }

        return Ok(resp);
    }

    /// <summary>
    /// Re-enumerate *.app.* files for an existing repoId.
    /// </summary>
    [HttpGet]
    [Authorize]
    [Route("~/api/AppRepo/Files")]
    public ActionResult<DataObjects.AppRepoFileListResponse> AppRepoFiles([FromQuery] string repoId)
    {
        var resp = new DataObjects.AppRepoFileListResponse { RepoId = repoId };
        if (string.IsNullOrWhiteSpace(repoId) || !_appRepoMap.TryGetValue(repoId, out var root)) {
            resp.Messages.Add("Invalid or expired repoId.");
            return Ok(resp);
        }

        try {
            resp.Files = EnumerateAppFiles(root);
            resp.Result = true;
        } catch (Exception ex) {
            resp.Messages.Add("Error enumerating files.");
            resp.Messages.Add(ex.Message);
        }

        return Ok(resp);
    }

    /// <summary>
    /// Return the content of a specific *.app.* file by relative path.
    /// </summary>
    [HttpPost]
    [Authorize]
    [Route("~/api/AppRepo/Content")]
    public ActionResult<DataObjects.AppRepoFileContentResponse> AppRepoContent([FromBody] DataObjects.AppRepoFileContentRequest req)
    {
        var resp = new DataObjects.AppRepoFileContentResponse { RepoId = req?.RepoId, RelativePath = req?.RelativePath };

        if (req == null || string.IsNullOrWhiteSpace(req.RepoId) || string.IsNullOrWhiteSpace(req.RelativePath)) {
            resp.Messages.Add("RepoId and RelativePath are required.");
            return BadRequest(resp);
        }

        if (!_appRepoMap.TryGetValue(req.RepoId, out var root)) {
            resp.Messages.Add("Invalid or expired repoId.");
            return Ok(resp);
        }

        try {
            var safeFullPath = SafeCombine(root, req.RelativePath);
            if (safeFullPath == null) {
                resp.Messages.Add("Invalid path.");
                return Ok(resp);
            }
            if (!_appFileRegex.IsMatch(req.RelativePath)) {
                resp.Messages.Add("Only .app files can be loaded.");
                return Ok(resp);
            }
            if (!System.IO.File.Exists(safeFullPath)) {
                resp.Messages.Add("File not found.");
                return Ok(resp);
            }

            // Read as UTF-8 (with BOM tolerance).
            resp.Content = System.IO.File.ReadAllText(safeFullPath, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: false));
            resp.Result = true;
        } catch (Exception ex) {
            resp.Messages.Add("Error reading file.");
            resp.Messages.Add(ex.Message);
        }

        return Ok(resp);
    }

    // ---------- Helpers (private) ----------

    private static string GetAppRepoRoot()
    {
        var root = Path.Combine(Path.GetTempPath(), "FreeManagerAppRepos");
        return root;
    }

    private static async Task<bool> RunGitClone(string url, string target, List<string> messages)
    {
        try {
            // git clone --depth 1 <url> "<target>"
            var psi = new ProcessStartInfo {
                FileName = "git",
                Arguments = $"clone --depth 1 {EscapeArg(url)} \"{target}\"",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var proc = new Process { StartInfo = psi };
            var output = new StringBuilder();
            var error = new StringBuilder();

            proc.OutputDataReceived += (s, e) => { if (e.Data != null) output.AppendLine(e.Data); };
            proc.ErrorDataReceived += (s, e) => { if (e.Data != null) error.AppendLine(e.Data); };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0) {
                messages.Add($"git clone failed (exit {proc.ExitCode}).");
                if (error.Length > 0) messages.Add(error.ToString());
                return false;
            }
            return Directory.Exists(target);
        } catch (Exception ex) {
            messages.Add("Error invoking git.");
            messages.Add(ex.Message);
            return false;
        }
    }

    private static string EscapeArg(string arg)
    {
        // Minimal argument escaping for URLs.
        if (arg.Contains('\"')) arg = arg.Replace("\"", "\\\"");
        return $"\"{arg}\"";
    }

    private static List<DataObjects.AppRepoFileItem> EnumerateAppFiles(string root)
    {
        var items = new List<DataObjects.AppRepoFileItem>();
        if (!Directory.Exists(root)) return items;

        var stack = new Stack<string>();
        stack.Push(root);

        while (stack.Count > 0) {
            var dir = stack.Pop();

            // Skip excluded directories
            foreach (var d in Directory.EnumerateDirectories(dir)) {
                var name = Path.GetFileName(d.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
                if (_excludeDirs.Any(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase))) continue;
                stack.Push(d);
            }

            foreach (var f in Directory.EnumerateFiles(dir)) {
                var rel = Path.GetRelativePath(root, f)
                    .Replace('\\', '/'); // normalize for client
                if (_appFileRegex.IsMatch(rel)) {
                    var fi = new FileInfo(f);
                    items.Add(new DataObjects.AppRepoFileItem {
                        RelativePath = rel,
                        FileName = Path.GetFileName(f),
                        Size = fi.Exists ? fi.Length : 0
                    });
                }
            }
        }

        // Sort by folder then file
        items = items.OrderBy(i => i.RelativePath, StringComparer.OrdinalIgnoreCase).ToList();
        return items;
    }

    private static string? SafeCombine(string root, string relative)
    {
        try {
            var full = Path.GetFullPath(Path.Combine(root, relative));
            var normRoot = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(normRoot, StringComparison.OrdinalIgnoreCase)) return null;
            return full;
        } catch {
            return null;
        }
    }
}
