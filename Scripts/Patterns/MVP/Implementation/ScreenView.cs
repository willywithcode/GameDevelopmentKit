namespace GameFoundation.Scripts.Patterns.MVP.Implementation
{
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.Signals;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Signals;
    using MessagePipe;

    public class ScreenView : BaseView
    {
    }

    public class ScreenPresenter<T> : BasePresenter<T> where T : ScreenView
    {
        public ScreenPresenter(
            IViewFactory                  viewFactory,
            UICanvas                      uiCanvas,
            IPublisher<OpenPresenterSignal> openPresenterPublisher,
            IPublisher<HidePresenterSignal> hidePresenterPublisher,
            IPublisher<OnButtonClickSignal> buttonClickPublisher
        ) : base(viewFactory, uiCanvas, openPresenterPublisher, hidePresenterPublisher, buttonClickPublisher) { }

        public override PresenterType Type => PresenterType.Screen;
    }

    public class ScreenPresenter<TView, TModel> : BasePresenter<TView, TModel> where TView : ScreenView
    {
        public ScreenPresenter(
            IViewFactory                  viewFactory,
            UICanvas                      uiCanvas,
            IPublisher<OpenPresenterSignal> openPresenterPublisher,
            IPublisher<HidePresenterSignal> hidePresenterPublisher,
            IPublisher<OnButtonClickSignal> buttonClickPublisher
        ) : base(viewFactory, uiCanvas, openPresenterPublisher, hidePresenterPublisher, buttonClickPublisher) { }

        public override PresenterType Type => PresenterType.Screen;
    }
}
