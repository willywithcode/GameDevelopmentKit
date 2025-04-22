namespace GameFoundation.Scripts.Patterns.MVP.Presenter
{
    using GameFoundation.Scripts.Patterns.MVP.View;

    public class BasePresenter<T> : IPresenter where T : IView
    {
        protected T view;
        public void SetView(IView view)
        {
            this.view = (T)view;
        }

        public virtual void Open()
        {
        }

        public virtual void Close()
        {
        }
    }
    public class BasePresenter<T,M> : IPresenter where T : IView where M : class
    {
        protected T view;
        protected M model;

        public void SetView(IView view)
        {
            this.view = (T)view;
        }

        public void SetModel(M model)
        {
            this.model = model;
        }

        public virtual void Open()
        {
        }

        public virtual void Close()
        {
        }
    }

}