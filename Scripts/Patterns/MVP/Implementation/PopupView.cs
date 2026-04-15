namespace GameFoundation.Scripts.Patterns.MVP.Implementation
{
    using Cysharp.Threading.Tasks;
    using DG.Tweening;
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.Signals;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Signals;
    using MessagePipe;
    using UnityEngine;
    using UnityEngine.UI;

    public class PopupView : BaseView
    {
        [Header("Popup Animation")] [SerializeField] private RectTransform contentRoot;

        [SerializeField]                 private CanvasGroup panelCanvasGroup;
        [SerializeField]                 private Image       dimmer;
        [SerializeField] [Range(0f, 1f)] private float       dimmerTargetAlpha = 0.7f;
        [SerializeField]                 private float       targetScale       = 1f;
        [SerializeField]                 private float       showDuration      = 0.4f;
        [SerializeField]                 private float       hideDuration      = 0.3f;
        [SerializeField]                 private Ease        showEase          = Ease.OutBack;
        [SerializeField]                 private Ease        hideEase          = Ease.InBack;

        private Sequence activeSequence;

        private Transform ContentTransform => this.contentRoot ? this.contentRoot : this.transform;

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
            this.gameObject.SetActive(false);
        }

        public virtual void Show(bool animate)
        {
            this.KillActiveSequence();
            base.Show();

            if (!animate)
            {
                this.ApplyVisibleState();
                return;
            }

            this.ResetHiddenState();

            this.activeSequence = DOTween.Sequence();
            this.activeSequence.Join(this.ContentTransform.DOScale(Vector3.one * this.targetScale, this.showDuration)
                .SetEase(this.showEase)
                .SetUpdate(UpdateType.Normal, true));
            this.activeSequence.Join(this.PanelGroup.DOFade(1f, this.showDuration).SetUpdate(UpdateType.Normal, true));

            if (this.dimmer)
            {
                this.activeSequence.Join(this.dimmer.DOFade(this.dimmerTargetAlpha, this.showDuration)
                    .SetUpdate(UpdateType.Normal, true));
            }

            // this.SetInteraction(false);
            this.activeSequence.OnComplete(() =>
            {
                // this.SetInteraction(true);
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
                // this.SetInteraction(false);
                base.Hide();
                this.ResetHiddenState();
                return;
            }

            // this.SetInteraction(false);

            this.activeSequence = DOTween.Sequence();
            this.activeSequence.Join(this.ContentTransform.DOScale(Vector3.zero, this.hideDuration)
                .SetEase(this.hideEase)
                .SetUpdate(UpdateType.Normal, true));
            this.activeSequence.Join(this.PanelGroup.DOFade(0f, this.hideDuration).SetUpdate(UpdateType.Normal, true));

            if (this.dimmer)
            {
                this.activeSequence.Join(this.dimmer.DOFade(0f, this.hideDuration).SetUpdate(UpdateType.Normal, true));
            }

            this.activeSequence.OnComplete(() =>
            {
                base.Hide();
                this.ResetHiddenState();
                this.activeSequence = null;
            });
        }

        private void ResetHiddenState()
        {
            this.ContentTransform.localScale = Vector3.zero;
            this.PanelGroup.alpha            = 0f;

            if (this.dimmer)
            {
                var color = this.dimmer.color;
                color.a           = 0f;
                this.dimmer.color = color;
            }
        }

        private void ApplyVisibleState()
        {
            this.ContentTransform.localScale = Vector3.one * this.targetScale;
            this.PanelGroup.alpha            = 1f;

            if (this.dimmer)
            {
                var color = this.dimmer.color;
                color.a           = this.dimmerTargetAlpha;
                this.dimmer.color = color;
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

        protected virtual void OnDisable()
        {
            this.KillActiveSequence();
            this.ResetHiddenState();
            // this.SetInteraction(false);
        }
    }

    public class PopupPresenter<T> : BasePresenter<T> where T : PopupView
    {
        public override PresenterType Type => PresenterType.Popup;

        protected override UniTask OnBeforeShow()
        {
            this.OnShowWithAnim(true);
            return base.OnBeforeShow();
        }

        protected virtual void OnShowWithAnim(bool haveAnimation)
        {
            if (this.view == null) return;
            this.view.Show(haveAnimation);
            this.openPresenterPublisher.Publish(new OpenPresenterSignal(this));
        }

        protected virtual void OnHideWithAnim(bool haveAnimation)
        {
            if (this.view == null) return;
            this.view.Hide(haveAnimation);
            this.hidePresenterPublisher.Publish(new HidePresenterSignal(this));
        }

        public PopupPresenter(
            IViewFactory                  viewFactory,
            UICanvas                      uiCanvas,
            IPublisher<OpenPresenterSignal> openPresenterPublisher,
            IPublisher<HidePresenterSignal> hidePresenterPublisher,
            IPublisher<OnButtonClickSignal> buttonClickPublisher
        ) : base(viewFactory, uiCanvas, openPresenterPublisher, hidePresenterPublisher, buttonClickPublisher) { }
    }

    public class PopupPresenter<TView, TModel> : BasePresenter<TView, TModel> where TView : PopupView
    {
        public override PresenterType Type => PresenterType.Popup;

        protected override UniTask OnBeforeShow()
        {
            this.OnShowWithAnim(true);
            return base.OnBeforeShow();
        }

        protected virtual void OnShowWithAnim(bool haveAnimation)
        {
            if (this.view == null) return;
            this.view.Show(haveAnimation);
            this.openPresenterPublisher.Publish(new OpenPresenterSignal(this));
        }

        protected virtual void OnHideWithAnim(bool haveAnimation)
        {
            if (this.view == null) return;
            this.view.Hide(haveAnimation);
            this.hidePresenterPublisher.Publish(new HidePresenterSignal(this));
        }

        public PopupPresenter(
            IViewFactory                  viewFactory,
            UICanvas                      uiCanvas,
            IPublisher<OpenPresenterSignal> openPresenterPublisher,
            IPublisher<HidePresenterSignal> hidePresenterPublisher,
            IPublisher<OnButtonClickSignal> buttonClickPublisher
        ) : base(viewFactory, uiCanvas, openPresenterPublisher, hidePresenterPublisher, buttonClickPublisher) { }
    }
}