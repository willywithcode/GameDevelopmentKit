namespace GameFoundation.Scripts.Utils
{
    using UnityEngine;
    public static class CameraExtensions
    {
        private static readonly Vector3[] WorldCornersCache = new Vector3[4];

        public static void FitCamera(Camera camera, Padding padding, Bounds targetBounds)
        {
            if (camera == null)
            {
                return;
            }

            if (!camera.orthographic)
            {
                camera.orthographic = true;
            }

            padding ??= new();

            var paddingValues   = ResolveNormalizedPadding(camera, padding);
            var leftPad         = paddingValues.x;
            var rightPad        = paddingValues.y;
            var topPad          = paddingValues.z;
            var bottomPad       = paddingValues.w;
            var horizontalSpace = Mathf.Clamp01(1f - (leftPad + rightPad));
            var verticalSpace   = Mathf.Clamp01(1f - (topPad + bottomPad));

            horizontalSpace = Mathf.Max(horizontalSpace, 0.01f);
            verticalSpace   = Mathf.Max(verticalSpace, 0.01f);

            var boundsSize = targetBounds.size;
            var halfHeight = Mathf.Max(boundsSize.y * 0.5f / verticalSpace, 0.01f);
            var aspect     = Mathf.Max(camera.aspect, 0.01f);
            var halfWidth  = Mathf.Max(boundsSize.x * 0.5f / (horizontalSpace * aspect), 0.01f);
            camera.orthographicSize = Mathf.Max(halfHeight, halfWidth, 0.01f);

            var viewHeight        = camera.orthographicSize * 2f;
            var viewWidth         = viewHeight * aspect;
            var offsetXNormalized = (leftPad - rightPad) * 0.5f;
            var offsetYNormalized = (bottomPad - topPad) * 0.5f;
            var targetCenter      = targetBounds.center;
            var position          = camera.transform.position;
            position.x                = targetCenter.x + viewWidth * offsetXNormalized;
            position.y                = targetCenter.y + viewHeight * offsetYNormalized;
            camera.transform.position = position;
        }

        private static Vector4 ResolveNormalizedPadding(Camera fallbackCamera, Padding padding)
        {
            var referenceCamera = padding.referenceCamera != null ? padding.referenceCamera : fallbackCamera;

            float pixelWidth = referenceCamera != null && referenceCamera.pixelWidth > 0
                ? referenceCamera.pixelWidth
                : Screen.width;
            float pixelHeight = referenceCamera != null && referenceCamera.pixelHeight > 0
                ? referenceCamera.pixelHeight
                : Screen.height;
            pixelWidth  = Mathf.Max(1f, pixelWidth);
            pixelHeight = Mathf.Max(1f, pixelHeight);

            var leftPadPx   = Mathf.Max(0f, padding.left);
            var rightPadPx  = Mathf.Max(0f, padding.right);
            var topPadPx    = Mathf.Max(0f, padding.top);
            var bottomPadPx = Mathf.Max(0f, padding.bottom);

            var leftPad   = Mathf.Clamp01(leftPadPx / pixelWidth);
            var rightPad  = Mathf.Clamp01(rightPadPx / pixelWidth);
            var topPad    = Mathf.Clamp01(topPadPx / pixelHeight);
            var bottomPad = Mathf.Clamp01(bottomPadPx / pixelHeight);

            leftPad   = Mathf.Clamp01(leftPad + GetNormalizedThickness(padding.leftRect, referenceCamera, true));
            rightPad  = Mathf.Clamp01(rightPad + GetNormalizedThickness(padding.rightRect, referenceCamera, true));
            topPad    = Mathf.Clamp01(topPad + GetNormalizedThickness(padding.topRect, referenceCamera, false));
            bottomPad = Mathf.Clamp01(bottomPad + GetNormalizedThickness(padding.bottomRect, referenceCamera, false));

            return new(leftPad, rightPad, topPad, bottomPad);
        }

        private static float GetNormalizedThickness(RectTransform rect, Camera referenceCamera, bool isHorizontal)
        {
            if (rect == null)
            {
                return 0f;
            }

            rect.GetWorldCorners(WorldCornersCache);
            var min = float.PositiveInfinity;
            var max = float.NegativeInfinity;

            for (var i = 0; i < WorldCornersCache.Length; i++)
            {
                var screenPoint      = RectTransformUtility.WorldToScreenPoint(referenceCamera, WorldCornersCache[i]);
                var value            = isHorizontal ? screenPoint.x : screenPoint.y;
                if (value < min) min = value;
                if (value > max) max = value;
            }

            if (!float.IsFinite(min) || !float.IsFinite(max))
            {
                return 0f;
            }

            var screenSize = isHorizontal ? Screen.width : Screen.height;
            if (screenSize <= 0f)
            {
                return 0f;
            }

            var thicknessPx = Mathf.Abs(max - min);
            return Mathf.Clamp01(thicknessPx / screenSize);
        }

        public class Padding
        {
            public float left;
            public float right;
            public float top;
            public float bottom;

            public RectTransform leftRect;
            public RectTransform rightRect;
            public RectTransform topRect;
            public RectTransform bottomRect;

            public Camera referenceCamera;
        }
    }
}