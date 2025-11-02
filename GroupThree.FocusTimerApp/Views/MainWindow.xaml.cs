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
        public SettingsService SettingsService { get; }
        public HotkeyService? HotkeyService { get; private set; }
        public TimerService TimerService { get; }
        public WindowService WindowService { get; }
        private readonly IOverlayService _overlayService;
        private MainViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            this.Closing += MainWindow_Closing;
            this.Loaded += MainWindow_Loaded;
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

            // Calculate angle for arc (0 to 360 degrees)
            // Add small minimum to show at least a tiny arc
            double angle = progress * 360;
            if (angle < 1 && progress > 0)
            {
                angle = 1; // Minimum visible arc
            }
            double radians = angle * Math.PI / 180;

            // Arc radius and center
            double radius = 133;
            double centerX = 140;
            double centerY = 140;

            // Calculate end point on the circle
            double endX = centerX + radius * Math.Sin(radians);
            double endY = centerY - radius * Math.Cos(radians);

            // Update arc
            if (ProgressArc != null)
            {
                // Use Dispatcher to ensure UI thread update
                Dispatcher.Invoke(() =>
                {
                    ProgressArc.Point = new Point(endX, endY);
                    ProgressArc.IsLargeArc = angle > 180;
                });
            }
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            // Show custom confirmation dialog
   var dialog = new ConfirmDialog("Confirm Exit", "Do you want to exit the application?")
      {
            Owner = this,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
     dialog.ShowDialog();

        if (dialog.Result)
        {
        // User clicked Yes - allow closing and shutdown
            e.Cancel = false;
    // Unsubscribe from events to prevent issues
            if (_viewModel != null)
     {
        _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
       }
      }
   else
        {
 // User clicked No - cancel closing
            e.Cancel = true;
     }
    }

        // Window drag to move
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double click to maximize/restore (optional - currently disabled due to ResizeMode)
            }
            else
            {
                DragMove();
            }
        }

        // Minimize button - hide to tray
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide window to tray instead of minimize
            this.Hide();
            this.ShowInTaskbar = false;
        }

        // Close button - confirm and exit
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
      // Trigger the closing event which will show confirmation
         Close();
        }

        // Sidebar toggle
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

        // Close sidebar button
        private void CloseSidebarButton_Click(object sender, RoutedEventArgs e)
        {
            SidebarToggle.IsChecked = false;
        }

        // Sidebar overlay click
        private void SidebarOverlay_Click(object sender, MouseButtonEventArgs e)
        {
            SidebarToggle.IsChecked = false;
        }
    }
}
