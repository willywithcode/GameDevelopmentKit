namespace GameFoundation.Scripts.Features.Highlight.Services
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Features.Highlight.View;
    using GameFoundation.Scripts.Patterns.MVP.Screen;
    using UnityEngine;
    using UnityEngine.Events;
    using UnityEngine.UI;
    public class HighlightService
    {
        private readonly IScreenManager screenManager;
        public HighlightService(IScreenManager screenManager)
        {
            this.screenManager = screenManager;
        }

        private readonly List<(Canvas canvas, GraphicRaycaster raycaster, Button button, UnityAction callback)> highlightedObjects = new();

        public void ShowHighLight(List<GameObject> objects, UnityAction onClickOutside = null, UnityAction onClickInside = null)
        {
            this.HideHighLight();
            this.screenManager.ShowScreen<HighlightBlockSplashPresenter, HighlightSplashModel>(new(onClickOutside));
            foreach (var highlightedObject in objects)
            {
                var    canvas      = highlightedObject.GetComponent<Canvas>();
                Canvas addedCanvas = null;
                if (canvas == null)
                {
                    canvas      = highlightedObject.AddComponent<Canvas>();
                    addedCanvas = canvas;
                }
                canvas.overrideSorting = true;
                canvas.sortingOrder    = 1000;

                var              raycaster      = highlightedObject.GetComponent<GraphicRaycaster>();
                GraphicRaycaster addedRaycaster = null;
                if (raycaster == null)
                {
                    raycaster      = highlightedObject.AddComponent<GraphicRaycaster>();
                    addedRaycaster = raycaster;
                }

                var    button      = highlightedObject.GetComponent<Button>();
                Button addedButton = null;
                if (button == null)
                {
                    button      = highlightedObject.AddComponent<Button>();
                    addedButton = button;
                }

                UnityAction callback = () => onClickInside?.Invoke();
                button.onClick.AddListener(callback);
                this.highlightedObjects.Add((addedCanvas, addedRaycaster, addedButton, callback));
            }
        }
        public void HideHighLight()
        {
            this.screenManager.HideScreen<HighlightBlockSplashPresenter>();
            foreach (var (canvas, raycaster, button, callback) in this.highlightedObjects)
            {
                if (raycaster != null) Object.Destroy(raycaster);
                if (canvas != null) Object.Destroy(canvas);
                if (button != null)
                {
                    button.onClick.RemoveListener(callback);
                    Object.Destroy(button);
                }
            }
            this.highlightedObjects.Clear();
        }
    }
}