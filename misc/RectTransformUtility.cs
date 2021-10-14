using UnityEngine;

namespace com.samwalz.unity_ui.misc
{
    public static class RectTransformUtility
    {
        private static readonly Vector3[] FourCorners = new Vector3[4];
        public static Rect GetLocalRect(RectTransform rectTransform)
        {
            rectTransform.GetLocalCorners(FourCorners);
            return Rect.MinMaxRect(
                FourCorners[0].x, FourCorners[0].y,
                FourCorners[2].x, FourCorners[2].y
            );
        }
        public static Rect GetWorldRect(RectTransform rectTransform)
        {
            rectTransform.GetWorldCorners(FourCorners);
            // from unity docs:
            // Each corner provides its world space value.
            // The returned array of 4 vertices is clockwise.
            // It starts bottom left and rotates to top left, then top right, and finally bottom right.
            // Note that bottom left, for example, is an (x, y, z) vector with x being left and y being bottom.
            return Rect.MinMaxRect(
                FourCorners[0].x, FourCorners[0].y,
                FourCorners[2].x, FourCorners[2].y
                );
        }
    }
}