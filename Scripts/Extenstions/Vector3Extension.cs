namespace GameFoundation.Scripts.Extenstions
{
    using UnityEngine;

    public static class Vector3Extension
    {
        public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
        {
            return new(x ?? vector.x, y ?? vector.y, z ?? vector.z);
        }
    }
}