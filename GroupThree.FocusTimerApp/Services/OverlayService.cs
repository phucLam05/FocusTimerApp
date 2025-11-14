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

        public bool IsOverlayVisible => _overlay != null && _overlay.IsVisible;

        public void ShowOverlay()
        {
            if (_overlay == null)
            {
                var vm = _service_provider_get_overlay_vm();
                var win = new OverlayWindow();
                win.DataContext = vm;

                _overlay = win;

                _overlay.Closed += (sender, e) => _overlay = null;

                _overlay.Owner = System.Windows.Application.Current?.MainWindow;
                _overlay.Show();
            }
            else
            {
                // Nếu cửa sổ đã tồn tại (chỉ đang bị ẩn),
                _overlay.Show();
                _overlay.Activate(); // Đảm bảo nó được focus
            }
        }

        public void HideOverlay()
        {
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
                System.Diagnostics.Debug.WriteLine("CRITICAL: OverlayViewModel is not registered in DI. Creating a fallback.");
                return new OverlayViewModel(null);
            }

            return vm;
        }
    }
}