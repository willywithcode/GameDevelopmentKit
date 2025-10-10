namespace GameFoundation.Scripts.Patterns.MVP.Implementation
{
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Patterns.SignalBus;

    public class OverlayView : BaseView
    {
    }

    public class OverlayPresenter<T> : BasePresenter<T> where T : OverlayView
    {
        public OverlayPresenter(IViewFactory viewFactory, SignalBus signalBus, UICanvas uiCanvas) : base(viewFactory, signalBus, uiCanvas) { }
    }

    public class OverlayPresenter<TView, TModel> : BasePresenter<TView, TModel> where TView : OverlayView
    {
        public OverlayPresenter(IViewFactory viewFactory, SignalBus signalBus, UICanvas uiCanvas, TModel model) : base(viewFactory, signalBus, uiCanvas, model) { }
    }
}