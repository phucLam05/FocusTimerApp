using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Shapes;
using GroupThree.FocusTimerApp.ViewModels;
using GroupThree.FocusTimerApp.Services;
using System.ComponentModel;
using System;
using Point = System.Windows.Point;

namespace GroupThree.FocusTimerApp.Views
{
    public partial class MainWindow : Window
    {
        public SettingsService? SettingsService { get; }
        public HotkeyService? HotkeyService { get; private set; }
        public TimerService? TimerService { get; }
        public WindowService? WindowService { get; }
        private readonly IOverlayService? _overlayService;
        private MainViewModel? _viewModel;
        private bool _isRealClosing = false;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            this.Loaded += MainWindow_Loaded;

            // Đăng ký sự kiện StateChanged
            this.StateChanged += MainWindow_StateChanged;
        }

        public MainWindow(MainViewModel vm, SettingsService settingsService, TimerService timerService, WindowService windowService, IOverlayService overlayService)
        {
            InitializeComponent();

            SettingsService = settingsService;
            TimerService = timerService;
            WindowService = windowService;
            _overlayService = overlayService;
            _viewModel = vm;

            this.DataContext = vm;
            this.Closing += MainWindow_Closing;
            this.Loaded += MainWindow_Loaded;

            // Đăng ký sự kiện StateChanged
            this.StateChanged += MainWindow_StateChanged;
        }

        private void MainWindow_StateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                // Check RunInBackground setting
                var cfg = SettingsService?.LoadSettings();
                bool runInBackground = cfg?.General?.RunInBackground ?? true;

                if (runInBackground)
                {
                    // RunInBackground = true: Hide from taskbar and show overlay
                    this.Hide();
                    this.ShowInTaskbar = false;
                    _overlayService?.ShowOverlay();
                }
                else
                {
                    // RunInBackground = false: Keep in taskbar, just show overlay
                    _overlayService?.ShowOverlay();
                }
            }
            else if (this.WindowState == WindowState.Normal)
            {
                // Tự động TẮT overlay khi khôi phục từ taskbar
                _overlayService?.HideOverlay();
            }
        }
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.Progress))
            {
                UpdateProgressRing();
            }
        }

        private void UpdateProgressRing()
        {
            if (_viewModel == null) return;

            double progress = _viewModel.Progress;

            // Clamp progress between 0 and 1
            progress = Math.Clamp(progress, 0, 1);

            // For Pomodoro countdown, invert progress (full ring at start, empty at end)
            if (_viewModel.IsPomodoroMode)
            {
                progress = 1 - progress;
            }

            double angle = progress * 360;
            if (angle < 1 && progress > 0)
            {
                angle = 1; // Minimum visible arc
            }
            double radians = angle * Math.PI / 180;
            double radius = 133;
            double centerX = 140;
            double centerY = 140;
            double endX = centerX + radius * Math.Sin(radians);
            double endY = centerY - radius * Math.Cos(radians);

            if (ProgressArc != null)
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressArc.Point = new Point(endX, endY);
                    ProgressArc.IsLargeArc = angle > 180;
                });
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            // Load RunInBackground setting
            var cfg = SettingsService?.LoadSettings();
            bool runInBackground = cfg?.General?.RunInBackground ?? true;

            if (_isRealClosing)
            {
                // Real exit: cleanup and allow close
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                }
                e.Cancel = false;
                return;
            }

            if (runInBackground)
            {
                // RunInBackground = true: Just hide window, don't show confirm, don't really exit
                e.Cancel = true;
                this.Hide();
                this.ShowInTaskbar = false;
                _overlayService?.ShowOverlay();
            }
            else
            {
                // RunInBackground = false: Show confirm dialog and really exit if confirmed
                var dialog = new ConfirmDialog("Confirm Exit", "Do you want to exit the application?")
                {
                    Owner = this,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };
                dialog.ShowDialog();

                if (dialog.Result)
                {
                    // User confirmed exit: really close the app
                    _isRealClosing = true;
                    if (_viewModel != null)
                    {
                        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
                    }
                    e.Cancel = false;
                    System.Windows.Application.Current.Shutdown();
                }
                else
                {
                    // User cancelled: don't close
                    e.Cancel = true;
                }
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
            {
                DragMove();
            }
        }

        // Nút Minimize (ẩn xuống khay hệ thống)
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            // Check RunInBackground setting
            var cfg = SettingsService?.LoadSettings();
            bool runInBackground = cfg?.General?.RunInBackground ?? true;

            if (runInBackground)
            {
                // RunInBackground = true: Hide from taskbar completely
                this.Hide();
                this.ShowInTaskbar = false;
                _overlayService?.ShowOverlay();
            }
            else
            {
                // RunInBackground = false: Just minimize to taskbar
                this.WindowState = WindowState.Minimized;
                // ShowInTaskbar remains true
                _overlayService?.ShowOverlay();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SidebarToggle_Checked(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)FindResource("ShowSidebarAnimation");
            storyboard.Begin();
        }

        private void SidebarToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)FindResource("HideSidebarAnimation");
            storyboard.Begin();
        }

        private void CloseSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            SidebarToggle.IsChecked = false;
        }

        private void SidebarOverlay_Click(object sender, MouseButtonEventArgs e)
        {
            SidebarToggle.IsChecked = false;
        }

        // Public method to force close without confirmation
        public void ForceClose()
        {
            _isRealClosing = true;
            Close();
        }
    }
}