namespace GameFoundation.Scripts.Patterns.MVP.View
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Extenstions;
    using GameFoundation.Scripts.Patterns.MVP.Attribute;
    using GameFoundation.Scripts.Patterns.MVP.Implementation;
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using UnityEngine;
    using VContainer;
    using ZLinq;
    using Object = UnityEngine.Object;

    public interface IViewFactory
    {
        T    CreateView<T>(IPresenter typePresenter) where T : BaseView;
        T    CreateView<T>(Transform parent) where T : BaseView;
        void ReturnToPool<T>(T       view) where T : BaseView;
    }

    public class ViewFactory : IViewFactory
    {
        private readonly IAssetsManager                   assetsManager;
        private readonly IObjectResolver                  resolver;
        private readonly UICanvas                         uiCanvas;
        private readonly Dictionary<Type, string>         viewPathCache = new();
        private readonly Dictionary<Type, List<BaseView>> viewPool      = new();

        [Inject]
        public ViewFactory(IAssetsManager assetsManager, IObjectResolver resolver, UICanvas uiCanvas)
        {
            this.assetsManager = assetsManager;
            this.resolver      = resolver;
            this.uiCanvas      = uiCanvas;
        }

        public TView CreateView<TView>(IPresenter presenter) where TView : BaseView
        {
            var parentTransform = this.uiCanvas.transform;
            if (presenter.Type == PresenterType.Overlay)
            {
                parentTransform = this.uiCanvas.OverlayTransform;
            }
            else if (presenter.Type == PresenterType.Popup)
            {
                parentTransform = this.uiCanvas.PopupTransform;
            }
            else if (presenter.Type == PresenterType.Screen)
            {
                parentTransform = this.uiCanvas.ScreenTransform;
            }
            else if (presenter.Type == PresenterType.Splash)
            {
                parentTransform = this.uiCanvas.SplashTransform;
            }
            return this.CreateView<TView>(parentTransform);
        }

        public T CreateView<T>(Transform parent) where T : BaseView
        {
            var viewType = typeof(T);
            if (this.viewPool.TryGetValue(viewType, out var pooledViews) && pooledViews.Count > 0)
            {
                var baseView = (T)pooledViews[^1];
                pooledViews.RemoveAt(pooledViews.Count - 1);

                if (parent != null)
                    baseView.transform.SetParent(parent, false);

                return baseView;
            }
            if (!this.viewPathCache.TryGetValue(viewType, out var prefabPath))
            {
                var attribute = Attribute.GetCustomAttribute(viewType, typeof(ViewAttribute)) as ViewAttribute;
                if (attribute == null) throw new($"View {viewType.Name} does not have a ViewAttribute");

                prefabPath                   = attribute.PrefabPath;
                this.viewPathCache[viewType] = prefabPath;
            }
            var prefab = this.assetsManager.LoadAsset<GameObject>(prefabPath);
            if (prefab == null) throw new($"Failed to load prefab for view {viewType.Name} at path {prefabPath}");
            var instance = Object.Instantiate(prefab, parent);
            var view     = instance.GetComponent<T>();

            if (view == null) throw new($"Prefab at {prefabPath} does not have component of type {viewType.Name}");
            view.Initialize();

            return view;
        }

        public void ReturnToPool<T>(T view) where T : BaseView
        {
            var viewType = typeof(T);

            if (!this.viewPool.TryGetValue(viewType, out var pooledViews))
            {
                pooledViews             = new();
                this.viewPool[viewType] = pooledViews;
            }

            view.Hide();
            view.transform.SetParent(null);
            pooledViews.Add(view);
        }
    }
}