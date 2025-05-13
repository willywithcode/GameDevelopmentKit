namespace GameFoundation.Scripts.Patterns.MVP.Presenter
{
    using GameFoundation.Scripts.Patterns.MVP.Signals;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using UnityEngine;

    public abstract class BasePresenter<TView> : IPresenter where TView : BaseView
    {
        #region Inject

        protected        IViewFactory viewFactory;
        private readonly SignalBus    signalBus;

        protected BasePresenter(
            IViewFactory viewFactory,
            SignalBus    signalBus
        )
        {
            this.viewFactory = viewFactory;
            this.signalBus   = signalBus;
        }

        #endregion

        protected TView     view;
        protected Transform parentTransform;

        public void SetParent(Transform parent)
        {
            this.parentTransform = parent;
        }

        public void SetView(IView view)
        {
            this.view = (TView)view;
        }

        public virtual void Open()
        {
            this.Bind();
            if (this.view == null)
            {
                this.view = this.viewFactory.CreateView<TView>(this.parentTransform);
                this.Ready();
            }
            this.signalBus.Fire(new OpenPresenterSignal(this));
            this.OnBeforeShow();
            this.view.Show();
            this.OnAfterShow();
        }

        public virtual void Close()
        {
            this.signalBus.Fire(new HidePresenterSignal(this));
            this.OnBeforeHide();
            this.view.Hide();
            this.OnAfterHide();
        }

        protected virtual void Bind()         { }
        protected virtual void Ready()        { }
        protected virtual void OnBeforeShow() { }
        protected virtual void OnAfterShow()  { }
        protected virtual void OnBeforeHide() { }
        protected virtual void OnAfterHide()  { }

        public void DestroyView()
        {
            if (this.view != null)
            {
                this.viewFactory.ReturnToPool(this.view);
                this.view = null;
            }
        }
    }

    public abstract class BasePresenter<TView, TModel> : BasePresenter<TView>
        where TView : BaseView
    {
        protected TModel model;

        protected BasePresenter(IViewFactory viewFactory, SignalBus signalBus) : base(viewFactory, signalBus)
        {
        }

        public void SetModel(TModel model)
        {
            this.model = model;
        }
    }
}