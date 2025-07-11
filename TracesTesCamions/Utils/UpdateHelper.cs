using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO.Compression;

namespace TracesTesCamions.Utils
{
    public static class UpdaterHelper
    {
        public static async Task<bool> DownloadAndReplaceAsync(string downloadUrl, string newFileName)
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "TracesTesCamionsUpdate");
            Directory.CreateDirectory(tempPath);

            string downloadPath = Path.Combine(tempPath, Path.GetFileName(downloadUrl));

            using (HttpClient client = new HttpClient())
            using (var response = await client.GetAsync(downloadUrl))
            using (var fs = new FileStream(downloadPath, FileMode.Create))
            {
                await response.Content.CopyToAsync(fs);
            }

            string currentExe = Process.GetCurrentProcess().MainModule!.FileName!;
            string newExePath;

            if (downloadUrl.EndsWith(".zip"))
            {
                string extractDir = Path.Combine(tempPath, "extracted");
                ZipFile.ExtractToDirectory(downloadPath, extractDir, true);
                newExePath = Path.Combine(extractDir, newFileName);
            }
            else
            {
                newExePath = downloadPath;
            }

            string backupPath = currentExe + ".bak";
            File.Copy(currentExe, backupPath, true);
            File.Copy(newExePath, currentExe, true);

            Process.Start(new ProcessStartInfo
            {
                FileName = currentExe,
                UseShellExecute = true
            });

            Environment.Exit(0); // ferme l'ancienne instance
            return true;
        }


    }
}
