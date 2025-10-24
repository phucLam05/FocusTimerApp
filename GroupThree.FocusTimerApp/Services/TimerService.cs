using System;
using System.Timers;

namespace GroupThree.FocusTimerApp.Services
{
    public enum TimerMode
    {
        Basic,
        Pomodoro,
        ShortBreak,
        LongBreak
    }

    public class TimerTickEventArgs : EventArgs
    {
        public TimeSpan Elapsed { get; init; }
        public TimeSpan Remaining { get; init; }
        public double Progress { get; init; }
        public TimerMode Mode { get; init; }
        public int CompletedShortBreaks { get; init; }
    }

    public class TimerService : ITimerService
    {
        private readonly System.Timers.Timer _timer;
        private DateTime? _startTime;
        private TimeSpan _targetDuration = TimeSpan.Zero;
        private bool _isRunning;
        private bool _isInBreak;
        private int _elapsedSeconds; // cumulative work seconds (excluding breaks)
        private int _workSecondsAccumulated; // accumulator for completed work segments
        private int _completedShortBreaks;
        private int _nextShortBreakAtSeconds;

        public TimerMode Mode { get; private set; } = TimerMode.Basic;
        public event EventHandler<TimerTickEventArgs>? Tick;
        public event EventHandler? Finished;
        // New event to request a notification message (UI/consumer should subscribe)
        public event EventHandler<string>? NotificationRequested;

        public bool IsRunning { get => _isRunning; }
        public bool IsInBreak { get => _isInBreak; }

        // Timer settings (in seconds for testing)
        public TimeSpan WorkDuration { get; set; } = TimeSpan.FromSeconds(180); // default 3 minutes
        public TimeSpan ReminderInterval { get; set; } = TimeSpan.FromSeconds(45);
        public TimeSpan ShortBreak { get; set; } = TimeSpan.FromSeconds(5);
        public TimeSpan ShortBreakAfter { get; set; } = TimeSpan.FromSeconds(40);
        public TimeSpan LongBreak { get; set; } = TimeSpan.FromSeconds(20);
        public int LongBreakAfterShortBreakCount { get; set; } = 2;

        public TimerService()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!_isRunning || !_startTime.HasValue) return;

            var sinceStart = DateTime.UtcNow - _startTime.Value;
            int currentSegmentSeconds = (int)sinceStart.TotalSeconds;

            // Calculate values differently for break vs work
            TimeSpan remaining = TimeSpan.Zero;
            double progress = 0;
            TimeSpan elapsedForTick = TimeSpan.Zero;

            if (_isInBreak)
            {
                // During break: remaining is break duration minus seconds since break start
                elapsedForTick = TimeSpan.FromSeconds(currentSegmentSeconds);
                remaining = _targetDuration - elapsedForTick;
                if (_targetDuration.TotalSeconds > 0)
                {
                    progress = Math.Clamp(elapsedForTick.TotalSeconds / _targetDuration.TotalSeconds, 0, 1);
                }

                // If break finished
                if (remaining <= TimeSpan.Zero)
                {
                    HandlePhaseEnd();
                    return;
                }
            }
            else
            {
                // Work segment (Basic or Pomodoro): elapsed is accumulated work seconds + seconds since current work start
                _elapsedSeconds = _workSecondsAccumulated + currentSegmentSeconds;
                elapsedForTick = TimeSpan.FromSeconds(_elapsedSeconds);

                if (Mode == TimerMode.Pomodoro)
                {
                    // remaining towards work target
                    remaining = _targetDuration - elapsedForTick;
                    if (_targetDuration.TotalSeconds > 0)
                    {
                        progress = Math.Clamp(elapsedForTick.TotalSeconds / _targetDuration.TotalSeconds, 0, 1);
                    }

                    // If Pomodoro reached its target
                    if (remaining <= TimeSpan.Zero)
                    {
                        HandlePhaseEnd();
                        return;
                    }
                }
                else
                {
                    // Basic mode: no target; only reminders
                    remaining = TimeSpan.Zero;
                    progress = 0;
                }

                // Reminders and break triggers are based on cumulative work seconds
                if (Mode == TimerMode.Basic)
                {
                    if (_elapsedSeconds > 0 && ReminderInterval.TotalSeconds > 0 && _elapsedSeconds % (int)ReminderInterval.TotalSeconds == 0)
                    {
                        ShowNotification($"You've worked for {_elapsedSeconds} seconds — take a short break!");
                    }
                }
                else if (Mode == TimerMode.Pomodoro && !_isInBreak)
                {
                    // trigger when reaching or passing next break point
                    if (_nextShortBreakAtSeconds > 0 && _elapsedSeconds >= _nextShortBreakAtSeconds)
                    {
                        // determine break type
                        if (_completedShortBreaks + 1 > LongBreakAfterShortBreakCount)
                        {
                            // start long break
                            StartBreak(TimerMode.LongBreak);
                            // reset completed short breaks count after taking long break
                            _completedShortBreaks = 0;
                            // set next break point after long break based on accumulated work
                            _nextShortBreakAtSeconds = _workSecondsAccumulated + (int)ShortBreakAfter.TotalSeconds;
                        }
                        else
                        {
                            StartBreak(TimerMode.ShortBreak);
                            _completedShortBreaks++;
                            // schedule next short break based on accumulated work
                            _nextShortBreakAtSeconds = _workSecondsAccumulated + (int)ShortBreakAfter.TotalSeconds;
                        }

                        // After starting break, exit to avoid double Tick invocation for this tick
                        return;
                    }
                }
            }

