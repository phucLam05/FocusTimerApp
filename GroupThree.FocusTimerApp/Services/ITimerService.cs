using System;

namespace GroupThree.FocusTimerApp.Services
{
    public interface ITimerService : IDisposable
    {
        TimerMode Mode { get; }
        event EventHandler<TimerTickEventArgs>? Tick;
        event EventHandler? Finished;
        event EventHandler<string>? NotificationRequested;

        bool IsRunning { get; }
        bool IsInBreak { get; }

        // operational methods
        void StartBasic();
        void StartPomodoro();
        void Pause();
        void Resume();
        void Stop();
        void SwitchMode();
        void ResetState();

        // Settings-backed properties
        TimeSpan WorkDuration { get; set; }
        TimeSpan ReminderInterval { get; set; }
        TimeSpan ShortBreak { get; set; }
        TimeSpan ShortBreakAfter { get; set; }
        TimeSpan LongBreak { get; set; }
        int LongBreakAfterShortBreakCount { get; set; }
    }
}
