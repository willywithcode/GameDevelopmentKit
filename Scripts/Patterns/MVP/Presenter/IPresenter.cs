namespace GameFoundation.Scripts.Patterns.MVP.Presenter
{
    using GameFoundation.Scripts.Patterns.MVP.View;

    public interface IPresenter
    {
        public void SetView(IView view);
        public void Open();
        public void Close();
    }
}