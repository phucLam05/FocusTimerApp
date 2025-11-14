using System;
using System.ComponentModel;
using System.Windows;

namespace GroupThree.FocusTimerApp.Services
{
    public class ThemeService : INotifyPropertyChanged
    {
        private bool _isDarkMode = true;
        private readonly SettingsService _settingsService;

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (_isDarkMode != value)
                {
                    _isDarkMode = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsDarkMode)));
                    ApplyTheme();
                    SaveThemePreference();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ThemeService(SettingsService settingsService)
        {
            _settingsService = settingsService;
            LoadThemePreference();
        }

        public void ToggleTheme()
        {
            IsDarkMode = !IsDarkMode;
        }

        private void ApplyTheme()
        {
            var app = System.Windows.Application.Current;
            if (app == null) return;

            var newTheme = new ResourceDictionary
            {
                Source = new Uri( _isDarkMode ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml", UriKind.Relative)
            };

            app.Resources.MergedDictionaries.Clear();
            app.Resources.MergedDictionaries.Add(newTheme);
        }

        private void LoadThemePreference()
        {
            var cfg = _settingsService.LoadSettings();
            _isDarkMode = cfg.General?.IsDarkMode ?? true;
        }

        private void SaveThemePreference()
        {
            var cfg = _settingsService.LoadSettings();
            cfg.General ??= new Models.GeneralSettings();
            cfg.General.IsDarkMode = _isDarkMode;
            _settingsService.SaveSettings(cfg);
        }

        public void Initialize()
        {
            LoadThemePreference();
            ApplyTheme();
        }
    }
}
