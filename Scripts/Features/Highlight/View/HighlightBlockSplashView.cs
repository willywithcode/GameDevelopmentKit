namespace GameFoundation.Scripts.Features.Highlight.View
{
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.MVP;
    using GameFoundation.Scripts.Patterns.MVP.Attribute;
    using GameFoundation.Scripts.Patterns.MVP.Implementation;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using UnityEngine.Events;
    using UnityEngine.UI;


    [View(nameof(HighlightBlockSplashView))]
    public class HighlightBlockSplashView : SplashView
    {
        public Button TapOutsideButton;
    }

    public class HighlightSplashModel
    {
        public readonly UnityAction OnTapOutside;
        public HighlightSplashModel(UnityAction onTapOutside)
        {
            this.OnTapOutside = onTapOutside;
        }
    }

    [Presenter(isSingleton: true)]
    public class HighlightBlockSplashPresenter : SplashPresenter<HighlightBlockSplashView, HighlightSplashModel>
    {
        public HighlightBlockSplashPresenter(
            IViewFactory viewFactory,
            SignalBus    signalBus,
            UICanvas     uiCanvas
        ) : base(viewFactory, signalBus, uiCanvas) { }

        protected override void Ready()
        {
            base.Ready();
            this.view.TapOutsideButton.onClick.AddListener(() =>
            {
                this.model.OnTapOutside?.Invoke();
            });
        }
        protected override UniTask OnBeforeShow()
        {
            this.OnShow(false).Forget();
            return base.OnBeforeShow();
        }
    }
}