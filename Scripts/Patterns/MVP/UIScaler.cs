namespace GameFoundation.Scripts.Patterns.MVP
{
    using UnityEngine;
    using UnityEngine.UI;

    [ExecuteAlways]
    public class UIScaler : MonoBehaviour
    {
        private void Awake()
        {
            this.CheckResolution();
        }

        private void CheckResolution()
        {
            var isTabletIpad = (float)(UnityEngine.Screen.height) / (float)(UnityEngine.Screen.width) <= 1.7f; // 1.65?? 1.7 or 1.73
            this.GetComponent<CanvasScaler>().matchWidthOrHeight = isTabletIpad ? 1 : 0;
        }

        #if UNITY_EDITOR
        private void Update()
        {
            this.CheckResolution();
        }
        #endif
    }

}