using GroupThree.FocusTimerApp.Services;
using GroupThree.FocusTimerApp.Models;
using System;
using System.Collections.Generic;
// using GroupThree.FocusTimerApp.Models; // <-- FIX: Removed duplicate line

namespace GroupThree.FocusTimerApp.AppControl
{
    public interface IAppControlService : IDisposable
    {
        IReadOnlyList<RegisteredAppModel> RegisteredApps { get; }

        event Action<RegisteredAppModel?> EnteredFocusZone;
        event Action<RegisteredAppModel?> LeftFocusZone;
        event Action<RegisteredAppModel?> ActiveAppChanged;
        event Action RegisteredAppsChanged;

        void StartMonitoring();
        void StopMonitoring();

        void AddRegisteredApp(string exePath);
        void RemoveRegisteredApp(string exePath);
        void ToggleMonitoring(bool enabled);

        RegisteredAppModel? GetRegisteredForExe(string exePath);
        bool IsPathRegistered(string exePath);
    }
}