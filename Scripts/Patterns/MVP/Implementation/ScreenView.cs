namespace GameFoundation.Scripts.Patterns.MVP.Implementation
{
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Patterns.SignalBus;

    public class ScreenView : BaseView
    {
    }

    public class ScreenPresenter<T> : BasePresenter<T> where T : ScreenView
    {
        public ScreenPresenter(IViewFactory viewFactory, SignalBus signalBus, UICanvas uiCanvas) : base(viewFactory, signalBus, uiCanvas) { }

        public override PresenterType Type => PresenterType.Screen;
    }

    public class ScreenPresenter<TView, TModel> : BasePresenter<TView, TModel> where TView : ScreenView
    {

        public ScreenPresenter(IViewFactory viewFactory, SignalBus signalBus, UICanvas uiCanvas) : base(viewFactory, signalBus, uiCanvas) { }

        public override PresenterType Type => PresenterType.Screen;
    }
}