using GroupThree.FocusTimerApp.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;

namespace GroupThree.FocusTimerApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ICommand StartCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand SwitchPhaseCommand { get; }

        private readonly DispatcherTimer _timer;
        private readonly TimerSetting _settings;
        private readonly TimerState _state;
        private readonly NotifyIcon _notifyIcon;

        //private int _reminderIntervalSeconds;
        //private int _shortBreakAfterSeconds;
        //private int _workDurationSeconds;

        private int _elapsedSeconds;
        private bool _isInBreak = false;

        public string CurrentPhase
        {
            get => _state.CurrentPhase;
            set { _state.CurrentPhase = value; OnPropertyChanged(); }
        }

        public bool IsRunning
        {
            get => _state.IsRunning;
            set { _state.IsRunning = value; OnPropertyChanged(); }
        }

        public int TimeLeft
        {
            get => (int)_state.TimeLeft.TotalSeconds;
            set { _state.TimeLeft = TimeSpan.FromSeconds(value); OnPropertyChanged(); }
        }

        public string TimeLeftDisplay => _state.TimeLeft.ToString(@"hh\:mm\:ss");

        // -------------------- TIMER --------------------
        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_state.TimeLeft.TotalSeconds > 0)
            {
                _state.TimeLeft = _state.TimeLeft.Subtract(TimeSpan.FromSeconds(1));
                if (!_isInBreak) _elapsedSeconds++;
                OnPropertyChanged(nameof(TimeLeftDisplay));
                HandlePhaseLogic();
            }
            else
            {
                EndCurrentPhase();
            }
        }

        private void HandlePhaseLogic()
        {
            if (CurrentPhase == TimerPhase.Basic.ToString())
            {
                // Nhắc mỗi ReminderInterval
                if (_elapsedSeconds > 0 && _elapsedSeconds % _settings.ReminderInterval == 0)
                {
                    ShowNotification($"You’ve worked for {_elapsedSeconds} seconds — take a short break!");
                }
            }
            else if (CurrentPhase == TimerPhase.Pomodoro.ToString())
            {
                // Short break
                if (_elapsedSeconds > 0 && _elapsedSeconds % _settings.ShortBreakAfter == 0)
                {
                    if (_state.CompletedShortBreaks >= _settings.LongBreakAfterShortBreakCount)
                    {
                        _state.CompletedShortBreaks = 0;
                        StartBreak(TimerPhase.LongBreak, _settings.LongBreak);

                    }
                    else
                    {
                        StartBreak(TimerPhase.ShortBreak, _settings.ShortBreak);

                    }
                    _state.CompletedShortBreaks++;
                }
            }
        }

        private void StartBreak(TimerPhase breakType, int duration)
        {
            _isInBreak = true;
            CurrentPhase = breakType.ToString();
            _state.TimeLeft = TimeSpan.FromSeconds(duration);
            //_state.TimeLeft = TimeSpan.FromMinutes(duration);
            OnPropertyChanged(nameof(TimeLeftDisplay));

            ShowNotification(breakType == TimerPhase.ShortBreak
                ? "Short break started!"
                : "Long break started!");

        }

        private void EndCurrentPhase()
        {
            _timer.Stop();

            if (_isInBreak)
            {
                // Kết thúc break
                _isInBreak = false;
                ShowNotification("Break over — time to focus!");
                if (CurrentPhase == TimerPhase.ShortBreak.ToString() || CurrentPhase == TimerPhase.LongBreak.ToString())
                {
                    CurrentPhase = TimerPhase.Pomodoro.ToString();
                    _state.TimeLeft = TimeSpan.FromSeconds(_settings.WorkDuration - _elapsedSeconds);
                    _timer.Start();
                }
            }
            else
            {
                // Kết thúc work session
                IsRunning = false;
                OnPropertyChanged(nameof(IsRunning));
                ((Commands.RelayCommand)StartCommand).RaiseCanExecuteChanged();
                ShowNotification("Session complete!");
            }
        }

        private void StartTimer()
        {
            IsRunning = true;
            _timer.Start();
            ((Commands.RelayCommand)StartCommand).RaiseCanExecuteChanged();
            ((Commands.RelayCommand)StopCommand).RaiseCanExecuteChanged();
            ShowNotification($"Timer started ({CurrentPhase})!");
        }

        private void StopTimer()
        {
            IsRunning = false;
            _timer.Stop();
            ((Commands.RelayCommand)StartCommand).RaiseCanExecuteChanged();
            ((Commands.RelayCommand)StopCommand).RaiseCanExecuteChanged();
        }

        private void ResetState()
        {
            _timer.Stop();
            IsRunning = false;
            _elapsedSeconds = 0;
            _state.CompletedShortBreaks = 0;
            _isInBreak = false;

            if (CurrentPhase == TimerPhase.Basic.ToString())
                _state.TimeLeft = TimeSpan.FromSeconds(_settings.WorkDuration);
                //_state.TimeLeft = TimeSpan.FromMinutes(_settings.WorkDuration);
            else
                _state.TimeLeft = TimeSpan.FromSeconds(_settings.WorkDuration);
                //_state.TimeLeft = TimeSpan.FromMinutes(_settings.WorkDuration);

            OnPropertyChanged(nameof(TimeLeftDisplay));
            ((Commands.RelayCommand)StartCommand).RaiseCanExecuteChanged();
            ((Commands.RelayCommand)StopCommand).RaiseCanExecuteChanged();
        }

        private void SwitchPhase()
        {
            if (CurrentPhase == TimerPhase.Basic.ToString())
                CurrentPhase = TimerPhase.Pomodoro.ToString();
            else
                CurrentPhase = TimerPhase.Basic.ToString();

            ResetState();
        }

        private void ShowNotification(string message)
        {
            _notifyIcon.BalloonTipTitle = "Focus Timer";
            _notifyIcon.BalloonTipText = message;
            _notifyIcon.ShowBalloonTip(3000);
        }

        public MainViewModel()
        {
            _settings = SettingManager.Load();

            //_workDurationSeconds = _settings.WorkDuration * 60;
            //_reminderIntervalSeconds = _settings.ReminderInterval * 60;
            //_shortBreakAfterSeconds = _settings.ShortBreakAfter * 60;


            _notifyIcon = new NotifyIcon()
            {
                Icon = SystemIcons.Information,
                Visible = true,
                Text = "Focus Timer"
            };

            _state = new TimerState
            {
                TimeLeft = TimeSpan.FromSeconds(_settings.WorkDuration)
                //TimeLeft = TimeSpan.FromMinutes(_settings.WorkDuration)
            };

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            _timer.Tick += Timer_Tick;

            StartCommand = new Commands.RelayCommand(_ => StartTimer(), _ => !_state.IsRunning);
            ResetCommand = new Commands.RelayCommand(_ => ResetState());
            StopCommand = new Commands.RelayCommand(_ => StopTimer(), _ => _state.IsRunning);
            SwitchPhaseCommand = new Commands.RelayCommand(_ => SwitchPhase());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
