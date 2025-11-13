namespace GroupThree.FocusTimerApp.ViewModels
{
    public class GeneralSettingsViewModel : ViewModelBase, ISettingsSectionViewModel
    {
        public string SectionName => "General";

        private readonly Services.SettingsService _settingsService;
        public Services.ThemeService ThemeService { get; }

        private bool _startWithWindows;
        public bool StartWithWindows { get => _startWithWindows; set => SetProperty(ref _startWithWindows, value); }

        private bool _runInBackground = true;
        public bool RunInBackground { get => _runInBackground; set => SetProperty(ref _runInBackground, value); }

        public System.Windows.Input.ICommand SaveCommand { get; }
        public System.Windows.Input.ICommand ResetDefaultsCommand { get; }
        public System.Windows.Input.ICommand ToggleThemeCommand { get; }

        public GeneralSettingsViewModel(Services.SettingsService settingsService, Services.ThemeService themeService)
        {
            _settingsService = settingsService;
            ThemeService = themeService;

            var cfg = _settingsService.LoadSettings();
            StartWithWindows = cfg.General?.StartWithWindows ?? false;
            RunInBackground = cfg.General?.RunInBackground ?? true;

            SaveCommand = new Commands.RelayCommand<object>(_ => Save());
            ResetDefaultsCommand = new Commands.RelayCommand<object>(_ => ResetDefaults());
            ToggleThemeCommand = new Commands.RelayCommand<object>(_ => ThemeService.ToggleTheme());
        }

        private void ResetDefaults()
        {
            StartWithWindows = false;
            RunInBackground = true;
        }

        private void Save()
        {
            var cfg = _settingsService.LoadSettings();
            cfg.General ??= new Models.GeneralSettings();
            cfg.General.StartWithWindows = StartWithWindows;
            cfg.General.RunInBackground = RunInBackground;
            _settingsService.SaveSettings(cfg);

            // Apply StartWithWindows immediately
            try
            {
                var sp = GroupThree.FocusTimerApp.App.ServiceProvider;
                var startupSvc = sp?.GetService(typeof(Services.StartupService)) as Services.StartupService;
                startupSvc?.ApplyStartupSetting(StartWithWindows);
            }
            catch { }

            // Show success dialog
            ShowSuccessDialog("Settings Saved", "General settings have been saved successfully!");
        }

        private void ShowSuccessDialog(string title, string message)
        {
            try
            {
                var dialog = new Views.SuccessDialog(title, message)
                {
                    Owner = System.Windows.Application.Current?.MainWindow
                };
                dialog.ShowDialog();
            }
            catch { }
        }
    }
}
