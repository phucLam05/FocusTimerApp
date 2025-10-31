using System; // Added for StringComparison and Environment
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
            // Lưu ý: Logic thông báo Entered/LeftWorkZone đã được chuyển sang App startup để chỉ đăng ký 1 lần toàn app.
        }

        private void LoadRunningApps()
        {
            RunningApps.Clear();

            // Loại bỏ bản thân ứng dụng khỏi danh sách
            int currentProcessId = Process.GetCurrentProcess().Id;
            string? currentProcessPath = Environment.ProcessPath;

            // ✅ Lấy các process có cửa sổ (có MainWindowTitle) và khác ứng dụng hiện tại
            var processes = Process.GetProcesses()
                .Where(p => p.Id != currentProcessId)
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
                    // Bảo vệ bổ sung: so sánh theo đường dẫn
                    if (!string.IsNullOrEmpty(currentProcessPath) &&
                        string.Equals(exePath, currentProcessPath, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

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
    }
}
