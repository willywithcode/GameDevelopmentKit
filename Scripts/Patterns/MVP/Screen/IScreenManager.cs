namespace GameFoundation.Scripts.Patterns.MVP.Screen
{
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.MVP.Presenter;

    public interface IScreenManager
    {
        void ShowScreen<T>() where T : IPresenter;
        void ShowScreen<T>(bool animate) where T : IPresenter;
        void ShowScreen<T, TModel>(TModel model) where T : IPresenter;
        void ShowScreen<T, TModel>(TModel model, bool animate) where T : IPresenter;

        UniTask ShowScreenAsync<T>(bool animate = true) where T : IPresenter;
        UniTask ShowScreenAsync<T, TModel>(TModel model, bool animate = true) where T : IPresenter;

        void    HideScreen<T>() where T : IPresenter;
        void    HideScreen<T>(bool animate) where T : IPresenter;
        UniTask HideScreenAsync<T>(bool animate = true) where T : IPresenter;

        void HideAllScreens();
        void HideAllScreens(PresenterType type);

        T                       GetScreen<T>() where T : IPresenter;
        bool                    IsScreenOpen<T>() where T : IPresenter;
        IEnumerable<IPresenter> GetAllScreens(bool          onlyOpened            = false);
        IEnumerable<IPresenter> GetAllScreens(PresenterType type, bool onlyOpened = false);
    }
}
