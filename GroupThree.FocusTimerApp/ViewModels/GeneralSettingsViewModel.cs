using System;
using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Services;
using GroupThree.FocusTimerApp.Views;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class GeneralSettingsViewModel : ViewModelBase, ISettingsSectionViewModel
    {
        public string SectionName => "General";

        private readonly SettingsService _settingsService;
        private readonly IThemeService _themeService;

        private bool _startWithWindows;
        public bool StartWithWindows { get => _startWithWindows; set => SetProperty(ref _startWithWindows, value); }

        private bool _runInBackground = true;
        public bool RunInBackground { get => _runInBackground; set => SetProperty(ref _runInBackground, value); }

        public IThemeService ThemeService => _themeService;

        public ICommand SaveCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand ResetDefaultsCommand { get; }

        public GeneralSettingsViewModel(SettingsService settingsService, IThemeService themeService)
        {
            _settingsService = settingsService;
            _themeService = themeService;
            
            LoadSettings();

            SaveCommand = new RelayCommand<object>(_ => Save());
            ToggleThemeCommand = new RelayCommand<object>(_ => _themeService.ToggleTheme());
            ResetDefaultsCommand = new RelayCommand<object>(_ => ResetDefaults());
        }

        private void LoadSettings()
        {
            var cfg = _settingsService.LoadSettings();
            StartWithWindows = cfg.General?.StartWithWindows ?? false;
            RunInBackground = cfg.General?.RunInBackground ?? true;
        }

        private void Save()
        {
            try
            {
                var cfg = _settingsService.LoadSettings();
                cfg.General ??= new Models.GeneralSettings();
                cfg.General.StartWithWindows = StartWithWindows;
                cfg.General.RunInBackground = RunInBackground;
                _settingsService.SaveSettings(cfg);
                
                System.Diagnostics.Debug.WriteLine("[GeneralSettings] Settings saved successfully");
                
                CustomMessageBox.Show(
                    "Your general settings have been saved successfully!",
                    "Settings Saved",
                    CustomMessageBox.MessageType.Success
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GeneralSettings] Save error: {ex.Message}");
                CustomMessageBox.Show(
                    $"Failed to save settings: {ex.Message}",
                    "Error",
                    CustomMessageBox.MessageType.Error
                );
            }
        }

        private void ResetDefaults()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("[GeneralSettings] Resetting general settings to defaults...");
                
                // Load current config
                var cfg = _settingsService.LoadSettings();
                
                // Reset ONLY general settings to defaults
                cfg.General ??= new Models.GeneralSettings();
                cfg.General.StartWithWindows = false;
                cfg.General.RunInBackground = true;
                
                // Save config (keeps other settings intact)
                _settingsService.SaveSettings(cfg);
                
                // Reload general settings
                LoadSettings();
                
                System.Diagnostics.Debug.WriteLine("[GeneralSettings] General settings reset to defaults");
                
                CustomMessageBox.Show(
                    "General settings have been reset to defaults!",
                    "Reset Complete",
                    CustomMessageBox.MessageType.Info
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GeneralSettings] Reset error: {ex.Message}");
                CustomMessageBox.Show(
                    $"Failed to reset general settings: {ex.Message}",
                    "Error",
                    CustomMessageBox.MessageType.Error
                );
            }
        }
    }
}
