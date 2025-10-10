namespace GameFoundation.Scripts.Patterns.MVP.Implementation
{
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Patterns.SignalBus;

    public class PopupView : BaseView
    {
    }

    public class PopupPresenter<T> : BasePresenter<T> where T : PopupView
    {
        public PopupPresenter(IViewFactory viewFactory, SignalBus signalBus, UICanvas uiCanvas) : base(viewFactory, signalBus, uiCanvas) { }
    }

    public class PopupPresenter<TView, TModel> : BasePresenter<TView, TModel> where TView : PopupView
    {
        public PopupPresenter(IViewFactory viewFactory, SignalBus signalBus, UICanvas uiCanvas, TModel model) : base(viewFactory, signalBus, uiCanvas, model) { }
    }
}