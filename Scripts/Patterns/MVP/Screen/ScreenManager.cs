namespace GameFoundation.Scripts.Patterns.MVP.Screen
{
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using IGameLogger = GameFoundation.Scripts.Features.Logger.Services.ILogger;
    using LoggerService = GameFoundation.Scripts.Features.Logger.Services.LoggerService;
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using VContainer;

    public class ScreenManager : IScreenManager
    {
        #region Inject

        private readonly IViewFactory                 viewFactory;
        private readonly IObjectResolver              resolver;
        private readonly UICanvas                     uiCanvas;
        private readonly IGameLogger                  logger;
        private readonly Dictionary<Type, IPresenter> presenters = new();

        [Inject]
        public ScreenManager(IViewFactory viewFactory, IObjectResolver resolver, UICanvas uiCanvas, IGameLogger logger = null)
        {
            this.viewFactory = viewFactory;
            this.resolver    = resolver;
            this.uiCanvas    = uiCanvas;
            this.logger      = logger ?? new LoggerService();
        }

        #endregion

        public void ShowScreen<T>() where T : IPresenter => this.ShowScreen<T>(true);

        public void ShowScreen<T>(bool animate) where T : IPresenter
        {
            this.ResolveOrGet<T>().Open(animate);
        }

        public void ShowScreen<T, TModel>(TModel model) where T : IPresenter => this.ShowScreen<T, TModel>(model, true);

        public void ShowScreen<T, TModel>(TModel model, bool animate) where T : IPresenter
        {
            var presenter = this.ResolveOrGet<T>();
            if (presenter is IPresenter<TModel> presenterWithModel)
                presenterWithModel.SetModel(model);
            presenter.Open(animate);
        }

        public UniTask ShowScreenAsync<T>(bool animate = true) where T : IPresenter
        {
            return this.ResolveOrGet<T>().OpenAsync(animate);
        }

        public UniTask ShowScreenAsync<T, TModel>(TModel model, bool animate = true) where T : IPresenter
        {
            var presenter = this.ResolveOrGet<T>();
            if (presenter is IPresenter<TModel> presenterWithModel)
                presenterWithModel.SetModel(model);
            return presenter.OpenAsync(animate);
        }

        public void HideScreen<T>() where T : IPresenter => this.HideScreen<T>(true);

        public void HideScreen<T>(bool animate) where T : IPresenter
        {
            var presenterType = typeof(T);

            if (this.presenters.TryGetValue(presenterType, out var presenter))
                presenter.Close(animate);
            else
                this.logger.Warning($"Attempting to hide screen {presenterType.Name} that was not shown.");
        }

        public UniTask HideScreenAsync<T>(bool animate = true) where T : IPresenter
        {
            var presenterType = typeof(T);

            if (this.presenters.TryGetValue(presenterType, out var presenter))
                return presenter.CloseAsync(animate);

            this.logger.Warning($"Attempting to hide screen {presenterType.Name} that was not shown.");
            return UniTask.CompletedTask;
        }

        private IPresenter ResolveOrGet<T>() where T : IPresenter
        {
            var presenterType = typeof(T);
            if (!this.presenters.TryGetValue(presenterType, out var presenter))
            {
                presenter = this.resolver.Resolve(presenterType) as IPresenter;
                if (presenter == null) throw new($"Could not resolve presenter of type {presenterType.Name}");
                this.presenters[presenterType] = presenter;
            }
            return presenter;
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
            foreach (var presenter in this.presenters.Values)
            {
                if (!onlyOpened || presenter.IsOpen)
                    yield return presenter;
            }
        }

        public IEnumerable<IPresenter> GetAllScreens(PresenterType type, bool onlyOpened = false)
        {
            foreach (var presenter in this.presenters.Values)
            {
                if (presenter.Type != type) continue;
                if (onlyOpened && !presenter.IsOpen) continue;
                yield return presenter;
            }
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
