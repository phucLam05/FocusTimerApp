using System;
using System.Timers;

namespace GroupThree.FocusTimerApp.Services
{
    /// <summary>
    /// Defines the timer operation mode
    /// </summary>
    public enum TimerMode
    {
        /// <summary>
        /// Continuous time tracking mode without breaks
        /// </summary>
        Tracking,
        
        /// <summary>
        /// Structured work/break cycle mode (Pomodoro Technique)
        /// </summary>
        Pomodoro
    }

    /// <summary>
    /// Event arguments for timer tick events
    /// Contains current timer state information
    /// </summary>
    public class TimerTickEventArgs : EventArgs
    {
        /// <summary>Time elapsed since timer started</summary>
        public TimeSpan Elapsed { get; init; }
        
        /// <summary>Time remaining until timer completes (Pomodoro only)</summary>
        public TimeSpan Remaining { get; init; }
        
        /// <summary>Progress percentage (0.0 to 1.0) for Pomodoro mode</summary>
        public double Progress { get; init; }
        
        /// <summary>Current timer mode</summary>
        public TimerMode Mode { get; init; }
        
        /// <summary>Number of completed Pomodoro cycles</summary>
        public int PomodoroCycleCount { get; init; }
    }

    /// <summary>
    /// Core timer service implementing both Tracking and Pomodoro modes
    /// Provides timer lifecycle management with events for UI updates
    /// </summary>
    public class TimerService : ITimerService
    {
        private readonly System.Timers.Timer _timer;
        private DateTime? _startTime;
        private TimeSpan _targetDuration = TimeSpan.Zero;
        private bool _isRunning;

        /// <summary>Current operating mode of the timer</summary>
        public TimerMode Mode { get; private set; } = TimerMode.Tracking;
        
        /// <summary>Event fired every second while timer is running</summary>
        public event EventHandler<TimerTickEventArgs>? Tick;
        
        /// <summary>Event fired when timer completes its target duration</summary>
        public event EventHandler? Finished;

        /// <summary>Indicates whether the timer is currently running</summary>
        public bool IsRunning { get => _isRunning; }

        // Pomodoro settings - can be configured via SettingsService
        /// <summary>Duration of work/focus sessions in Pomodoro mode (default: 25 minutes)</summary>
        public TimeSpan WorkDuration { get; set; } = TimeSpan.FromMinutes(25);
        
        /// <summary>Duration of short breaks in Pomodoro mode (default: 5 minutes)</summary>
        public TimeSpan ShortBreak { get; set; } = TimeSpan.FromMinutes(5);
        
        /// <summary>Duration of long breaks in Pomodoro mode (default: 15 minutes)</summary>
        public TimeSpan LongBreak { get; set; } = TimeSpan.FromMinutes(15);
        
        /// <summary>Number of work cycles before taking a long break (default: 4)</summary>
        public int LongBreakEvery { get; set; } = 4;

        private int _pomodoroCount = 0;

        /// <summary>
        /// Initializes the timer service with 1-second tick interval
        /// </summary>
        public TimerService()
        {
            _timer = new System.Timers.Timer(1000); // Tick every second
            _timer.Elapsed += OnTimerElapsed;
        }

        /// <summary>
        /// Handles timer elapsed event - updates state and fires events
        /// </summary>
        private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
        {
            if (!_isRunning || !_startTime.HasValue) return;

            var elapsed = DateTime.UtcNow - _startTime.Value;
            TimeSpan remaining = Mode == TimerMode.Tracking 
                ? TimeSpan.Zero 
                : _targetDuration - elapsed;
            
            double progress = 0;
            if (Mode == TimerMode.Pomodoro && _targetDuration.TotalSeconds > 0)
            {
                progress = Math.Clamp(elapsed.TotalSeconds / _targetDuration.TotalSeconds, 0, 1);
            }

            // Check if Pomodoro cycle is complete
            if (Mode == TimerMode.Pomodoro && remaining <= TimeSpan.Zero)
            {
                _isRunning = false;
                _timer.Stop();

                // Increment counter if work session completed
                if (_targetDuration == WorkDuration)
                {
                    _pomodoroCount++;
                }

                // Fire completion event
                Finished?.Invoke(this, EventArgs.Empty);
            }

            // Fire tick event with current state
            Tick?.Invoke(this, new TimerTickEventArgs
            {
                Elapsed = elapsed,
                Remaining = remaining,
                Progress = progress,
                Mode = Mode,
                PomodoroCycleCount = _pomodoroCount
            });
        }

        /// <summary>
        /// Starts timer in Tracking mode (continuous counting up)
        /// </summary>
        public void StartTracking()
        {
            Mode = TimerMode.Tracking;
            _startTime = DateTime.UtcNow;
            _isRunning = true;
            _timer.Start();
            System.Diagnostics.Debug.WriteLine("[TimerService] Tracking started");
        }

        /// <summary>
        /// Starts a Pomodoro work session with configured work duration
        /// </summary>
        public void StartPomodoroWork()
        {
            Mode = TimerMode.Pomodoro;
            _targetDuration = WorkDuration;
            _startTime = DateTime.UtcNow;
            _isRunning = true;
            _timer.Start();
            System.Diagnostics.Debug.WriteLine($"[TimerService] Pomodoro work started - Duration: {WorkDuration.TotalMinutes}min");
        }

        /// <summary>
        /// Starts a Pomodoro break session (short or long)
        /// </summary>
        /// <param name="longBreak">True for long break, false for short break</param>
        public void StartPomodoroBreak(bool longBreak)
        {
            Mode = TimerMode.Pomodoro;
            _targetDuration = longBreak ? LongBreak : ShortBreak;
            _startTime = DateTime.UtcNow;
            _isRunning = true;
            _timer.Start();
            System.Diagnostics.Debug.WriteLine($"[TimerService] Pomodoro {(longBreak ? "long" : "short")} break started");
        }

        /// <summary>
        /// Pauses the currently running timer
        /// </summary>
        public void Pause()
        {
            if (!_isRunning) return;
            _isRunning = false;
            _timer.Stop();
            System.Diagnostics.Debug.WriteLine("[TimerService] Timer paused");
        }

        /// <summary>
        /// Resumes a paused timer from where it left off
        /// </summary>
        public void Resume()
        {
            if (_isRunning) return;
            if (!_startTime.HasValue) 
                _startTime = DateTime.UtcNow;
            _isRunning = true;
            _timer.Start();
            System.Diagnostics.Debug.WriteLine("[TimerService] Timer resumed");
        }

        /// <summary>
        /// Stops the timer completely and resets state
        /// </summary>
        public void Stop()
        {
            _isRunning = false;
            _timer.Stop();
            _startTime = null;
            _targetDuration = TimeSpan.Zero;
            System.Diagnostics.Debug.WriteLine("[TimerService] Timer stopped and reset");
        }

        /// <summary>
        /// Cleans up timer resources
        /// </summary>
        public void Dispose()
        {
            _timer.Dispose();
            System.Diagnostics.Debug.WriteLine("[TimerService] Disposed");
        }
    }
}
