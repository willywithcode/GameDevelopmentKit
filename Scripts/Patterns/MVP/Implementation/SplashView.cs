namespace GameFoundation.Scripts.Patterns.MVP.Implementation
{
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;

    public class SplashView : BaseView
    {
        [Header("Splash Animation")] [SerializeField] private RectTransform contentRoot;

        [SerializeField]                 private CanvasGroup panelCanvasGroup;
        [SerializeField]                 private Image       backgroundImage;
        [SerializeField] [Range(0f, 1f)] private float       backgroundTargetAlpha = 1f;
        [SerializeField]                 private float       hiddenScale            = 0.9f;
        [SerializeField]                 private float       targetScale            = 1f;
        [SerializeField]                 private float       showDuration           = 0.75f;
        [SerializeField]                 private float       hideDuration           = 0.6f;
        [SerializeField]                 private Ease        showEase               = Ease.OutCubic;
        [SerializeField]                 private Ease        hideEase               = Ease.InCubic;

        [Header("Events")] [SerializeField] private UnityEvent onSplashShown;
        [SerializeField]                    private UnityEvent onSplashHidden;

        private Sequence                activeSequence;
        private CancellationTokenSource loadingCts;
        private Transform               ContentTransform => this.contentRoot ? this.contentRoot : this.transform;

        private CanvasGroup PanelGroup
        {
            get
            {
                if (!this.panelCanvasGroup)
                {
                    this.panelCanvasGroup = this.CanvasGroup;
                }

                return this.panelCanvasGroup;
            }
        }

        public override void Initialize()
        {
            base.Initialize();

            if (!this.contentRoot && this.transform is RectTransform rectTransform)
            {
                this.contentRoot = rectTransform;
            }

            this.ResetHiddenState();
            this.SetInteraction(false);
            this.gameObject.SetActive(false);
        }
        public virtual void Show(bool animate)
        {
            this.KillActiveSequence();
            base.Show();

            if (!animate)
            {
                this.ApplyVisibleState();
                this.onSplashShown?.Invoke();
                return;
            }

            this.ResetHiddenState();

            this.activeSequence = DOTween.Sequence();
            this.activeSequence.Join(this.ContentTransform.DOScale(Vector3.one * this.targetScale, this.showDuration)
                .SetEase(this.showEase)
                .SetUpdate(UpdateType.Normal, true));
            this.activeSequence.Join(this.PanelGroup.DOFade(1f, this.showDuration).SetUpdate(UpdateType.Normal, true));

            if (this.backgroundImage)
            {
                this.activeSequence.Join(this.backgroundImage.DOFade(this.backgroundTargetAlpha, this.showDuration)
                    .SetUpdate(UpdateType.Normal, true));
            }

            this.SetInteraction(false);
            this.activeSequence.OnComplete(() =>
            {
                this.SetInteraction(true);
                this.onSplashShown?.Invoke();
                this.activeSequence = null;
            });
        }

        public virtual void Hide(bool animate)
        {
            this.KillActiveSequence();

            if (!this.gameObject.activeSelf)
            {
                return;
            }

            if (!animate)
            {
                this.CancelLoadingOperation();
                this.SetInteraction(false);
                base.Hide();
                this.ResetHiddenState();
                this.onSplashHidden?.Invoke();
                return;
            }

            this.SetInteraction(false);

            this.activeSequence = DOTween.Sequence();
            this.activeSequence.Join(this.ContentTransform.DOScale(Vector3.one * this.hiddenScale, this.hideDuration)
                .SetEase(this.hideEase)
                .SetUpdate(UpdateType.Normal, true));
            this.activeSequence.Join(this.PanelGroup.DOFade(0f, this.hideDuration).SetUpdate(UpdateType.Normal, true));

            if (this.backgroundImage)
            {
                this.activeSequence.Join(this.backgroundImage.DOFade(0f, this.hideDuration)
                    .SetUpdate(UpdateType.Normal, true));
            }

            this.activeSequence.OnComplete(() =>
            {
                this.CancelLoadingOperation();
                base.Hide();
                this.ResetHiddenState();
                this.onSplashHidden?.Invoke();
                this.activeSequence = null;
            });
        }

        public async UniTask PlaySplashAsync(Func<CancellationToken, UniTask> loadOperation, bool animateIn = true, bool animateOut = true)
        {
            this.CancelLoadingOperation();

            this.Show(animateIn);
            Sequence showSequence = animateIn ? this.activeSequence : null;

            if (showSequence != null)
            {
                await showSequence.AsyncWaitForCompletion();
            }

            if (loadOperation != null)
            {
                this.loadingCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());

                try
                {
                    await loadOperation(this.loadingCts.Token);
                }
                finally
                {
                    this.loadingCts.Dispose();
                    this.loadingCts = null;
                }
            }

            this.Hide(animateOut);
            Sequence hideSequence = animateOut ? this.activeSequence : null;

            if (hideSequence != null)
            {
                await hideSequence.AsyncWaitForCompletion();
            }
        }

        public void CancelLoadingOperation()
        {
            if (this.loadingCts == null) return;

            this.loadingCts.Cancel();
            this.loadingCts.Dispose();
            this.loadingCts = null;
        }

        private void ResetHiddenState()
        {
            this.ContentTransform.localScale = Vector3.one * this.hiddenScale;
            this.PanelGroup.alpha            = 0f;

            if (this.backgroundImage)
            {
                var color = this.backgroundImage.color;
                color.a                   = 0f;
                this.backgroundImage.color = color;
            }

        }

        private void ApplyVisibleState()
        {
            this.ContentTransform.localScale = Vector3.one * this.targetScale;
            this.PanelGroup.alpha            = 1f;

            if (this.backgroundImage)
            {
                var color = this.backgroundImage.color;
                color.a                   = this.backgroundTargetAlpha;
                this.backgroundImage.color = color;
            }

            this.SetInteraction(true);
        }

        private void SetInteraction(bool enable)
        {
            var group = this.PanelGroup;
            group.interactable   = enable;
            group.blocksRaycasts = enable;
        }

        private void KillActiveSequence()
        {
            if (this.activeSequence == null) return;

            this.activeSequence.Kill();
            this.activeSequence = null;
        }

        private void OnDisable()
        {
            this.KillActiveSequence();
            this.CancelLoadingOperation();
            this.ResetHiddenState();
            this.SetInteraction(false);
        }
    }

    public class SplashPresenter<T> : BasePresenter<T> where T : SplashView
    {
        public SplashPresenter(IViewFactory viewFactory, SignalBus signalBus, UICanvas uiCanvas) : base(viewFactory, signalBus, uiCanvas) { }

        protected void OnShow(bool haveAnimation)
        {
            if (this.view == null) return;
            this.view.Show(haveAnimation);
        }

        protected void OnHide(bool haveAnimation)
        {
            if (this.view == null) return;
            this.view.Hide(haveAnimation);
        }

        protected UniTask PlaySplashAsync(Func<CancellationToken, UniTask> loadOperation, bool animateIn = true, bool animateOut = true)
        {
            if (this.view == null) return UniTask.CompletedTask;
            return this.view.PlaySplashAsync(loadOperation, animateIn, animateOut);
        }

        protected void CancelLoading()
        {
            if (this.view == null) return;
            this.view.CancelLoadingOperation();
        }
    }

    public class SplashPresenter<TView, TModel> : BasePresenter<TView, TModel> where TView : SplashView
    {
        public SplashPresenter(IViewFactory viewFactory, SignalBus signalBus, UICanvas uiCanvas, TModel model) : base(viewFactory, signalBus, uiCanvas, model) { }

        protected void OnShow(bool haveAnimation)
        {
            if (this.view == null) return;
            this.view.Show(haveAnimation);
        }

        protected void OnHide(bool haveAnimation)
        {
            if (this.view == null) return;
            this.view.Hide(haveAnimation);
        }

        protected UniTask PlaySplashAsync(Func<CancellationToken, UniTask> loadOperation, bool animateIn = true, bool animateOut = true)
        {
            if (this.view == null) return UniTask.CompletedTask;
            return this.view.PlaySplashAsync(loadOperation, animateIn, animateOut);
        }

        protected void CancelLoading()
        {
            if (this.view == null) return;
            this.view.CancelLoadingOperation();
        }
    }
}