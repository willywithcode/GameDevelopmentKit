namespace GameFoundation.Scripts.Patterns.MVP.View
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    [RequireComponent(typeof(CanvasGroup))]
    public abstract class BaseView : MonoBehaviour, IView
    {
        protected bool        isInitialized;
        protected CanvasGroup canvasGroup;

        public CanvasGroup CanvasGroup
        {
            get
            {
                if (!this.canvasGroup)
                {
                    this.canvasGroup = this.GetComponent<CanvasGroup>();
                }
                return this.canvasGroup;
            }
        }

        public virtual void Initialize()
        {
            if (this.isInitialized) return;
            this.isInitialized = true;
        }

        public virtual UniTask Show(bool animate = true)
        {
            this.gameObject.SetActive(true);
            return UniTask.CompletedTask;
        }

        public virtual UniTask Hide(bool animate = true)
        {
            this.gameObject.SetActive(false);
            return UniTask.CompletedTask;
        }

        public GameObject GetGameObject()
        {
            return this.gameObject;
        }
    }
}
