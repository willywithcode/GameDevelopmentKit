namespace GameFoundation.Scripts.Patterns.MVP.Implementation
{
    using GameFoundation.Scripts.Patterns.MVP.Presenter;
    using GameFoundation.Scripts.Patterns.MVP.Signals;
    using GameFoundation.Scripts.Patterns.MVP.View;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using GameFoundation.Scripts.Signals;

    public class ScreenView : BaseView
    {
    }

    public class ScreenPresenter<T> : BasePresenter<T> where T : ScreenView
    {
        public ScreenPresenter(
            IViewFactory viewFactory,
            SignalBus    signalBus,
            UICanvas     uiCanvas
        ) : base(viewFactory, signalBus, uiCanvas) { }

        public override PresenterType Type => PresenterType.Screen;
    }

    public class ScreenPresenter<TView, TModel> : ScreenPresenter<TView>, IPresenter<TModel>
        where TView : ScreenView
    {
        protected TModel model;

        public ScreenPresenter(
            IViewFactory viewFactory,
            SignalBus    signalBus,
            UICanvas     uiCanvas
        ) : base(viewFactory, signalBus, uiCanvas) { }

        public void SetModel(TModel model) => this.model = model;
    }
}
