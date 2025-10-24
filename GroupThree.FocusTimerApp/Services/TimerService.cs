using System;
using System.Timers;

namespace GroupThree.FocusTimerApp.Services
{
    public enum TimerMode
    {
        Tracking,
        Pomodoro
    }

    public class TimerTickEventArgs : EventArgs
    {
        public TimeSpan Elapsed { get; init; }
        public TimeSpan Remaining { get; init; }
        public double Progress { get; init; }
        public TimerMode Mode { get; init; }
        public int PomodoroCycleCount { get; init; }
    }

    public class TimerService : ITimerService
    {
        private readonly System.Timers.Timer _timer;
        private DateTime? _startTime;
        private TimeSpan _elapsedBeforePause = TimeSpan.Zero; // ✅ lưu thời gian đã đếm trước khi pause
        private TimeSpan _targetDuration = TimeSpan.Zero;
        private bool _isRunning;
        private bool _isPaused;

        public TimerMode Mode { get; private set; } = TimerMode.Tracking;
        public event EventHandler<TimerTickEventArgs>? Tick;
        public event EventHandler? Finished;

        public bool IsRunning => _isRunning;

        // Pomodoro settings
        public TimeSpan WorkDuration { get; set; } = TimeSpan.FromMinutes(50);
        public TimeSpan ShortBreak { get; set; } = TimeSpan.FromMinutes(10);
        public TimeSpan LongBreak { get; set; } = TimeSpan.FromMinutes(30);
        public int LongBreakEvery { get; set; } = 4;

        private int _pomodoroCount = 0;

        public TimerService()
        {
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += OnTimerElapsed;
        }

        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!_isRunning || !_startTime.HasValue) return;

            // ✅ tính đúng elapsed có tính đến pause
            var elapsed = _elapsedBeforePause + (DateTime.UtcNow - _startTime.Value);
            TimeSpan remaining = Mode == TimerMode.Tracking ? TimeSpan.Zero : _targetDuration - elapsed;

            double progress = 0;
            if (Mode == TimerMode.Pomodoro && _targetDuration.TotalSeconds > 0)
                progress = Math.Clamp(elapsed.TotalSeconds / _targetDuration.TotalSeconds, 0, 1);

            if (Mode == TimerMode.Pomodoro && remaining <= TimeSpan.Zero)
            {
                _isRunning = false;
                _timer.Stop();

                if (_targetDuration == WorkDuration)
                    _pomodoroCount++;

                Finished?.Invoke(this, EventArgs.Empty);
            }

            Tick?.Invoke(this, new TimerTickEventArgs
            {
                Elapsed = elapsed,
                Remaining = remaining,
                Progress = progress,
                Mode = Mode,
                PomodoroCycleCount = _pomodoroCount
            });
        }

        public void StartTracking()
        {
            Mode = TimerMode.Tracking;
            _elapsedBeforePause = TimeSpan.Zero;
            _startTime = DateTime.UtcNow;
            _isPaused = false;
            _isRunning = true;
            _timer.Start();
        }

        public void StartPomodoroWork()
        {
            Mode = TimerMode.Pomodoro;
            _elapsedBeforePause = TimeSpan.Zero;
            _targetDuration = WorkDuration;
            _startTime = DateTime.UtcNow;
            _isPaused = false;
            _isRunning = true;
            _timer.Start();
        }

        public void StartPomodoroBreak(bool longBreak)
        {
            Mode = TimerMode.Pomodoro;
            _elapsedBeforePause = TimeSpan.Zero;
            _targetDuration = longBreak ? LongBreak : ShortBreak;
            _startTime = DateTime.UtcNow;
            _isPaused = false;
            _isRunning = true;
            _timer.Start();
        }

        public void Pause()
        {
            if (!_isRunning) return;

            // ✅ lưu lại thời gian đã trôi
            _elapsedBeforePause += (DateTime.UtcNow - _startTime!.Value);

            _isRunning = false;
            _isPaused = true;
            _timer.Stop();
        }

        public void Resume()
        {
            if (_isRunning || !_isPaused) return;

            // ✅ bắt đầu lại, không reset elapsed
            _startTime = DateTime.UtcNow;
            _isRunning = true;
            _isPaused = false;
            _timer.Start();
        }

        public void Stop()
        {
            _isRunning = false;
            _isPaused = false;
            _timer.Stop();
            _startTime = null;
            _elapsedBeforePause = TimeSpan.Zero;
            _targetDuration = TimeSpan.Zero;
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}
