using GroupThree.FocusTimerApp.Services;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class GeneralSettingsViewModel : ViewModelBase, ISettingsSectionViewModel
    {
        public string SectionName => "General";

        private readonly Services.SettingsService _settingsService;
        private readonly IThemeService _themeService;

        private bool _startWithWindows;
        public bool StartWithWindows { get => _startWithWindows; set => SetProperty(ref _startWithWindows, value); }

        private bool _runInBackground = true;
        public bool RunInBackground { get => _runInBackground; set => SetProperty(ref _runInBackground, value); }

        public IThemeService ThemeService => _themeService;

        public System.Windows.Input.ICommand SaveCommand { get; }
        public System.Windows.Input.ICommand ToggleThemeCommand { get; }

        public GeneralSettingsViewModel(Services.SettingsService settingsService, IThemeService themeService)
        {
            _settingsService = settingsService;
            _themeService = themeService;
            var cfg = _settingsService.LoadSettings();
            StartWithWindows = cfg.General?.StartWithWindows ?? false;
            RunInBackground = cfg.General?.RunInBackground ?? true;

            SaveCommand = new Commands.RelayCommand<object>(_ => Save());
            ToggleThemeCommand = new Commands.RelayCommand<object>(_ => _themeService.ToggleTheme());
        }

        private void Save()
        {
            var cfg = _settingsService.LoadSettings();
            cfg.General ??= new Models.GeneralSettings();
            cfg.General.StartWithWindows = StartWithWindows;
            cfg.General.RunInBackground = RunInBackground;
            _settingsService.SaveSettings(cfg);
        }
    }
}
