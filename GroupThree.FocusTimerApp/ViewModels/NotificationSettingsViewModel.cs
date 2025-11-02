namespace GroupThree.FocusTimerApp.ViewModels
{
    public class NotificationSettingsViewModel : ViewModelBase, ISettingsSectionViewModel
    {
        public string SectionName => "Notification";

        private readonly Services.SettingsService _settingsService;

        private bool _enableNotifications = true;
        public bool EnableNotifications { get => _enableNotifications; set => SetProperty(ref _enableNotifications, value); }

        private bool _enableSound = true;
        public bool EnableSound { get => _enableSound; set => SetProperty(ref _enableSound, value); }

        public System.Windows.Input.ICommand SaveCommand { get; }
        public System.Windows.Input.ICommand ResetDefaultsCommand { get; }

        public NotificationSettingsViewModel(Services.SettingsService settingsService)
        {
            _settingsService = settingsService;
            var cfg = _settingsService.LoadSettings();
            EnableNotifications = cfg.Notification?.EnableNotifications ?? true;
            EnableSound = cfg.Notification?.EnableSound ?? true;

            SaveCommand = new Commands.RelayCommand<object>(_ => Save());
            ResetDefaultsCommand = new Commands.RelayCommand<object>(_ => ResetDefaults());
        }

        private void ResetDefaults()
        {
            EnableNotifications = true;
            EnableSound = true;
        }

        private void Save()
        {
            var cfg = _settings_service_or_default();
            cfg.Notification ??= new Models.NotificationSettings();
            cfg.Notification.EnableNotifications = EnableNotifications;
            cfg.Notification.EnableSound = EnableSound;
            _settingsService.SaveSettings(cfg);

            // Show success dialog
            ShowSuccessDialog("Settings Saved", "Notification settings have been saved successfully!");
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

        private Models.ConfigSetting _settings_service_or_default()
        {
            return _settingsService.LoadSettings();
        }
    }
}
