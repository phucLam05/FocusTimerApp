using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using GroupThree.FocusTimerApp.Models;
using GroupThree.FocusTimerApp.Services;
using GroupThree.FocusTimerApp.Commands;
using System.Windows.Forms; // ⚠️ thêm using này (Forms)
using System.Threading.Tasks;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class AppControlViewModel : ViewModelBase
    {
        private readonly AppFocusService _focusService;
        private readonly TimerService _timerService;

        // 🟢 Danh sách app đang chạy
        public ObservableCollection<RegisteredAppModel> RunningApps { get; } = new();

        // 🟢 Danh sách app đã đăng ký
        public ObservableCollection<RegisteredAppModel> RegisteredApps { get; } = new();

        private RegisteredAppModel? _selectedApp;
        public RegisteredAppModel? SelectedApp
        {
            get => _selectedApp;
            set
            {
                SetProperty(ref _selectedApp, value);
                // 🔹 Khi thay đổi SelectedApp, cập nhật lại trạng thái nút Remove
                (RemoveAppCommand as RelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand AddAppCommand { get; }
        public ICommand RemoveAppCommand { get; }
        public ICommand RefreshAppsCommand { get; }
        public ICommand RegisterCommand { get; }

        public AppControlViewModel(AppFocusService focusService, TimerService timerService)
        {
            _focusService = focusService;
            _timerService = timerService;

            AddAppCommand = new RelayCommand(AddApp);
            RemoveAppCommand = new RelayCommand(RemoveApp, () => SelectedApp != null);
            RefreshAppsCommand = new RelayCommand(LoadRunningApps);
            RegisterCommand = new RelayCommand<RegisteredAppModel>(RegisterApp);

            LoadRunningApps();
            LoadRegisteredApps();

            // 🟢 Lắng nghe sự kiện từ AppFocusService
            _focusService.EnteredWorkZone += () =>
            {
                if (_isShowingMessage) return;
                _isShowingMessage = true;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var notify = new NotifyIcon
                    {
                        Icon = System.Drawing.SystemIcons.Information,
                        Visible = true,
                        BalloonTipTitle = "Focus Timer",
                        BalloonTipText = "Chào mừng quay lại vùng làm việc!"
                    };
                    notify.ShowBalloonTip(3000);

                    if (!_timerService.IsRunning)
                        _timerService.Resume();

                    Task.Delay(3500).ContinueWith(_ =>
                    {
                        notify.Visible = false;
                        notify.Dispose();
                    });
                });

                Task.Delay(2000).ContinueWith(_ => _isShowingMessage = false);
            };

            _focusService.LeftWorkZone += () =>
            {
                if (_isShowingMessage) return;
                _isShowingMessage = true;

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    var notify = new NotifyIcon
                    {
                        Icon = System.Drawing.SystemIcons.Warning,
                        Visible = true,
                        BalloonTipTitle = "Focus Timer",
                        BalloonTipText = "Bạn đã rời khỏi vùng làm việc!"
                    };
                    notify.ShowBalloonTip(3000);

                    if (_timerService.IsRunning)
                        _timerService.Pause();

                    Task.Delay(3500).ContinueWith(_ =>
                    {
                        notify.Visible = false;
                        notify.Dispose();
                    });
                });

                Task.Delay(2000).ContinueWith(_ => _isShowingMessage = false);
            };


        }

        private void LoadRunningApps()
        {
            RunningApps.Clear();

            // ✅ Lấy các process có cửa sổ (có MainWindowTitle)
            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle))
                .DistinctBy(p => p.ProcessName);

            foreach (var proc in processes)
            {
                string exePath = string.Empty;
                try
                {
                    exePath = proc.MainModule?.FileName ?? string.Empty;
                }
                catch { /* bỏ lỗi truy cập */ }

                if (!string.IsNullOrEmpty(exePath))
                {
                    RunningApps.Add(new RegisteredAppModel
                    {
                        AppName = proc.ProcessName,
                        ExecutablePath = exePath,
                        IsRunning = true
                    });
                }
            }
        }

        private void LoadRegisteredApps()
        {
            RegisteredApps.Clear();
            foreach (var app in _focusService.GetRegisteredApps())
                RegisteredApps.Add(app);
        }

        private void RegisterApp(RegisteredAppModel? app)
        {
            if (app == null) return;
            if (!RegisteredApps.Any(a => a.ExecutablePath == app.ExecutablePath))
            {
                _focusService.RegisterApp(app);
                RegisteredApps.Add(app);
            }
        }

        private void AddApp()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Chọn file .exe của ứng dụng",
                Filter = "Executable files (*.exe)|*.exe",
                CheckFileExists = true
            };
            if (dlg.ShowDialog() == true)
            {
                var model = new RegisteredAppModel
                {
                    AppName = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName),
                    ExecutablePath = dlg.FileName
                };
                _focusService.RegisterApp(model);
                RegisteredApps.Add(model);
            }
        }

        private void RemoveApp()
        {
            if (SelectedApp == null) return;
            _focusService.UnregisterApp(SelectedApp.ExecutablePath);
            RegisteredApps.Remove(SelectedApp);
        }
        private bool _isShowingMessage = false;
    }
}
