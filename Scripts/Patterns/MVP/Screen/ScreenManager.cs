namespace GameFoundation.Scripts.Patterns.MVP.Screen
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Extenstions;
    using GameFoundation.Scripts.Patterns.MVP.Implementation;
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using UnityEngine;
    using VContainer;
    using ZLinq;

    public class ScreenManager : IScreenManager
    {
        #region Inject

        private readonly IViewFactory                 viewFactory;
        private readonly IObjectResolver              resolver;
        private readonly UICanvas                     uiCanvas;
        private readonly Dictionary<Type, IPresenter> presenters = new();

        [Inject]
        public ScreenManager(IViewFactory viewFactory, IObjectResolver resolver, UICanvas uiCanvas)
        {
            this.viewFactory = viewFactory;
            this.resolver    = resolver;
            this.uiCanvas    = uiCanvas;
        }

        #endregion

        public void ShowScreen<T>() where T : IPresenter
        {
            var presenterType = typeof(T);
            if (!this.presenters.TryGetValue(presenterType, out var presenter))
            {
                presenter = this.resolver.Resolve(presenterType) as IPresenter;
                if (presenter == null) throw new($"Could not resolve presenter of type {presenterType.Name}");
                this.presenters[presenterType] = presenter;
            }
            presenter.Open();
        }

        public void ShowScreen<T, TModel>(TModel model) where T : IPresenter
        {
            var presenterType = typeof(T);
            if (!this.presenters.TryGetValue(presenterType, out var presenter))
            {
                presenter = this.resolver.Resolve(presenterType) as IPresenter;

                if (presenter == null) throw new($"Could not resolve presenter of type {presenterType.Name}");
                this.presenters[presenterType] = presenter;
            }

            if (presenter is IPresenter<TModel> presenterWithModel)
                presenterWithModel.SetModel(model);
            presenter.Open();
        }

        public void HideScreen<T>() where T : IPresenter
        {
            var presenterType = typeof(T);

            if (this.presenters.TryGetValue(presenterType, out var presenter))
                presenter.Close();
            else
                Debug.LogWarning($"Attempting to hide screen {presenterType.Name} that was not shown.");
        }

        public void HideAllScreens()
        {
            foreach (var presenter in this.presenters.Values)
            {
                presenter.Close();
            }
        }

        public void HideAllScreens(PresenterType type)
        {
            foreach (var presenter in this.presenters.Values)
            {
                if(presenter.Type == type) presenter.Close();
            }
        }

        public T GetScreen<T>() where T : IPresenter
        {
            var presenterType = typeof(T);
            if (this.presenters.TryGetValue(presenterType, out var presenter)) return (T)presenter;
            return default;
            // presenter = this.resolver.Resolve(presenterType) as IPresenter;
            // if (presenter == null) throw new($"Could not resolve presenter of type {presenterType.Name}");
            // this.presenters[presenterType] = presenter;
            // presenter.Close();
            // return (T)presenter;
        }

        public bool IsScreenOpen<T>() where T : IPresenter
        {
            var presenterType = typeof(T);
            if (this.presenters.TryGetValue(presenterType, out var presenter)) return presenter?.IsOpen ?? false;
            return false;
        }

        public IEnumerable<IPresenter> GetAllScreens(bool onlyOpened = false)
        {
            return onlyOpened ? this.presenters.Values.AsValueEnumerable().Where(p => (p as BasePresenter<BaseView>)?.IsOpen ?? false).ToList() : this.presenters.Values;
        }

        public IEnumerable<IPresenter> GetAllScreens(PresenterType type, bool onlyOpened = false)
        {
            var result = new List<IPresenter>();
            foreach (var presenter in this.presenters.Values)
            {
                var presenterType = presenter.GetType();
                var isOfType = type switch
                {
                    PresenterType.Screen => presenterType.GetBaseTypes()
                        .AsValueEnumerable().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(ScreenPresenter<>)),
                    PresenterType.Popup => presenterType.GetBaseTypes()
                        .AsValueEnumerable().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(PopupPresenter<>)),
                    PresenterType.Overlay => presenterType.GetBaseTypes()
                        .AsValueEnumerable().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(OverlayPresenter<>)),
                    PresenterType.Splash => presenterType.GetBaseTypes()
                        .AsValueEnumerable().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(SplashPresenter<>)),
                    _ => false
                };
                if (isOfType && (!onlyOpened || (presenter as BasePresenter<BaseView>)?.IsOpen == true))
                    result.Add(presenter);
            }
            return result;
        }

        public void DestroyScreen<T>() where T : IPresenter
        {
            var presenterType = typeof(T);

            if (this.presenters.TryGetValue(presenterType, out var presenter))
            {
                presenter.Close();
                presenter.Destroy();
                this.presenters.Remove(presenterType);
            }
        }
    }
}