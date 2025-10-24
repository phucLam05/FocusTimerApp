using System;

namespace GroupThree.FocusTimerApp.Services
{
    public interface ITimerService : IDisposable
    {
        event EventHandler<TimerTickEventArgs>? Tick;
        event EventHandler? Finished;
        bool IsRunning { get; }
        void StartTracking();
        void StartPomodoroWork();
        void StartPomodoroBreak(bool longBreak);
        void Pause();
        void Resume();
        void Stop();
    }
}
