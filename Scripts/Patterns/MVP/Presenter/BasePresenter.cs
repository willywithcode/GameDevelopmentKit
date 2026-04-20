namespace GameFoundation.Scripts.Patterns.MVP.Presenter
{
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.MVP.Signals;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Signals;
    using MessagePipe;

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

        private CancellationTokenSource lifetimeCts;

        /// <summary>
        /// Cancelled when the presenter is closed or destroyed. Use this token in awaits
        /// inside OnBeforeShow/OnAfterShow/OnBeforeHide/OnAfterHide to abort pending work
        /// when the user closes the view early.
        /// </summary>
        protected CancellationToken LifetimeToken => this.lifetimeCts?.Token ?? CancellationToken.None;

        public abstract PresenterType Type { get; }

        public bool  IsOpen => this.view != null && this.view.gameObject.activeInHierarchy;
        public IView View   => this.view;

        public virtual void Open()              => this.OpenAsync(true).Forget();
        public virtual void Open(bool animate)  => this.OpenAsync(animate).Forget();
        public virtual void Close()             => this.CloseAsync(true).Forget();
        public virtual void Close(bool animate) => this.CloseAsync(animate).Forget();

        public virtual async UniTask OpenAsync(bool animate = true)
        {
            this.CancelLifetime();
            this.lifetimeCts = new CancellationTokenSource();
            var ct = this.lifetimeCts.Token;

            if (this.view == null)
            {
                this.view = await this.viewFactory.CreateViewAsync<TView>(this);
                this.Ready();
            }

            this.Bind();
            this.view.transform.SetAsLastSibling();
            this.openPresenterPublisher.Publish(new OpenPresenterSignal(this));

            try
            {
                await this.OnBeforeShow();
                ct.ThrowIfCancellationRequested();
                await this.view.Show(animate);
                ct.ThrowIfCancellationRequested();
                await this.OnAfterShow();
            }
            catch (System.OperationCanceledException) { }
        }

        public virtual async UniTask CloseAsync(bool animate = true)
        {
            if (this.view == null) return;

            this.CancelLifetime();
            this.lifetimeCts = new CancellationTokenSource();
            var ct = this.lifetimeCts.Token;

            this.hidePresenterPublisher.Publish(new HidePresenterSignal(this));

            try
            {
                await this.OnBeforeHide();
                ct.ThrowIfCancellationRequested();
                await this.view.Hide(animate);
                ct.ThrowIfCancellationRequested();
                await this.OnAfterHide();
            }
            catch (System.OperationCanceledException) { }
        }

        public virtual void Destroy()
        {
            this.CancelLifetime();
            this.DestroyView();
        }

        private void CancelLifetime()
        {
            if (this.lifetimeCts == null) return;
            this.lifetimeCts.Cancel();
            this.lifetimeCts.Dispose();
            this.lifetimeCts = null;
        }

        protected virtual void Bind() { }

        protected virtual void Ready() { }

        protected virtual UniTask OnBeforeShow() => UniTask.CompletedTask;
        protected virtual UniTask OnAfterShow()  => UniTask.CompletedTask;
        protected virtual UniTask OnBeforeHide() => UniTask.CompletedTask;
        protected virtual UniTask OnAfterHide()  => UniTask.CompletedTask;

        public void DestroyView()
        {
            if (this.view != null)
            {
                this.viewFactory.ReturnToPool(this.view);
                this.view = null;
            }
        }
    }
}
