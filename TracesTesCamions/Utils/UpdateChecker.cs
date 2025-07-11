using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TracesTesCamions.Utils
{
    public class GithubRelease
    {
        public string Tag_name { get; set; }
        public string Name { get; set; }
        public string Body { get; set; }
        public List<GithubAsset> Assets { get; set; }
    }

    public class GithubAsset
    {
        public string Name { get; set; }
        public string Browser_download_url { get; set; }
    }

    public static class UpdateChecker
    {
        private const string Repo = "BleuRenard44/TracesTesCamions"; // Ex: OpenAI/ChatApp

        public static async Task<GithubRelease?> CheckForUpdateAsync(string currentVersion)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("TracesTesCamions");

            var json = await client.GetStringAsync($"https://api.github.com/repos/{Repo}/releases/latest");

            var release = JsonSerializer.Deserialize<GithubRelease>(json);
            if (release == null || release.Tag_name == null)
                return null;

            // Compare les versions (simple : v1.0.0 vs 1.0.0)
            string latest = release.Tag_name.TrimStart('v');
            if (latest != currentVersion)
                return release;

            return null;
        }
    }
}
