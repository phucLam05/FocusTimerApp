using System;
using System.Windows;
using GroupThree.FocusTimerApp.Services;

namespace GroupThree.FocusTimerApp.Tests
{
    public class TestHotkeyService : IDisposable
    {
        private readonly HotkeyService _hotkeyService;
        private readonly SettingsService _settingsService;

        public TestHotkeyService(Window window)
        {
            _settingsService = new SettingsService();
            _hotkeyService = new HotkeyService(window, _settingsService);

            // Khi nhấn hotkey -> gọi event này
            _hotkeyService.HotkeyPressed += OnHotkeyPressed;
        }

        // Bắt đầu test: đăng ký các hotkey
        public void StartTest()
        {
            Console.WriteLine("===== ⚙️ TEST HotkeyService START =====");
            _hotkeyService.RegisterHotkeys();
            Console.WriteLine("✅ Hotkeys registered. Try pressing:");
            Console.WriteLine("   • Alt + P  → Start/Pause");
            Console.WriteLine("   • Alt + Q  → Switch Mode");
            Console.WriteLine("   • Alt + A  → Open Alarm");
            Console.WriteLine("----------------------------------------");
            Console.WriteLine("⚠️ Keep the app window active and try pressing the keys!");
        }

        // Khi nhấn hotkey, sự kiện này được gọi
        private void OnHotkeyPressed(string action)
        {
            switch (action)
            {
                case "StartStop":
                    Console.WriteLine("▶️ [HOTKEY] StartStop triggered (Alt + P)");
                    break;

                case "SwitchMode":
                    Console.WriteLine("🔄 [HOTKEY] SwitchMode triggered (Alt + Q)");
                    break;

                case "OpenAlarm":
                    Console.WriteLine("⏰ [HOTKEY] OpenAlarm triggered (Alt + A)");
                    break;

                default:
                    Console.WriteLine($"⚡ [HOTKEY] Unknown action: {action}");
                    break;
            }
        }

        public void Dispose()
        {
            _hotkeyService.UnregisterAll();
            _hotkeyService.Dispose();
            Console.WriteLine("🧹 All hotkeys unregistered. Test finished.");
        }
    }
}
