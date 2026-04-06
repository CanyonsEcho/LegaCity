using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using LegaCity.Models;

namespace LegaCity.Services
{
    public class SettingsService
    {
        private readonly string _settingsPath;
        private AppSettings _settings;

        public SettingsService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LegaCity"
            );
            _settingsPath = Path.Combine(appDataPath, "settings.json");
            _settings = new AppSettings();
        }

        public async Task<AppSettings> LoadSettingsAsync()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    var json = await File.ReadAllTextAsync(_settingsPath);
                    _settings = JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"error loading settings {ex.Message}");
            }

            return _settings;
        }

        public async Task SaveSettingsAsync(AppSettings settings)
        {
            try
            {
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "LegaCity"
                );

                Directory.CreateDirectory(appDataPath);

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_settingsPath, json);
                _settings = settings;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"errir kdkhdskj {ex.Message}");
            }
        }

        public AppSettings GetCurrentSettings() => _settings;
    }
}
