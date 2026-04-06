using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using System;
using System.Threading.Tasks;
using LegaCity.Models;
using LegaCity.Services;

namespace LegaCity
{
    public partial class MainWindow : Window
    {
        private SettingsService _settingsService;
        private ClientService _clientService;
        private UpdateService _updateService;
        private AudioService _audioService;
        private AppSettings _currentSettings;
        private NotificationService _notificationService;
        private Button? _activeDownloadButton;

        public MainWindow()
        {
            InitializeComponent();
            InitializeServices();
            InitializeNotifications();
            InitializeAudio();
            LoadSettings();
            UpdateVersionUI();
            SelectTab("home");
            HookButtonEvents();
        }

        private void InitializeServices()
        {
            _clientService = new ClientService();
            _settingsService = new SettingsService();
            _updateService = new UpdateService(_clientService);
            _updateService.OnStatusChanged += UpdateService_OnStatusChanged;
        }

        private void InitializeAudio()
        {
            _audioService = new AudioService();
            _audioService.PlayBackgroundMusic("Assets/brokenclocks.mp3", 0.15f);
        }

        private void HookButtonEvents()
        {
            var buttons = new[] { HomeButton, VersionsButton, SettingsButton, PlayButton, UpdateButton, CustomDownloadButton, SaveSettingsButton };
            foreach (var button in buttons)
            {
                if (button != null)
                {
                    button.PointerEntered += OnButtonPointerEnter;
                }
            }
        }

        private void UpdateService_OnStatusChanged(DownloadStatus status)
        {
            if (status == DownloadStatus.Extracting && _activeDownloadButton != null)
            {
                _activeDownloadButton.Content = "Extracting...";
            }
        }

        private void InitializeNotifications()
        {
            if (NotificationContainer != null)
            {
                _notificationService = new NotificationService(NotificationContainer);
            }
        }

        private async void LoadSettings()
        {
            _currentSettings = await _settingsService.LoadSettingsAsync();
            if (_currentSettings != null)
            {
                if (UsernameInput != null)
                {
                    UsernameInput.Text = _currentSettings.Username ?? "";
                }
                if (FullscreenToggle != null)
                {
                    FullscreenToggle.IsChecked = _currentSettings.Fullscreen;
                }
            }
        }

        private void UpdateVersionUI()
        {
            if (UpdateButton == null || VersionText == null) return;

            if (_clientService.IsClientInstalled())
            {
                VersionText.Text = "Installed";
                UpdateButton.Content = "Reinstall";
            }
            else
            {
                VersionText.Text = "Not installed";
                UpdateButton.Content = "Install Latest Client";
            }
        }

        private void SelectTab(string tabName)
        {
            if (HomeTab != null) HomeTab.IsVisible = false;
            if (VersionsTab != null) VersionsTab.IsVisible = false;
            if (SettingsTab != null) SettingsTab.IsVisible = false;

            switch (tabName)
            {
                case "home":
                    if (HomeTab != null) HomeTab.IsVisible = true;
                    break;
                case "versions":
                    if (VersionsTab != null) VersionsTab.IsVisible = true;
                    UpdateVersionUI();
                    break;
                case "settings":
                    if (SettingsTab != null) SettingsTab.IsVisible = true;
                    break;
            }
        }

        private void OnTabButtonClick(object? sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;

            var tag = button.Tag?.ToString() ?? "home";
            SelectTab(tag);
        }

        private async void OnPlayClick(object? sender, RoutedEventArgs e)
        {
            if (!_clientService.IsClientInstalled())
            {
                SelectTab("versions");
                return;
            }

            if (UsernameInput == null || FullscreenToggle == null) return;

            var username = UsernameInput.Text;
            var fullscreen = FullscreenToggle.IsChecked == true;

            var success = await _clientService.LaunchClientAsync(username, fullscreen);
            if (success)
            {
                Close();
            }
            else
            {
                await ShowErrorAsync("Failed to launch the client.");
            }
        }

