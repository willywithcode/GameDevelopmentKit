namespace GameFoundation.Scripts.Patterns.MVP.Screen
{
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using UnityEngine;

    public interface IScreenManager
    {
        void ShowScreen<T>() where T : IPresenter;
        void ShowScreen<T, TModel>(TModel model) where T : IPresenter;
        void HideScreen<T>() where T : IPresenter;
        void SetScreenParent(Transform parent);
    }
}