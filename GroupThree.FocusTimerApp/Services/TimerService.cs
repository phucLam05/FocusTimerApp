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
        private int _elapsedSeconds; // used for Basic mode and progress calc
        private int _completedShortBreaks;
        private TimeSpan _segmentElapsedOffset = TimeSpan.Zero; // accumulated elapsed for current segment across pauses

        public TimerMode Mode { get; private set; } = TimerMode.Basic;
        public event EventHandler<TimerTickEventArgs>? Tick;
        public event EventHandler? Finished;
        public event EventHandler<string>? NotificationRequested;

        public bool IsRunning => _isRunning;
        public bool IsInBreak => _isInBreak;

        // Allow enabling/disabling notifications from settings
        public bool NotificationsEnabled { get; set; } = true;

        // Engine parameters (set from settings)
        public TimeSpan WorkDuration { get; set; } = TimeSpan.FromMinutes(25);
        public TimeSpan ReminderInterval { get; set; } = TimeSpan.FromMinutes(15);
        public TimeSpan ShortBreak { get; set; } = TimeSpan.FromMinutes(5);
        // In classic Pomodoro, break starts when work finishes; keep this for possible future use
        public TimeSpan ShortBreakAfter { get; set; } = TimeSpan.FromMinutes(25);
        public TimeSpan LongBreak { get; set; } = TimeSpan.FromMinutes(15);
        public int LongBreakAfterShortBreakCount { get; set; } = 4;

        public TimerService()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!_isRunning || !_startTime.HasValue) return;

            var elapsedThisRun = DateTime.UtcNow - _startTime.Value;
            var elapsedTotal = _segmentElapsedOffset + elapsedThisRun;
            int currentSegmentSeconds = (int)elapsedTotal.TotalSeconds;

            TimeSpan remaining = TimeSpan.Zero;
            double progress = 0;
            TimeSpan elapsedForTick = TimeSpan.Zero;

            if (_isInBreak)
            {
                // break phase
                elapsedForTick = TimeSpan.FromSeconds(currentSegmentSeconds);
                remaining = _targetDuration - elapsedForTick;
                if (_targetDuration.TotalSeconds > 0)
                {
                    progress = Math.Clamp(elapsedForTick.TotalSeconds / _targetDuration.TotalSeconds, 0, 1);
                }

                if (remaining <= TimeSpan.Zero)
                {
                    // break ended -> resume work
                    ResumeWorkAfterBreak();
                    return;
                }
            }
            else
            {
                if (Mode == TimerMode.Pomodoro)
                {
                    // work phase in Pomodoro
                    elapsedForTick = TimeSpan.FromSeconds(currentSegmentSeconds);
                    remaining = _targetDuration - elapsedForTick;
                    if (_targetDuration.TotalSeconds > 0)
                    {
                        progress = Math.Clamp(elapsedForTick.TotalSeconds / _targetDuration.TotalSeconds, 0, 1);
                    }

                    if (remaining <= TimeSpan.Zero)
                    {
                        // Work session finished -> start break (short or long)
                        StartNextBreakAfterWork();
                        return;
                    }
                }
                else // Basic mode
                {
                    _elapsedSeconds = currentSegmentSeconds;
                    elapsedForTick = TimeSpan.FromSeconds(_elapsedSeconds);
                    remaining = TimeSpan.Zero;
                    progress = 0;

                    if (_elapsedSeconds > 0 && ReminderInterval.TotalSeconds > 0 && _elapsedSeconds % (int)ReminderInterval.TotalSeconds == 0)
                    {
                        ShowNotification($"You've worked for {_elapsedSeconds} seconds – take a short break!");
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

        private void ResumeWorkAfterBreak()
        {
            _isInBreak = false;
            Mode = TimerMode.Pomodoro;
            _targetDuration = WorkDuration;
            _segmentElapsedOffset = TimeSpan.Zero;
            _startTime = DateTime.UtcNow;
            _isRunning = true;
            _timer.Start();
            ShowNotification("Break over – time to focus!");
        }

        private void StartNextBreakAfterWork()
        {
            // Decide break type based on completed short breaks count
            if (LongBreakAfterShortBreakCount > 0 && (_completedShortBreaks + 1) >= LongBreakAfterShortBreakCount)
            {
                StartBreak(TimerMode.LongBreak);
                _completedShortBreaks = 0; // reset after scheduling long break
            }
            else
            {
                StartBreak(TimerMode.ShortBreak);
                _completedShortBreaks++; // count this short break
            }
        }

        private void StartBreak(TimerMode breakMode)
        {
            Mode = breakMode;
            _isInBreak = true;
            _targetDuration = breakMode == TimerMode.ShortBreak ? ShortBreak : LongBreak;
            _segmentElapsedOffset = TimeSpan.Zero;
            _startTime = DateTime.UtcNow;
            _isRunning = true;
            _timer.Start();
            ShowNotification(breakMode == TimerMode.ShortBreak ? "Short break started!" : "Long break started!");
        }

        public void StartBasic()
        {
            Mode = TimerMode.Basic;
            _targetDuration = TimeSpan.Zero;
            _segmentElapsedOffset = TimeSpan.Zero;
            _startTime = DateTime.UtcNow;
            _isRunning = true;
            _isInBreak = false;
            _elapsedSeconds = 0;
            _timer.Start();
        }

        public void StartPomodoro()
        {
            Mode = TimerMode.Pomodoro;
            _targetDuration = WorkDuration;
            _segmentElapsedOffset = TimeSpan.Zero;
            _startTime = DateTime.UtcNow;
            _isRunning = true;
            _isInBreak = false;
            _elapsedSeconds = 0;
            _completedShortBreaks = 0;
            _timer.Start();
        }

        public void Pause()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _timer.Stop();
            if (_startTime.HasValue)
            {
                _segmentElapsedOffset += DateTime.UtcNow - _startTime.Value; // accumulate elapsed so far
            }
        }

        public void Resume()
        {
            if (_isRunning) return;
            // resume measuring from now, keeping accumulated offset
            _startTime = DateTime.UtcNow;
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
            _segmentElapsedOffset = TimeSpan.Zero;
            _completedShortBreaks = 0;
        }

        public void SwitchMode()
        {
            Stop();
            Mode = Mode == TimerMode.Basic ? TimerMode.Pomodoro : TimerMode.Basic;
        }

        public void ResetState()
        {
            Stop();
            _isInBreak = false;
            _elapsedSeconds = 0;
            _startTime = null;
            _segmentElapsedOffset = TimeSpan.Zero;
            _targetDuration = Mode == TimerMode.Pomodoro ? WorkDuration : TimeSpan.Zero;
        }

        private void ShowNotification(string message)
        {
            if (!NotificationsEnabled) return;
            NotificationRequested?.Invoke(this, message);
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
