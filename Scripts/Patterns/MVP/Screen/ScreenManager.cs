namespace GameFoundation.Scripts.Patterns.MVP.Screen
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Extenstions;
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
        private readonly Dictionary<Type, IPresenter> presenters = new();
        private          Transform                    parentTransform;

        [Inject]
        public ScreenManager(IViewFactory viewFactory, IObjectResolver resolver)
        {
            this.viewFactory = viewFactory;
            this.resolver    = resolver;
        }

        #endregion

        public void SetScreenParent(Transform parent)
        {
            this.parentTransform = parent;
        }

        public void ShowScreen<T>() where T : IPresenter
        {
            var presenterType = typeof(T);
            if (!this.presenters.TryGetValue(presenterType, out var presenter))
            {
                presenter = this.resolver.Resolve(presenterType) as IPresenter;

                if (presenter == null) throw new($"Could not resolve presenter of type {presenterType.Name}");
                var isBasePresenter = presenter.GetType().GetBaseTypes()
                    .AsValueEnumerable().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(BasePresenter<>));
                if (isBasePresenter)
                {
                    var setParentMethod = presenter.GetType().GetMethod("SetParent", new[] { typeof(Transform) });
                    if (setParentMethod != null)
                    {
                        setParentMethod.Invoke(presenter, new object[] { this.parentTransform });
                    }
                }

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
                var isBasePresenter = presenter.GetType().GetBaseTypes()
                    .AsValueEnumerable().Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(BasePresenter<>));
                if (isBasePresenter)
                {
                    var setParentMethod = presenter.GetType().GetMethod("SetParent", new[] { typeof(Transform) });
                    if (setParentMethod != null)
                    {
                        setParentMethod.Invoke(presenter, new object[] { this.parentTransform });
                    }
                }

                this.presenters[presenterType] = presenter;
            }
            
            var setModelMethod = presenter.GetType().GetMethod("SetModel", new[] { typeof(TModel) });
            if (setModelMethod != null)
            {
                setModelMethod.Invoke(presenter, new object[] { model });
            }
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

        public void DestroyScreen<T>() where T : IPresenter
        {
            var presenterType = typeof(T);

            if (this.presenters.TryGetValue(presenterType, out var presenter))
            {
                presenter.Close();
                if (presenter is BasePresenter<BaseView> basePresenter) basePresenter.DestroyView();
                this.presenters.Remove(presenterType);
            }
        }
    }
}