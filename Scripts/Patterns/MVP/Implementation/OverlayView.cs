namespace GameFoundation.Scripts.Patterns.MVP.Implementation
{
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.Signals;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Signals;
    using MessagePipe;

    public class OverlayView : BaseView
    {
    }

    public class OverlayPresenter<T> : BasePresenter<T> where T : OverlayView
    {
        public OverlayPresenter(
            IViewFactory                  viewFactory,
            UICanvas                      uiCanvas,
            IPublisher<OpenPresenterSignal> openPresenterPublisher,
            IPublisher<HidePresenterSignal> hidePresenterPublisher,
            IPublisher<OnButtonClickSignal> buttonClickPublisher
        ) : base(viewFactory, uiCanvas, openPresenterPublisher, hidePresenterPublisher, buttonClickPublisher) { }

        public override PresenterType Type => PresenterType.Overlay;
    }

    public class OverlayPresenter<TView, TModel> : OverlayPresenter<TView>, IPresenter<TModel>
        where TView : OverlayView
    {
        protected TModel model;

        public OverlayPresenter(
            IViewFactory                  viewFactory,
            UICanvas                      uiCanvas,
            IPublisher<OpenPresenterSignal> openPresenterPublisher,
            IPublisher<HidePresenterSignal> hidePresenterPublisher,
            IPublisher<OnButtonClickSignal> buttonClickPublisher
        ) : base(viewFactory, uiCanvas, openPresenterPublisher, hidePresenterPublisher, buttonClickPublisher) { }

        public void SetModel(TModel model) => this.model = model;
    }
}
