using System;
using System.Windows.Input;
using GroupThree.FocusTimerApp.Commands;
using GroupThree.FocusTimerApp.Services;
using System.Collections.ObjectModel;
using GroupThree.FocusTimerApp.Models;
using System.IO;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly ITimerService _timerService;
        private readonly IWindowService _windowService;
        private readonly IOverlayService _overlay_service;
        private readonly SettingsService _settingsService;
        private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
        private readonly IMp3LibraryService? _mp3Library;
        private readonly IPlaylistStorageService? _playlistStorage;
        private readonly IMediaPlaybackService? _mediaPlayback;

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

        public ObservableCollection<Mp3Track> Tracks { get; } = new();

        public ICommand StartCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand OpenSettingsCommand { get; }
        public ICommand ToggleOverlayCommand { get; }
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

        public MainViewModel(ITimerService timerService, IWindowService windowService, IOverlayService overlayService, SettingsService settingsService, IMp3LibraryService? mp3Library = null, IPlaylistStorageService? playlistStorage = null, IMediaPlaybackService? mediaPlayback = null)
        {
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));
            _window_service_or_default(windowService);
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
            _overlay_service_or_default(overlayService);
            _overlay_service = overlayService ?? throw new ArgumentNullException(nameof(overlayService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _mp3Library = mp3Library; // optional DI
            _playlistStorage = playlistStorage; // optional DI
            _mediaPlayback = mediaPlayback; // optional DI

            // setup tray icon for notifications (use fully-qualified types to avoid ambiguous using)
            _notifyIcon = new System.Windows.Forms.NotifyIcon()
            {
                Icon = System.Drawing.SystemIcons.Information,
                Visible = true,
                Text = "Focus Timer"
            };
            ConfigureTrayIcon();

            StartCommand = new RelayCommand<object>(_ => Start());
            // Make Pause button toggle between Pause and Resume
            PauseCommand = new RelayCommand<object>(_ => TogglePause());
            StopCommand = new RelayCommand<object>(_ => Stop());
            OpenSettingsCommand = new RelayCommand<object>(_ => _windowService.ShowSettingsWindow());
            ToggleOverlayCommand = new RelayCommand<object>(_ => _overlay_service.ToggleOverlay());
            AddTrackCommand = new RelayCommand<object>(_ => AddTrack());
            RemoveSelectedTrackCommand = new RelayCommand<object>(_ => RemoveSelectedTrack(), _ => SelectedTrack != null);
            PlaySelectedTrackCommand = new RelayCommand<object>(_ => PlaySelectedTrack(), _ => SelectedTrack != null);
            StopPlaybackCommand = new RelayCommand<object>(_ => StopPlayback(), _ => _mediaPlayback?.IsPlaying == true);

            _timerService.Tick += OnTick;
            _timerService.Finished += OnFinished;
            _timerService.NotificationRequested += OnNotificationRequested;

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
            System.Windows.Application.Current?.Shutdown();
        }

        private void OnTick(object? s, TimerTickEventArgs e)
        {
            // Show elapsed (count-up) for both Basic and Pomodoro
            TimeText = e.Elapsed.ToString(@"hh\:mm\:ss");
            Progress = e.Progress;
            IsTimerRunning = _timerService.IsRunning;
        }

        private void OnFinished(object? s, EventArgs e)
        {
            // keep simple: show a balloon
            try
            {
                _notifyIcon.Visible = true;
                _notifyIcon.BalloonTipTitle = "Focus Timer";
                _notifyIcon.BalloonTipText = "Phase finished.";
                _notifyIcon.ShowBalloonTip(2000);
            }
            catch { }
            IsTimerRunning = _timerService.IsRunning;
            CanPauseResume = false; // finished means no active session
        }

        private void OnNotificationRequested(object? s, string message)
        {
            try
            {
                _notifyIcon.Visible = true;
                _notifyIcon.BalloonTipTitle = "Focus Timer";
                _notifyIcon.BalloonTipText = message;
                _notifyIcon.ShowBalloonTip(3000);
            }
            catch { }
        }

        public void Start()
        {
            // ensure tray icon visible when user starts a phase
            try { _notifyIcon.Visible = true; } catch { }

            if (string.Equals(SelectedMode, "Pomodoro", StringComparison.OrdinalIgnoreCase))
            {
                _timerService.StartPomodoro();
            }
            else
            {
                _timerService.StartBasic();
            }
            IsTimerRunning = _timerService.IsRunning;
            CanPauseResume = true; // session started
        }

        public void Stop()
        {
            _timerService.Stop();
            // Reset UI immediately to zero
            TimeText = TimeSpan.Zero.ToString(@"hh\:mm\:ss");
            Progress = 0;
            IsTimerRunning = _timerService.IsRunning;
            CanPauseResume = false;
        }

        public void TogglePause()
        {
            if (!CanPauseResume) return; // ignore when not started

            if (_timerService.IsRunning)
            {
                _timerService.Pause();
            }
            else
            {
                _timerService.Resume();
            }
            IsTimerRunning = _timerService.IsRunning;
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
            IsTimerRunning = _timerService.IsRunning;
            CanPauseResume = false; // no active session after reset
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
                (StopPlaybackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
            }
            catch { }
        }

        private void StopPlayback()
        {
            if (_mediaPlayback == null) return;
            try
            {
                _mediaPlayback.Stop();
                (StopPlaybackCommand as RelayCommand<object>)?.RaiseCanExecuteChanged();
            }
            catch { }
        }
    }
}
