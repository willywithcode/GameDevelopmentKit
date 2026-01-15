namespace GameFoundation.Scripts.Utils
{
    using UnityEngine;

    public static class TransformExtensions
    {
        public static Vector3 SetX(this Transform transform, float x)
        {
            var position = transform.position;
            position.x         = x;
            transform.position = position;

            return position;
        }

        public static Vector3 SetY(this Transform transform, float y)
        {
            var position = transform.position;
            position.y         = y;
            transform.position = position;

            return position;
        }

        public static Vector3 SetZ(this Transform transform, float z)
        {
            var position = transform.position;
            position.z         = z;
            transform.position = position;

            return position;
        }

        public static Vector2 SetX(this RectTransform transform, float x)
        {
            var position = transform.anchoredPosition;
            position.x                 = x;
            transform.anchoredPosition = position;

            return position;
        }

        public static Vector2 SetY(this RectTransform transform, float y)
        {
            var position = transform.anchoredPosition;
            position.y                 = y;
            transform.anchoredPosition = position;

            return position;
        }
    }
}