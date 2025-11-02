using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Services;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class TimerSettingsViewModel : ViewModelBase, ISettingsSectionViewModel
    {
        public string SectionName => "Timer";

        private readonly SettingsService _settingsService;

        private int _workDuration = 50;
        private int _shortBreak = 10;
        private int _longBreak = 30;
        private int _longBreakEvery = 4;
        private int _trackingInterval = 15; // minutes

        public int WorkDuration { get => _workDuration; set => SetProperty(ref _workDuration, value); }
        public int ShortBreak { get => _shortBreak; set => SetProperty(ref _shortBreak, value); }
        public int LongBreak { get => _longBreak; set => SetProperty(ref _longBreak, value); }
        public int LongBreakEvery { get => _longBreakEvery; set => SetProperty(ref _longBreakEvery, value); }
        public int TrackingInterval { get => _trackingInterval; set => SetProperty(ref _trackingInterval, value); }

        public ICommand SaveCommand { get; }
        public ICommand ResetDefaultsCommand { get; }

        public TimerSettingsViewModel(Services.SettingsService settingsService)
        {
            _settings_service_or_default(settingsService);

            _settingsService = settingsService;
            var cfg = _settingsService.LoadSettings();
            WorkDuration = cfg.TimerSettings.WorkDuration;
            ShortBreak = cfg.TimerSettings.BreakDuration;
            LongBreak = cfg.TimerSettings.LongBreakDuration;
            LongBreakEvery = cfg.TimerSettings.LongBreakEvery;
            TrackingInterval = cfg.TimerSettings.TrackingInterval;

            SaveCommand = new RelayCommand<object>(_ => Save());
            ResetDefaultsCommand = new RelayCommand<object>(_ => ResetDefaults());
        }

        private void ResetDefaults()
        {
            WorkDuration = 25;
            ShortBreak = 5;
            LongBreak = 15;
            LongBreakEvery = 4;
            TrackingInterval = 15;
        }

        public void Save()
        {
            var cfg = _settingsService.LoadSettings();
            cfg.TimerSettings.WorkDuration = WorkDuration;
            cfg.TimerSettings.BreakDuration = ShortBreak;
            cfg.TimerSettings.LongBreakDuration = LongBreak;
            cfg.TimerSettings.LongBreakEvery = LongBreakEvery;
            cfg.TimerSettings.TrackingInterval = TrackingInterval;
            _settingsService.SaveSettings(cfg);

            // Show success dialog
            ShowSuccessDialog("Settings Saved", "Timer settings have been saved successfully!");
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

        private void _settings_service_or_default(Services.SettingsService settingsService)
        {
            // helper to avoid analyzers complaining; no-op
            _ = settingsService;
        }
    }
}
