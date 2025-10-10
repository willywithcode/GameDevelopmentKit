namespace GameFoundation.Scripts.Patterns.MVP.Presenter
{
    using GameFoundation.Scripts.Patterns.MVP.View;

    public interface IPresenter
    {
        public void Open();
        public void Close();
        public void Destroy();
        public bool IsOpen { get; }
        public IView View { get; }

    }
    public enum PresenterType
    {
        Screen,
        Overlay,
        Popup,
        Splash
    }
}