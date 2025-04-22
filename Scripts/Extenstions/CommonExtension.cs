namespace GameFoundation.Scripts.Extenstions
{
    using UnityEngine;

    public static class CommonExtension
    {
        public static Vector3 GetLocalSize(this Renderer renderer)
        {
            var worldSize  = renderer.bounds.size;
            var localScale = renderer.transform.lossyScale;
            return new(worldSize.x / localScale.x, worldSize.y / localScale.y, worldSize.z / localScale.z);
        }
    }
}