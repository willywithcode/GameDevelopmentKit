namespace GameFoundation.Scripts.Utils
{
    using UnityEngine;

    public static class CameraExtensions
    {
        public static void FitCamera(Camera camera, Padding padding, Bounds targetBounds)
        {
            if (camera == null) return;
            if (!camera.orthographic) camera.orthographic = true;

            padding ??= new();

            var refWidth    = padding.referenceWidth > 0 ? padding.referenceWidth : Screen.width;
            var refHeight   = padding.referenceHeight > 0 ? padding.referenceHeight : Screen.height;
            var screenWidth  = (float)Screen.width;
            var screenHeight = (float)Screen.height;

            // Calculate scale factor like CanvasScaler does
            var scaleW      = screenWidth / refWidth;
            var scaleH      = screenHeight / refHeight;
            var match       = padding.matchWidthOrHeight;
            var scaleFactor = Mathf.Pow(scaleW, 1f - match) * Mathf.Pow(scaleH, match);

            // Calculate actual padding in screen pixels, then normalize
            var actualBottom = padding.bottom * scaleFactor;
            var actualTop    = padding.top * scaleFactor;
            var actualLeft   = padding.left * scaleFactor;
            var actualRight  = padding.right * scaleFactor;

            var bottomPad = Mathf.Clamp01(actualBottom / screenHeight);
            var topPad    = Mathf.Clamp01(actualTop / screenHeight);
            var leftPad   = Mathf.Clamp01(actualLeft / screenWidth);
            var rightPad  = Mathf.Clamp01(actualRight / screenWidth);

            var verticalSpace   = Mathf.Max(0.01f, 1f - topPad - bottomPad);
            var horizontalSpace = Mathf.Max(0.01f, 1f - leftPad - rightPad);

            var boundsSize = targetBounds.size;
            var aspect     = Mathf.Max(camera.aspect, 0.01f);

            var halfHeightForVertical   = boundsSize.y * 0.5f / verticalSpace;
            var halfHeightForHorizontal = boundsSize.x * 0.5f / (horizontalSpace * aspect);
            camera.orthographicSize = Mathf.Max(halfHeightForVertical, halfHeightForHorizontal, 0.01f);

            var viewHeight = camera.orthographicSize * 2f;
            var viewWidth  = viewHeight * aspect;

            // Calculate offset to center bounds in available space
            var offsetY = (topPad - bottomPad) * 0.5f * viewHeight;
            var offsetX = (rightPad - leftPad) * 0.5f * viewWidth;

            var position = camera.transform.position;
            position.x                = targetBounds.center.x + offsetX;
            position.y                = targetBounds.center.y + offsetY;
            camera.transform.position = position;
        }

        public class Padding
        {
            public float left;
            public float right;
            public float top;
            public float bottom;
            public float referenceWidth;
            public float referenceHeight;
            public float matchWidthOrHeight; // 0 = width, 1 = height, 0.5 = blend
        }
    }
}
