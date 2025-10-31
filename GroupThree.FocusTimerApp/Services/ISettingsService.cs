using GroupThree.FocusTimerApp.Models;
using System;
using System.Collections.Generic;

namespace GroupThree.FocusTimerApp.Services
{
    public interface ISettingsService
    {
        event Action<ConfigSetting>? SettingsChanged;

        ConfigSetting LoadSettings();
        void SaveSettings(ConfigSetting settings);
        List<HotkeyBinding> LoadHotkeys();
        void UpdateHotkeys(List<HotkeyBinding> newHotkeys);
        void ResetToDefault();
    }
}
