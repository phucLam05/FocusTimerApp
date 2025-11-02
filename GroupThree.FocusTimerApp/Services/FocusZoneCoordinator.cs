using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using GroupThree.FocusTimerApp.Services;

namespace GroupThree.FocusTimerApp.Services
{
    // Orchestrates focus-zone events: show notifications and control timer.
    // Pure service (no ViewModels), subscribes once via DI.
    public class FocusZoneCoordinator
    {
        private readonly AppFocusService _focusService;
        private readonly TimerService _timerService;
        private bool _notifyBusy;

        public FocusZoneCoordinator(AppFocusService focusService, TimerService timerService)
        {
            _focusService = focusService ?? throw new ArgumentNullException(nameof(focusService));
            _timerService = timerService ?? throw new ArgumentNullException(nameof(timerService));

            _focusService.EnteredWorkZone += OnEnteredWorkZone;
            _focusService.LeftWorkZone += OnLeftWorkZone;
        }

        private void OnEnteredWorkZone()
        {
            if (_notifyBusy) return;
            _notifyBusy = true;

            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                var notify = new NotifyIcon
                {
                    Icon = System.Drawing.SystemIcons.Information,
                    Visible = true,
                    BalloonTipTitle = "Focus Timer",
                    BalloonTipText = "Welcome back to work zone!"
                };
                notify.ShowBalloonTip(3000);

                if (!_timerService.IsRunning)
                    _timerService.Resume();

                Task.Delay(3500).ContinueWith(_ =>
                {
                    try { notify.Visible = false; notify.Dispose(); } catch { }
                });
            });

            Task.Delay(2000).ContinueWith(_ => _notifyBusy = false);
        }

        private void OnLeftWorkZone()
        {
            if (_notifyBusy) return;
            _notifyBusy = true;

            System.Windows.Application.Current?.Dispatcher?.Invoke(() =>
            {
                var notify = new NotifyIcon
                {
                    Icon = System.Drawing.SystemIcons.Warning,
                    Visible = true,
                    BalloonTipTitle = "Focus Timer",
                    BalloonTipText = "You have left the work zone!"
                };
                notify.ShowBalloonTip(3000);

                if (_timerService.IsRunning)
                    _timerService.Pause();

                Task.Delay(3500).ContinueWith(_ =>
                {
                    try { notify.Visible = false; notify.Dispose(); } catch { }
                });
            });

            Task.Delay(2000).ContinueWith(_ => _notifyBusy = false);
        }
    }
}
