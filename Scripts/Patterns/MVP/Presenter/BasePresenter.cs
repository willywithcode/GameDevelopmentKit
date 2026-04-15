namespace GameFoundation.Scripts.Patterns.MVP.Presenter
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.MVP.Signals;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Signals;
    using MessagePipe;
    using UnityEngine.UI;

    public abstract class BasePresenter<TView> : IPresenter where TView : BaseView
    {
        #region Inject

        protected readonly IViewFactory                  viewFactory;
        protected readonly UICanvas                      uiCanvas;
        protected readonly IPublisher<OpenPresenterSignal> openPresenterPublisher;
        protected readonly IPublisher<HidePresenterSignal> hidePresenterPublisher;
        protected readonly IPublisher<OnButtonClickSignal> buttonClickPublisher;

        protected BasePresenter(
            IViewFactory                  viewFactory,
            UICanvas                      uiCanvas,
            IPublisher<OpenPresenterSignal> openPresenterPublisher,
            IPublisher<HidePresenterSignal> hidePresenterPublisher,
            IPublisher<OnButtonClickSignal> buttonClickPublisher
        )
        {
            this.viewFactory            = viewFactory;
            this.uiCanvas               = uiCanvas;
            this.openPresenterPublisher = openPresenterPublisher;
            this.hidePresenterPublisher = hidePresenterPublisher;
            this.buttonClickPublisher   = buttonClickPublisher;
        }

        #endregion

        protected TView view;

        public abstract PresenterType Type { get; }

        public virtual void Open()
        {
            if (this.view == null)
            {
                this.view = this.viewFactory.CreateView<TView>(this);
                this.Ready();
            }
            this.Bind();
            this.view.transform.SetAsLastSibling();
            this.openPresenterPublisher.Publish(new OpenPresenterSignal(this));
            this.OnBeforeShow().Forget();
            this.view.Show();
            this.OnAfterShow().Forget();
        }

        public virtual void Close()
        {
            this.hidePresenterPublisher.Publish(new HidePresenterSignal(this));
            this.OnBeforeHide().Forget();
            this.view.Hide();
            this.OnAfterHide().Forget();
        }

        public virtual void Destroy()
        {
            this.DestroyView();
        }

        public bool  IsOpen => this.view != null && this.view.gameObject.activeInHierarchy;
        public IView View   => this.view;

        protected virtual void Bind() { }

        protected virtual void Ready()
        {
            this.AssignButtonClickEffect();
        }

        protected virtual async UniTask OnBeforeShow() { }
        protected virtual async UniTask OnAfterShow()  { }
        protected virtual async UniTask OnBeforeHide() { }
        protected virtual async UniTask OnAfterHide()  { }

        private void AssignButtonClickEffect()
        {
            var buttons = this.view.GetComponentsInChildren<Button>(true);
            foreach (var button in buttons)
            {
                button.onClick.AddListener(() =>
                {
                    this.buttonClickPublisher.Publish(new OnButtonClickSignal());
                });
            }
        }

        public void DestroyView()
        {
            if (this.view != null)
            {
                this.viewFactory.ReturnToPool(this.view);
                this.view = null;
            }
        }
    }

    public abstract class BasePresenter<TView, TModel> : BasePresenter<TView>, IPresenter<TModel>
        where TView : BaseView
    {
        protected TModel model;

        protected BasePresenter(
            IViewFactory                  viewFactory,
            UICanvas                      uiCanvas,
            IPublisher<OpenPresenterSignal> openPresenterPublisher,
            IPublisher<HidePresenterSignal> hidePresenterPublisher,
            IPublisher<OnButtonClickSignal> buttonClickPublisher
        ) : base(viewFactory, uiCanvas, openPresenterPublisher, hidePresenterPublisher, buttonClickPublisher) { }

        public void SetModel(TModel model)
        {
            this.model = model;
        }
    }
}
