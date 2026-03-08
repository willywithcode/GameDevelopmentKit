namespace GameFoundation.Scripts.Patterns.MVP.Presenter
{
    using GameFoundation.Scripts.Patterns.MVP.View;

    public interface IPresenter
    {
        public PresenterType Type { get; }
        public void Open();
        public void Close();
        public void Destroy();
        public bool IsOpen { get; }
        public IView View { get; }

    }
    public interface IPresenter<TModel> : IPresenter 
    {
        public void SetModel(TModel model);
    }
    public enum PresenterType
    {
        Screen,
        Overlay,
        Popup,
        Splash
    }
}