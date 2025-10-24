using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Microsoft.Win32;
using GroupThree.FocusTimerApp.Models;
using GroupThree.FocusTimerApp.Services;
using GroupThree.FocusTimerApp.Commands;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class AppControlViewModel : ViewModelBase
    {
        private readonly AppFocusService _focusService;
        public ICommand RegisterCommand { get; }

        public ObservableCollection<RegisteredAppModel> RegisteredApps { get; } = new();

        private RegisteredAppModel? _selectedApp;
        public RegisteredAppModel? SelectedApp
        {
            get => _selectedApp;
            set => SetProperty(ref _selectedApp, value);
        }

        public ICommand AddAppCommand { get; }
        public ICommand RemoveAppCommand { get; }
        public ICommand RefreshAppsCommand { get; }

        public AppControlViewModel(AppFocusService focusService)
        {
            _focusService = focusService;

            AddAppCommand = new RelayCommand(AddApp);
            RemoveAppCommand = new RelayCommand(RemoveApp, () => SelectedApp != null);
            RefreshAppsCommand = new RelayCommand(LoadRunningApps);

            // Load danh sách app đang chạy ngay khi mở
            LoadRunningApps();

            // Lắng nghe sự kiện từ AppFocusService
            _focusService.EnteredWorkZone += app =>
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show($"Bạn đã quay lại {app.AppName}");
                });

            _focusService.LeftWorkZone += app =>
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    System.Windows.MessageBox.Show($"Bạn đã rời khỏi vùng làm việc ({app.AppName})");
                });
        }

        private void LoadRunningApps()
        {
            RegisteredApps.Clear();

            // ✅ Lấy danh sách các process đang chạy
            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle)) // bỏ background process
                .DistinctBy(p => p.ProcessName);

            foreach (var proc in processes)
            {
                string exePath = string.Empty;
                try
                {
                    exePath = proc.MainModule?.FileName ?? string.Empty;
                }
                catch { /* bỏ lỗi truy cập process system */ }

                if (!string.IsNullOrEmpty(exePath))
                {
                    var model = new RegisteredAppModel
                    {
                        AppName = proc.ProcessName,
                        ExecutablePath = exePath,
                        IsRunning = true
                    };
                    RegisteredApps.Add(model);
                }
            }

            // ✅ Gộp thêm app đã đăng ký trước đó
            foreach (var app in _focusService.GetRegisteredApps())
            {
                if (!RegisteredApps.Any(a => a.ExecutablePath == app.ExecutablePath))
                    RegisteredApps.Add(app);
            }
        }

        private void AddApp()
        {
            var dlg = new OpenFileDialog
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
