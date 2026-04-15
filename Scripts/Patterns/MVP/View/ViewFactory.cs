namespace GameFoundation.Scripts.Patterns.MVP.View
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
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
        UniTask<T> CreateViewAsync<T>(IPresenter typePresenter) where T : BaseView;
        UniTask<T> CreateViewAsync<T>(Transform  parent) where T : BaseView;
        void       ReturnToPool<T>(T             view) where T : BaseView;
    }

    public class ViewFactory : IViewFactory
    {
        private readonly IAssetsManager                   assetsManager;
        private readonly IObjectResolver                  resolver;
        private readonly UICanvas                         uiCanvas;
        private readonly Dictionary<Type, string>         viewPathCache    = new();
        private readonly Dictionary<Type, int>            viewMaxPoolCache = new();
        private readonly Dictionary<Type, List<BaseView>> viewPool         = new();

        [Inject]
        public ViewFactory(IAssetsManager assetsManager, IObjectResolver resolver, UICanvas uiCanvas)
        {
            this.assetsManager = assetsManager;
            this.resolver      = resolver;
            this.uiCanvas      = uiCanvas;
        }

        public UniTask<TView> CreateViewAsync<TView>(IPresenter presenter) where TView : BaseView
        {
            var parentTransform = presenter.Type switch
            {
                PresenterType.Overlay => this.uiCanvas.OverlayTransform,
                PresenterType.Popup   => this.uiCanvas.PopupTransform,
                PresenterType.Screen  => this.uiCanvas.ScreenTransform,
                PresenterType.Splash  => this.uiCanvas.SplashTransform,
                _                     => this.uiCanvas.transform,
            };
            return this.CreateViewAsync<TView>(parentTransform);
        }

        public async UniTask<T> CreateViewAsync<T>(Transform parent) where T : BaseView
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

                prefabPath                      = attribute.PrefabPath;
                this.viewPathCache[viewType]    = prefabPath;
                this.viewMaxPoolCache[viewType] = attribute.MaxPoolSize;
            }
            var prefab = await this.assetsManager.LoadAssetAsync<GameObject>(prefabPath);
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

            view.Hide(false).Forget();

            var maxPoolSize = this.viewMaxPoolCache.TryGetValue(viewType, out var cached) ? cached : 5;
            if (pooledViews.Count >= maxPoolSize)
            {
                Object.Destroy(view.gameObject);
                if (this.viewPathCache.TryGetValue(viewType, out var prefabPath))
                {
                    this.assetsManager.Release(prefabPath);
                }
                return;
            }

            view.transform.SetParent(null);
            pooledViews.Add(view);
        }
    }
}