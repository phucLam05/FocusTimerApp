using System;
using System.Collections.Generic;
using GroupThree.FocusTimerApp.Models;

namespace GroupThree.FocusTimerApp.Services
{
    public interface IHotkeyService : IDisposable
    {
        event Action<string>? HotkeyPressed;
        void RegisterHotkeys(IEnumerable<HotkeyBinding> hotkeys);
        void ReloadHotkeys();
        void UnregisterAll();
        List<HotkeyBinding> CurrentHotkeys { get; }
    }
}
