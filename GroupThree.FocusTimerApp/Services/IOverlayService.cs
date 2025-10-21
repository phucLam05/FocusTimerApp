namespace GroupThree.FocusTimerApp.Services
{
    public interface IOverlayService
    {
        void ToggleOverlay();
        void ShowOverlay();
        void HideOverlay();
        bool IsOverlayVisible { get; }
    }
}
