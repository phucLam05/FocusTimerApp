using System;
using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Services;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ITimerService _timerService;
        private readonly IWindowService _windowService;
        private readonly IOverlayService _overlay_service;
        private readonly System.Windows.Forms.NotifyIcon _notifyIcon;

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

        private string _selectedMode = "Basic";
        public string SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (SetProperty(ref _selectedMode, value))
                {
                    // reset timer state when UI mode changes
                    ResetState();
                }
            }
        }

        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ToggleOverlayCommand { get; }

        public MainViewModel(ITimerService timerService, IWindowService windowService, IOverlayService overlayService)
        {
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _window_service_or_default(windowService);
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _overlay_service_or_default(overlayService);
            _overlay_service = overlayService ?? throw new ArgumentNullException(nameof(overlayService));

            // setup tray icon for notifications (use fully-qualified types to avoid ambiguous using)
            _notifyIcon = new System.Windows.Forms.NotifyIcon()
            {
                Icon = System.Drawing.SystemIcons.Information,
                Visible = true,
                Text = "Focus Timer"
            };

            StartCommand = new RelayCommand<object>(_ => Start());
            PauseCommand = new RelayCommand<object>(_ => Pause());
            StopCommand = new RelayCommand<object>(_ => Stop());
            OpenSettingsCommand = new RelayCommand<object>(_ => _windowService.ShowSettingsWindow());
            ToggleOverlayCommand = new RelayCommand<object>(_ => _overlay_service.ToggleOverlay());

            _timerService.Tick += OnTick;
            _timerService.Finished += OnFinished;
            _timerService.NotificationRequested += OnNotificationRequested;

            // initialize display according to current mode
            ResetState();
        }

        private void OnTick(object? s, TimerTickEventArgs e)
        {
            // Show elapsed (count-up) for both Basic and Pomodoro
            TimeText = e.Elapsed.ToString(@"hh\:mm\:ss");
            Progress = e.Progress;
        }

        private void OnFinished(object? s, EventArgs e)
        {
            // keep simple: show a balloon
            try
            {
                _notifyIcon.BalloonTipTitle = "Focus Timer";
                _notifyIcon.BalloonTipText = "Phase finished.";
                _notifyIcon.ShowBalloonTip(2000);
            }
            catch { }
        }

        private void OnNotificationRequested(object? s, string message)
        {
            try
            {
                _notifyIcon.BalloonTipTitle = "Focus Timer";
                _notifyIcon.BalloonTipText = message;
                _notifyIcon.ShowBalloonTip(3000);
            }
            catch { }
        }

        public void Start()
        {
            if (string.Equals(SelectedMode, "Pomodoro", StringComparison.OrdinalIgnoreCase))
            {
                _timerService.StartPomodoro();
                return;
            }

            _timerService.StartBasic();
        }

        public void Pause() => _timerService.Pause();
        public void Stop()
        {
            _timerService.Stop();
            try { _notifyIcon.Visible = false; } catch { }
        }

        public void TogglePause()
        {
            if (_timerService.IsRunning)
            {
                _timerService.Pause();
            }
            else
            {
                _timerService.Resume();
            }
        }

        public void HandleHotkeyAction(string action)
        {
            if (string.Equals(action, "ToggleOverlay", StringComparison.OrdinalIgnoreCase))
            {
                _overlay_service_toggle();
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

        private void _overlay_service_toggle() => _overlay_service.ToggleOverlay();

        private void _window_service_or_default(IWindowService windowService)
        {
            // helper to avoid analyzers complaining; no-op
            _ = windowService;
        }

        private void _overlay_service_or_default(IOverlayService overlayService)
        {
            // helper to avoid analyzers complaining; no-op
            _ = overlayService;
        }

        // Reset timer display/state when UI mode changes
        public void ResetState()
        {
            // stop any running timer
            try
            {
                _timerService.ResetState();
            }
            catch
            {
                // ignore
            }

            // show elapsed starting at zero for both modes (count-up)
            TimeText = TimeSpan.Zero.ToString(@"hh\:mm\:ss");
            Progress = 0;
        }
    }
}
