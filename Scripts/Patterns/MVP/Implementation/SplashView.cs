namespace GameFoundation.Scripts.Patterns.MVP.Implementation
{
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Patterns.SignalBus;

    public class SplashView : BaseView
    {
    }

    public class SplashPresenter<T> : BasePresenter<T> where T : SplashView
    {
        public SplashPresenter(IViewFactory viewFactory, SignalBus signalBus, UICanvas uiCanvas) : base(viewFactory, signalBus, uiCanvas) { }
    }

    public class SplashPresenter<TView, TModel> : BasePresenter<TView, TModel> where TView : SplashView
    {
        public SplashPresenter(IViewFactory viewFactory, SignalBus signalBus, UICanvas uiCanvas, TModel model) : base(viewFactory, signalBus, uiCanvas, model) { }
    }
}