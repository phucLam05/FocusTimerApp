using System;

namespace GroupThree.FocusTimerApp.Services
{
    public interface IOverlayService
    {
        bool IsOverlayVisible { get; }
        void ShowOverlay();
        void HideOverlay();
        void ToggleOverlay();
    }
}
