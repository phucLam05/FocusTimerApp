using System;
using System.Windows;
using GroupThree.FocusTimerApp.Views;

namespace GroupThree.FocusTimerApp.ViewModels
{
    /// <summary>
    /// ViewModel for Notification Settings page
    /// Manages user preferences for notification behavior and alerts
    /// </summary>
    public class NotificationSettingsViewModel : ViewModelBase, ISettingsSectionViewModel
    {
        public string SectionName => "Notification";

        private readonly Services.SettingsService _settingsService;
        private readonly Services.INotificationService? _notificationService;

        private bool _enableNotifications = true;
        private bool _enableSound = true;
        private bool _autoDismissNotifications = true;
        private bool _showOnAllWorkspaces = false;

        /// <summary>
        /// Master switch to enable/disable all notifications
        /// </summary>
        public bool EnableNotifications 
        { 
            get 
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationSettings] Get EnableNotifications: {_enableNotifications}");
                return _enableNotifications;
            }
            set 
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationSettings] Set EnableNotifications: {value}");
                SetProperty(ref _enableNotifications, value);
            }
        }

        /// <summary>
        /// Whether to play sound alerts with notifications
        /// </summary>
        public bool EnableSound 
        { 
            get 
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationSettings] Get EnableSound: {_enableSound}");
                return _enableSound;
            }
            set 
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationSettings] Set EnableSound: {value}");
                SetProperty(ref _enableSound, value);
            }
        }

        /// <summary>
        /// Whether notifications should automatically close after a timeout
        /// </summary>
        public bool AutoDismissNotifications 
        { 
            get 
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationSettings] Get AutoDismissNotifications: {_autoDismissNotifications}");
                return _autoDismissNotifications;
            }
            set 
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationSettings] Set AutoDismissNotifications: {value}");
                SetProperty(ref _autoDismissNotifications, value);
            }
        }

        /// <summary>
        /// Whether notifications appear on all virtual desktops/workspaces
        /// </summary>
        public bool ShowOnAllWorkspaces 
        { 
            get 
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationSettings] Get ShowOnAllWorkspaces: {_showOnAllWorkspaces}");
                return _showOnAllWorkspaces;
            }
            set 
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationSettings] Set ShowOnAllWorkspaces: {value}");
                SetProperty(ref _showOnAllWorkspaces, value);
            }
        }

        public System.Windows.Input.ICommand SaveCommand { get; }
        public System.Windows.Input.ICommand TestNotificationCommand { get; }

        /// <summary>
        /// Constructor without NotificationService (for simple DI)
        /// </summary>
        public NotificationSettingsViewModel(Services.SettingsService settingsService)
            : this(settingsService, null)
        {
        }

        /// <summary>
        /// Constructor with dependency injection including optional NotificationService for testing
        /// Loads current notification settings from configuration
        /// </summary>
        public NotificationSettingsViewModel(
            Services.SettingsService settingsService,
            Services.INotificationService? notificationService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _notificationService = notificationService;
            
            // Initialize commands first
            SaveCommand = new Commands.RelayCommand<object>(_ => Save());
            TestNotificationCommand = new Commands.RelayCommand<object>(_ => TestNotification());
            
            // Load existing settings with defaults
            LoadSettings();
            
            System.Diagnostics.Debug.WriteLine("[NotificationSettingsViewModel] Constructor completed");
        }

        /// <summary>
        /// Loads notification settings from configuration file
        /// Uses default values if settings are not found
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                var cfg = _settingsService.LoadSettings();
                
                // Load values and trigger PropertyChanged
                EnableNotifications = cfg.Notification?.EnableNotifications ?? true;
                EnableSound = cfg.Notification?.EnableSound ?? true;
                AutoDismissNotifications = cfg.Notification?.AutoDismissNotifications ?? true;
                ShowOnAllWorkspaces = cfg.Notification?.ShowOnAllWorkspaces ?? false;

                System.Diagnostics.Debug.WriteLine(
                    $"[NotificationSettings] Loaded - Enabled: {EnableNotifications}, " +
                    $"Sound: {EnableSound}, AutoDismiss: {AutoDismissNotifications}, " +
                    $"AllWorkspaces: {ShowOnAllWorkspaces}"
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationSettings] Load error: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves notification settings to configuration file
        /// Shows success or error message to user
        /// </summary>
        private void Save()
        {
            try
            {
                var cfg = _settingsService.LoadSettings();
                cfg.Notification ??= new Models.NotificationSettings();
                cfg.Notification.EnableNotifications = EnableNotifications;
                cfg.Notification.EnableSound = EnableSound;
                cfg.Notification.AutoDismissNotifications = AutoDismissNotifications;
                cfg.Notification.ShowOnAllWorkspaces = ShowOnAllWorkspaces;
                _settingsService.SaveSettings(cfg);
                
                System.Diagnostics.Debug.WriteLine("[NotificationSettings] Settings saved successfully");
                
                // Show custom success message
                CustomMessageBox.Show(
                    "Your notification settings have been saved successfully!",
                    "Settings Saved",
                    CustomMessageBox.MessageType.Success
                );
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationSettings] Save error: {ex.Message}");
                CustomMessageBox.Show(
                    $"Failed to save settings: {ex.Message}",
                    "Error",
                    CustomMessageBox.MessageType.Error
                );
            }
        }

        /// <summary>
        /// Tests the notification system by showing a sample notification
        /// Allows user to verify notification settings are working
        /// </summary>
        private void TestNotification()
        {
            try
            {
                // Create temporary notification service if not injected
                var notificationService = _notificationService ?? new Services.NotificationService(_settingsService);
                
                // Show test notification
                notificationService.ShowNotification(
                    "Test Notification ??",
                    "This is a test notification! If you can see this, your notification settings are working correctly."
                );
                
                System.Diagnostics.Debug.WriteLine("[NotificationSettings] Test notification triggered");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[NotificationSettings] Test notification error: {ex.Message}");
                CustomMessageBox.Show(
                    $"Failed to show test notification: {ex.Message}",
                    "Test Failed",
                    CustomMessageBox.MessageType.Error
                );
            }
        }
    }
}