        private async void OnUpdateClick(object? sender, RoutedEventArgs e)
        {
            if (UpdateButton == null) return;

            _activeDownloadButton = UpdateButton;
            UpdateButton.IsEnabled = false;
            UpdateButton.Content = "Downloading...";

            try
            {
                var progress = new Progress<double>(p =>
                {
                    if (UpdateButton != null)
                    {
                        UpdateButton.Content = $"Downloading... {(p * 100):F0}%";
                    }
                });

                var success = await _updateService.DownloadAndInstallLatestAsync(progress);

                if (success)
                {
                    UpdateVersionUI();
                    if (UpdateButton != null)
                    {
                        UpdateButton.Content = "Installation complete!";
                    }
                    await Task.Delay(2000);
                    UpdateVersionUI();
                }
                else
                {
                    await ShowErrorAsync("Failed to download and install the client.");
                    UpdateVersionUI();
                }
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"error {ex.Message}");
                UpdateVersionUI();
            }
            finally
            {
                if (UpdateButton != null)
                {
                    UpdateButton.IsEnabled = true;
                }
                _activeDownloadButton = null;
            }
        }

        private async void OnSaveSettingsClick(object? sender, RoutedEventArgs e)
        {
            if (UsernameInput == null || FullscreenToggle == null || SaveSettingsButton == null) return;

            var settings = new AppSettings
            {
                Username = UsernameInput.Text,
                Fullscreen = FullscreenToggle.IsChecked == true
            };

            await _settingsService.SaveSettingsAsync(settings);
            _currentSettings = settings;

            SaveSettingsButton.Content = "Saved!";
            await Task.Delay(2000);
            SaveSettingsButton.Content = "Save Settings";
        }

        private async Task ShowErrorAsync(string message)
        {
            if (_notificationService != null)
            {
                _notificationService.ShowError(message);
            }
            System.Diagnostics.Debug.WriteLine($"error {message}");
        }

        private async void OnCustomDownloadClick(object? sender, RoutedEventArgs e)
        {
            if (CustomDownloadUrlInput == null || CustomDownloadButton == null) return;

            var url = CustomDownloadUrlInput.Text?.Trim();
            if (string.IsNullOrEmpty(url))
            {
                await ShowErrorAsync("Please enter a valid link to a client fork.");
                return;
            }

            _activeDownloadButton = CustomDownloadButton;
            CustomDownloadButton.IsEnabled = false;

            try
            {
                var progress = new Progress<double>(p =>
                {
                    if (CustomDownloadButton != null)
                    {
                        CustomDownloadButton.Content = $"Downloading... {(p * 100):F0}%";
                    }
                });

                var success = await _updateService.DownloadAndInstallCustomAsync(url, progress);

                if (success)
                {
                    UpdateVersionUI();
                    if (CustomDownloadButton != null)
                    {
                        CustomDownloadButton.Content = "Installation complete!";
                    }
                    await Task.Delay(2000);
                    CustomDownloadButton.Content = "Download";
                }
                else
                {
                    await ShowErrorAsync("Failed to download and install from custom URL.");
                    CustomDownloadButton.Content = "Download";
                }
            }
            catch (InvalidOperationException ex)
            {
                await ShowErrorAsync($"validation error {ex.Message}");
                CustomDownloadButton.Content = "Download";
                UpdateVersionUI();
            }
            catch (Exception ex)
            {
                await ShowErrorAsync($"error {ex.Message}");
                CustomDownloadButton.Content = "Download";
                UpdateVersionUI();
            }
            finally
            {
                if (CustomDownloadButton != null)
                {
                    CustomDownloadButton.IsEnabled = true;
                }
                _activeDownloadButton = null;
            }
        }

        private void OnButtonPointerEnter(object? sender, PointerEventArgs e)
        {
            _audioService?.PlaySoundEffect("Assets/focus.wav");
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _audioService?.Dispose();
        }
    }
}