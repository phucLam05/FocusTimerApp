using System;
using System.Windows;
using GroupThree.FocusTimerApp.Views;
using GroupThree.FocusTimerApp.ViewModels;

namespace GroupThree.FocusTimerApp.Services
{
    public class OverlayService : IOverlayService
    {
        private readonly IServiceProvider _serviceProvider;
        private OverlayWindow? _overlay;

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
                _overlay.Owner = System.Windows.Application.Current?.MainWindow;
                _overlay.Show();
            }
            else
            {
                _overlay.Show();
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
            var vm = _serviceProvider.GetService(typeof(OverlayViewModel)) as OverlayViewModel;
            if (vm != null) return vm;
            return new OverlayViewModel();
        }
    }
}
