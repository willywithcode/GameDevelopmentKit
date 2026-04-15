namespace GameFoundation.Scripts.Patterns.MVP.Signals
{
    using GameFoundation.Scripts.Patterns.MVP.Presenter;

    public readonly struct HidePresenterSignal
    {
        public IPresenter Presenter { get; }

        public HidePresenterSignal(IPresenter presenter)
        {
            this.Presenter = presenter;
        }
    }
}
