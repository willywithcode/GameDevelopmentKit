namespace GameFoundation.Scripts.Patterns.MVP.View
{
    using UnityEngine;

    public abstract class BaseView : MonoBehaviour, IView
    {
        protected bool isInitialized;

        public virtual void Initialize()
        {
            if (this.isInitialized) return;
            this.isInitialized = true;
        }

        public virtual void Show()
        {
            this.gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            this.gameObject.SetActive(false);
        }

        public GameObject GetGameObject()
        {
            return this.gameObject;
        }
    }
}