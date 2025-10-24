using System;
using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Services;

namespace GroupThree.FocusTimerApp.ViewModels
{
    /// <summary>
    /// Main ViewModel for the application's primary window
    /// Manages timer operations, UI state, and coordinates with various services
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        private readonly ITimerService _timerService;
        private readonly IWindowService _windowService;
        private readonly IOverlayService _overlayService;
        private readonly IThemeService _themeService;
        private readonly INotificationService _notificationService;

        private string _timeText = "00:00:00";
        public string TimeText
        {
            get => _timeText;
            set => SetProperty(ref _timeText, value);
        }

        private double _progress;
        public double Progress
        {
            get => _progress;
            set => SetProperty(ref _progress, value);
        }

        private string _selectedMode = "Tracking";
        public string SelectedMode
        {
            get => _selectedMode;
            set => SetProperty(ref _selectedMode, value);
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set => SetProperty(ref _isRunning, value);
        }

        public IThemeService ThemeService => _themeService;

        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ToggleOverlayCommand { get; }
        public ICommand ToggleThemeCommand { get; }

        /// <summary>
        /// Main constructor with dependency injection
        /// </summary>
        public MainViewModel(
            ITimerService timerService, 
            IWindowService windowService, 
            IOverlayService overlayService, 
            IThemeService themeService,
            INotificationService notificationService)
        {
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _overlayService = overlayService ?? throw new ArgumentNullException(nameof(overlayService));
            _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));

            // Initialize commands
            StartCommand = new RelayCommand<object>(_ => Start());
            PauseCommand = new RelayCommand<object>(_ => Pause());
            StopCommand = new RelayCommand<object>(_ => Stop());
            OpenSettingsCommand = new RelayCommand<object>(_ => _windowService.ShowSettingsWindow());
            ToggleOverlayCommand = new RelayCommand<object>(_ => _overlayService.ToggleOverlay());
            ToggleThemeCommand = new RelayCommand<object>(_ => _themeService.ToggleTheme());

            // Subscribe to timer events
            _timerService.Tick += OnTick;
            _timerService.Finished += OnFinished;
        }

        /// <summary>
        /// Handles timer tick event - updates UI with current time and progress
        /// </summary>
        private void OnTick(object? s, TimerTickEventArgs e)
        {
            IsRunning = _timerService.IsRunning;
            
            if (e.Mode == TimerMode.Tracking)
            {
                // Tracking mode: shows elapsed time, no progress bar
                TimeText = e.Elapsed.ToString(@"hh\:mm\:ss");
                Progress = 0;
            }
            else
            {
                // Pomodoro mode: shows remaining time with progress
                TimeText = e.Remaining.ToString(@"hh\:mm\:ss");
                Progress = e.Progress;
            }
        }

        /// <summary>
        /// Handles timer completion event - shows notification and updates UI
        /// </summary>
        private void OnFinished(object? s, EventArgs e)
        {
            IsRunning = false;
            
            // Determine which type of timer just finished
            string timerType = DetermineTimerType();
            
            // Show notification to user
            _notificationService.ShowTimerCompletionNotification(timerType);
            
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Timer finished: {timerType}");
        }

        /// <summary>
        /// Determines the type of timer based on current mode and state
        /// </summary>
        private string DetermineTimerType()
        {
            if (string.Equals(SelectedMode, "Tracking", StringComparison.OrdinalIgnoreCase))
            {
                return "Tracking";
            }
            
            // For Pomodoro, we need to check if it was a work session or break
            // This is a simplified version - in production you might track this state
            return "Work";
        }

        /// <summary>
        /// Starts the timer based on selected mode (Pomodoro or Tracking)
        /// </summary>
        public void Start()
        {
            if (string.Equals(SelectedMode, "Pomodoro", StringComparison.OrdinalIgnoreCase))
            {
                _timerService.StartPomodoroWork();
                System.Diagnostics.Debug.WriteLine("[MainViewModel] Started Pomodoro work session");
            }
            else
            {
                _timerService.StartTracking();
                System.Diagnostics.Debug.WriteLine("[MainViewModel] Started tracking session");
            }
            IsRunning = true;
        }

        /// <summary>
        /// Pauses the currently running timer
        /// </summary>
        public void Pause()
        {
            _timerService.Pause();
            IsRunning = false;
            System.Diagnostics.Debug.WriteLine("[MainViewModel] Timer paused");
        }

        /// <summary>
        /// Stops the timer and resets the display
        /// </summary>
        public void Stop()
        {
            _timerService.Stop();
            IsRunning = false;
            TimeText = "00:00:00";
            Progress = 0;
            System.Diagnostics.Debug.WriteLine("[MainViewModel] Timer stopped and reset");
        }

        /// <summary>
        /// Toggles between pause and resume states
        /// </summary>
        public void TogglePause()
        {
            if (_timerService.IsRunning)
            {
                _timerService.Pause();
                IsRunning = false;
                System.Diagnostics.Debug.WriteLine("[MainViewModel] Timer paused (toggled)");
            }
            else
            {
                _timerService.Resume();
                IsRunning = true;
                System.Diagnostics.Debug.WriteLine("[MainViewModel] Timer resumed (toggled)");
            }
        }

        /// <summary>
        /// Handles hotkey actions triggered by the HotkeyService
        /// </summary>
        /// <param name="action">The action name from hotkey binding</param>
        public void HandleHotkeyAction(string action)
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Hotkey action received: {action}");

            if (string.Equals(action, "ToggleOverlay", StringComparison.OrdinalIgnoreCase))
            {
                _overlayService.ToggleOverlay();
                return;
            }

            if (string.Equals(action, "Start", StringComparison.OrdinalIgnoreCase))
            {
                Start();
                return;
            }

            if (string.Equals(action, "Pause", StringComparison.OrdinalIgnoreCase))
            {
                Pause();
                return;
            }

            if (string.Equals(action, "Stop", StringComparison.OrdinalIgnoreCase))
            {
                Stop();
                return;
            }

            if (string.Equals(action, "TogglePause", StringComparison.OrdinalIgnoreCase))
            {
                TogglePause();
            }
        }
    }
}
