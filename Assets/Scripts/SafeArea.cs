using UnityEngine;

namespace Pong
{
    /// How far in from each edge content must sit to clear notches, rounded corners and gesture
    /// areas, measured in panel points rather than pixels.
    public readonly struct SafeAreaInsets
    {
        public SafeAreaInsets(float left, float right, float top, float bottom)
        {
            Left = left;
            Right = right;
            Top = top;
            Bottom = bottom;
        }

        public static SafeAreaInsets None => new SafeAreaInsets(0f, 0f, 0f, 0f);

        public float Left { get; }
        public float Right { get; }
        public float Top { get; }
        public float Bottom { get; }

        public bool IsNone => Left <= 0f && Right <= 0f && Top <= 0f && Bottom <= 0f;
    }

    /// Turns a screen's safe area into panel-space insets. Pure maths, so every cutout a device can
    /// have is cheap to check without owning one.
    public static class SafeArea
    {
        /// The panel is measured in points and the safe area in pixels, so the two only agree after
        /// scaling. The safe area is measured from the bottom left, the panel from the top left,
        /// which is why top and bottom are not symmetrical here.
        public static SafeAreaInsets Insets(Rect safeArea, Vector2 screenSize, Vector2 panelSize)
        {
            if (screenSize.x <= 0f || screenSize.y <= 0f || panelSize.x <= 0f || panelSize.y <= 0f)
            {
                return SafeAreaInsets.None;
            }

            float scaleX = panelSize.x / screenSize.x;
            float scaleY = panelSize.y / screenSize.y;

            return new SafeAreaInsets(
                Mathf.Max(0f, safeArea.xMin) * scaleX,
                Mathf.Max(0f, screenSize.x - safeArea.xMax) * scaleX,
                Mathf.Max(0f, screenSize.y - safeArea.yMax) * scaleY,
                Mathf.Max(0f, safeArea.yMin) * scaleY
            );
        }
    }
}
