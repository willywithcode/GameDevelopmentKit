namespace GameFoundation.Scripts.Patterns.MVP.View
{
    using UnityEngine;

    public interface IView
    {
        void Initialize();
        void Show();
        void Hide();
        GameObject GetGameObject();
    }
}