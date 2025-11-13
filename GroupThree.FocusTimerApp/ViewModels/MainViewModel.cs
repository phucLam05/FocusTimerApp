using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Services;
using System.Collections.ObjectModel;
using GroupThree.FocusTimerApp.Models;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ITimerService _timerService;
        private readonly IWindowService _windowService;
        private readonly IOverlayService _overlay_service;
        private readonly SettingsService _settingsService;
        private readonly NotifyIcon _notifyIcon;
        private readonly IMp3LibraryService? _mp3Library;
        private readonly IPlaylistStorageService? _playlistStorage;
        private readonly IMediaPlaybackService? _mediaPlayback;

        public ThemeService ThemeService { get; }

        private string _timeText = "00:00:00";
        public string TimeText
        {
            get => _timeText;
            set => SetProperty(ref _timeText, value);
        }

        private string _currentPhase = "Ready";
        public string CurrentPhase
        {
            get => _currentPhase;
            set => SetProperty(ref _currentPhase, value);
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

                    // Notify mode-related properties
                    RaisePropertyChanged(nameof(IsPomodoroMode));
                    RaisePropertyChanged(nameof(IsTrackingMode));

                    // persist selected mode to settings
                    try
                    {
                        var cfg = _settingsService.LoadSettings();
                        cfg.TimerSettings.Mode = _selectedMode;
                        _settingsService.SaveSettings(cfg);
                    }
                    catch { }
                }
            }
        }

        private bool _isTimerRunning;
        public bool IsTimerRunning
        {
            get => _isTimerRunning;
            private set => SetProperty(ref _isTimerRunning, value);
        }

        // Alias for better XAML binding clarity
        public bool IsRunning => IsTimerRunning;

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            private set => SetProperty(ref _isPlaying, value);
        }

        public bool IsPomodoroMode => string.Equals(SelectedMode, "Pomodoro", StringComparison.OrdinalIgnoreCase);
        public bool IsTrackingMode => string.Equals(SelectedMode, "Tracking", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(SelectedMode, "Basic", StringComparison.OrdinalIgnoreCase);

        private bool _canPauseResume;
        public bool CanPauseResume
        {
            get => _canPauseResume;
            private set
            {
                if (SetProperty(ref _canPauseResume, value))
                {
                    (RemoveSelectedTrackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                    (PlaySelectedTrackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                    (StopPlaybackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                }
            }
        }

        // Track if timer is paused (started but not running)
        private bool _isPaused;
        public bool IsPaused
        {
            get => _isPaused;
            private set
            {
                if (SetProperty(ref _isPaused, value))
                {
                    RaisePropertyChanged(nameof(PauseButtonText));
                }
            }
        }

        // Dynamic text for pause/resume button
        public string PauseButtonText => IsPaused ? "Resume" : "Pause";

        public ObservableCollection<Mp3Track> Tracks { get; } = new();

        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ToggleOverlayCommand { get; }
        public ICommand ToggleThemeCommand { get; }
        public ICommand AddTrackCommand { get; }
        public ICommand RemoveSelectedTrackCommand { get; }
        public ICommand PlaySelectedTrackCommand { get; }
        public ICommand StopPlaybackCommand { get; }

        private Mp3Track? _selectedTrack;
        public Mp3Track? SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                if (SetProperty(ref _selectedTrack, value))
                {
                    (RemoveSelectedTrackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                    (PlaySelectedTrackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                    (StopPlaybackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                }
            }
        }

        public MainViewModel(ITimerService timerService, IWindowService windowService, IOverlayService overlayService, SettingsService settingsService, ThemeService themeService, IMp3LibraryService? mp3Library = null, IPlaylistStorageService? playlistStorage = null, IMediaPlaybackService? mediaPlayback = null)
        {
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _window_service_or_default(windowService);
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _overlay_service_or_default(overlayService);
            _overlay_service = overlayService ?? throw new ArgumentNullException(nameof(overlayService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            ThemeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
            _mp3Library = mp3Library; // optional DI
            _playlistStorage = playlistStorage; // optional DI
            _mediaPlayback = mediaPlayback; // optional DI

            // setup tray icon for notifications with custom icon
            _notifyIcon = new System.Windows.Forms.NotifyIcon()
            {
                Icon = Properties.Resources.AppIcon,
                Visible = false, // Start hidden, will be shown based on RunInBackground setting
                Text = "Focus Timer"
            };
            ConfigureTrayIcon();

            // Subscribe to settings changes to update tray icon visibility
            _settingsService.SettingsChanged += OnSettingsChanged;
            
            // Set initial tray icon visibility based on current settings
            UpdateTrayIconVisibility();

            StartCommand = new RelayCommand<object>(_ => Start());
            // Make Pause button toggle between Pause and Resume
            PauseCommand = new RelayCommand<object>(_ => TogglePause());
            StopCommand = new RelayCommand<object>(_ => Stop());
            OpenSettingsCommand = new RelayCommand<object>(_ => _windowService.ShowSettingsWindow());
            ToggleOverlayCommand = new RelayCommand<object>(_ => _overlay_service.ToggleOverlay());
            ToggleThemeCommand = new RelayCommand<object>(_ => ThemeService.ToggleTheme());
            AddTrackCommand = new RelayCommand<object>(_ => AddTrack());
            RemoveSelectedTrackCommand = new RelayCommand<object>(_ => RemoveSelectedTrack(), _ => SelectedTrack != null);
            PlaySelectedTrackCommand = new RelayCommand<object>(_ => PlaySelectedTrack(), _ => SelectedTrack != null);
            StopPlaybackCommand = new RelayCommand<object>(_ => StopPlayback(), _ => _mediaPlayback?.IsPlaying == true);

            _timerService.Tick += OnTick;
            _timerService.Finished += OnFinished;
            _timerService.NotificationRequested += OnNotificationRequested;

            // Subscribe to media playback ended event
            if (_mediaPlayback != null)
            {
                _mediaPlayback.PlaybackEnded += OnPlaybackEnded;
            }

            // initialize SelectedMode from settings
            try
            {
                var cfg = _settingsService.LoadSettings();
                var initial = string.IsNullOrWhiteSpace(cfg.TimerSettings.Mode) ? "Basic" : cfg.TimerSettings.Mode;
                _selectedMode = initial; // set backing field to avoid double Reset
            }
            catch { }

            ResetState();
            LoadPlaylistOrDefault();
        }

        private void OnSettingsChanged(ConfigSetting cfg)
        {
            UpdateTrayIconVisibility();
        }

        private void UpdateTrayIconVisibility()
        {
            try
            {
                var cfg = _settingsService.LoadSettings();
                bool runInBackground = cfg?.General?.RunInBackground ?? true;
                _notifyIcon.Visible = runInBackground;
            }
            catch
            {
                _notifyIcon.Visible = true; // default to visible if error
            }
        }

        private void ConfigureTrayIcon()
        {
            var menu = new System.Windows.Forms.ContextMenuStrip();
            var openItem = new System.Windows.Forms.ToolStripMenuItem("Open");

            openItem.Click += (s, e) => ShowMainWindow();

            var settingsItem = new System.Windows.Forms.ToolStripMenuItem("Settings");
            settingsItem.Click += (s, e) => _windowService.ShowSettingsWindow();
            var exitItem = new System.Windows.Forms.ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => ExitApp();

            menu.Items.Add(openItem);
            menu.Items.Add(settingsItem);
            menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
            menu.Items.Add(exitItem);

            _notifyIcon.ContextMenuStrip = menu;

            _notifyIcon.DoubleClick += (s, e) => ShowMainWindow();
        }

        private void ShowMainWindow()
        {
            try
            {
                _overlay_service.HideOverlay();

                var win = System.Windows.Application.Current?.MainWindow;
                if (win != null)
                {
                    win.ShowInTaskbar = true;
                    win.Show();
                    win.WindowState = System.Windows.WindowState.Normal;
                    win.Activate();
                }
            }
            catch { }
        }

        private void ExitApp()
        {
            try
            {
                _notifyIcon.Visible = false;
                _notifyIcon.Dispose();
            }
            catch { }
            
            try
            {
                // Use ForceClose if MainWindow supports it
                var mainWindow = System.Windows.Application.Current?.MainWindow as Views.MainWindow;
                if (mainWindow != null)
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        mainWindow.ForceClose();
                    });
                }
                else
                {
                    System.Windows.Application.Current?.Shutdown();
                }
            }
            catch 
            {
                System.Windows.Application.Current?.Shutdown();
            }
        }

        private void OnTick(object? s, TimerTickEventArgs e)
        {
            // For Pomodoro mode, show countdown (remaining time)
            // For Basic/Tracking mode, show count-up (elapsed time)
            if (string.Equals(SelectedMode, "Pomodoro", StringComparison.OrdinalIgnoreCase))
            {
                TimeText = e.Remaining.ToString(@"hh\:mm\:ss");

                // Update phase indicator
                CurrentPhase = e.Mode switch
                {
                    Services.TimerMode.Pomodoro => "Focus Time",
                    Services.TimerMode.ShortBreak => "Short Break",
                    Services.TimerMode.LongBreak => "Long Break",
                    _ => "Ready"
                };
            }
            else
            {
                TimeText = e.Elapsed.ToString(@"hh\:mm\:ss");
                CurrentPhase = "Tracking";
            }

            Progress = e.Progress;
            IsTimerRunning = _timerService.IsRunning;
            RaisePropertyChanged(nameof(IsRunning));
        }

        private void OnFinished(object? s, EventArgs e)
        {
            // Use NotificationService which checks EnableNotifications setting
            NotificationService.Show("Focus Timer", "Phase finished.", System.Windows.Forms.ToolTipIcon.Info);

            IsTimerRunning = _timerService.IsRunning;
            CanPauseResume = false; // finished means no active session
            IsPaused = false; // reset paused state
        }

        private void OnNotificationRequested(object? s, string message)
        {
            // Use NotificationService which checks EnableNotifications setting
            NotificationService.Show("Focus Timer", message, System.Windows.Forms.ToolTipIcon.Info);
        }

        public void Start()
        {
            // Check if timer is paused - if yes, resume instead of starting fresh
            if (IsPaused)
            {
                // Timer was paused, so resume it
                _timerService.Resume();
                IsPaused = false;
            }
            else
            {
                // Starting fresh
                if (string.Equals(SelectedMode, "Pomodoro", StringComparison.OrdinalIgnoreCase))
                {
                    _timerService.StartPomodoro();
                }
                else
                {
                    _timerService.StartBasic();
                }
            }

            IsTimerRunning = _timerService.IsRunning;
            RaisePropertyChanged(nameof(IsRunning));
            CanPauseResume = true; // session started
        }

        public void Stop()
        {
            _timerService.Stop();
            // Reset UI immediately to zero
            TimeText = TimeSpan.Zero.ToString(@"hh\:mm\:ss");
            Progress = 0;
            IsTimerRunning = _timerService.IsRunning;
            RaisePropertyChanged(nameof(IsRunning));
            CanPauseResume = false;
            IsPaused = false;
        }

        public void TogglePause()
        {
            if (!CanPauseResume) return; // ignore when not started

            if (_timerService.IsRunning)
            {
                _timerService.Pause();
                IsPaused = true;
            }
            else
            {
                _timerService.Resume();
                IsPaused = false;
            }
            IsTimerRunning = _timerService.IsRunning;
            RaisePropertyChanged(nameof(IsRunning));
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

            if (string.Equals(action, "Pause", StringComparison.OrdinalIgnoreCase) || string.Equals(action, "TogglePause", StringComparison.OrdinalIgnoreCase))
            {
                TogglePause();
                return;
            }

            if (string.Equals(action, "Stop", StringComparison.OrdinalIgnoreCase))
            {
                Stop();
                return;
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
            CurrentPhase = "Ready";
            IsTimerRunning = _timerService.IsRunning;
            CanPauseResume = false; // no active session after reset
            IsPaused = false; // reset paused state
        }

        private void LoadPlaylistOrDefault()
        {
            Tracks.Clear();

            bool loadedFromPlaylist = false;
            if (_playlistStorage != null)
            {
                try
                {
                    var saved = _playlistStorage.Load();
                    foreach (var t in saved)
                        Tracks.Add(t);
                    loadedFromPlaylist = saved.Count > 0;
                }
                catch { }
            }

            if (!loadedFromPlaylist && _mp3Library != null)
            {
                try
                {
                    var music = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    if (!string.IsNullOrWhiteSpace(music))
                    {
                        foreach (var t in _mp3Library.LoadFromFolder(music))
                            Tracks.Add(t);
                      }
                }
                catch { }
            }
        }

        private void SavePlaylist()
        {
            if (_playlistStorage == null) return;
            try
            {
                _playlistStorage.Save(Tracks);
            }
            catch { }
        }

        private void AddTrack()
        {
            if (_mp3Library == null) return;
            try
            {
                var dlg = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Audio Files|*.mp3;*.m4a;*.flac;*.ogg;*.wav;*.wma|All Files|*.*",
                    Multiselect = true
                };
                var ok = dlg.ShowDialog() ?? false;
                if (!ok) return;

                foreach (var f in dlg.FileNames)
                {
                    if (_mp3Library.TryReadFile(f, out var track) && track != null)
                    {
                        // do not copy; keep original file path and show title via TagLib
                        Tracks.Add(track);
                    }
                }
                SavePlaylist();
            }
            catch { }
        }

        private void RemoveSelectedTrack()
        {
            if (SelectedTrack == null) return;
            try
            {
                Tracks.Remove(SelectedTrack);
                SelectedTrack = null;
                SavePlaylist();
            }
            catch { }
        }

        private void PlaySelectedTrack()
        {
            if (SelectedTrack == null || _mediaPlayback == null) return;
            try
            {
                _mediaPlayback.Play(SelectedTrack.FilePath);
                IsPlaying = true;
                (StopPlaybackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                (PlaySelectedTrackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
            }
            catch { }
        }

        private void StopPlayback()
        {
            if (_mediaPlayback == null) return;
            try
            {
                _mediaPlayback.Stop();
                IsPlaying = false;
                (StopPlaybackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
                (PlaySelectedTrackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
            }
            catch { }
        }

        private void OnPlaybackEnded(object? sender, EventArgs e)
        {
            // Update UI when playback ends
            IsPlaying = false;
            (StopPlaybackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
            (PlaySelectedTrackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
        }
    }
}
