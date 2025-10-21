using System;
using System.Collections.Generic;
using GroupThree.FocusTimerApp.Models;
using System.Windows;

namespace GroupThree.FocusTimerApp.Services
{
    // Simple adapter that forwards to existing HotkeyService instance created with Window handle.
    // This keeps IHotkeyService interface while reusing the concrete HotkeyService implementation.
    public class HotkeyServiceAdapter : IHotkeyService
    {
        private readonly HotkeyService _inner;

        public HotkeyServiceAdapter(HotkeyService inner)
        {
            _inner = inner;
            _inner.HotkeyPressed += action => HotkeyPressed?.Invoke(action);
        }

        public event Action<string>? HotkeyPressed;

        // Adapter does not maintain its own list; return empty list for now
        public List<HotkeyBinding> CurrentHotkeys => new List<HotkeyBinding>();

        public void RegisterHotkeys(IEnumerable<HotkeyBinding> hotkeys)
        {
            // delegate to inner registration which reads from SettingsService
            _inner.RegisterHotkeys();
        }

        public void ReloadHotkeys() => _inner.ReloadHotkeys();

        public void UnregisterAll() => _inner.UnregisterAll();

        public void Dispose() => _inner.Dispose();
    }
}
