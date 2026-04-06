using System;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace LegaCity.Services
{
    public class UpdateService
    {
        private readonly ClientService _clientService;
        private readonly string _downloadUrl = "https://github.com/smartcmd/MinecraftConsoles/releases/download/nightly/LCEWindows64.zip";

        public Action<DownloadStatus>? OnStatusChanged { get; set; }
        public Action<DownloadStatus>? OnExtractionProgress { get; set; }

        public UpdateService(ClientService clientService)
        {
            _clientService = clientService;
        }

        public bool IsUpdateAvailable()
        {
            return !_clientService.IsClientInstalled();
        }

        public async Task<bool> DownloadAndInstallLatestAsync(IProgress<double>? progress = null)
        {
            try
            {
                var clientPath = _clientService.GetClientPath();

                _clientService.CleanupClientFolder();
                Directory.CreateDirectory(clientPath);

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10);

                    var response = await client.GetAsync(_downloadUrl, HttpCompletionOption.ResponseHeadersRead);
                    if (!response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine($"download faifgd {response.StatusCode}");
                        return false;
                    }

                    var totalBytes = response.Content.Headers.ContentLength ?? 0;
                    var canReportProgress = totalBytes != 0 && progress != null;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = File.Create(Path.Combine(clientPath, "LCEWindows64.zip")))
                    {
                        var totalRead = 0L;
                        var buffer = new byte[8192];
                        int read;

                        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, read);
                            totalRead += read;

                            if (canReportProgress)
                            {
                                progress?.Report((double)totalRead / totalBytes);
                            }
                        }
                    }
                }

                var zipPath = Path.Combine(clientPath, "LCEWindows64.zip");

                if (!IsValidZipFile(zipPath))
                {
                    File.Delete(zipPath);
                    System.Diagnostics.Debug.WriteLine("is not zip??");
                    return false;
                }

                OnStatusChanged?.Invoke(DownloadStatus.Extracting);

                await Task.Run(() =>
                {
                    ZipFile.ExtractToDirectory(zipPath, clientPath, true);
                });

                File.Delete(zipPath);

                if (!File.Exists(Path.Combine(clientPath, "Minecraft.Client.exe")))
                {
                    _clientService.CleanupClientFolder();
                    System.Diagnostics.Debug.WriteLine("i cant find exectyrtdfgx");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"error downloading {ex.Message}");
                return false;
            }
        }

        public string GetVersionText()
        {
            return _clientService.IsClientInstalled() 
                ? "Current version is: Latest" 
                : "Not installed";
        }

        public async Task<bool> DownloadAndInstallCustomAsync(string customUrl, IProgress<double>? progress = null)
        {
            try
            {
                var clientPath = _clientService.GetClientPath();

                _clientService.CleanupClientFolder();
                Directory.CreateDirectory(clientPath);

                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(10);

                    var response = await client.GetAsync(customUrl, HttpCompletionOption.ResponseHeadersRead);
                    if (!response.IsSuccessStatusCode)
                    {
                        System.Diagnostics.Debug.WriteLine($"donlowd failed {response.StatusCode}");
                        return false;
                    }

                    var totalBytes = response.Content.Headers.ContentLength ?? 0;
                    var canReportProgress = totalBytes != 0 && progress != null;

                    var zipFileName = "client.zip";
                    var zipPath = Path.Combine(clientPath, zipFileName);

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = File.Create(zipPath))
                    {
                        var totalRead = 0L;
                        var buffer = new byte[8192];
                        int read;

                        while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, read);
                            totalRead += read;

                            if (canReportProgress)
                            {
                                progress?.Report((double)totalRead / totalBytes);
                            }
                        }
                    }

                    if (!IsValidZipFile(zipPath))
                    {
                        File.Delete(zipPath);
                        _clientService.CleanupClientFolder();
                        System.Diagnostics.Debug.WriteLine("the file istn a zip");
                        throw new InvalidOperationException("its not a zip");
                    }

                    OnStatusChanged?.Invoke(DownloadStatus.Extracting);

                    await Task.Run(() =>
                    {
                        ZipFile.ExtractToDirectory(zipPath, clientPath, true);
                    });

                    File.Delete(zipPath);

                    if (!File.Exists(Path.Combine(clientPath, "Minecraft.Client.exe")))
                    {
                        _clientService.CleanupClientFolder();
                        System.Diagnostics.Debug.WriteLine("no exectuabeosjroisjs");
                        throw new InvalidOperationException("the doesnt contain executahgkj");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"error downloading {ex.Message}");
                throw;
            }
        }

        private bool IsValidZipFile(string filePath)
        {
            try
            {
                using (var zipFile = ZipFile.OpenRead(filePath))
                {
                    return zipFile.Entries.Count > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
