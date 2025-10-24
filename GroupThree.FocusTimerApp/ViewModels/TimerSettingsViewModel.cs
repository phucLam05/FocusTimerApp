using System;
using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Services;
using GroupThree.FocusTimerApp.Views;

namespace GroupThree.FocusTimerApp.ViewModels
{
    /// <summary>
    /// ViewModel for Timer Settings page
    /// Manages Pomodoro timer durations and tracking interval settings
    /// </summary>
    public class TimerSettingsViewModel : ViewModelBase, ISettingsSectionViewModel
    {
        public string SectionName => "Timer";

        private readonly SettingsService _settingsService;
        private readonly TimerService? _timerService;

        // Default values for Pomodoro technique
        private int _workDuration = 25;
        private int _shortBreak = 5;
        private int _longBreak = 15;
        private int _longBreakEvery = 4;
        private int _trackingInterval = 15;

        /// <summary>
        /// Work/Focus session duration in minutes (default: 25)
        /// </summary>
        public int WorkDuration 
        { 
            get 
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Get WorkDuration: {_workDuration}");
                return _workDuration;
            }
            set 
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Set WorkDuration: {value}");
                if (SetProperty(ref _workDuration, value))
                {
                    System.Diagnostics.Debug.WriteLine($"[TimerSettings] WorkDuration changed to: {value}");
                }
            }
        }
        
        /// <summary>
        /// Short break duration in minutes (default: 5)
        /// </summary>
        public int ShortBreak 
        { 
            get 
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Get ShortBreak: {_shortBreak}");
                return _shortBreak;
            }
            set 
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Set ShortBreak: {value}");
                if (SetProperty(ref _shortBreak, value))
                {
                    System.Diagnostics.Debug.WriteLine($"[TimerSettings] ShortBreak changed to: {value}");
                }
            }
        }
        
        /// <summary>
        /// Long break duration in minutes (default: 15)
        /// </summary>
        public int LongBreak 
        { 
            get 
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Get LongBreak: {_longBreak}");
                return _longBreak;
            }
            set 
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Set LongBreak: {value}");
                if (SetProperty(ref _longBreak, value))
                {
                    System.Diagnostics.Debug.WriteLine($"[TimerSettings] LongBreak changed to: {value}");
                }
            }
        }
        
        /// <summary>
        /// Number of work cycles before a long break (default: 4)
        /// </summary>
        public int LongBreakEvery 
        { 
            get 
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Get LongBreakEvery: {_longBreakEvery}");
                return _longBreakEvery;
            }
            set 
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Set LongBreakEvery: {value}");
                if (SetProperty(ref _longBreakEvery, value))
                {
                    System.Diagnostics.Debug.WriteLine($"[TimerSettings] LongBreakEvery changed to: {value}");
                }
            }
        }
        
        /// <summary>
        /// Interval for tracking notifications in minutes (default: 15)
        /// </summary>
        public int TrackingInterval 
        { 
            get 
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Get TrackingInterval: {_trackingInterval}");
                return _trackingInterval;
            }
            set 
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Set TrackingInterval: {value}");
                if (SetProperty(ref _trackingInterval, value))
                {
                    System.Diagnostics.Debug.WriteLine($"[TimerSettings] TrackingInterval changed to: {value}");
                }
            }
        }

        public ICommand SaveCommand { get; }

        /// <summary>
        /// Constructor for DI without TimerService (used in Settings window)
        /// </summary>
        public TimerSettingsViewModel(SettingsService settingsService)
            : this(settingsService, null)
        {
        }

        /// <summary>
        /// Main constructor with optional TimerService for syncing
        /// </summary>
        public TimerSettingsViewModel(SettingsService settingsService, TimerService? timerService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _timerService = timerService;
            
            // Initialize command first
            SaveCommand = new RelayCommand<object>(_ => Save());
            
            // Load settings from config
            LoadSettings();
            
            System.Diagnostics.Debug.WriteLine("[TimerSettingsViewModel] Constructor completed");
        }

        /// <summary>
        /// Loads timer settings from configuration file
        /// Uses default values if settings are missing or invalid
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                var cfg = _settingsService.LoadSettings();
                
                // Load with fallback to defaults if values are invalid (trigger PropertyChanged)
                WorkDuration = cfg.TimerSettings.WorkDuration > 0 ? cfg.TimerSettings.WorkDuration : 25;
                ShortBreak = cfg.TimerSettings.BreakDuration > 0 ? cfg.TimerSettings.BreakDuration : 5;
                LongBreak = cfg.TimerSettings.LongBreakDuration > 0 ? cfg.TimerSettings.LongBreakDuration : 15;
                LongBreakEvery = cfg.TimerSettings.LongBreakEvery > 0 ? cfg.TimerSettings.LongBreakEvery : 4;
                TrackingInterval = cfg.TimerSettings.TrackingInterval > 0 ? cfg.TimerSettings.TrackingInterval : 15;
                
                System.Diagnostics.Debug.WriteLine(
                    $"[TimerSettings] Loaded - Work: {WorkDuration}min, Short: {ShortBreak}min, " +
                    $"Long: {LongBreak}min, Every: {LongBreakEvery} cycles, Tracking: {TrackingInterval}min"
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Load error: {ex.Message}");
                // Keep default values if loading fails
            }
        }

        /// <summary>
        /// Saves timer settings to configuration and syncs with TimerService if available
        /// </summary>
        public void Save()
        {
            try
            {
                // Validate input values
                if (!ValidateSettings())
                {
                    CustomMessageBox.Show(
                        "Please enter valid positive numbers for all timer durations.",
                        "Invalid Input",
                        CustomMessageBox.MessageType.Warning
                    );
                    return;
                }

                // Save to configuration file
                var cfg = _settingsService.LoadSettings();
                cfg.TimerSettings.WorkDuration = WorkDuration;
                cfg.TimerSettings.BreakDuration = ShortBreak;
                cfg.TimerSettings.LongBreakDuration = LongBreak;
                cfg.TimerSettings.LongBreakEvery = LongBreakEvery;
                cfg.TimerSettings.TrackingInterval = TrackingInterval;
                _settingsService.SaveSettings(cfg);

                // Sync with TimerService if available
                SyncToTimerService();
                
                System.Diagnostics.Debug.WriteLine("[TimerSettings] Settings saved and synced successfully");
                
                // Show success message
                CustomMessageBox.Show(
                    "Your timer settings have been saved successfully!",
                    "Settings Saved",
                    CustomMessageBox.MessageType.Success
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[TimerSettings] Save error: {ex.Message}");
                CustomMessageBox.Show(
                    $"Failed to save settings: {ex.Message}",
                    "Error",
                    CustomMessageBox.MessageType.Error
                );
            }
        }

        /// <summary>
        /// Validates that all timer duration values are positive
        /// </summary>
        private bool ValidateSettings()
        {
            return WorkDuration > 0 
                && ShortBreak > 0 
                && LongBreak > 0 
                && LongBreakEvery > 0 
                && TrackingInterval > 0;
        }

        /// <summary>
        /// Syncs current settings to TimerService if instance is available
        /// This ensures runtime timer uses the latest settings without app restart
        /// </summary>
        private void SyncToTimerService()
        {
            if (_timerService != null)
            {
                _timerService.WorkDuration = TimeSpan.FromMinutes(WorkDuration);
                _timerService.ShortBreak = TimeSpan.FromMinutes(ShortBreak);
                _timerService.LongBreak = TimeSpan.FromMinutes(LongBreak);
                _timerService.LongBreakEvery = LongBreakEvery;
                
                System.Diagnostics.Debug.WriteLine("[TimerSettings] Settings synced to TimerService");
            }
        }
    }
}
