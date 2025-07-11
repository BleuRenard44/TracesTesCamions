using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

public class GitHubRelease
{
    public string Tag_name { get; set; }
    public GitHubAsset[] Assets { get; set; }
}

public class GitHubAsset
{
    public string Name { get; set; }
    public string Browser_download_url { get; set; }
}

public static class UpdateChecker
{
    private static readonly string RepoOwner = "BleuRenard44";
    private static readonly string RepoName = "TracesTesCamions";

    public static async Task<GitHubRelease?> CheckForUpdateAsync(string currentVersion)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("User-Agent", "TracesTesCamions"); // Obligatoire pour GitHub API

        // ✅ CORRECTION : bonne URL de l'API GitHub
        var response = await client.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");

        if (!response.IsSuccessStatusCode)
            return null;

        var content = await response.Content.ReadAsStringAsync();

        var release = JsonSerializer.Deserialize<GitHubRelease>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (release == null || string.IsNullOrWhiteSpace(release.Tag_name))
            return null;

        var latest = release.Tag_name.TrimStart('v');

        if (Version.TryParse(latest, out var latestVersion) &&
            Version.TryParse(currentVersion, out var current))
        {
            if (latestVersion > current)
                return release;
        }

        return null;
    }
}
