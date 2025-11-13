using GroupThree.FocusTimerApp.Services;
using System;
using System.Windows; // This using is still necessary for the Dispatcher

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class OverlayViewModel : ViewModelBase, IDisposable
    {
        private readonly TimerService? _timerService;

        private string _time = "00:00:00";
        public string Time { get => _time; set => SetProperty(ref _time, value); }

        private double _progress = 0;
        public double Progress { get => _progress; set => SetProperty(ref _progress, value); }

        public OverlayViewModel(TimerService? timerService)
        {
            _timerService = timerService;

            if (_timerService != null)
            {
                // Register to listen for the Tick event from TimerService
                _timerService.Tick += OnTimerTick;
            }

            // Set initial state
            Time = "00:00:00";
            Progress = 0;
        }

        private void OnTimerTick(object? sender, TimerTickEventArgs e)
        {
            // TimerService runs on a separate thread.
            // We must use the Dispatcher to update the UI (UI Thread)

            // FIX: Explicitly specify System.Windows.Application to avoid ambiguity
            System.Windows.Application.Current?.Dispatcher?.InvokeAsync(() =>
            {
                // This logic mirrors the logic from MainViewModel
                // to display countdown (Pomodoro) or count-up (Basic/Tracking)
                if (e.Mode == TimerMode.Pomodoro || e.Mode == TimerMode.ShortBreak || e.Mode == TimerMode.LongBreak)
                {
                    Time = e.Remaining.ToString(@"hh\:mm\:ss");
                }
                else // Basic (Tracking) mode
                {
                    Time = e.Elapsed.ToString(@"hh\:mm\:ss");
                }
                Progress = e.Progress;
            });
        }

        // Clean up subscription when no longer needed
        public void Dispose()
        {
            if (_timerService != null)
            {
                _timerService.Tick -= OnTimerTick;
            }
            GC.SuppressFinalize(this);
        }
    }
}