namespace GameFoundation.Scripts.Patterns.MVP.Screen
{
    using GameFoundation.Scripts.Patterns.MVP.Presenter;

    public interface IScreenManager
    {
        public void ShowScreen<T>() where T : IPresenter, new();
        public void HideScreen<T>() where T : IPresenter;
    }
}