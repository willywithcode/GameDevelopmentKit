namespace GameFoundation.Scripts.Patterns.MVP.Signals
{
    using GameFoundation.Scripts.Patterns.MVP.Presenter;

    public readonly struct OpenPresenterSignal
    {
        public IPresenter Presenter { get; }

        public OpenPresenterSignal(IPresenter presenter)
        {
            this.Presenter = presenter;
        }
    }
}
