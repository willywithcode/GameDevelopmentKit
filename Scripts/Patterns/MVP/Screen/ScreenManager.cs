namespace GameFoundation.Scripts.Patterns.MVP.Screen
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Patterns.MVP.Attribute;
    using GameFoundation.Scripts.Patterns.MVP.Presenter;

    public class ScreenManager : IScreenManager
    {
        #region Inject

        private readonly IAssetsManager assetsManager;

        public ScreenManager(IAssetsManager assetsManager)
        {
            this.assetsManager = assetsManager;
        }

        #endregion

        private List<IPresenter> presenters;

        public void ShowScreen<T>() where T : IPresenter, new()
        {
            if (!this.Contains<T>())
            {
                var presenter = new T();
                var genericParameterType = typeof(T).GenericTypeArguments;
                if (genericParameterType.Length == 1)
                {
                    var attribute = Attribute.GetCustomAttribute(genericParameterType[0], typeof(ScreenAttribute)) as ScreenAttribute;
                    
                }
            }
            foreach (var presenter in this.presenters)
            {
                if (presenter is T)
                {
                    presenter.Open();
                }
                else
                {
                    presenter.Close();
                }
            }
        }

        public void HideScreen<T>() where T : IPresenter
        {
            foreach (var presenter in this.presenters.OfType<T>())
            {
                presenter.Close();
                return;
            }

            throw new Exception($"Presenter of type {typeof(T)} not found.");
        }

        private bool Contains<T>()
        {
            return this.presenters.OfType<T>().Any();
        }
    }
}