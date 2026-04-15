namespace GameFoundation.Scripts.Patterns.MVP.View
{
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public interface IView
    {
        void       Initialize();
        UniTask    Show(bool animate = true);
        UniTask    Hide(bool animate = true);
        GameObject GetGameObject();
    }
}
