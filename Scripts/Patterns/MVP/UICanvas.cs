namespace GameFoundation.Scripts.Patterns.MVP
{
    using UnityEngine;

    public class UICanvas : MonoBehaviour
    {
        [SerializeField] private RectTransform screenTransform;
        [SerializeField] private RectTransform popupTransform;
        [SerializeField] private RectTransform overlayTransform;
        [SerializeField] private RectTransform splashTransform;

        public RectTransform ScreenTransform  => this.screenTransform;
        public RectTransform PopupTransform   => this.popupTransform;
        public RectTransform OverlayTransform => this.overlayTransform;
        public RectTransform SplashTransform  => this.splashTransform;
    }
}