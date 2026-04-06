using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace LegaCity.Services
{
    public class ClientService
    {
        private readonly string _clientPath;

        public ClientService()
        {
            _clientPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LegaCity",
                "client"
            );
        }

        public bool IsClientInstalled()
        {
            return Directory.Exists(_clientPath) && 
                   File.Exists(Path.Combine(_clientPath, "Minecraft.Client.exe"));
        }

        public async Task<bool> LaunchClientAsync(string? username = null, bool fullscreen = false)
        {
            if (!IsClientInstalled())
            {
                System.Diagnostics.Debug.WriteLine("we didn install");
                return false;
            }

            try
            {
                var exePath = Path.Combine(_clientPath, "Minecraft.Client.exe");
                var args = "";

                if (!string.IsNullOrWhiteSpace(username))
                {
                    args += $"-username {username}";
                }

                if (fullscreen)
                {
                    args += " -fullscreen";
                }

                var processInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = args.Trim(),
                    UseShellExecute = true
                };

                using (var process = Process.Start(processInfo))
                {
                    await Task.Delay(500);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"i cant launch {ex.Message}");
                return false;
            }
        }

        public string GetClientPath() => _clientPath;

        public void CleanupClientFolder()
        {
            try
            {
                if (Directory.Exists(_clientPath))
                {
                    var di = new DirectoryInfo(_clientPath);
                    foreach (var file in di.GetFiles())
                    {
                        // skip my nuts
                        if (file.Extension.ToLower() != ".zip")
                        {
                            file.Delete();
                        }
                    }
                    foreach (var dir in di.GetDirectories())
                    {
                        dir.Delete(true);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"error cleaning up {ex.Message}");
            }
        }
    }
}
