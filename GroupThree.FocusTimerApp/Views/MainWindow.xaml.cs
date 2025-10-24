using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using GroupThree.FocusTimerApp.ViewModels;
using GroupThree.FocusTimerApp.Services;

namespace GroupThree.FocusTimerApp.Views
{
    /// <summary>
    /// Main application window
    /// Displays timer, progress, and provides navigation to other features
    /// </summary>
    public partial class MainWindow : Window
    {
        public SettingsService SettingsService { get; }
        public HotkeyService? HotkeyService { get; private set; }
        public TimerService TimerService { get; }
        public WindowService WindowService { get; }
        private readonly IOverlayService _overlayService;
        private MainViewModel? _viewModel;

        /// <summary>
        /// Default constructor (used by XAML designer)
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        /// <summary>
        /// Main constructor with dependency injection
        /// </summary>
        public MainWindow(
            MainViewModel vm, 
            SettingsService settingsService, 
            TimerService timerService, 
            WindowService windowService, 
            IOverlayService overlayService)
        {
            InitializeComponent();

            SettingsService = settingsService;
            TimerService = timerService;
            WindowService = windowService;
            _overlayService = overlayService;
            _viewModel = vm;

            this.DataContext = vm;
            this.Loaded += Window_Loaded;

            // Load timer settings from configuration
            LoadTimerSettings();

            // Subscribe to progress changes for circular progress animation
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        /// <summary>
        /// Loads timer settings from configuration and applies to TimerService
        /// </summary>
        private void LoadTimerSettings()
        {
            try
            {
                var settings = SettingsService.LoadSettings();
                var timerSettings = settings.TimerSettings;

                // Apply settings to TimerService
                TimerService.WorkDuration = TimeSpan.FromMinutes(timerSettings.WorkDuration);
                TimerService.ShortBreak = TimeSpan.FromMinutes(timerSettings.BreakDuration);
                TimerService.LongBreak = TimeSpan.FromMinutes(timerSettings.LongBreakDuration);
                TimerService.LongBreakEvery = timerSettings.LongBreakEvery;

                System.Diagnostics.Debug.WriteLine(
                    $"[MainWindow] Timer settings loaded - Work: {timerSettings.WorkDuration}min, " +
                    $"Short Break: {timerSettings.BreakDuration}min, Long Break: {timerSettings.LongBreakDuration}min"
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[MainWindow] Error loading timer settings: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles property changes from ViewModel
        /// Updates circular progress visualization when progress changes
        /// </summary>
        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.Progress))
            {
                UpdateCircularProgress();
            }
        }

        /// <summary>
        /// Updates the circular progress arc based on current progress value
        /// </summary>
        private void UpdateCircularProgress()
        {
            if (_viewModel == null) return;

            var progressPath = this.FindName("ProgressPath") as Path;
            var progressArc = this.FindName("ProgressArc") as ArcSegment;

            if (progressPath == null || progressArc == null) return;

            double progress = _viewModel.Progress;
            double angle = progress * 360.0;
            double radius = 133;
            double centerX = 140;
            double centerY = 140;

            // Calculate end point of arc using polar coordinates
            double radians = (angle - 90) * Math.PI / 180.0;
            double endX = centerX + radius * Math.Cos(radians);
            double endY = centerY + radius * Math.Sin(radians);

            progressArc.Point = new Point(endX, endY);
            progressArc.IsLargeArc = angle > 180;

            // Hide path if no progress
            progressPath.Visibility = progress > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Handles window loaded event
        /// Applies theme and initializes UI state
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Apply theme when window is loaded
            if (_viewModel?.ThemeService != null)
            {
                _viewModel.ThemeService.ApplyTheme();
            }

            // Initial progress update
            UpdateCircularProgress();
        }

        #region Window Control Handlers

        /// <summary>
        /// Handles title bar drag to move window
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        /// <summary>
        /// Minimizes the window
        /// </summary>
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Closes the application
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

        #region Sidebar Control Handlers

        /// <summary>
        /// Shows sidebar with animation when toggle is checked
        /// </summary>
        private void SidebarToggle_Checked(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)this.FindResource("ShowSidebarAnimation");
            storyboard.Begin();
        }

        /// <summary>
        /// Hides sidebar with animation when toggle is unchecked
        /// </summary>
        private void SidebarToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)this.FindResource("HideSidebarAnimation");
            storyboard.Begin();
        }

        /// <summary>
        /// Closes sidebar when close button is clicked
        /// </summary>
        private void CloseSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            SidebarToggle.IsChecked = false;
        }

        /// <summary>
        /// Closes sidebar when overlay background is clicked
        /// </summary>
        private void SidebarOverlay_Click(object sender, MouseButtonEventArgs e)
        {
            SidebarToggle.IsChecked = false;
        }

        #endregion
    }
}
