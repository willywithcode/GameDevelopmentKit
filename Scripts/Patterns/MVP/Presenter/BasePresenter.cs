namespace GameFoundation.Scripts.Patterns.MVP.Presenter
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.MVP.Signals;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using GameFoundation.Scripts.Signals;
    using UnityEngine.UI;

    public abstract class BasePresenter<TView> : IPresenter where TView : BaseView
    {
        #region Inject

        protected readonly IViewFactory viewFactory;
        protected readonly SignalBus    signalBus;
        protected readonly UICanvas     uiCanvas;

        protected BasePresenter(
            IViewFactory viewFactory,
            SignalBus    signalBus,
            UICanvas     uiCanvas
        )
        {
            this.viewFactory = viewFactory;
            this.signalBus   = signalBus;
            this.uiCanvas    = uiCanvas;
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
            this.signalBus.Fire(new OpenPresenterSignal(this));
            this.OnBeforeShow().Forget();
            this.view.Show();
            this.OnAfterShow().Forget();
        }

        public virtual void Close()
        {
            this.signalBus.Fire(new HidePresenterSignal(this));
            this.OnBeforeHide().Forget();
            this.view.Hide();
            this.OnAfterHide().Forget();
        }

        public void Destroy()
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
                    this.signalBus.Fire<OnButtonClickSignal>(new());
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

        protected BasePresenter(IViewFactory viewFactory, SignalBus signalBus, UICanvas uiCanvas) : base(viewFactory, signalBus, uiCanvas)
        {
        }

        public void SetModel(TModel model)
        {
            this.model = model;
        }
    }
}