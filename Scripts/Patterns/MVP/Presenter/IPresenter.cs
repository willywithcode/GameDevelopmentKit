namespace GameFoundation.Scripts.Patterns.MVP.Presenter
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.MVP.View;

    public interface IPresenter
    {
        public PresenterType Type { get; }
        public bool          IsOpen { get; }
        public IView         View { get; }

        public void    Open();
        public void    Open(bool animate);
        public UniTask OpenAsync(bool animate = true);

        public void    Close();
        public void    Close(bool animate);
        public UniTask CloseAsync(bool animate = true);

        public void Destroy();
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