            Tick?.Invoke(this, new TimerTickEventArgs
            {
                Elapsed = elapsedForTick,
                Remaining = remaining,
                Progress = progress,
                Mode = Mode,
                CompletedShortBreaks = _completedShortBreaks
            });
        }

        private void HandlePhaseEnd()
        {
            if (_isInBreak)
            {
                _isInBreak = false;
                if (Mode == TimerMode.ShortBreak || Mode == TimerMode.LongBreak)
                {
                    // return to pomodoro work; keep accumulated work seconds
                    Mode = TimerMode.Pomodoro;
                    _targetDuration = WorkDuration;
                    // reset startTime for new work segment
                    _startTime = DateTime.UtcNow;
                    _isRunning = true;
                    _timer.Start();

                    // notify only once when focus resumes
                    ShowNotification("Break over — time to focus!");
                }
            }
            else
            {
                // work session ended
                Stop();
                // signal finished session
                Finished?.Invoke(this, EventArgs.Empty);
            }

            // do not invoke Finished here for break-start or break-end to avoid duplicate notifications
        }

        private void StartBreak(TimerMode breakMode)
        {
            // store accumulated work seconds so we can resume later
            _workSecondsAccumulated = _elapsedSeconds;

            Mode = breakMode;
            _isInBreak = true;
            _targetDuration = breakMode == TimerMode.ShortBreak ? ShortBreak : LongBreak;
            _startTime = DateTime.UtcNow;
            _isRunning = true;
            _timer.Start();

            // notify only once when break starts
            ShowNotification(breakMode == TimerMode.ShortBreak ? "Short break started!" : "Long break started!");
        }

        public void StartBasic()
        {
            Mode = TimerMode.Basic;
            _targetDuration = TimeSpan.Zero; // no target for tracking/basic
            _startTime = DateTime.UtcNow;
            _isRunning = true;
            _isInBreak = false;
            _workSecondsAccumulated = 0;
            _elapsedSeconds = 0;
            _nextShortBreakAtSeconds = 0;
            _timer.Start();
        }

        public void StartPomodoro()
        {
            Mode = TimerMode.Pomodoro;
            _targetDuration = WorkDuration;
            _startTime = DateTime.UtcNow;
            _isRunning = true;
            _isInBreak = false;
            _workSecondsAccumulated = 0;
            _elapsedSeconds = 0;
            _completedShortBreaks = 0;
            _nextShortBreakAtSeconds = (int)ShortBreakAfter.TotalSeconds;
            _timer.Start();
        }

        public void Pause()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _timer.Stop();
        }

        public void Resume()
        {
            if (_isRunning) return;
            if (!_startTime.HasValue) _startTime = DateTime.UtcNow;
            _isRunning = true;
            _timer.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _timer.Stop();
            _startTime = null;
            _targetDuration = TimeSpan.Zero;
            _isInBreak = false;
            _elapsedSeconds = 0;
            _workSecondsAccumulated = 0;
            _completedShortBreaks = 0;
            _nextShortBreakAtSeconds = 0;
        }

        public void SwitchMode()
        {
            Stop();
            Mode = Mode == TimerMode.Basic ? TimerMode.Pomodoro : TimerMode.Basic;
        }

        // Reset state for UI when mode changes
        public void ResetState()
        {
            Stop();
            _isInBreak = false;
            _elapsedSeconds = 0;
            _workSecondsAccumulated = 0;
            _completedShortBreaks = 0;
            _startTime = null;
            _targetDuration = Mode == TimerMode.Pomodoro ? WorkDuration : TimeSpan.Zero;
            _nextShortBreakAtSeconds = Mode == TimerMode.Pomodoro ? (int)ShortBreakAfter.TotalSeconds : 0;
        }

        private void ShowNotification(string message)
        {
            NotificationRequested?.Invoke(this, message);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
