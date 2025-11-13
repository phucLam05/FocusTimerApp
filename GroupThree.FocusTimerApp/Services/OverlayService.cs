using System;
using System.Windows;
using GroupThree.FocusTimerApp.Views;
using GroupThree.FocusTimerApp.ViewModels;

namespace GroupThree.FocusTimerApp.Services
{
    public class OverlayService : IOverlayService
    {
        private readonly IServiceProvider _serviceProvider;

        private static OverlayWindow? _overlay;

        public OverlayService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // Truy cập biến static
        public bool IsOverlayVisible => _overlay != null && _overlay.IsVisible;

        public void ShowOverlay()
        {
            // Thao tác trên biến static
            if (_overlay == null)
            {
                var vm = _service_provider_get_overlay_vm();
                var win = new OverlayWindow();
                win.DataContext = vm;

                _overlay = win; // Gán cho biến static

                _overlay.Closed += (sender, e) => _overlay = null;

                _overlay.Owner = System.Windows.Application.Current?.MainWindow;
                _overlay.Show();
            }
            else
            {
                // Nếu cửa sổ đã tồn tại (chỉ đang bị ẩn),
                // hãy hiển thị lại và mang nó lên trên cùng.
                _overlay.Show();
                _overlay.Activate(); // Đảm bảo nó được focus
            }
        }

        public void HideOverlay()
        {
            // Thao tác trên biến static
            if (_overlay != null)
            {
                _overlay.Hide();
            }
        }

        public void ToggleOverlay()
        {
            if (IsOverlayVisible) HideOverlay(); else ShowOverlay();
        }

        private object _service_provider_get_overlay_vm()
        {
            // Request the ViewModel from DI (Dependency Injection).
            // DI will automatically inject the TimerService singleton into the new constructor of OverlayViewModel.
            var vm = _serviceProvider.GetService(typeof(OverlayViewModel));

            if (vm == null)
            {
                // This case should not happen if App.xaml.cs is configured correctly.
                // We pass 'null' to the VM to avoid a crash, but it won't update.
                System.Diagnostics.Debug.WriteLine("CRITICAL: OverlayViewModel is not registered in DI. Creating a fallback.");
                return new OverlayViewModel(null); // Giả sử OverlayViewModel có một constructor chấp nhận null
            }

            return vm;
        }
    }
}