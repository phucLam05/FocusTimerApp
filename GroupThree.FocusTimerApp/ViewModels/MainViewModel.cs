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
        private readonly IOverlayService _overlayService;

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

        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ToggleOverlayCommand { get; }

        public MainViewModel(ITimerService timerService, IWindowService windowService, IOverlayService overlayService)
        {
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _overlayService = overlayService ?? throw new ArgumentNullException(nameof(overlayService));

            StartCommand = new RelayCommand<object>(_ => Start());
            PauseCommand = new RelayCommand<object>(_ => Pause());
            StopCommand = new RelayCommand<object>(_ => Stop());
            OpenSettingsCommand = new RelayCommand<object>(_ => _windowService.ShowSettingsWindow());
            ToggleOverlayCommand = new RelayCommand<object>(_ => _overlayService.ToggleOverlay());

            _timerService.Tick += OnTick;
            _timerService.Finished += OnFinished;
        }

        private void OnTick(object? s, TimerTickEventArgs e)
        {
            if (e.Mode == TimerMode.Tracking)
            {
                TimeText = e.Elapsed.ToString(@"hh\:mm\:ss");
                Progress = 0;
            }
            else
            {
                TimeText = e.Remaining.ToString(@"hh\:mm\:ss");
                Progress = e.Progress;
            }
        }

        private void OnFinished(object? s, EventArgs e)
        {
            // TODO: notify user
        }

        public void Start()
        {
            if (string.Equals(SelectedMode, "Pomodoro", StringComparison.OrdinalIgnoreCase))
            {
                _timerService.StartPomodoroWork();
                return;
            }

            _timerService.StartTracking();
        }

        public void Pause() => _timerService.Pause();
        public void Stop() => _timerService.Stop();

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

        private void _overlay_service_toggle() => _overlayService.ToggleOverlay();
    }
}
